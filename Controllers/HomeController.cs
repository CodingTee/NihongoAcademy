using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string[] CorePathCategories = { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course" };
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAppClock _clock;
        private readonly PublicCatalogCache _catalog;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, IAppClock clock, PublicCatalogCache catalog)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _clock = clock;
            _catalog = catalog;
        }

        public async Task<IActionResult> Index()
        {
            var counts = await _catalog.GetCountsAsync();
            var viewModel = new HomeViewModel
            {
                Categories = await _catalog.GetCategoriesAsync(),
                RecommendedLessons = await _catalog.GetRecommendedLessonsAsync(),
                FeaturedQuizzes = await _catalog.GetFeaturedQuizzesAsync(),
                TotalLessons = counts.Lessons,
                TotalVocabulary = counts.Vocabulary,
                TotalQuizzes = counts.Quizzes,
            };

            // Logged-in user data
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    var profile = await _context.UserProfiles.AsNoTracking()
                        .SingleOrDefaultAsync(p => p.UserId == userId);
                    if (profile != null && (!string.IsNullOrWhiteSpace(profile.CurrentLevel) || !string.IsNullOrWhiteSpace(profile.LearningGoal)))
                    {
                        var allPublishedLessons = await _context.Lessons.AsNoTracking()
                            .Where(l => l.IsPublished)
                            .Include(l => l.Category)
                            .ToListAsync();
                        viewModel.RecommendedLessons = PersonalizeRecommendations(allPublishedLessons, profile);
                        viewModel.RecommendationReason = BuildRecommendationReason(profile);
                    }

                    // Continue Learning: in-progress lessons (not completed, most recently started)
                    viewModel.ContinueLearning = await _context.LessonProgress.AsNoTracking()
                        .Where(p => p.UserId == userId && !p.IsCompleted && p.Lesson!.IsPublished &&
                            CorePathCategories.Contains(p.Lesson.Category!.Name) &&
                            !_context.LessonPrerequisites.Any(required => required.LessonId == p.LessonId &&
                                !_context.LessonProgress.Any(done => done.UserId == userId &&
                                    done.LessonId == required.PrerequisiteLessonId && done.FirstMasteryPassedDate.HasValue)))
                        .Include(p => p.Lesson!)
                            .ThenInclude(l => l.Category)
                        .OrderByDescending(p => p.StartedDate)
                        .Take(3)
                        .ToListAsync();

                    // If no in-progress lessons, suggest next un-started lessons
                    if (!viewModel.ContinueLearning.Any())
                    {
                        var startedLessonIds = await _context.LessonProgress.AsNoTracking()
                            .Where(p => p.UserId == userId)
                            .Select(p => p.LessonId)
                            .ToListAsync();

                        viewModel.NextLessons = await _context.Lessons.AsNoTracking()
                            .Where(l => l.IsPublished && !startedLessonIds.Contains(l.Id) &&
                                CorePathCategories.Contains(l.Category!.Name) &&
                                !_context.LessonPrerequisites.Any(required => required.LessonId == l.Id &&
                                    !_context.LessonProgress.Any(done => done.UserId == userId &&
                                        done.LessonId == required.PrerequisiteLessonId && done.FirstMasteryPassedDate.HasValue)))
                            .Include(l => l.Category)
                            .OrderBy(l => l.DisplayOrder)
                            .Take(3)
                            .ToListAsync();
                    }

                    // Quick Progress
                    var completedLessons = await _context.LessonProgress.AsNoTracking()
                        .CountAsync(p => p.UserId == userId && p.IsCompleted);

                    viewModel.LessonProgressPercent = counts.Lessons > 0
                        ? (int)Math.Round((double)completedLessons / counts.Lessons * 100)
                        : 0;

                    viewModel.WordsLearned = await _context.FlashcardProgress.AsNoTracking()
                        .CountAsync(p => p.UserId == userId && p.State >= 1);

                    // Daily Progress: words reviewed today
                    var todayStart = _clock.Today;
                    viewModel.DailyProgress = await _context.FlashcardProgress.AsNoTracking()
                        .CountAsync(p => p.UserId == userId && p.LastReview >= todayStart);

                    // Streak: count consecutive days with activity
                    var activityDays = await _context.LessonProgress.AsNoTracking()
                        .Where(p => p.UserId == userId && p.StartedDate.HasValue)
                        .Select(p => p.StartedDate!.Value.Date)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .ToListAsync();

                    var flashcardDays = await _context.FlashcardProgress.AsNoTracking()
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

            var today = _clock.Today;
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

        private static List<Lesson> PersonalizeRecommendations(IEnumerable<Lesson> lessons, UserProfile profile)
        {
            var preferredCategory = profile.CurrentLevel switch
            {
                "Absolute beginner" => "Hiragana",
                "Knows hiragana" => "Katakana",
                "Conversational basics" => "A1 Course",
                "Intermediate or above" => "A2 Course",
                _ => "Hiragana"
            };
            var goalTerms = profile.LearningGoal switch
            {
                "Travel & culture" => new[] { "travel", "direction", "food", "order", "shop", "price", "greeting" },
                "Work & career" => new[] { "introduce", "schedule", "request", "permission", "report", "polite" },
                "Study & exams" => new[] { "kanji", "grammar", "number", "time", "reading", "foundation" },
                "Anime & entertainment" => new[] { "friend", "casual", "likes", "invite", "experience", "describe" },
                _ => Array.Empty<string>()
            };

            return lessons
                .Select(lesson => new
                {
                    Lesson = lesson,
                    Score = (lesson.Category?.Name == preferredCategory ? 100 : 0)
                        + goalTerms.Count(term => ($"{lesson.Title} {lesson.Description}").Contains(term, StringComparison.OrdinalIgnoreCase)) * 15
                        - lesson.DisplayOrder
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Lesson.Category!.DisplayOrder)
                .ThenBy(x => x.Lesson.DisplayOrder)
                .Select(x => x.Lesson)
                .Take(6)
                .ToList();
        }

        private static string BuildRecommendationReason(UserProfile profile)
        {
            var level = string.IsNullOrWhiteSpace(profile.CurrentLevel) ? "your current level" : profile.CurrentLevel.ToLowerInvariant();
            var goal = string.IsNullOrWhiteSpace(profile.LearningGoal) ? "your learning goal" : profile.LearningGoal.ToLowerInvariant();
            return $"Recommended for {level} learners focused on {goal}.";
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Info(string section = "about")
            => section.Equals("privacy", StringComparison.OrdinalIgnoreCase)
                ? View(nameof(Privacy))
                : View(nameof(About));

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProgressData()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false });

            // Continue Learning: in-progress lessons
            var continueLearning = await _context.LessonProgress.AsNoTracking()
                .Where(p => p.UserId == userId && !p.IsCompleted && p.Lesson!.IsPublished &&
                    CorePathCategories.Contains(p.Lesson.Category!.Name) &&
                    !_context.LessonPrerequisites.Any(required => required.LessonId == p.LessonId &&
                        !_context.LessonProgress.Any(done => done.UserId == userId &&
                            done.LessonId == required.PrerequisiteLessonId && done.FirstMasteryPassedDate.HasValue)))
                .Include(p => p.Lesson!)
                    .ThenInclude(l => l.Category)
                .OrderByDescending(p => p.StartedDate)
                .Take(3)
                .ToListAsync();

            // If all lessons completed or none started, suggest next
            var nextLessons = new List<Lesson>();
            if (!continueLearning.Any())
            {
                var startedLessonIds = await _context.LessonProgress.AsNoTracking()
                    .Where(p => p.UserId == userId)
                    .Select(p => p.LessonId)
                    .ToListAsync();

                nextLessons = await _context.Lessons.AsNoTracking()
                    .Where(l => l.IsPublished && !startedLessonIds.Contains(l.Id) &&
                        CorePathCategories.Contains(l.Category!.Name) &&
                        !_context.LessonPrerequisites.Any(required => required.LessonId == l.Id &&
                            !_context.LessonProgress.Any(done => done.UserId == userId &&
                                done.LessonId == required.PrerequisiteLessonId && done.FirstMasteryPassedDate.HasValue)))
                    .Include(l => l.Category)
                    .OrderBy(l => l.DisplayOrder)
                    .Take(3)
                    .ToListAsync();
            }

            // Quick Progress
            var completedLessons = await _context.LessonProgress.AsNoTracking()
                .CountAsync(p => p.UserId == userId && p.IsCompleted);
            var totalLessons = await _context.Lessons.CountAsync(l => l.IsPublished);
            var lessonProgressPercent = totalLessons > 0
                ? (int)Math.Round((double)completedLessons / totalLessons * 100) : 0;

            var wordsLearned = await _context.FlashcardProgress.AsNoTracking()
                .CountAsync(p => p.UserId == userId && p.State >= 1);

            var todayStart = _clock.Today;
            var dailyProgress = await _context.FlashcardProgress.AsNoTracking()
                .CountAsync(p => p.UserId == userId && p.LastReview >= todayStart);

            // Streak
            var activityDays = await _context.LessonProgress.AsNoTracking()
                .Where(p => p.UserId == userId && p.StartedDate.HasValue)
                .Select(p => p.StartedDate!.Value.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();
            var flashcardDays = await _context.FlashcardProgress.AsNoTracking()
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StatusPage(int code)
        {
            var supportedCode = code is 400 or 401 or 403 or 404 or 408 or 429 ? code : 404;
            Response.StatusCode = supportedCode;
            return View(supportedCode);
        }
    }

    public class HomeViewModel
    {
        // Categories (used for Explore Japan culture link)
        public List<Category> Categories { get; set; } = new();
        public List<Lesson> RecommendedLessons { get; set; } = new();
        public List<Quiz> FeaturedQuizzes { get; set; } = new();
        public string? RecommendationReason { get; set; }
        public int TotalLessons { get; set; }
        public int TotalVocabulary { get; set; }
        public int TotalQuizzes { get; set; }

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
