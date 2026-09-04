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

        public CommunityController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Community?category=StudyTips
        [AllowAnonymous]
        public async Task<IActionResult> Index(CommunityCategory? category, int page = 1)
        {
            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            int pageSize = 15;

            var query = _context.CommunityPosts
                .Include(p => p.User)
                .AsQueryable();

            // Category filter (ForYou = show all)
            if (category.HasValue && category.Value != CommunityCategory.ForYou)
            {
                query = query.Where(p => p.Category == category.Value);
            }

            var totalPosts = await query.CountAsync();

            // Get paginated posts first, then load counts in a single round-trip
            var posts = await query
                .OrderByDescending(p => p.CreatedDate)
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

            // Load avatar JSON for post authors
            var authorIds = posts.Select(p => p.UserId).Distinct().ToList();
            var avatarMap = await _context.UserProfiles
                .Where(p => authorIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => p.AvatarJson);

            // Which posts the current user has liked
            var likedPostIds = new HashSet<int>();
            if (userId != null)
            {
                likedPostIds = (await _context.CommunityLikes
                    .Where(l => l.UserId == userId && postIds.Contains(l.PostId))
                    .Select(l => l.PostId)
                    .ToListAsync())
                    .ToHashSet();
            }

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
                TrendingCategories = trendingCategories,
                PopularLearners = popularLearners.Select(pl => new PopularLearnerInfo
                {
                    UserId = pl.UserId,
                    UserName = popularUsers.FirstOrDefault(u => u.Id == pl.UserId)?.UserName ?? "Learner",
                    AvatarJson = popularProfiles.FirstOrDefault(p => p.UserId == pl.UserId)?.AvatarJson,
                    PostCount = pl.PostCount
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: /Community/Create
        public IActionResult Create()
        {
            return View();
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

            TempData["Success"] = "Post created successfully!";
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

            // Organize comments: top-level + their replies
            var topLevelComments = post.Comments
                .Where(c => c.ParentCommentId == null)
                .OrderBy(c => c.CreatedDate)
                .ToList();

            var likedPostIds = new HashSet<int>();
            if (userId != null)
            {
                likedPostIds = (await _context.CommunityLikes
                    .Where(l => l.UserId == userId && l.PostId == id)
                    .Select(l => l.PostId)
                    .ToListAsync())
                    .ToHashSet();
            }

            // Load avatar JSONs for all users in this thread
            var allUserIds = new List<string> { post.UserId };
            allUserIds.AddRange(post.Comments.Select(c => c.UserId));
            allUserIds.AddRange(post.Comments.Where(c => c.Replies != null).SelectMany(c => c.Replies).Select(r => r.UserId));
            if (userId != null) allUserIds.Add(userId);
            var avatarMap = await _context.UserProfiles
                .Where(p => allUserIds.Distinct().Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => p.AvatarJson);

            ViewBag.TopLevelComments = topLevelComments;
            ViewBag.IsLiked = likedPostIds.Contains(id);
            ViewBag.CurrentUserId = userId;
            ViewBag.IsOwner = userId == post.UserId;
            ViewBag.AvatarMap = avatarMap;

            return View(post);
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

            return RedirectToAction(nameof(Details), new { id = model.PostId });
        }

        // POST: /Community/ToggleLike (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false, error = "Not authenticated" });

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
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.CommunityLikes.CountAsync(l => l.PostId == postId);

            return Json(new { success = true, isLiked, likeCount });
        }

        // POST: /Community/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var post = await _context.CommunityPosts.FindAsync(id);

            if (post == null) return NotFound();
            if (post.UserId != userId) return Forbid();

            _context.CommunityPosts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Post deleted.";
            return RedirectToAction(nameof(Index));
        }
    }

    // View Models
    public class CommunityIndexViewModel
    {
        public List<CommunityPost> Posts { get; set; } = new();
        public Dictionary<int, int> PostLikeCounts { get; set; } = new();
        public Dictionary<int, int> PostCommentCounts { get; set; } = new();
        public Dictionary<string, string?> AuthorAvatars { get; set; } = new();
        public CommunityCategory CurrentCategory { get; set; }
        public int Page { get; set; }
        public int TotalPosts { get; set; }
        public int PageSize { get; set; }
        public HashSet<int> LikedPostIds { get; set; } = new();
        public List<CommunityCategory> TrendingCategories { get; set; } = new();
        public List<PopularLearnerInfo> PopularLearners { get; set; } = new();
    }

    public class PopularLearnerInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? AvatarJson { get; set; }
        public int PostCount { get; set; }
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
}
