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
    /// Site-wide announcement broadcaster. Reuses the existing UserNotification system
    /// (already used for moderation notices) — just fans a message out to many recipients.
    /// </summary>
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminBroadcastController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditLogService _audit;

        public AdminBroadcastController(ApplicationDbContext context, UserManager<IdentityUser> userManager, AuditLogService audit)
        {
            _context = context;
            _userManager = userManager;
            _audit = audit;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string title, string message, string audience, string? link)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Title and message are required.";
                return View(nameof(Index));
            }

            audience = string.IsNullOrEmpty(audience) ? "All" : audience;

            var targetIds = new HashSet<string>();
            if (audience == "All")
            {
                foreach (var role in new[] { AppRoles.Student, AppRoles.Admin, AppRoles.SuperAdmin })
                foreach (var u in await _userManager.GetUsersInRoleAsync(role))
                    targetIds.Add(u.Id);
            }
            else if (audience == "Students")
            {
                foreach (var u in await _userManager.GetUsersInRoleAsync(AppRoles.Student))
                    targetIds.Add(u.Id);
            }
            else // Admins
            {
                foreach (var role in new[] { AppRoles.Admin, AppRoles.SuperAdmin })
                foreach (var u in await _userManager.GetUsersInRoleAsync(role))
                    targetIds.Add(u.Id);
            }

            var senderId = _userManager.GetUserId(User);
            var targetUrl = string.IsNullOrWhiteSpace(link) ? "/Community" : link;

            foreach (var uid in targetIds)
            {
                _context.UserNotifications.Add(new UserNotification
                {
                    UserId = uid,
                    SenderUserId = senderId,
                    Type = "announcement",
                    Title = title,
                    Message = message,
                    TargetUrl = targetUrl,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await _audit.RecordOperationAsync("Broadcast", "UserNotification", null,
                $"Sent announcement '{title}' to {targetIds.Count} recipient(s) (audience: {audience}).");

            TempData["Success"] = $"Announcement sent to {targetIds.Count} recipient(s).";
            return RedirectToAction(nameof(Index));
        }
    }
}
