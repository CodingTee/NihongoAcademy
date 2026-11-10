using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Notifications/Recent
        [HttpGet]
        public async Task<IActionResult> Recent()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var notifications = await _context.UserNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(15)
                .Select(n => new
                {
                    n.Id,
                    n.Type,
                    n.Title,
                    n.Message,
                    n.TargetUrl,
                    n.IsRead,
                    CreatedDate = n.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
                })
                .ToListAsync();

            var unreadNotifs = await _context.UserNotifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            var unreadDms = await _context.DirectMessages
                .CountAsync(m => m.ReceiverUserId == userId && !m.IsRead);

            return Json(new
            {
                success = true,
                notifications,
                unreadCount = unreadNotifs + unreadDms,
                unreadNotifsCount = unreadNotifs,
                unreadDmsCount = unreadDms
            });
        }

        // GET: /Notifications/RecentChatsAndBuddies
        [HttpGet]
        public async Task<IActionResult> RecentChatsAndBuddies()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var unreadDms = await _context.DirectMessages
                .CountAsync(m => m.ReceiverUserId == userId && !m.IsRead);

            // Fetch recent messages
            var myMessages = await _context.DirectMessages
                .Where(m => m.SenderUserId == userId || m.ReceiverUserId == userId)
                .OrderByDescending(m => m.SentAt)
                .Take(100)
                .ToListAsync();

            var partnerIds = myMessages
                .Select(m => m.SenderUserId == userId ? m.ReceiverUserId : m.SenderUserId)
                .Distinct()
                .Take(15)
                .ToList();

            // Fetch buddies
            var relationships = await _context.UserRelationships
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
            var buddyIds = relationships.Select(r => r.TargetUserId).Distinct().ToList();

            var allRelevantUserIds = partnerIds.Union(buddyIds).Distinct().ToList();

            var relevantUsers = await _context.Users
                .Where(u => allRelevantUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "learner");

            var relevantProfiles = await _context.UserProfiles
                .Where(p => allRelevantUserIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => p);

            var mutualBuddyIds = await _context.UserRelationships
                .Where(r => buddyIds.Contains(r.UserId) && r.TargetUserId == userId)
                .Select(r => r.UserId)
                .ToListAsync();

            var recentChats = new List<object>();
            foreach (var pid in partnerIds)
            {
                var lastMsg = myMessages.First(m => (m.SenderUserId == pid && m.ReceiverUserId == userId) || (m.SenderUserId == userId && m.ReceiverUserId == pid));
                var unreadFromPartner = myMessages.Count(m => m.SenderUserId == pid && m.ReceiverUserId == userId && !m.IsRead);
                var pUserName = relevantUsers.ContainsKey(pid) ? relevantUsers[pid] : "learner";
                var pProfile = relevantProfiles.ContainsKey(pid) ? relevantProfiles[pid] : null;

                recentChats.Add(new
                {
                    userId = pid,
                    userName = pUserName,
                    displayName = pProfile?.DisplayName ?? pUserName,
                    avatarJson = pProfile?.AvatarJson,
                    avatarImagePath = pProfile?.AvatarImagePath,
                    avatarMode = pProfile?.AvatarMode ?? "generated",
                    lastMessage = lastMsg.Content.Length > 45 ? lastMsg.Content.Substring(0, 42) + "..." : lastMsg.Content,
                    sentAtFormatted = lastMsg.SentAt.ToLocalTime().ToString("MMM d, HH:mm"),
                    isMine = lastMsg.SenderUserId == userId,
                    unreadCount = unreadFromPartner,
                    isMutual = mutualBuddyIds.Contains(pid)
                });
            }

            var buddies = new List<object>();
            foreach (var bid in buddyIds)
            {
                var bUserName = relevantUsers.ContainsKey(bid) ? relevantUsers[bid] : "learner";
                var bProfile = relevantProfiles.ContainsKey(bid) ? relevantProfiles[bid] : null;

                buddies.Add(new
                {
                    userId = bid,
                    userName = bUserName,
                    displayName = bProfile?.DisplayName ?? bUserName,
                    avatarJson = bProfile?.AvatarJson,
                    avatarImagePath = bProfile?.AvatarImagePath,
                    avatarMode = bProfile?.AvatarMode ?? "generated",
                    currentLevel = bProfile?.CurrentLevel ?? "N5",
                    totalXp = bProfile?.TotalXp ?? 0,
                    isMutual = mutualBuddyIds.Contains(bid)
                });
            }

            return Json(new
            {
                success = true,
                unreadDmsCount = unreadDms,
                recentChats,
                studyBuddies = buddies
            });
        }

        // POST: /Notifications/MarkDmAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDmAsRead([FromBody] MarkDmReadRequest? request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            if (!string.IsNullOrEmpty(request?.SenderUserId))
            {
                var unreadList = await _context.DirectMessages
                    .Where(m => m.SenderUserId == request.SenderUserId && m.ReceiverUserId == userId && !m.IsRead)
                    .ToListAsync();

                foreach (var msg in unreadList)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // POST: /Notifications/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkReadRequest? request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            if (request?.NotificationId != null)
            {
                var notif = await _context.UserNotifications
                    .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == userId);
                if (notif != null)
                {
                    notif.IsRead = true;
                }
            }
            else
            {
                // Mark all as read
                var unreadList = await _context.UserNotifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();
                foreach (var n in unreadList)
                {
                    n.IsRead = true;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class MarkDmReadRequest
    {
        public string SenderUserId { get; set; } = string.Empty;
    }

    public class MarkReadRequest
    {
        public int? NotificationId { get; set; }
    }
}
