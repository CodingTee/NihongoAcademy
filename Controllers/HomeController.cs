using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string[] CorePathCategories = { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course" };
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var recommendedLessons = await _context.Lessons
                .Where(l => l.IsPublished)
                .Include(l => l.Category)
                .OrderBy(l => l.DisplayOrder)
                .Take(3)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                RecommendedLessons = recommendedLessons,
            };

            // Logged-in user data
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    // Continue Learning: in-progress lessons (not completed, most recently started)
                    viewModel.ContinueLearning = await _context.LessonProgress
                        .Where(p => p.UserId == userId && !p.IsCompleted && p.Lesson!.IsPublished &&
                            CorePathCategories.Contains(p.Lesson.Category!.Name) &&
                            !_context.LessonPrerequisites.Any(required => required.LessonId == p.LessonId &&
                                !_context.LessonProgress.Any(done => done.UserId == userId &&
                                    done.LessonId == required.PrerequisiteLessonId && done.StableMasteryDate != null)))
                        .Include(p => p.Lesson!)
                            .ThenInclude(l => l.Category)
                        .OrderByDescending(p => p.StartedDate)
                        .Take(3)
                        .ToListAsync();

                    // If no in-progress lessons, suggest next un-started lessons
                    if (!viewModel.ContinueLearning.Any())
                    {
                        var startedLessonIds = await _context.LessonProgress
                            .Where(p => p.UserId == userId)
                            .Select(p => p.LessonId)
                            .ToListAsync();

                        viewModel.NextLessons = await _context.Lessons
                            .Where(l => l.IsPublished && !startedLessonIds.Contains(l.Id) &&
                                CorePathCategories.Contains(l.Category!.Name) &&
                                !_context.LessonPrerequisites.Any(required => required.LessonId == l.Id &&
                                    !_context.LessonProgress.Any(done => done.UserId == userId &&
                                        done.LessonId == required.PrerequisiteLessonId && done.StableMasteryDate != null)))
                            .Include(l => l.Category)
                            .OrderBy(l => l.DisplayOrder)
                            .Take(3)
                            .ToListAsync();
                    }

                    // Quick Progress
                    var completedLessons = await _context.LessonProgress
                        .CountAsync(p => p.UserId == userId && p.IsCompleted);

                    var totalLessons = await _context.Lessons.CountAsync(l => l.IsPublished);
                    viewModel.LessonProgressPercent = totalLessons > 0
                        ? (int)Math.Round((double)completedLessons / totalLessons * 100)
                        : 0;

                    viewModel.WordsLearned = await _context.FlashcardProgress
                        .CountAsync(p => p.UserId == userId && p.State >= 1);

                    // Daily Progress: words reviewed today
                    var todayStart = DateTime.UtcNow.Date;
                    viewModel.DailyProgress = await _context.FlashcardProgress
                        .CountAsync(p => p.UserId == userId && p.LastReview >= todayStart);

                    // Streak: count consecutive days with activity
                    var activityDays = await _context.LessonProgress
                        .Where(p => p.UserId == userId)
                        .Select(p => p.StartedDate!.Value.Date)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .ToListAsync();

                    var flashcardDays = await _context.FlashcardProgress
                        .Where(p => p.UserId == userId && p.LastReview != null)
                        .Select(p => p.LastReview!.Value.Date)
                        .Distinct()
                        .ToListAsync();

                    var allActiveDays = activityDays.Union(flashcardDays).OrderByDescending(d => d).ToList();
                    viewModel.UserStreak = CalculateStreak(allActiveDays);

                    viewModel.CompletedLessons = completedLessons;
                }
            }

            return View(viewModel);
        }

        private int CalculateStreak(List<DateTime> activeDays)
        {
            if (activeDays.Count == 0) return 0;

            var today = DateTime.UtcNow.Date;
            var streak = 0;
            var checkDate = today;

            // If no activity today, start checking from yesterday
            if (!activeDays.Contains(today))
            {
                checkDate = today.AddDays(-1);
            }

            while (activeDays.Contains(checkDate))
            {
                streak++;
                checkDate = checkDate.AddDays(-1);
            }

            return streak;
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProgressData()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false });

            // Continue Learning: in-progress lessons
            var continueLearning = await _context.LessonProgress
                .Where(p => p.UserId == userId && !p.IsCompleted && p.Lesson!.IsPublished &&
                    CorePathCategories.Contains(p.Lesson.Category!.Name) &&
                    !_context.LessonPrerequisites.Any(required => required.LessonId == p.LessonId &&
                        !_context.LessonProgress.Any(done => done.UserId == userId &&
                            done.LessonId == required.PrerequisiteLessonId && done.StableMasteryDate != null)))
                .Include(p => p.Lesson!)
                    .ThenInclude(l => l.Category)
                .OrderByDescending(p => p.StartedDate)
                .Take(3)
                .ToListAsync();

            // If all lessons completed or none started, suggest next
            var nextLessons = new List<Lesson>();
            if (!continueLearning.Any())
            {
                var startedLessonIds = await _context.LessonProgress
                    .Where(p => p.UserId == userId)
                    .Select(p => p.LessonId)
                    .ToListAsync();

                nextLessons = await _context.Lessons
                    .Where(l => l.IsPublished && !startedLessonIds.Contains(l.Id) &&
                        CorePathCategories.Contains(l.Category!.Name) &&
                        !_context.LessonPrerequisites.Any(required => required.LessonId == l.Id &&
                            !_context.LessonProgress.Any(done => done.UserId == userId &&
                                done.LessonId == required.PrerequisiteLessonId && done.StableMasteryDate != null)))
                    .Include(l => l.Category)
                    .OrderBy(l => l.DisplayOrder)
                    .Take(3)
                    .ToListAsync();
            }

            // Quick Progress
            var completedLessons = await _context.LessonProgress
                .CountAsync(p => p.UserId == userId && p.IsCompleted);
            var totalLessons = await _context.Lessons.CountAsync(l => l.IsPublished);
            var lessonProgressPercent = totalLessons > 0
                ? (int)Math.Round((double)completedLessons / totalLessons * 100) : 0;

            var wordsLearned = await _context.FlashcardProgress
                .CountAsync(p => p.UserId == userId && p.State >= 1);

            var todayStart = DateTime.UtcNow.Date;
            var dailyProgress = await _context.FlashcardProgress
                .CountAsync(p => p.UserId == userId && p.LastReview >= todayStart);

            // Streak
            var activityDays = await _context.LessonProgress
                .Where(p => p.UserId == userId)
                .Select(p => p.StartedDate!.Value.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();
            var flashcardDays = await _context.FlashcardProgress
                .Where(p => p.UserId == userId && p.LastReview != null)
                .Select(p => p.LastReview!.Value.Date)
                .Distinct()
                .ToListAsync();
            var allActiveDays = activityDays.Union(flashcardDays).OrderByDescending(d => d).ToList();
            var streak = CalculateStreak(allActiveDays);

            return Json(new
            {
                success = true,
                streak,
                wordsLearned,
                dailyProgress,
                dailyGoal = 10,
                lessonProgressPercent,
                completedLessons,
                continueLearning = continueLearning.Select(p => new
                {
                    lessonId = p.LessonId,
                    title = p.Lesson?.Title ?? "",
                    japaneseTitle = p.Lesson?.JapaneseTitle ?? "",
                    category = p.Lesson?.Category?.Name ?? "",
                    completionPercentage = p.CompletionPercentage,
                    estimatedMinutes = p.Lesson?.EstimatedMinutes ?? 0
                }),
                nextLessons = nextLessons.Select(l => new
                {
                    lessonId = l.Id,
                    title = l.Title,
                    japaneseTitle = l.JapaneseTitle ?? "",
                    category = l.Category?.Name ?? "",
                    estimatedMinutes = l.EstimatedMinutes,
                    difficultyLevel = l.DifficultyLevel
                })
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class HomeViewModel
    {
        // Categories (used for Explore Japan culture link)
        public List<Category> Categories { get; set; } = new();
        public List<Lesson> RecommendedLessons { get; set; } = new();

        // Continue Learning (for logged-in users)
        public List<LessonProgress> ContinueLearning { get; set; } = new();

        // When all in-progress lessons are completed, suggest next un-started lesson
        public List<Lesson> NextLessons { get; set; } = new();

        // Quick Progress
        public int UserStreak { get; set; }
        public int WordsLearned { get; set; }
        public int DailyGoal { get; set; } = 10; // default: 10 words/day
        public int DailyProgress { get; set; }
        public int LessonProgressPercent { get; set; }
        public int CompletedLessons { get; set; }
    }
}
