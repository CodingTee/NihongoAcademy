using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    [Authorize]
    public class CommunityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Services.AchievementService _achievementService;
        private readonly Services.XpService _xpService;

        public CommunityController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            Services.AchievementService achievementService,
            Services.XpService xpService)
        {
            _context = context;
            _userManager = userManager;
            _achievementService = achievementService;
            _xpService = xpService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Hub(string mode = "feed", int? id = null, CommunityCategory? category = null, string? search = null, string sort = "latest", int page = 1)
        {
            if (mode.Equals("post", StringComparison.OrdinalIgnoreCase) && id.HasValue)
                return await Details(id.Value);
            if (mode.Equals("create", StringComparison.OrdinalIgnoreCase))
                return Create();
            return await Index(category, search, sort, page);
        }

        // GET: /Community?category=Study&search=te-form&sort=popular
        [AllowAnonymous]
        public async Task<IActionResult> Index(CommunityCategory? category, string? search = null, string sort = "latest", int page = 1)
        {
            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            int pageSize = 15;

            var query = _context.CommunityPosts
                .Include(p => p.User)
                .AsQueryable();

            // 1. Category filter (ForYou = show all)
            if (category.HasValue && category.Value != CommunityCategory.ForYou)
            {
                query = query.Where(p => p.Category == category.Value);
            }

            // 2. Keyword Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    p.Title.Contains(term) ||
                    p.Content.Contains(term) ||
                    (p.User != null && p.User.UserName != null && p.User.UserName.Contains(term)));
            }

            // 2b. Hide taken-down content from everyone except moderators and the author.
            var isFeedAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (!isFeedAdmin)
            {
                query = query.Where(p => !p.IsTakenDown || p.UserId == userId);
            }

            // 3. Multi-dimensional Sorting & Quick-Filters
            sort = string.IsNullOrWhiteSpace(sort) ? "latest" : sort.ToLowerInvariant();

            if (sort == "unanswered")
            {
                query = query.Where(p => !p.Comments.Any());
                query = query.OrderByDescending(p => p.CreatedDate);
            }
            else if (sort == "feedback")
            {
                query = query.Where(p => p.FeedbackWanted);
                query = query.OrderByDescending(p => p.CreatedDate);
            }
            else if (sort == "saved")
            {
                if (userId != null)
                {
                    query = query.Where(p => _context.CommunityBookmarks.Any(b => b.PostId == p.Id && b.UserId == userId));
                }
                else
                {
                    // Anonymous users have no saved posts on server
                    query = query.Where(p => false);
                }
                query = query.OrderByDescending(p => p.CreatedDate);
            }
            else if (sort == "popular" || sort == "top")
            {
                query = query.OrderByDescending(p => p.Likes.Count + p.Comments.Count * 2).ThenByDescending(p => p.CreatedDate);
            }
            else
            {
                // default: latest
                sort = "latest";
                query = query.OrderByDescending(p => p.CreatedDate);
            }

            var totalPosts = await query.CountAsync();

            // Get paginated posts
            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postIds = posts.Select(p => p.Id).ToList();

            // Batch load like/comment counts + user avatar info
            var likeCounts = await _context.CommunityLikes
                .Where(l => postIds.Contains(l.PostId))
                .GroupBy(l => l.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PostId, x => x.Count);

            var commentCounts = await _context.CommunityComments
                .Where(c => postIds.Contains(c.PostId))
                .GroupBy(c => c.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PostId, x => x.Count);

            // Load community avatar preferences for post authors and current user
            var authorIds = posts.Select(p => p.UserId).Distinct().ToList();
            if (userId != null && !authorIds.Contains(userId))
            {
                authorIds.Add(userId);
            }
            var avatarMap = await _context.UserProfiles
                .Where(p => authorIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => new CommunityAvatarInfo
                {
                    AvatarJson = p.AvatarJson,
                    AvatarImagePath = p.AvatarImagePath,
                    AvatarMode = p.AvatarMode
                });

            // Which posts the current user has liked
            var likedPostIds = new HashSet<int>();
            // Which posts the current user has bookmarked
            var bookmarkedPostIds = new HashSet<int>();
            if (userId != null)
            {
                likedPostIds = (await _context.CommunityLikes
                    .Where(l => l.UserId == userId && postIds.Contains(l.PostId))
                    .Select(l => l.PostId)
                    .ToListAsync())
                    .ToHashSet();

                bookmarkedPostIds = (await _context.CommunityBookmarks
                    .Where(b => b.UserId == userId && postIds.Contains(b.PostId))
                    .Select(b => b.PostId)
                    .ToListAsync())
                    .ToHashSet();
            }

            // Quick-filter counters (within current category)
            var baseCategoryQuery = _context.CommunityPosts.AsQueryable();
            if (category.HasValue && category.Value != CommunityCategory.ForYou)
            {
                baseCategoryQuery = baseCategoryQuery.Where(p => p.Category == category.Value);
            }

            var unansweredCount = await baseCategoryQuery.Where(p => !p.Comments.Any()).CountAsync();
            var feedbackCount = await baseCategoryQuery.Where(p => p.FeedbackWanted).CountAsync();
            var savedCount = userId != null
                ? await _context.CommunityBookmarks.Where(b => b.UserId == userId).CountAsync()
                : 0;

            // Trending tags (top categories by post count in last 7 days)
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var trendingCategories = await _context.CommunityPosts
                .Where(p => p.CreatedDate >= sevenDaysAgo)
                .GroupBy(p => p.Category)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToListAsync();

            // Popular learners (top 3 by post count)
            var popularLearners = await _context.CommunityPosts
                .GroupBy(p => p.UserId)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new { UserId = g.Key, PostCount = g.Count() })
                .ToListAsync();

            var popularUserIds = popularLearners.Select(p => p.UserId).ToList();
            var popularUsers = await _context.Users
                .Where(u => popularUserIds.Contains(u.Id))
                .ToListAsync();

            var popularProfiles = await _context.UserProfiles
                .Where(p => popularUserIds.Contains(p.UserId))
                .ToListAsync();

            var weeklyLeaderboard = await _xpService.GetWeeklyLeaderboardAsync(userId, top: 5);

            var viewModel = new CommunityIndexViewModel
            {
                Posts = posts,
                PostLikeCounts = likeCounts,
                PostCommentCounts = commentCounts,
                AuthorAvatars = avatarMap,
                CurrentCategory = category ?? CommunityCategory.ForYou,
                Page = page,
                TotalPosts = totalPosts,
                PageSize = pageSize,
                LikedPostIds = likedPostIds,
                BookmarkedPostIds = bookmarkedPostIds,
                TrendingCategories = trendingCategories,
                WeeklyLeaderboard = weeklyLeaderboard,
                SearchQuery = search,
                SortBy = sort,
                UnansweredCount = unansweredCount,
                FeedbackCount = feedbackCount,
                SavedCount = savedCount,
                PopularLearners = popularLearners.Select(pl => new PopularLearnerInfo
                {
                    UserId = pl.UserId,
                    UserName = popularUsers.FirstOrDefault(u => u.Id == pl.UserId)?.UserName ?? "Learner",
                    AvatarJson = popularProfiles.FirstOrDefault(p => p.UserId == pl.UserId)?.AvatarJson,
                    AvatarImagePath = popularProfiles.FirstOrDefault(p => p.UserId == pl.UserId)?.AvatarImagePath,
                    AvatarMode = popularProfiles.FirstOrDefault(p => p.UserId == pl.UserId)?.AvatarMode ?? "generated",
                    PostCount = pl.PostCount
                }).ToList()
            };

            return View(nameof(Index), viewModel);
        }

        // GET: /Community/Leaderboard
        [AllowAnonymous]
        public async Task<IActionResult> Leaderboard()
        {
            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            var model = await _xpService.GetWeeklyLeaderboardAsync(userId, top: 50);
            return View(model);
        }

        // GET: /Community/Create
        public IActionResult Create()
        {
            return View(nameof(Create));
        }

        // POST: /Community/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            // ForYou (0) is a filter-only value, never a real category
            if (model.Category == CommunityCategory.ForYou)
            {
                ModelState.AddModelError(nameof(model.Category), "Please select a valid category.");
            }

            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            // Ensure user has a profile
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            var post = new CommunityPost
            {
                UserId = userId,
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                Category = model.Category,
                FeedbackWanted = model.FeedbackWanted,
                CreatedDate = DateTime.UtcNow
            };

            _context.CommunityPosts.Add(post);
            await _context.SaveChangesAsync();

            // Award +10 XP for creating a community discussion
            try
            {
                await _xpService.AwardCappedXpAsync(userId, 10, "community_post", 30, $"Created discussion: {post.Title}");
            }
            catch { /* XP logging is non-blocking */ }

            TempData["Success"] = "Post created successfully! (+10 XP earned)";
            return RedirectToAction(nameof(Details), new { id = post.Id });
        }

        // GET: /Community/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;

            var post = await _context.CommunityPosts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.User)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var isModerator = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            ViewBag.IsModerator = isModerator;
            ViewBag.PostTakenDown = post.IsTakenDown;

            // Taken-down posts are invisible to regular users and to non-owners.
            if (post.IsTakenDown && !isModerator && post.UserId != userId)
            {
                return View("Removed", "This post has been removed by a moderator.");
            }

            // Organize comments: top-level + their replies
            var topLevelComments = post.Comments
                .Where(c => c.ParentCommentId == null && (isModerator || !c.IsTakenDown))
                .OrderBy(c => c.CreatedDate)
                .ToList();

            // Hide taken-down replies from regular users.
            if (!isModerator)
            {
                foreach (var c in topLevelComments)
                    c.Replies = c.Replies.Where(r => !r.IsTakenDown).ToList();
            }

            var likedPostIds = new HashSet<int>();
            var isBookmarked = false;
            if (userId != null)
            {
                likedPostIds = (await _context.CommunityLikes
                    .Where(l => l.UserId == userId && l.PostId == id)
                    .Select(l => l.PostId)
                    .ToListAsync())
                    .ToHashSet();

                isBookmarked = await _context.CommunityBookmarks
                    .AnyAsync(b => b.UserId == userId && b.PostId == id);
            }

            // Load community avatar preferences for all users in this thread
            var allUserIds = new List<string> { post.UserId };
            allUserIds.AddRange(post.Comments.Select(c => c.UserId));
            allUserIds.AddRange(post.Comments.Where(c => c.Replies != null).SelectMany(c => c.Replies).Select(r => r.UserId));
            if (userId != null) allUserIds.Add(userId);
            var avatarMap = await _context.UserProfiles
                .Where(p => allUserIds.Distinct().Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => new CommunityAvatarInfo
                {
                    AvatarJson = p.AvatarJson,
                    AvatarImagePath = p.AvatarImagePath,
                    AvatarMode = p.AvatarMode
                });

            ViewBag.TopLevelComments = topLevelComments;
            ViewBag.IsLiked = likedPostIds.Contains(id);
            ViewBag.IsBookmarked = isBookmarked;
            ViewBag.CurrentUserId = userId;
            ViewBag.IsOwner = userId == post.UserId;
            ViewBag.AvatarMap = avatarMap;

            return View(nameof(Details), post);
        }

        // POST: /Community/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Comment must be between 1 and 2000 characters.";
                return RedirectToAction(nameof(Details), new { id = model.PostId });
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();
            if (!await _context.CommunityPosts.AnyAsync(p => p.Id == model.PostId)) return NotFound();

            // Ensure user has a profile
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            // If replying, make sure parent comment belongs to same post and is top-level
            if (model.ParentCommentId.HasValue)
            {
                var parent = await _context.CommunityComments.FindAsync(model.ParentCommentId.Value);
                if (parent == null || parent.PostId != model.PostId || parent.ParentCommentId != null)
                {
                    return BadRequest("Can only reply to top-level comments.");
                }
            }

            var comment = new CommunityComment
            {
                PostId = model.PostId,
                UserId = userId,
                Content = model.Content.Trim(),
                ParentCommentId = model.ParentCommentId,
                CreatedDate = DateTime.UtcNow
            };

            _context.CommunityComments.Add(comment);
            await _context.SaveChangesAsync();

            // Award +5 XP for contributing advice/comment
            try
            {
                await _xpService.AwardCappedXpAsync(userId, 5, "community_comment", 25, $"Contributed to discussion #{model.PostId}");
            }
            catch { /* XP logging is non-blocking */ }

            // Create notification for post owner or parent comment owner
            var post = await _context.CommunityPosts.FindAsync(model.PostId);
            if (post != null)
            {
                var currentUserName = User.Identity?.Name ?? "A learner";
                if (model.ParentCommentId.HasValue)
                {
                    var parentComment = await _context.CommunityComments.FindAsync(model.ParentCommentId.Value);
                    if (parentComment != null && parentComment.UserId != userId)
                    {
                        _context.UserNotifications.Add(new UserNotification
                        {
                            UserId = parentComment.UserId,
                            SenderUserId = userId,
                            Type = "reply",
                            Title = "New reply to your comment",
                            Message = $"@{currentUserName} replied: \"{(comment.Content.Length > 60 ? comment.Content.Substring(0, 57) + "..." : comment.Content)}\"",
                            TargetUrl = $"/Community/Details/{model.PostId}",
                            CreatedDate = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                else if (post.UserId != userId)
                {
                    _context.UserNotifications.Add(new UserNotification
                    {
                        UserId = post.UserId,
                        SenderUserId = userId,
                        Type = "comment",
                        Title = "New comment on your post",
                        Message = $"@{currentUserName} commented: \"{(comment.Content.Length > 60 ? comment.Content.Substring(0, 57) + "..." : comment.Content)}\"",
                        TargetUrl = $"/Community/Details/{model.PostId}",
                        CreatedDate = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "Comment posted! (+5 XP earned)";
            return RedirectToAction(nameof(Details), new { id = model.PostId });
        }

        // POST: /Community/ToggleLike (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false, error = "Please sign in to like posts." });
            
            var post = await _context.CommunityPosts.FindAsync(postId);
            if (post == null) return NotFound();

            var existing = await _context.CommunityLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            bool isLiked;
            if (existing != null)
            {
                _context.CommunityLikes.Remove(existing);
                isLiked = false;
            }
            else
            {
                _context.CommunityLikes.Add(new CommunityLike
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                });
                isLiked = true;

                // Send notification to post author if not liking own post
                if (post.UserId != userId)
                {
                    var currentUserName = User.Identity?.Name ?? "A learner";
                    _context.UserNotifications.Add(new UserNotification
                    {
                        UserId = post.UserId,
                        SenderUserId = userId,
                        Type = "like",
                        Title = "New Heart on your post",
                        Message = $"@{currentUserName} gave a heart to your post: \"{(post.Title.Length > 40 ? post.Title.Substring(0, 37) + "..." : post.Title)}\"",
                        TargetUrl = $"/Community/Details/{postId}",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.CommunityLikes.CountAsync(l => l.PostId == postId);

            return Json(new { success = true, isLiked, likeCount });
        }

        // POST: /Community/ToggleBookmark (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBookmark(int postId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false, error = "Please sign in to bookmark posts." });

            var post = await _context.CommunityPosts.FindAsync(postId);
            if (post == null) return NotFound();

            var existing = await _context.CommunityBookmarks
                .FirstOrDefaultAsync(b => b.PostId == postId && b.UserId == userId);

            bool isBookmarked;
            if (existing != null)
            {
                _context.CommunityBookmarks.Remove(existing);
                isBookmarked = false;
            }
            else
            {
                _context.CommunityBookmarks.Add(new CommunityBookmark
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                });
                isBookmarked = true;
            }

            await _context.SaveChangesAsync();

            var bookmarkCount = await _context.CommunityBookmarks.CountAsync(b => b.UserId == userId);

            return Json(new { success = true, isBookmarked, bookmarkCount });
        }

        // POST: /Community/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var post = await _context.CommunityPosts.FindAsync(id);
            if (post == null) return NotFound();

            if (post.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            _context.CommunityPosts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Post deleted.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Community/DeleteComment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var comment = await _context.CommunityComments.Include(c => c.Replies).FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            if (comment.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            int postId = comment.PostId;
            if (comment.Replies.Any())
            {
                _context.CommunityComments.RemoveRange(comment.Replies);
            }
            _context.CommunityComments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment deleted.";
            return RedirectToAction(nameof(Details), new { id = postId });
        }

        // GET: /Community/Report?targetType=Post&targetId=5
        // Any signed-in learner can report a post or comment for moderation review.
        public async Task<IActionResult> Report(ReportTargetType targetType, int targetId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            bool exists = targetType == ReportTargetType.Post
                ? await _context.CommunityPosts.AnyAsync(p => p.Id == targetId)
                : await _context.CommunityComments.AnyAsync(c => c.Id == targetId);
            if (!exists) return NotFound();

            var model = new ReportViewModel
            {
                TargetType = targetType,
                TargetId = targetId
            };
            return View(model);
        }

        // POST: /Community/Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(ReportViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            bool exists = model.TargetType == ReportTargetType.Post
                ? await _context.CommunityPosts.AnyAsync(p => p.Id == model.TargetId)
                : await _context.CommunityComments.AnyAsync(c => c.Id == model.TargetId);
            if (!exists) return NotFound();

            if (!ModelState.IsValid) return View(model);

            // Avoid spamming duplicates: one open report per user per target.
            var already = await _context.ContentReports.AnyAsync(r =>
                r.ReporterUserId == userId && r.TargetType == model.TargetType && r.TargetId == model.TargetId && r.Status == ReportStatus.Pending);
            if (already)
            {
                TempData["Info"] = "You have already reported this. A moderator will review it.";
            }
            else
            {
                _context.ContentReports.Add(new ContentReport
                {
                    ReporterUserId = userId,
                    TargetType = model.TargetType,
                    TargetId = model.TargetId,
                    Reason = model.Reason,
                    Detail = model.Detail,
                    Status = ReportStatus.Pending,
                    CreatedDate = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Report submitted. Thank you for helping keep the community safe!";
            }

            if (model.TargetType == ReportTargetType.Post)
                return RedirectToAction(nameof(Details), new { id = model.TargetId });
            var postId = await _context.CommunityComments.Where(c => c.Id == model.TargetId).Select(c => c.PostId).FirstOrDefaultAsync();
            return RedirectToAction(nameof(Details), new { id = postId });
        }

        // GET: /Community/UserProfile/guid
        [AllowAnonymous]
        public async Task<IActionResult> UserProfile(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == id);
            var posts = await _context.CommunityPosts
                .Where(p => p.UserId == id)
                .OrderByDescending(p => p.CreatedDate)
                .Take(20)
                .ToListAsync();

            var postIds = posts.Select(p => p.Id).ToList();

            var likeCounts = await _context.CommunityLikes
                .Where(l => postIds.Contains(l.PostId))
                .GroupBy(l => l.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PostId, x => x.Count);

            var commentCounts = await _context.CommunityComments
                .Where(c => postIds.Contains(c.PostId))
                .GroupBy(c => c.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PostId, x => x.Count);

            var allUserPostIds = await _context.CommunityPosts
                .Where(p => p.UserId == id)
                .Select(p => p.Id)
                .ToListAsync();

            var totalLikesReceived = await _context.CommunityLikes
                .CountAsync(l => allUserPostIds.Contains(l.PostId));

            var completedLessonsCount = await _context.LessonProgress
                .CountAsync(lp => lp.UserId == id && lp.IsCompleted);

            var badges = await _achievementService.GetUserBadgesAsync(id);
            var currentUserId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;

            // Weekly XP
            var weekStart = Services.XpService.GetCurrentWeekStartUtc();
            var weeklyXp = await _context.UserXpLogs
                .Where(x => x.UserId == id && x.EarnedAt >= weekStart)
                .SumAsync(x => x.Amount);

            // Study Buddy & compatibility match
            bool isStudyBuddy = false;
            bool isMutualBuddy = false;
            string? matchTag = null;
            if (!string.IsNullOrEmpty(currentUserId) && currentUserId != id)
            {
                isStudyBuddy = await _context.UserRelationships
                    .AnyAsync(r => r.UserId == currentUserId && r.TargetUserId == id);

                var targetFollowsBack = await _context.UserRelationships
                    .AnyAsync(r => r.UserId == id && r.TargetUserId == currentUserId);

                isMutualBuddy = isStudyBuddy && targetFollowsBack;

                var myProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == currentUserId);
                if (myProfile != null && profile != null)
                {
                    if (!string.IsNullOrWhiteSpace(myProfile.CurrentLevel) && myProfile.CurrentLevel == profile.CurrentLevel)
                    {
                        matchTag = $"✨ Both targeting {profile.CurrentLevel}";
                    }
                    else if (!string.IsNullOrWhiteSpace(myProfile.LearningGoal) && !string.IsNullOrWhiteSpace(profile.LearningGoal))
                    {
                        var myGoals = myProfile.LearningGoal.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var theirGoals = profile.LearningGoal.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var commonGoal = myGoals.Intersect(theirGoals, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
                        if (commonGoal != null)
                        {
                            matchTag = $"🎯 Common Goal: {commonGoal}";
                        }
                    }
                    else if (weeklyXp > 100)
                    {
                        matchTag = "🔥 Active study partner";
                    }
                }
            }

            var buddyCount = await _context.UserRelationships.CountAsync(r => r.TargetUserId == id);

            // Fetch study buddies of this profile
            var relationships = await _context.UserRelationships
                .Where(r => r.UserId == id)
                .OrderByDescending(r => r.CreatedDate)
                .Take(24)
                .ToListAsync();

            var buddyTargetIds = relationships.Select(r => r.TargetUserId).ToList();

            var buddyUsers = await _context.Users
                .Where(u => buddyTargetIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "learner");

            var buddyProfiles = await _context.UserProfiles
                .Where(p => buddyTargetIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => p);

            var mutualTargetIds = await _context.UserRelationships
                .Where(r => buddyTargetIds.Contains(r.UserId) && r.TargetUserId == id)
                .Select(r => r.UserId)
                .ToListAsync();

            var buddyList = new List<StudyBuddyItemViewModel>();
            foreach (var rel in relationships)
            {
                var tid = rel.TargetUserId;
                if (!buddyUsers.ContainsKey(tid)) continue;
                var bUser = buddyUsers[tid];
                var bProf = buddyProfiles.ContainsKey(tid) ? buddyProfiles[tid] : null;

                buddyList.Add(new StudyBuddyItemViewModel
                {
                    UserId = tid,
                    UserName = bUser,
                    DisplayName = bProf?.DisplayName ?? bUser,
                    CurrentLevel = bProf?.CurrentLevel ?? "N5",
                    TotalXp = bProf?.TotalXp ?? 0,
                    IsMutual = mutualTargetIds.Contains(tid),
                    CreatedDate = rel.CreatedDate,
                    AvatarInfo = new CommunityAvatarInfo
                    {
                        AvatarJson = bProf?.AvatarJson,
                        AvatarImagePath = bProf?.AvatarImagePath,
                        AvatarMode = bProf?.AvatarMode ?? "generated"
                    }
                });
            }

            var model = new CommunityUserProfileViewModel
            {
                UserId = id,
                UserName = user.UserName ?? "learner",
                DisplayName = profile?.DisplayName ?? user.UserName ?? "Learner",
                Bio = profile?.Bio,
                CurrentLevel = profile?.CurrentLevel,
                LearningGoal = profile?.LearningGoal,
                Country = profile?.Country,
                NativeLanguage = profile?.NativeLanguage,
                MemberSince = profile?.CreatedDate ?? DateTime.UtcNow,
                TotalXp = profile?.TotalXp ?? 0,
                WeeklyXp = weeklyXp,
                TotalLikesReceived = totalLikesReceived,
                CompletedLessonsCount = completedLessonsCount,
                IsCurrentUser = currentUserId == id,
                IsStudyBuddy = isStudyBuddy,
                IsMutualStudyBuddy = isMutualBuddy,
                StudyBuddyCount = buddyCount,
                CommonGoalMatch = matchTag,
                AvatarInfo = new CommunityAvatarInfo
                {
                    AvatarJson = profile?.AvatarJson,
                    AvatarImagePath = profile?.AvatarImagePath,
                    AvatarMode = profile?.AvatarMode ?? "generated"
                },
                Posts = posts,
                PostLikeCounts = likeCounts,
                PostCommentCounts = commentCounts,
                Badges = badges,
                Buddies = buddyList
            };

            return View(nameof(UserProfile), model);
        }

        // POST: /Community/ToggleStudyBuddy (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStudyBuddy(string targetUserId)
        {
            if (!User.Identity?.IsAuthenticated == true)
                return Json(new { success = false, message = "Please sign in to add study buddies." });

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId) || currentUserId == targetUserId)
                return Json(new { success = false, message = "Invalid request." });

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (targetUser == null)
                return Json(new { success = false, message = "User not found." });

            var currentUserName = User.Identity?.Name ?? "Learner";

            var existing = await _context.UserRelationships
                .FirstOrDefaultAsync(r => r.UserId == currentUserId && r.TargetUserId == targetUserId);

            bool isBuddy;
            if (existing != null)
            {
                _context.UserRelationships.Remove(existing);
                await _context.SaveChangesAsync();
                isBuddy = false;
            }
            else
            {
                _context.UserRelationships.Add(new UserRelationship
                {
                    UserId = currentUserId,
                    TargetUserId = targetUserId,
                    CreatedDate = DateTime.UtcNow
                });

                var followsBack = await _context.UserRelationships
                    .AnyAsync(r => r.UserId == targetUserId && r.TargetUserId == currentUserId);

                if (followsBack)
                {
                    _context.UserNotifications.Add(new UserNotification
                    {
                        UserId = targetUserId,
                        SenderUserId = currentUserId,
                        Type = "buddy",
                        Title = "🌸 Mutual Study Buddies!",
                        Message = $"You and @{currentUserName} are now Mutual Study Buddies! Ganbatte!",
                        TargetUrl = $"/Community/UserProfile/{currentUserId}",
                        CreatedDate = DateTime.UtcNow
                    });
                }
                else
                {
                    _context.UserNotifications.Add(new UserNotification
                    {
                        UserId = targetUserId,
                        SenderUserId = currentUserId,
                        Type = "buddy",
                        Title = "🌸 New Study Buddy",
                        Message = $"@{currentUserName} added you as a Study Buddy!",
                        TargetUrl = $"/Community/UserProfile/{currentUserId}",
                        CreatedDate = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                isBuddy = true;
            }

            var mutual = isBuddy && await _context.UserRelationships
                .AnyAsync(r => r.UserId == targetUserId && r.TargetUserId == currentUserId);

            var count = await _context.UserRelationships.CountAsync(r => r.TargetUserId == targetUserId);

            return Json(new
            {
                success = true,
                isBuddy = isBuddy,
                isMutual = mutual,
                buddyCount = count,
                message = isBuddy ? (mutual ? "Mutual Study Buddies! 🌸" : "Added as Study Buddy! 🌸") : "Removed from Study Buddies"
            });
        }

        // POST: /Community/SendCheer (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCheer(string targetUserId)
        {
            if (!User.Identity?.IsAuthenticated == true)
                return Json(new { success = false, message = "Please sign in to send cheers." });

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId) || currentUserId == targetUserId)
                return Json(new { success = false, message = "Invalid request." });

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (targetUser == null)
                return Json(new { success = false, message = "User not found." });

            // Daily cheer anti-abuse check (1 cheer per buddy per day)
            var todayUtc = DateTime.UtcNow.Date;
            var alreadyCheeredToday = await _context.UserNotifications
                .AnyAsync(n => n.UserId == targetUserId
                            && n.SenderUserId == currentUserId
                            && n.Type == "cheer"
                            && n.CreatedDate >= todayUtc);

            if (alreadyCheeredToday)
            {
                return Json(new
                {
                    success = false,
                    message = "You have already cheered this buddy today! Come back tomorrow 🌸"
                });
            }

            var currentUserName = User.Identity?.Name ?? "Learner";

            // Cheers are peer-to-peer and repeatable, so both sides are capped per day.
            try
            {
                await _xpService.AwardCappedXpAsync(targetUserId, 5, "Study Buddy Cheer", 20, $"Received a cheer from @{currentUserName}");
                await _xpService.AwardCappedXpAsync(currentUserId, 2, "Sent Study Cheer", 20, $"Cheered on @{targetUser.UserName}");
            }
            catch { /* XP logging is non-blocking */ }

            _context.UserNotifications.Add(new UserNotification
            {
                UserId = targetUserId,
                SenderUserId = currentUserId,
                Type = "cheer",
                Title = "🌸 Ganbatte! Cheer Received",
                Message = $"@{currentUserName} sent you a sakura cheer: がんばって！ (+5 XP)",
                TargetUrl = $"/Community/UserProfile/{currentUserId}",
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Sent sakura cheer! がんばって！ (+2 XP for you, +5 XP for them 🌸)"
            });
        }

        // POST: /Community/GetConversation (AJAX). Opening a thread marks messages read,
        // so it is a POST: a GET must stay free of side effects (prefetch, refresh, crawlers).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetConversation(string targetUserId)
        {
            if (!User.Identity?.IsAuthenticated == true)
                return Json(new { success = false, message = "Please sign in." });

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(targetUserId))
                return Json(new { success = false });

            // Latest 50 messages, then back to chronological order for the chat thread.
            var messages = await _context.DirectMessages
                .AsNoTracking()
                .Where(m => (m.SenderUserId == currentUserId && m.ReceiverUserId == targetUserId)
                         || (m.SenderUserId == targetUserId && m.ReceiverUserId == currentUserId))
                .OrderByDescending(m => m.SentAt)
                .Take(50)
                .Select(m => new
                {
                    id = m.Id,
                    isMine = m.SenderUserId == currentUserId,
                    content = m.Content,
                    sentAtFormatted = m.SentAt.ToLocalTime().ToString("MMM d, HH:mm"),
                    isRead = m.IsRead
                })
                .ToListAsync();
            messages.Reverse();

            await _context.DirectMessages
                .Where(m => m.SenderUserId == targetUserId && m.ReceiverUserId == currentUserId && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, m => true));

            return Json(new { success = true, messages = messages });
        }

        // POST: /Community/SendMessage (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverUserId, string content)
        {
            if (!User.Identity?.IsAuthenticated == true)
                return Json(new { success = false, message = "Please sign in to send messages." });

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrWhiteSpace(content))
                return Json(new { success = false, message = "Message cannot be empty." });

            content = content.Trim();
            if (content.Length > 1000)
                content = content.Substring(0, 1000);

            var dm = new DirectMessage
            {
                SenderUserId = currentUserId,
                ReceiverUserId = receiverUserId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.DirectMessages.Add(dm);

            var currentUserName = User.Identity?.Name ?? "Learner";
            _context.UserNotifications.Add(new UserNotification
            {
                UserId = receiverUserId,
                SenderUserId = currentUserId,
                Type = "message",
                Title = $"New message from @{currentUserName}",
                Message = content.Length > 60 ? content.Substring(0, 57) + "..." : content,
                TargetUrl = $"/Community/UserProfile/{currentUserId}?openChat=true",
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = new
                {
                    id = dm.Id,
                    isMine = true,
                    content = dm.Content,
                    sentAtFormatted = dm.SentAt.ToLocalTime().ToString("MMM d, HH:mm")
                }
            });
        }
    }

    // ViewModels
    public class CommunityUserProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? CurrentLevel { get; set; }
        public string? LearningGoal { get; set; }
        public string? Country { get; set; }
        public string? NativeLanguage { get; set; }
        public DateTime MemberSince { get; set; }
        public int TotalXp { get; set; }
        public int WeeklyXp { get; set; }
        public int TotalLikesReceived { get; set; }
        public int CompletedLessonsCount { get; set; }
        public bool IsCurrentUser { get; set; }
        public bool IsStudyBuddy { get; set; }
        public bool IsMutualStudyBuddy { get; set; }
        public int StudyBuddyCount { get; set; }
        public string? CommonGoalMatch { get; set; }
        public CommunityAvatarInfo AvatarInfo { get; set; } = new();
        public List<CommunityPost> Posts { get; set; } = new();
        public Dictionary<int, int> PostLikeCounts { get; set; } = new();
        public Dictionary<int, int> PostCommentCounts { get; set; } = new();
        public List<Services.AchievementBadge> Badges { get; set; } = new();
        public List<StudyBuddyItemViewModel> Buddies { get; set; } = new();
    }

    public class StudyBuddyItemViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string CurrentLevel { get; set; } = "N5";
        public int TotalXp { get; set; }
        public bool IsMutual { get; set; }
        public DateTime CreatedDate { get; set; }
        public CommunityAvatarInfo AvatarInfo { get; set; } = new();
    }

    public class CommunityIndexViewModel
    {
        public List<CommunityPost> Posts { get; set; } = new();
        public Dictionary<int, int> PostLikeCounts { get; set; } = new();
        public Dictionary<int, int> PostCommentCounts { get; set; } = new();
        public Dictionary<string, CommunityAvatarInfo> AuthorAvatars { get; set; } = new();
        public CommunityCategory CurrentCategory { get; set; }
        public int Page { get; set; }
        public int TotalPosts { get; set; }
        public int PageSize { get; set; }
        public HashSet<int> LikedPostIds { get; set; } = new();
        public HashSet<int> BookmarkedPostIds { get; set; } = new();
        public List<CommunityCategory> TrendingCategories { get; set; } = new();
        public List<PopularLearnerInfo> PopularLearners { get; set; } = new();
        public Services.LeaderboardPageViewModel? WeeklyLeaderboard { get; set; }
        public string? SearchQuery { get; set; }
        public string SortBy { get; set; } = "latest";
        public int UnansweredCount { get; set; }
        public int FeedbackCount { get; set; }
        public int SavedCount { get; set; }
    }

    public class PopularLearnerInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? AvatarJson { get; set; }
        public string? AvatarImagePath { get; set; }
        public string AvatarMode { get; set; } = "generated";
        public int PostCount { get; set; }
    }

    public class CommunityAvatarInfo
    {
        public string? AvatarJson { get; set; }
        public string? AvatarImagePath { get; set; }
        public string AvatarMode { get; set; } = "generated";
        public bool UseUploadedImage => AvatarMode == "uploaded" && !string.IsNullOrWhiteSpace(AvatarImagePath);
    }

    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 5000 characters.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        public CommunityCategory Category { get; set; } = CommunityCategory.General;

        public bool FeedbackWanted { get; set; }
    }

    public class CommentViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Invalid post ID.")]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Comment cannot be empty.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 2000 characters.")]
        public string Content { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; }
    }

    public class ReportViewModel
    {
        public ReportTargetType TargetType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid target.")]
        public int TargetId { get; set; }

        [Required(ErrorMessage = "Please choose a reason.")]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Details cannot exceed 1000 characters.")]
        public string? Detail { get; set; }
    }
}
