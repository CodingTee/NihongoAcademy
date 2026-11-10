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
    /// Content moderation queue. Accessible to BOTH admin levels (content is their job),
    /// but role-account management lives in AdminAccountsController (Super Admin only).
    /// </summary>
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminModerationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditLogService _audit;

        public AdminModerationController(ApplicationDbContext context, UserManager<IdentityUser> userManager, AuditLogService audit)
        {
            _context = context;
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<IActionResult> Index(ReportStatus? status = null, string? type = null)
        {
            var query = _context.ContentReports
                .Include(r => r.ReporterUser)
                .AsQueryable();

            if (status.HasValue) query = query.Where(r => r.Status == status.Value);
            if (!string.IsNullOrEmpty(type))
            {
                if (Enum.TryParse<ReportTargetType>(type, ignoreCase: true, out var tt))
                    query = query.Where(r => r.TargetType == tt);
            }

            var reports = await query
                .OrderByDescending(r => r.CreatedDate)
                .Take(200)
                .ToListAsync();

            // Light-weight previews of the reported content for the queue.
            var postIds = reports.Where(r => r.TargetType == ReportTargetType.Post).Select(r => r.TargetId).Distinct().ToList();
            var commentIds = reports.Where(r => r.TargetType == ReportTargetType.Comment).Select(r => r.TargetId).Distinct().ToList();
            var postTitles = await _context.CommunityPosts
                .Where(p => postIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Title);
            var commentTexts = await _context.CommunityComments
                .Where(c => commentIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Content);
            var takenDownPostIds = (await _context.CommunityPosts
                .Where(p => postIds.Contains(p.Id) && p.IsTakenDown)
                .Select(p => p.Id).ToListAsync()).ToHashSet();
            var takenDownCommentIds = (await _context.CommunityComments
                .Where(c => commentIds.Contains(c.Id) && c.IsTakenDown)
                .Select(c => c.Id).ToListAsync()).ToHashSet();

            ViewBag.PostTitles = postTitles;
            ViewBag.CommentTexts = commentTexts;
            ViewBag.TakenDownPostIds = takenDownPostIds;
            ViewBag.TakenDownCommentIds = takenDownCommentIds;
            ViewBag.StatusFilter = status;
            ViewBag.TypeFilter = type;
            return View(reports);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var report = await _context.ContentReports
                .Include(r => r.ReporterUser)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            object? target = null;
            if (report.TargetType == ReportTargetType.Post)
            {
                target = await _context.CommunityPosts
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == report.TargetId);
            }
            else
            {
                target = await _context.CommunityComments
                    .Include(c => c.User)
                    .Include(c => c.Post)
                    .FirstOrDefaultAsync(c => c.Id == report.TargetId);
            }

            ViewBag.Target = target;
            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? note)
        {
            var report = await _context.ContentReports.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            string? ownerId = null;
            string? ownerEmail = null;
            string targetLabel = report.TargetType == ReportTargetType.Post ? "post" : "comment";

            if (report.TargetType == ReportTargetType.Post)
            {
                var post = await _context.CommunityPosts.FirstOrDefaultAsync(p => p.Id == report.TargetId);
                if (post != null)
                {
                    post.IsTakenDown = true;
                    post.TakenDownAt = DateTime.UtcNow;
                    post.TakenDownByUserId = _userManager.GetUserId(User);
                    post.TakenDownReason = note;
                    ownerId = post.UserId;
                }
            }
            else
            {
                var comment = await _context.CommunityComments.FirstOrDefaultAsync(c => c.Id == report.TargetId);
                if (comment != null)
                {
                    comment.IsTakenDown = true;
                    comment.TakenDownAt = DateTime.UtcNow;
                    comment.TakenDownByUserId = _userManager.GetUserId(User);
                    comment.TakenDownReason = note;
                    ownerId = comment.UserId;
                }
            }

            report.Status = ReportStatus.Approved;
            report.ResolvedDate = DateTime.UtcNow;
            report.ResolvedByUserId = _userManager.GetUserId(User);
            report.ResolutionNote = note;

            if (!string.IsNullOrEmpty(ownerId))
            {
                var owner = await _userManager.FindByIdAsync(ownerId);
                ownerEmail = owner?.Email;
                _context.UserNotifications.Add(new UserNotification
                {
                    UserId = ownerId,
                    SenderUserId = _userManager.GetUserId(User),
                    Type = "moderation",
                    Title = "Your content was taken down for review",
                    Message = $"Your {targetLabel} was reported and taken down by a moderator. Reason: {note ?? report.Reason}. Please revise it and contact support if you believe this was a mistake.",
                    TargetUrl = report.TargetType == ReportTargetType.Post
                        ? $"/Community/Details/{report.TargetId}"
                        : $"/Community/Details/{_context.CommunityComments.Where(c => c.Id == report.TargetId).Select(c => c.PostId).FirstOrDefault()}",
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await _audit.RecordOperationAsync("ApproveReport", "ContentReport", report.Id.ToString(),
                $"Took down {targetLabel} #{report.TargetId}. Reason: {report.Reason}.");

            TempData["Success"] = $"Report approved. The {targetLabel} was taken down for rectification.";
            return RedirectToAction(nameof(Index), new { status = (int)ReportStatus.Pending });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dismiss(int id, string? note)
        {
            var report = await _context.ContentReports.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            report.Status = ReportStatus.Dismissed;
            report.ResolvedDate = DateTime.UtcNow;
            report.ResolvedByUserId = _userManager.GetUserId(User);
            report.ResolutionNote = note;
            await _context.SaveChangesAsync();

            await _audit.RecordOperationAsync("DismissReport", "ContentReport", report.Id.ToString(),
                $"Dismissed report on {report.TargetType} #{report.TargetId}. Reason: {report.Reason}.");

            TempData["Success"] = "Report dismissed. No action taken.";
            return RedirectToAction(nameof(Index), new { status = (int)ReportStatus.Pending });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int targetId, ReportTargetType targetType)
        {
            if (targetType == ReportTargetType.Post)
            {
                var post = await _context.CommunityPosts.FirstOrDefaultAsync(p => p.Id == targetId);
                if (post != null) { post.IsTakenDown = false; post.TakenDownReason = null; }
            }
            else
            {
                var comment = await _context.CommunityComments.FirstOrDefaultAsync(c => c.Id == targetId);
                if (comment != null) { comment.IsTakenDown = false; comment.TakenDownReason = null; }
            }
            await _context.SaveChangesAsync();
            await _audit.RecordOperationAsync("RestoreContent", targetType.ToString(), targetId.ToString(),
                $"Restored {targetType.ToString().ToLowerInvariant()} #{targetId}.");

            TempData["Success"] = "Content restored.";
            return RedirectToAction(nameof(Index));
        }
    }
}
