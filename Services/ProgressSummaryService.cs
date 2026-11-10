using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Services
{
    public sealed class ProgressSummaryService
    {
        private static readonly string[] CorePathCategories = { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course" };
        private static readonly TimeSpan NavbarCacheTtl = TimeSpan.FromSeconds(60);
        private readonly ApplicationDbContext _context;
        private readonly IAppClock _clock;
        private readonly IMemoryCache _cache;

        public ProgressSummaryService(ApplicationDbContext context, IAppClock clock, IMemoryCache cache)
        {
            _context = context;
            _clock = clock;
            _cache = cache;
        }

        public async Task<ProgressSummary> BuildAsync(string userId)
        {
            var lessonProgress = await _context.LessonProgress.AsNoTracking()
                .Where(p => p.UserId == userId)
                .Include(p => p.Lesson)
                    .ThenInclude(l => l!.Category)
                .OrderByDescending(p => p.StartedDate)
                .ToListAsync();

            var quizAttempts = await _context.QuizAttempts.AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.Quiz)
                    .ThenInclude(q => q!.Category)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();

            var totalLessons = await _context.Lessons.CountAsync(l => l.IsPublished);
            var completedLessons = lessonProgress.Count(p => p.IsCompleted);
            var stableMasteredLessons = lessonProgress.Count(p => p.StableMasteryDate.HasValue);
            var now = _clock.Now;
            var today = _clock.Today;

            var lessonDays = lessonProgress
                .Where(p => p.StartedDate.HasValue)
                .Select(p => p.StartedDate!.Value.Date);

            var flashcardDays = await _context.FlashcardProgress.AsNoTracking()
                .Where(p => p.UserId == userId && p.LastReview != null)
                .Select(p => p.LastReview!.Value.Date)
                .Distinct()
                .ToListAsync();

            var activeDays = lessonDays.Union(flashcardDays).Distinct().OrderByDescending(d => d).ToList();
            var dueReviews = lessonProgress
                .Where(p => !p.IsCompleted && p.FirstMasteryPassedDate.HasValue && p.MasteryReviewDueDate.HasValue && p.MasteryReviewDueDate.Value <= now)
                .OrderBy(p => p.MasteryReviewDueDate)
                .ToList();
            var awaitingReviews = lessonProgress
                .Where(p => !p.IsCompleted && p.FirstMasteryPassedDate.HasValue)
                .ToList();

            var continueLesson = lessonProgress
                .Where(p => !p.IsCompleted && p.Lesson?.IsPublished == true && IsCorePath(p.Lesson))
                .OrderByDescending(p => p.StartedDate)
                .FirstOrDefault();

            var nextLesson = continueLesson == null
                ? await GetNextAvailableLessonAsync(userId)
                : null;

            var categoryProgress = await _context.Categories.AsNoTracking()
                .Select(c => new CategoryProgressItem
                {
                    CategoryName = c.Name,
                    TotalLessons = c.Lessons.Count(l => l.IsPublished),
                    CompletedLessons = c.Lessons.Count(l => l.IsPublished && l.ProgressRecords.Any(p => p.UserId == userId && p.IsCompleted))
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            var quizAverage = quizAttempts.Count > 0 ? (int)Math.Round(quizAttempts.Average(a => a.ScorePercentage)) : 0;
            var quizInsight = BuildQuizInsight(quizAttempts, categoryProgress);
            var nextAction = BuildNextAction(dueReviews.FirstOrDefault(), continueLesson, nextLesson, quizAttempts);

            // Calculate 5-Skill Radar Scores
            var listeningAttempts = await _context.PracticeAttempts.AsNoTracking()
                .Where(a => a.UserId == userId && a.ExerciseType == "listening")
                .ToListAsync();
            var listeningScore = listeningAttempts.Count == 0 ? 0 :
                (int)Math.Min(100, Math.Round(listeningAttempts.Count(a => a.IsCorrect) * 100d / listeningAttempts.Count));

            var readingAttempts = await _context.ReadingAttempts.AsNoTracking()
                .Where(a => a.UserId == userId)
                .ToListAsync();
            var readingScore = readingAttempts.Count == 0 ? 0 :
                (int)Math.Min(100, Math.Round(readingAttempts.Count(a => a.IsCorrect) * 100d / readingAttempts.Count));

            var writingSubmissions = await _context.WritingSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId)
                .ToListAsync();
            var writingScore = writingSubmissions.Count == 0 ? 0 :
                (int)Math.Min(100, Math.Round(writingSubmissions.Average(s => s.Score)));

            var vocabScore = (int)Math.Min(100, Math.Round((double)completedLessons / Math.Max(1, totalLessons) * 100));

            var grammarAttempts = await _context.PracticeAttempts.AsNoTracking()
                .Where(a => a.UserId == userId && (a.ExerciseType == "recognition" || a.ExerciseType == "recall" || a.GrammarPointId != null))
                .ToListAsync();
            var grammarScore = grammarAttempts.Count == 0 ? 0 :
                (int)Math.Min(100, Math.Round(grammarAttempts.Count(a => a.IsCorrect) * 100d / grammarAttempts.Count));

            var skillRadarScores = new Dictionary<string, int>
            {
                { "Vocabulary", Math.Max(vocabScore, 20) },
                { "Grammar", Math.Max(grammarScore, 20) },
                { "Listening", Math.Max(listeningScore, 15) },
                { "Reading", Math.Max(readingScore, 15) },
                { "Writing", Math.Max(writingScore, 15) }
            };

            var jlptN5Target = 15;
            var jlptN4Target = 30;
            var jlptN5Pct = Math.Min(100, (int)Math.Round((double)completedLessons / jlptN5Target * 100));
            var jlptN4Pct = Math.Min(100, (int)Math.Round((double)completedLessons / jlptN4Target * 100));

            // Personalized Goal Plan based on Onboarding details
            var profile = await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);
            var goalPlan = BuildPersonalizedGoalPlan(profile?.LearningGoal, profile?.CurrentLevel, completedLessons);

            return new ProgressSummary
            {
                LessonProgress = lessonProgress,
                QuizAttempts = quizAttempts.Take(10).ToList(),
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                StableMasteredLessons = stableMasteredLessons,
                TotalQuizAttempts = quizAttempts.Count,
                PassedQuizzes = quizAttempts.Count(a => a.IsPassed),
                AverageScore = quizAverage,
                CategoryProgress = categoryProgress,
                RecentQuizScores = quizAttempts.Take(10).Reverse().Select(a => a.ScorePercentage).ToList(),
                RecentQuizLabels = quizAttempts.Take(10).Reverse().Select(a => a.AttemptDate.ToString("MM/dd")).ToList(),
                CurrentStreak = CalculateCurrentStreak(activeDays, today),
                LongestStreak = CalculateLongestStreak(activeDays),
                ActiveDays = activeDays.ToHashSet(),
                AwaitingReviewCount = awaitingReviews.Count,
                DueReviewCount = dueReviews.Count,
                ContinueLesson = continueLesson,
                NextLesson = nextLesson,
                NextAction = nextAction,
                QuizInsight = quizInsight,
                Today = today,
                SkillRadarScores = skillRadarScores,
                JlptN5Percentage = jlptN5Pct,
                JlptN4Percentage = jlptN4Pct,
                DisplayName = profile?.DisplayName,
                GoalPlan = goalPlan
            };
        }

        public async Task<NavbarProgressSummary> BuildNavbarAsync(string userId)
        {
            var today = _clock.Today;
            // Active days change at most once per day, so a short cache removes two queries
            // from every authenticated page view. Daily reviews stay live for the goal ring.
            var cacheKey = $"navbar:days:{userId}:{today:yyyyMMdd}";
            var activityDays = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = NavbarCacheTtl;
                return await GetActiveDaysAsync(userId);
            }) ?? new List<DateTime>();

            var dailyReviews = await _context.FlashcardProgress.AsNoTracking()
                .CountAsync(p => p.UserId == userId && p.LastReview != null && p.LastReview.Value.Date == today);

            return new NavbarProgressSummary
            {
                Streak = CalculateCurrentStreak(activityDays, today),
                DailyProgress = dailyReviews,
                DailyGoal = 10
            };
        }

        private async Task<List<DateTime>> GetActiveDaysAsync(string userId)
        {
            // Two simple queries, merged in memory. The caller caches the result, so this runs
            // at most once per minute per user and adds no new SQL translation risk.
            var lessonDays = await _context.LessonProgress
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.StartedDate != null)
                .Select(p => p.StartedDate!.Value.Date)
                .Distinct()
                .ToListAsync();
            var flashcardDays = await _context.FlashcardProgress
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.LastReview != null)
                .Select(p => p.LastReview!.Value.Date)
                .Distinct()
                .ToListAsync();
            return lessonDays.Union(flashcardDays).Distinct().OrderByDescending(d => d).ToList();
        }

        private async Task<Lesson?> GetNextAvailableLessonAsync(string userId)
        {
            var startedLessonIds = await _context.LessonProgress.AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => p.LessonId)
                .ToListAsync();

            return await _context.Lessons
                .Where(l => l.IsPublished && !startedLessonIds.Contains(l.Id) &&
                    CorePathCategories.Contains(l.Category!.Name) &&
                    !_context.LessonPrerequisites.Any(required => required.LessonId == l.Id &&
                        !_context.LessonProgress.Any(done => done.UserId == userId &&
                            done.LessonId == required.PrerequisiteLessonId && done.FirstMasteryPassedDate.HasValue)))
                .Include(l => l.Category)
                .OrderBy(l => l.Category!.DisplayOrder)
                .ThenBy(l => l.DisplayOrder)
                .FirstOrDefaultAsync();
        }

        private static ProgressNextAction BuildNextAction(LessonProgress? dueReview, LessonProgress? continueLesson, Lesson? nextLesson, List<QuizAttempt> quizAttempts)
        {
            if (dueReview?.Lesson != null)
            {
                return new ProgressNextAction("Review due", dueReview.Lesson.Title, "Finish the delayed review to lock in stable mastery.", "Lessons", "Details", dueReview.LessonId, "bi-clock-history");
            }
            if (continueLesson?.Lesson != null)
            {
                return new ProgressNextAction("Continue lesson", continueLesson.Lesson.Title, $"{continueLesson.CompletionPercentage}% complete. Pick up where you left off.", "Lessons", "Details", continueLesson.LessonId, "bi-play-circle");
            }
            if (nextLesson != null)
            {
                return new ProgressNextAction("Next lesson", nextLesson.Title, "Start the next available step in your learning path.", "Lessons", "Details", nextLesson.Id, "bi-arrow-right-circle");
            }
            return new ProgressNextAction("Take a quiz", "Check your recall", quizAttempts.Any() ? "Refresh recent material with a quick quiz." : "Start your first quiz to establish a baseline.", "Quizzes", "Index", null, "bi-patch-question");
        }

        private static QuizInsight BuildQuizInsight(List<QuizAttempt> attempts, List<CategoryProgressItem> categoryProgress)
        {
            if (attempts.Count < 2)
            {
                return new QuizInsight("Not enough quiz history yet", "Take at least two quizzes to see a trend.", "Start with a short quiz after your next lesson.");
            }

            var recent = attempts.Take(5).Reverse().Select(a => a.ScorePercentage).ToList();
            var delta = recent.Last() - recent.First();
            var trend = delta >= 5 ? "Improving" : delta <= -5 ? "Needs attention" : "Steady";
            var spread = recent.Max() - recent.Min();
            var weakest = categoryProgress
                .Where(c => c.TotalLessons > 0)
                .OrderBy(c => (double)c.CompletedLessons / c.TotalLessons)
                .FirstOrDefault();
            var recommendation = weakest != null
                ? $"Review {weakest.CategoryName} next; it has the most room to grow."
                : "Keep alternating quizzes with lesson review.";
            var detail = spread >= 25 ? "Scores are swinging a lot, so review before moving on." : $"Recent change: {(delta >= 0 ? "+" : "")}{delta} points.";
            return new QuizInsight(trend, detail, recommendation);
        }

        private static bool IsCorePath(Lesson lesson) => lesson.Category != null && CorePathCategories.Contains(lesson.Category.Name);

        public static int CalculateCurrentStreak(IEnumerable<DateTime> activeDays, DateTime today)
        {
            var days = activeDays.Select(d => d.Date).ToHashSet();
            if (days.Count == 0) return 0;
            var streak = 0;
            var checkDate = days.Contains(today) ? today : today.AddDays(-1);
            while (days.Contains(checkDate))
            {
                streak++;
                checkDate = checkDate.AddDays(-1);
            }
            return streak;
        }

        public static int CalculateLongestStreak(IEnumerable<DateTime> activeDays)
        {
            var sorted = activeDays.Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
            if (sorted.Count == 0) return 0;
            var longest = 1;
            var current = 1;
            for (var i = 1; i < sorted.Count; i++)
            {
                if ((sorted[i] - sorted[i - 1]).Days == 1) current++;
                else current = 1;
                if (current > longest) longest = current;
            }
            return longest;
        }

        private static GoalPlanInfo BuildPersonalizedGoalPlan(string? goal, string? level, int completedLessons)
        {
            var g = (goal ?? "").ToLowerInvariant();
            var lvl = (level ?? "").ToLowerInvariant();

            if (g.Contains("travel") || g.Contains("trip"))
            {
                return new GoalPlanInfo
                {
                    GoalTitle = "Travel & Survival Japanese",
                    GoalIcon = "bi-airplane",
                    ThemeColor = "#e67e22",
                    Description = "Focused on conversational fluency, asking directions, ordering food, and navigating Japan effortlessly.",
                    RecommendedPace = "3 lessons / week + Listening drills",
                    CurrentFocus = completedLessons < 5 ? "Foundation: Essential Kana & Greetings" : completedLessons < 15 ? "Practical: Food, Shopping & Transit" : "Immersion: Real-world conversations",
                    Milestones = new List<string>
                    {
                        "Master Kana & Essential Pronunciation",
                        "Order at restaurants & handle payments",
                        "Ask directions & read train station signs"
                    },
                    RecommendedActionLabel = "Practice Listening",
                    RecommendedController = "Practice",
                    RecommendedAction = "Listening"
                };
            }

            if (g.Contains("anime") || g.Contains("culture") || g.Contains("manga"))
            {
                return new GoalPlanInfo
                {
                    GoalTitle = "Anime & Pop Culture Immersion",
                    GoalIcon = "bi-tv",
                    ThemeColor = "#9b59b6",
                    Description = "Learn authentic colloquial dialogue, character catchphrases, and emotional expressions without relying on subtitles.",
                    RecommendedPace = "Daily audio exposure + Vocabulary reviews",
                    CurrentFocus = completedLessons < 5 ? "Kana sounds & basic emotional phrases" : completedLessons < 15 ? "Common anime dialogue sentence endings" : "Casual speech & unscripted conversations",
                    Milestones = new List<string>
                    {
                        "Read Hiragana/Katakana fluently",
                        "Recognize common anime idioms & slang",
                        "Understand character speech patterns (da/yo/ne)"
                    },
                    RecommendedActionLabel = "Vocabulary Flashcards",
                    RecommendedController = "Flashcards",
                    RecommendedAction = "Index"
                };
            }

            if (g.Contains("jlpt") || g.Contains("career") || g.Contains("job") || g.Contains("business"))
            {
                return new GoalPlanInfo
                {
                    GoalTitle = "JLPT Mastery & Career Track",
                    GoalIcon = "bi-briefcase",
                    ThemeColor = "#9A7BC4",
                    Description = "Structured pathway targeting official JLPT certification, grammatical precision, and Kanji literacy for professional settings.",
                    RecommendedPace = "4-5 lessons / week + Weekly quizzes",
                    CurrentFocus = completedLessons < 15 ? "JLPT N5 Core Grammar & Kanji Basics" : completedLessons < 30 ? "JLPT N4 Grammar Patterns & Reading" : "Intermediate Grammar & Comprehension",
                    Milestones = new List<string>
                    {
                        "Complete N5 Core Curriculum (15 lessons)",
                        "Master 100 essential Kanji compounds",
                        "Pass N5 benchmark practice quizzes"
                    },
                    RecommendedActionLabel = "Take a Quiz",
                    RecommendedController = "Quizzes",
                    RecommendedAction = "Index"
                };
            }

            // Default: General / Hobby
            return new GoalPlanInfo
            {
                GoalTitle = "Personal Growth & Fluency",
                GoalIcon = "bi-heart",
                ThemeColor = "#ee96a4",
                Description = "A balanced, enjoyable learning journey covering vocabulary, grammar, and cultural nuances at your own comfortable pace.",
                RecommendedPace = "2-3 lessons / week + Regular reviews",
                CurrentFocus = completedLessons < 5 ? "Getting started: Hiragana & Katakana" : completedLessons < 15 ? "Daily life sentence building & basic grammar" : "Expanding reading & vocabulary horizons",
                Milestones = new List<string>
                {
                    "Complete introductory Kana lessons",
                    "Build a 200+ core vocabulary bank",
                    "Achieve consistent 7-day study streak"
                },
                RecommendedActionLabel = "Continue Lessons",
                RecommendedController = "Lessons",
                RecommendedAction = "Index"
            };
        }
    }

    public sealed record ProgressNextAction(string Label, string Title, string Description, string Controller, string Action, int? RouteId, string Icon);
    public sealed record QuizInsight(string Trend, string Detail, string Recommendation);

    public sealed class NavbarProgressSummary
    {
        public int Streak { get; set; }
        public int DailyProgress { get; set; }
        public int DailyGoal { get; set; }
    }

    public sealed class GoalPlanInfo
    {
        public string GoalTitle { get; set; } = string.Empty;
        public string GoalIcon { get; set; } = string.Empty;
        public string ThemeColor { get; set; } = "#ee96a4";
        public string Description { get; set; } = string.Empty;
        public string RecommendedPace { get; set; } = string.Empty;
        public string CurrentFocus { get; set; } = string.Empty;
        public List<string> Milestones { get; set; } = new();
        public string RecommendedActionLabel { get; set; } = "Continue";
        public string RecommendedController { get; set; } = "Lessons";
        public string RecommendedAction { get; set; } = "Index";
    }

    public sealed class ProgressSummary
    {
        public List<LessonProgress> LessonProgress { get; set; } = new();
        public List<QuizAttempt> QuizAttempts { get; set; } = new();
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int StableMasteredLessons { get; set; }
        public int TotalQuizAttempts { get; set; }
        public int PassedQuizzes { get; set; }
        public int AverageScore { get; set; }
        public List<CategoryProgressItem> CategoryProgress { get; set; } = new();
        public List<int> RecentQuizScores { get; set; } = new();
        public List<string> RecentQuizLabels { get; set; } = new();
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public HashSet<DateTime> ActiveDays { get; set; } = new();
        public int AwaitingReviewCount { get; set; }
        public int DueReviewCount { get; set; }
        public LessonProgress? ContinueLesson { get; set; }
        public Lesson? NextLesson { get; set; }
        public ProgressNextAction NextAction { get; set; } = new("Take a quiz", "Check your recall", "Start a quick quiz.", "Quizzes", "Index", null, "bi-patch-question");
        public QuizInsight QuizInsight { get; set; } = new("No quiz history", "Take a quiz to begin tracking.", "Start with a short quiz.");
        public DateTime Today { get; set; }
        public Dictionary<string, int> SkillRadarScores { get; set; } = new();
        public int JlptN5Percentage { get; set; }
        public int JlptN4Percentage { get; set; }
        public string? DisplayName { get; set; }
        public GoalPlanInfo? GoalPlan { get; set; }
    }

    public sealed class CategoryProgressItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
    }
}
