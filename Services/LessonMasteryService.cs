using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Services
{
    public record ExerciseMasteryResult(int Count, int Accuracy, int Covered, int Required);
    public record MasteryEvaluation(
        bool Mastered,
        bool FirstPassComplete,
        bool AwaitingDelayedReview,
        DateTime? ReviewDueDate,
        Dictionary<string, ExerciseMasteryResult> Results);

    public class LessonMasteryService
    {
        private static readonly string[] CoreTypes = { "recognition", "listening", "recall" };
        private readonly ApplicationDbContext _context;

        public LessonMasteryService(ApplicationDbContext context) => _context = context;

        public async Task<MasteryEvaluation> EvaluateAsync(string userId, int lessonId, DateTime? evaluatedAt = null)
        {
            var now = evaluatedAt ?? DateTime.Now;
            var lesson = await _context.Lessons
                .Where(l => l.Id == lessonId)
                .Select(l => new { l.Title })
                .FirstAsync();
            var vocabularyIds = await _context.LessonVocabulary
                .Where(x => x.LessonId == lessonId)
                .Select(x => x.VocabularyId)
                .ToListAsync();
            var attempts = await _context.PracticeAttempts
                .Where(a => a.UserId == userId && a.LessonId == lessonId)
                .ToListAsync();
            var requiredTypes = lesson.Title.StartsWith("A1:") || lesson.Title.StartsWith("A2:")
                ? CoreTypes.Concat(new[] { "production" }).ToArray()
                : CoreTypes;

            var results = requiredTypes.ToDictionary(
                type => type,
                type =>
                {
                    var group = attempts.Where(a => a.ExerciseType == type).ToList();
                    return new ExerciseMasteryResult(
                        group.Count,
                        group.Count == 0 ? 0 : (int)Math.Round(group.Count(a => a.IsCorrect) * 100d / group.Count),
                        group.Where(a => a.VocabularyId.HasValue).Select(a => a.VocabularyId).Distinct().Count(),
                        type == "production" ? 1 : vocabularyIds.Count);
                });

            var firstPassComplete = results.All(x => x.Key == "production"
                ? x.Value.Count >= 1 && x.Value.Accuracy == 100
                : x.Value.Count >= Math.Max(5, vocabularyIds.Count)
                    && x.Value.Covered >= vocabularyIds.Count
                    && x.Value.Accuracy >= 80);

            var requiredActivityIds = await _context.LessonActivities
                .Where(a => a.LessonId == lessonId && a.ActivityType != "dialogue")
                .Select(a => a.Id)
                .ToListAsync();
            firstPassComplete = firstPassComplete && requiredActivityIds.All(activityId => attempts.Any(a =>
                a.LessonActivityId == activityId && a.IsCorrect));

            var progress = await _context.LessonProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);
            if (progress?.IsCompleted == true)
                return new MasteryEvaluation(true, true, false, progress.MasteryReviewDueDate, results);

            var awaitingReview = false;
            DateTime? reviewDue = progress?.MasteryReviewDueDate;
            if (firstPassComplete)
            {
                if (progress == null)
                {
                    progress = new LessonProgress { UserId = userId, LessonId = lessonId, StartedDate = now };
                    _context.LessonProgress.Add(progress);
                }

                if (!progress.FirstMasteryPassedDate.HasValue)
                {
                    progress.FirstMasteryPassedDate = now;
                    progress.MasteryReviewDueDate = now.AddDays(1);
                    reviewDue = progress.MasteryReviewDueDate;
                    progress.CompletionPercentage = 90;
                    progress.IsCompleted = false;
                    awaitingReview = true;
                }
                else
                {
                    reviewDue ??= progress.FirstMasteryPassedDate.Value.AddDays(1);
                    progress.MasteryReviewDueDate = reviewDue;
                    if (now >= reviewDue && HasPassedDelayedReview(attempts, vocabularyIds, reviewDue.Value))
                    {
                        progress.CompletionPercentage = 100;
                        progress.IsCompleted = true;
                        progress.CompletedDate ??= now;
                        progress.StableMasteryDate = now;
                    }
                    else
                    {
                        progress.CompletionPercentage = 90;
                        awaitingReview = true;
                    }
                }
                await _context.SaveChangesAsync();
            }

            return new MasteryEvaluation(progress?.IsCompleted == true, firstPassComplete, awaitingReview, reviewDue, results);
        }

        private static bool HasPassedDelayedReview(List<PracticeAttempt> attempts, List<int> vocabularyIds, DateTime reviewDue)
        {
            var delayed = attempts.Where(a => a.AttemptedAt >= reviewDue).ToList();
            return CoreTypes.All(type =>
            {
                var group = delayed.Where(a => a.ExerciseType == type).ToList();
                var requiredCoverage = Math.Min(3, vocabularyIds.Count);
                var accuracy = group.Count == 0 ? 0 : (int)Math.Round(group.Count(a => a.IsCorrect) * 100d / group.Count);
                return group.Count >= requiredCoverage
                    && group.Where(a => a.VocabularyId.HasValue).Select(a => a.VocabularyId).Distinct().Count() >= requiredCoverage
                    && accuracy >= 80;
            });
        }
    }
}
