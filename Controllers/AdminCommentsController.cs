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
    /// Proactive comment moderation. Both admin levels can use it (content is their job).
    /// Reuses the IsTakenDown machinery already used by AdminModerationController, but lets
    /// an admin browse / filter / act on comments directly instead of waiting for a report.
    /// </summary>
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditLogService _audit;

        public AdminCommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, AuditLogService audit)
        {
            _context = context;
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<IActionResult> Index(string? search, string? status)
        {
            var query = _context.CommunityComments
                .Include(c => c.User)
                .Include(c => c.Post)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var kw = search.Trim();
                query = query.Where(c =>
                    (c.Content != null && c.Content.Contains(kw)) ||
                    (c.User != null && c.User.Email != null && c.User.Email.Contains(kw)) ||
                    (c.User != null && c.User.UserName != null && c.User.UserName.Contains(kw)));
            }

            // status filter: "active" (default) | "taken" | "all"
            var effectiveStatus = string.IsNullOrEmpty(status) ? "active" : status;
            if (effectiveStatus == "active")
                query = query.Where(c => !c.IsTakenDown);
            else if (effectiveStatus == "taken")
                query = query.Where(c => c.IsTakenDown);

            var comments = await query
                .OrderByDescending(c => c.CreatedDate)
                .Take(300)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = effectiveStatus;
            ViewBag.TotalCount = await _context.CommunityComments.CountAsync();
            ViewBag.TakenCount = await _context.CommunityComments.CountAsync(c => c.IsTakenDown);
            ViewBag.ActiveCount = await _context.CommunityComments.CountAsync(c => !c.IsTakenDown);
            return View(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeDown(int id, string? reason)
        {
            var comment = await _context.CommunityComments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            comment.IsTakenDown = true;
            comment.TakenDownAt = DateTime.UtcNow;
            comment.TakenDownByUserId = _userManager.GetUserId(User);
            comment.TakenDownReason = reason;

            if (!string.IsNullOrEmpty(comment.UserId))
            {
                _context.UserNotifications.Add(new UserNotification
                {
                    UserId = comment.UserId,
                    SenderUserId = _userManager.GetUserId(User),
                    Type = "moderation",
                    Title = "Your comment was taken down for review",
                    Message = $"A moderator took down your comment. Reason: {reason ?? "violated community guidelines"}. Please revise it and contact support if you believe this was a mistake.",
                    TargetUrl = comment.PostId > 0 ? $"/Community/Details/{comment.PostId}" : "/Community",
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await _audit.RecordOperationAsync("TakeDownComment", "CommunityComment", comment.Id.ToString(),
                $"Took down comment #{comment.Id}. Reason: {reason ?? "(none)"}.");

            TempData["Success"] = "Comment taken down. The author was notified.";
            return RedirectToAction(nameof(Index), new { status = "taken" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var comment = await _context.CommunityComments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            comment.IsTakenDown = false;
            comment.TakenDownReason = null;
            await _context.SaveChangesAsync();
            await _audit.RecordOperationAsync("RestoreComment", "CommunityComment", comment.Id.ToString(),
                $"Restored comment #{comment.Id}.");

            TempData["Success"] = "Comment restored.";
            return RedirectToAction(nameof(Index), new { status = "active" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.CommunityComments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            // Remove replies first so we don't leave orphaned child comments.
            var replies = await _context.CommunityComments
                .Where(c => c.ParentCommentId == id)
                .ToListAsync();
            _context.CommunityComments.RemoveRange(replies);
            _context.CommunityComments.Remove(comment);

            await _context.SaveChangesAsync();
            await _audit.RecordOperationAsync("DeleteComment", "CommunityComment", comment.Id.ToString(),
                $"Permanently deleted comment #{comment.Id} and {replies.Count} repl(y/ies).");

            TempData["Success"] = "Comment deleted permanently.";
            return RedirectToAction(nameof(Index), new { status = "all" });
        }
    }
}
