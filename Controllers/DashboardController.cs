using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;

namespace NihongoAcademy.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var lessonProgress = await _context.LessonProgress
                .Where(p => p.UserId == userId)
                .Include(p => p.Lesson)
                .ThenInclude(l => l!.Category)
                .OrderByDescending(p => p.StartedDate)
                .ToListAsync();

            var quizAttempts = await _context.QuizAttempts
                .Where(a => a.UserId == userId)
                .Include(a => a.Quiz)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();

            var totalLessons = await _context.Lessons.CountAsync(l => l.IsPublished);
            var completedLessons = lessonProgress.Count(p => p.IsCompleted);
            var totalQuizAttempts = quizAttempts.Count;
            var passedQuizzes = quizAttempts.Count(a => a.IsPassed);
            var avgScore = quizAttempts.Count > 0
                ? (int)quizAttempts.Average(a => a.ScorePercentage)
                : 0;

            var categoryProgress = await _context.Categories
                .Select(c => new CategoryProgressItem
                {
                    CategoryName = c.Name,
                    TotalLessons = c.Lessons.Count(l => l.IsPublished),
                    CompletedLessons = c.Lessons.Count(l => l.IsPublished && l.ProgressRecords.Any(p => p.UserId == userId && p.IsCompleted))
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            // Calculate streak (same logic as HomeController)
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
            var currentStreak = CalculateStreak(allActiveDays);
            var longestStreak = CalculateLongestStreak(allActiveDays);

            var viewModel = new DashboardViewModel
            {
                LessonProgress = lessonProgress,
                QuizAttempts = quizAttempts.Take(10).ToList(),
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                TotalQuizAttempts = totalQuizAttempts,
                PassedQuizzes = passedQuizzes,
                AverageScore = avgScore,
                CategoryProgress = categoryProgress,
                RecentQuizScores = quizAttempts.Take(10).Reverse().Select(a => a.ScorePercentage).ToList(),
                RecentQuizLabels = quizAttempts.Take(10).Reverse().Select(a => a.AttemptDate.ToString("MM/dd")).ToList(),
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak
            };

            return View(viewModel);
        }

        private int CalculateStreak(List<DateTime> activeDays)
        {
            if (activeDays.Count == 0) return 0;
            var today = DateTime.UtcNow.Date;
            var streak = 0;
            var checkDate = today;
            if (!activeDays.Contains(checkDate)) checkDate = checkDate.AddDays(-1);
            while (activeDays.Contains(checkDate))
            {
                streak++;
                checkDate = checkDate.AddDays(-1);
            }
            return streak;
        }

        private int CalculateLongestStreak(List<DateTime> activeDays)
        {
            if (activeDays.Count == 0) return 0;
            var sorted = activeDays.Distinct().OrderBy(d => d).ToList();
            var longest = 1;
            var current = 1;
            for (int i = 1; i < sorted.Count; i++)
            {
                if ((sorted[i] - sorted[i - 1]).Days == 1)
                {
                    current++;
                    if (current > longest) longest = current;
                }
                else
                {
                    current = 1;
                }
            }
            return longest;
        }
    }

    public class DashboardViewModel
    {
        public List<Models.LessonProgress> LessonProgress { get; set; } = new();
        public List<Models.QuizAttempt> QuizAttempts { get; set; } = new();
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalQuizAttempts { get; set; }
        public int PassedQuizzes { get; set; }
        public int AverageScore { get; set; }
        public List<CategoryProgressItem> CategoryProgress { get; set; } = new();
        public List<int> RecentQuizScores { get; set; } = new();
        public List<string> RecentQuizLabels { get; set; } = new();
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
    }

    public class CategoryProgressItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
    }
}
