using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;

namespace NihongoAcademy.Services
{
    public sealed class AchievementBadge
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-award";
        public string Color { get; set; } = "#ee96a4";
        public bool IsUnlocked { get; set; }
        public string ProgressText { get; set; } = string.Empty;
    }

    public sealed class AchievementService
    {
        private readonly ApplicationDbContext _context;

        public AchievementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AchievementBadge>> GetUserBadgesAsync(string userId)
        {
            // 1. Lesson completions
            var completedLessons = await _context.LessonProgress.AsNoTracking()
                .Where(p => p.UserId == userId && p.IsCompleted)
                .Include(p => p.Lesson)
                    .ThenInclude(l => l!.Category)
                .ToListAsync();

            var completedCount = completedLessons.Count;
            var stableCount = completedLessons.Count(p => p.StableMasteryDate != null);

            var kanaCompleted = completedLessons.Any(p => p.Lesson?.Category?.Name == "Hiragana") &&
                                completedLessons.Any(p => p.Lesson?.Category?.Name == "Katakana");

            // 2. Streaks from active days. This counts every day the learner started
            // something, matching how ProgressSummaryService builds the dashboard streak.
            var lessonDays = await _context.LessonProgress.AsNoTracking()
                .Where(p => p.UserId == userId && p.StartedDate.HasValue)
                .Select(p => p.StartedDate!.Value.Date)
                .Distinct()
                .ToListAsync();

            var flashcardDays = await _context.FlashcardProgress.AsNoTracking()
                .Where(p => p.UserId == userId && p.LastReview != null)
                .Select(p => p.LastReview!.Value.Date)
                .Distinct()
                .ToListAsync();

            var allDays = lessonDays.Union(flashcardDays).Distinct().ToList();
            var longestStreak = ProgressSummaryService.CalculateLongestStreak(allDays);

            // 3. Quiz attempts
            var quizAttempts = await _context.QuizAttempts.AsNoTracking()
                .Where(a => a.UserId == userId)
                .ToListAsync();
            var avgScore = quizAttempts.Count > 0 ? (int)Math.Round(quizAttempts.Average(a => a.ScorePercentage)) : 0;

            // 4. Community contributions
            var postCount = await _context.CommunityPosts.AsNoTracking()
                .CountAsync(p => p.UserId == userId);

            var postIds = await _context.CommunityPosts.AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .ToListAsync();

            var totalHearts = await _context.CommunityLikes.AsNoTracking()
                .CountAsync(l => postIds.Contains(l.PostId));

            return new List<AchievementBadge>
            {
                new AchievementBadge
                {
                    Id = "streak_7",
                    Title = "Week Warrior",
                    Description = "Maintain a 7-day daily study streak.",
                    Icon = "bi-fire",
                    Color = "#e74c3c",
                    IsUnlocked = longestStreak >= 7,
                    ProgressText = $"{Math.Min(longestStreak, 7)} / 7 days"
                },
                new AchievementBadge
                {
                    Id = "kana_master",
                    Title = "Kana Pioneer",
                    Description = "Study and complete introductory Kana lessons.",
                    Icon = "bi-translate",
                    Color = "#9b59b6",
                    IsUnlocked = kanaCompleted,
                    ProgressText = kanaCompleted ? "Unlocked" : "Hiragana & Katakana"
                },
                new AchievementBadge
                {
                    Id = "dedicated_10",
                    Title = "Dedicated Learner",
                    Description = "Complete 10 or more core curriculum lessons.",
                    Icon = "bi-journal-check",
                    Color = "#27ae60",
                    IsUnlocked = completedCount >= 10,
                    ProgressText = $"{Math.Min(completedCount, 10)} / 10 lessons"
                },
                new AchievementBadge
                {
                    Id = "quiz_ace",
                    Title = "Quiz Ace",
                    Description = "Maintain an 85%+ score average across quizzes.",
                    Icon = "bi-trophy-fill",
                    Color = "#f39c12",
                    IsUnlocked = quizAttempts.Count >= 3 && avgScore >= 85,
                    ProgressText = $"{avgScore}% avg ({quizAttempts.Count} quizzes)"
                },
                new AchievementBadge
                {
                    Id = "first_post",
                    Title = "Community Voice",
                    Description = "Publish your first post in the learner community.",
                    Icon = "bi-chat-heart-fill",
                    Color = "#3498db",
                    IsUnlocked = postCount >= 1,
                    ProgressText = postCount >= 1 ? "Unlocked" : "0 / 1 posts"
                },
                new AchievementBadge
                {
                    Id = "heart_magnet",
                    Title = "Heart Magnet",
                    Description = "Receive 5 or more hearts from fellow learners.",
                    Icon = "bi-heart-fill",
                    Color = "#e84393",
                    IsUnlocked = totalHearts >= 5,
                    ProgressText = $"{Math.Min(totalHearts, 5)} / 5 hearts"
                },
                new AchievementBadge
                {
                    Id = "stable_master",
                    Title = "Stable Master",
                    Description = "Lock in stable mastery on your delayed review.",
                    Icon = "bi-patch-check-fill",
                    Color = "#16a085",
                    IsUnlocked = stableCount >= 1,
                    ProgressText = stableCount >= 1 ? "Unlocked" : "Complete review"
                }
            };
        }
    }
}
