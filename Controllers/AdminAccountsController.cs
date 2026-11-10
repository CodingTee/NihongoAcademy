using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    /// <summary>
    /// Account &amp; role management. SUPER ADMIN ONLY — ordinary admins may not touch
    /// role accounts (see requirement: ordinary admins manage content only and must not touch role accounts).
    /// Every role / lockout / password change calls UpdateSecurityStampAsync so the
    /// affected account's existing sessions expire immediately (combined with the
    /// zero validation interval configured in Program.cs).
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    public class AdminAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AuditLogService _audit;
        private readonly ProgressSummaryService _progress;

        public AdminAccountsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AuditLogService audit, ProgressSummaryService progress)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _audit = audit;
            _progress = progress;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var keyword = search?.Trim();
            var users = await _userManager.Users
                .Where(user => string.IsNullOrEmpty(keyword)
                    || (user.Email != null && user.Email.Contains(keyword))
                    || (user.UserName != null && user.UserName.Contains(keyword)))
                .OrderBy(user => user.Email)
                .ToListAsync();

            var userIds = users.Select(user => user.Id).ToList();
            var userRolePairs = await _context.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_context.Roles, ur => ur.RoleId, role => role.Id, (ur, role) => new { ur.UserId, role.Name })
                .ToListAsync();
            var userRoles = users.ToDictionary(user => user.Id, _ => (IList<string>)new List<string>());
            foreach (var group in userRolePairs.GroupBy(pair => pair.UserId))
            {
                userRoles[group.Key] = group.Select(pair => pair.Name!).ToList();
            }

            var superAdminCount = await _userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);

            ViewBag.UserRoles = userRoles;
            ViewBag.SuperAdminCount = superAdminCount.Count;
            ViewBag.RoleOptions = new[] { AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.Student };
            ViewBag.CurrentUserId = _userManager.GetUserId(User);
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string role)
        {
            var validRoles = new[] { AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.Student };
            if (!validRoles.Contains(role))
            {
                TempData["Error"] = "Invalid role.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault() ?? AppRoles.Student;
            if (currentRole == role)
            {
                TempData["Success"] = $"{user.Email} is already a {role}.";
                return RedirectToAction(nameof(Index));
            }

            // Never remove the last SuperAdmin.
            if (currentRole == AppRoles.SuperAdmin && role != AppRoles.SuperAdmin)
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);
                if (superAdmins.Count <= 1)
                {
                    TempData["Error"] = "Cannot demote the last Super Admin account.";
                    return RedirectToAction(nameof(Index));
                }
            }

            foreach (var r in currentRoles)
                await _userManager.RemoveFromRoleAsync(user, r);
            await _userManager.AddToRoleAsync(user, role);

            // Immediately expire the target's sessions.
            await _userManager.UpdateSecurityStampAsync(user);

            await _audit.RecordOperationAsync("ChangeRole", "IdentityUser", user.Id,
                $"Changed role of {user.Email} from {currentRole} to {role}.");

            TempData["Success"] = $"Changed {user.Email} to {role}. Their session was invalidated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            ViewBag.TargetUser = user;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword, string confirmPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 8)
            {
                TempData["Error"] = "Password must be at least 8 characters.";
                ViewBag.TargetUser = user;
                return View();
            }
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                ViewBag.TargetUser = user;
                return View();
            }

            // Replace the password directly (admin-privileged change, no email token needed).
            // Accounts created via external login may have no local password — just add one.
            if (await _userManager.HasPasswordAsync(user))
            {
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = string.Join(" ", removeResult.Errors.Select(e => e.Description));
                    ViewBag.TargetUser = user;
                    return View();
                }
            }
            var addResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addResult.Succeeded)
            {
                TempData["Error"] = string.Join(" ", addResult.Errors.Select(e => e.Description));
                ViewBag.TargetUser = user;
                return View();
            }

            // Force the user to sign in again with the new password. A lock is an explicit,
            // separate control — changing the password does not silently lift it.
            await _userManager.UpdateSecurityStampAsync(user);

            await _audit.RecordOperationAsync("ChangePassword", "IdentityUser", user.Id,
                $"Admin changed password for {user.Email}.");

            TempData["Success"] = $"Password for {user.Email} has been changed. Their session was invalidated.";
            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------- account details ---

        /// <summary>Full account inspector: identity fields, profile, and recent logins.</summary>
        [HttpGet]
        public async Task<IActionResult> Details(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            ViewBag.Profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
            ViewBag.Roles = await _userManager.GetRolesAsync(user);
            ViewBag.RecentLogins = await _context.LoginLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.LoginTime)
                .Take(6)
                .ToListAsync();
            ViewBag.SuccessfulLogins = await _context.LoginLogs
                .Where(l => l.UserId == userId && l.Succeeded)
                .CountAsync();
            ViewBag.TargetUser = user;

            // Streak is derived from activity (not a stored column) — show it read-only.
            try
            {
                var summary = await _progress.BuildAsync(userId);
                ViewBag.CurrentStreak = summary.CurrentStreak;
                ViewBag.LongestStreak = summary.LongestStreak;
            }
            catch
            {
                ViewBag.CurrentStreak = 0;
                ViewBag.LongestStreak = 0;
            }

            return View();
        }

        /// <summary>Admin adjustment of a learner's XP and/or sparks. Every change is written to
        /// UserXpLog (for XP) and the audit log, and totals are clamped at zero.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustPoints(string userId, int? xpDelta, int? sparksDelta)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _context.UserProfiles.Add(profile);
            }

            var actor = User.Identity?.Name ?? "admin";
            var changed = false;

            if (xpDelta.HasValue && xpDelta.Value != 0)
            {
                var before = profile.TotalXp;
                profile.TotalXp = Math.Max(0, profile.TotalXp + xpDelta.Value);
                _context.UserXpLogs.Add(new UserXpLog
                {
                    UserId = userId,
                    Amount = xpDelta.Value,
                    Source = "AdminAdjust",
                    Description = $"Adjusted by {actor} (was {before}).",
                    EarnedAt = DateTime.UtcNow
                });
                await _audit.RecordOperationAsync("AdjustXp", "UserProfile", userId,
                    $"Changed XP of {user.Email} by {xpDelta.Value} (was {before}, now {profile.TotalXp}).");
                changed = true;
            }

            if (sparksDelta.HasValue && sparksDelta.Value != 0)
            {
                var before = profile.Sparks;
                profile.Sparks = Math.Max(0, profile.Sparks + sparksDelta.Value);
                await _audit.RecordOperationAsync("AdjustSparks", "UserProfile", userId,
                    $"Changed sparks of {user.Email} by {sparksDelta.Value} (was {before}, now {profile.Sparks}).");
                changed = true;
            }

            if (!changed)
            {
                TempData["Error"] = "No change specified (set a non-zero XP or Sparks value).";
                return RedirectToAction(nameof(Details), new { userId });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Updated points for {user.Email}.";
            return RedirectToAction(nameof(Details), new { userId });
        }

        /// <summary>Admin edit of the account's personal profile + phone / email-confirmed flags.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
            string userId, string? displayName, string? bio, string? nativeLanguage,
            string? country, string? learningGoal, string? currentLevel, string? ageGroup,
            string? themePreference, bool dailyReminder, string? phoneNumber, bool emailConfirmed)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // keep values inside the column limits (same rules as UserProfile's attributes)
            static string? Clamp(string? value, int max) =>
                string.IsNullOrWhiteSpace(value) ? null
                    : (value.Length <= max ? value : value.Substring(0, max));

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _context.UserProfiles.Add(profile);
            }

            profile.DisplayName = Clamp(displayName, 50);
            profile.Bio = Clamp(bio, 240);
            profile.NativeLanguage = Clamp(nativeLanguage, 30);
            profile.Country = Clamp(country, 50);
            profile.LearningGoal = Clamp(learningGoal, 200);
            profile.CurrentLevel = Clamp(currentLevel, 50);
            profile.AgeGroup = Clamp(ageGroup, 30);
            profile.ThemePreference = themePreference == "dark" || themePreference == "system"
                ? themePreference
                : "light";
            profile.DailyReminder = dailyReminder;
            profile.ModifiedDate = DateTime.UtcNow;

            user.PhoneNumber = Clamp(phoneNumber, 50);
            user.EmailConfirmed = emailConfirmed;

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                TempData["Error"] = string.Join(" ", update.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Details), new { userId });
            }
            await _context.SaveChangesAsync();

            await _audit.RecordOperationAsync("EditProfile", "IdentityUser", user.Id,
                $"Edited profile of {user.Email}.");

            TempData["Success"] = $"Profile of {user.Email} has been updated.";
            return RedirectToAction(nameof(Details), new { userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockout(string userId, bool disable)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["Error"] = "You cannot disable your own account.";
                return RedirectToAction(nameof(Index));
            }

            if (disable)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains(AppRoles.SuperAdmin))
                {
                    var superAdmins = await _userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);
                    if (superAdmins.Count <= 1)
                    {
                        TempData["Error"] = "Cannot lock the last Super Admin account.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                // Lockout only takes effect when LockoutEnabled is true. Setting the end date
                // alone leaves IsLockedOut() == false for accounts whose lockout was never
                // enabled, so the account would still be able to sign in.
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                await _userManager.ResetAccessFailedCountAsync(user);
                await _audit.RecordOperationAsync("LockUser", "IdentityUser", user.Id,
                    $"Locked account {user.Email}.");
                TempData["Success"] = $"Locked {user.Email}. They were signed out and cannot log in until unlocked.";
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
                await _audit.RecordOperationAsync("UnlockUser", "IdentityUser", user.Id,
                    $"Unlocked account {user.Email}.");
                TempData["Success"] = $"Unlocked {user.Email}. They can sign in again.";
            }

            // Immediately expire the target's sessions on disable.
            if (disable)
                await _userManager.UpdateSecurityStampAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
