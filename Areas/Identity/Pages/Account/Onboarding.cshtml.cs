using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Areas.Identity.Pages.Account
{
    [Authorize]
    public class OnboardingModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IWebHostEnvironment _env;

        public OnboardingModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; private set; } = "/";

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter a username handle.")]
            [StringLength(32, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 32 characters.")]
            [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Use letters, numbers, dot, hyphen or underscore only.")]
            [Display(Name = "Username handle")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter a display name or nickname.")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 50 characters.")]
            [Display(Name = "Display name / Nickname")]
            public string DisplayName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please select your native language.")]
            [StringLength(30)]
            [Display(Name = "Native language")]
            public string NativeLanguage { get; set; } = "English";

            [Required(ErrorMessage = "Please select your country or region.")]
            [StringLength(50)]
            [Display(Name = "Country / Region")]
            public string Country { get; set; } = "Malaysia";

            public string? AvatarJson { get; set; }

            public string AvatarMode { get; set; } = "generated";

            [Display(Name = "Upload photo")]
            public IFormFile? AvatarFile { get; set; }

            [Display(Name = "Learning goals")]
            public List<string> LearningGoals { get; set; } = new();

            public string LearningGoal { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please select your current Japanese level.")]
            [StringLength(50), RegularExpression("^(Absolute beginner|Knows hiragana|Conversational basics|Intermediate or above)$", ErrorMessage = "Please select a valid level.")]
            public string CurrentLevel { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please select your age group.")]
            [StringLength(30), RegularExpression("^(Under 18|18 – 24|25 – 34|35 or older)$", ErrorMessage = "Please select a valid age group.")]
            public string AgeGroup { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = SafeReturnUrl(returnUrl);
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.UserProfiles.AsNoTracking().SingleOrDefaultAsync(p => p.UserId == userId);

            var currentUserName = user?.UserName ?? string.Empty;
            var suggestedUsername = currentUserName;
            if (currentUserName.Contains('@'))
            {
                var rawPrefix = currentUserName.Split('@')[0];
                suggestedUsername = Regex.Replace(rawPrefix, @"[^a-zA-Z0-9_.-]", "");
                if (suggestedUsername.Length < 3) suggestedUsername = suggestedUsername.PadRight(3, '0');
                if (suggestedUsername.Length > 30) suggestedUsername = suggestedUsername.Substring(0, 30);
            }

            if (profile != null)
            {
                Input.Username = (!string.IsNullOrWhiteSpace(user?.UserName) && !user.UserName.Contains('@'))
                    ? user.UserName
                    : suggestedUsername;
                Input.DisplayName = !string.IsNullOrWhiteSpace(profile.DisplayName)
                    ? profile.DisplayName
                    : suggestedUsername;
                Input.NativeLanguage = profile.NativeLanguage ?? "English";
                Input.Country = profile.Country ?? "Malaysia";
                Input.AvatarJson = profile.AvatarJson;
                Input.AvatarMode = profile.AvatarMode ?? "generated";
                Input.LearningGoal = profile.LearningGoal ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(profile.LearningGoal))
                {
                    Input.LearningGoals = profile.LearningGoal
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList();
                }
                Input.CurrentLevel = profile.CurrentLevel ?? string.Empty;
                Input.AgeGroup = profile.AgeGroup ?? string.Empty;
            }
            else
            {
                Input.Username = suggestedUsername;
                Input.DisplayName = suggestedUsername;
                Input.NativeLanguage = "English";
                Input.Country = "Malaysia";
            }
            return Page();
        }

        public async Task<IActionResult> OnGetCheckUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new JsonResult(new { valid = false, available = false, message = "Username cannot be empty." });
            }
            username = username.Trim();
            if (username.Length < 3 || username.Length > 32)
            {
                return new JsonResult(new { valid = false, available = false, message = "Username must be between 3 and 32 characters." });
            }
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_.-]+$"))
            {
                return new JsonResult(new { valid = false, available = false, message = "Only letters, numbers, dot, hyphen or underscore allowed." });
            }

            var currentUserId = _userManager.GetUserId(User);
            var existing = await _userManager.FindByNameAsync(username);
            if (existing != null && existing.Id != currentUserId)
            {
                return new JsonResult(new { valid = true, available = false, message = "This username is already taken. Please choose another." });
            }

            return new JsonResult(new { valid = true, available = true, message = "Username is available!" });
        }

        public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile avatarFile)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            if (avatarFile == null || avatarFile.Length == 0)
            {
                return new JsonResult(new { success = false, message = "No file selected." });
            }

            var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowed.Contains(ext))
            {
                return new JsonResult(new { success = false, message = "Only JPG, PNG, GIF or WebP images are allowed." });
            }
            if (avatarFile.Length > 3 * 1024 * 1024)
            {
                return new JsonResult(new { success = false, message = "Image size must be smaller than 3 MB." });
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{userId}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/avatars/{fileName}";
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _context.UserProfiles.Add(profile);
            }
            profile.AvatarImagePath = relativePath;
            profile.AvatarMode = "uploaded";
            profile.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, avatarUrl = relativePath, message = "Photo uploaded and saved successfully!" });
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = SafeReturnUrl(returnUrl);
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cleanUsername = Input.Username?.Trim() ?? string.Empty;

            // Check username uniqueness
            var existingUser = await _userManager.FindByNameAsync(cleanUsername);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("Input.Username", "This username is already taken. Please choose another.");
            }

            // Process uploaded avatar photo first if supplied so it's not lost
            string? uploadedAvatarPath = null;
            if (Input.AvatarFile != null && Input.AvatarFile.Length > 0)
            {
                var ext = Path.GetExtension(Input.AvatarFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("Input.AvatarFile", "Only JPG, PNG, GIF or WebP images are allowed.");
                }
                else if (Input.AvatarFile.Length > 3 * 1024 * 1024)
                {
                    ModelState.AddModelError("Input.AvatarFile", "Image must be smaller than 3 MB.");
                }
                else
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                    Directory.CreateDirectory(uploadsDir);
                    var fileName = $"{userId}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.AvatarFile.CopyToAsync(stream);
                    }
                    uploadedAvatarPath = $"/uploads/avatars/{fileName}";
                }
            }

            var validGoals = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Travel & culture", "Work & career", "Study & exams", "Anime & entertainment"
            };
            var chosenGoals = Input.LearningGoals?.Where(g => validGoals.Contains(g)).Distinct().ToList() ?? new List<string>();
            if (chosenGoals.Count == 0 && !string.IsNullOrWhiteSpace(Input.LearningGoal) && validGoals.Contains(Input.LearningGoal))
            {
                chosenGoals.Add(Input.LearningGoal);
            }

            if (chosenGoals.Count == 0)
            {
                ModelState.AddModelError("Input.LearningGoals", "Please select at least one learning goal.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Update username in Identity
            var currentUserName = await _userManager.GetUserNameAsync(user);
            if (!string.Equals(currentUserName, cleanUsername, StringComparison.OrdinalIgnoreCase))
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(user, cleanUsername);
                if (!setUsernameResult.Succeeded)
                {
                    foreach (var err in setUsernameResult.Errors)
                    {
                        ModelState.AddModelError("Input.Username", err.Description);
                    }
                    return Page();
                }
            }

            var profile = await _context.UserProfiles.SingleOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _context.UserProfiles.Add(profile);
            }

            profile.DisplayName = Input.DisplayName.Trim();
            profile.NativeLanguage = Input.NativeLanguage?.Trim();
            profile.Country = Input.Country?.Trim();
            profile.AvatarJson = Input.AvatarJson;
            profile.AvatarMode = Input.AvatarMode == "uploaded" ? "uploaded" : "generated";
            if (uploadedAvatarPath != null)
            {
                profile.AvatarImagePath = uploadedAvatarPath;
            }
            profile.LearningGoal = string.Join(", ", chosenGoals);
            profile.CurrentLevel = Input.CurrentLevel;
            profile.AgeGroup = Input.AgeGroup;
            profile.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Welcome to NihongoAcademy! Your profile and study plan have been set up.";
            TempData["TriggerAppTour"] = "true";
            return LocalRedirect(ReturnUrl);
        }

        private string SafeReturnUrl(string? returnUrl) =>
            !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/" && returnUrl != "~/"
                ? returnUrl 
                : Url.Content("~/Lessons");
    }
}
