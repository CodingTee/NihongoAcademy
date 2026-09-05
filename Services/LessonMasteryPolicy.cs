namespace NihongoAcademy.Services
{
    public record ExerciseMasteryResult(int Count, int Accuracy, int Covered, int Required);
    public record MasteryAttemptData(string ExerciseType, int? VocabularyId, bool IsCorrect, DateTime AttemptedAt);

    public static class LessonMasteryPolicy
    {
        public static readonly string[] CoreTypes = { "recognition", "listening", "recall" };

        public static bool HasPassedFirstCheck(
            IReadOnlyDictionary<string, ExerciseMasteryResult> results,
            int vocabularyCount,
            bool activitiesComplete)
        {
            if (!activitiesComplete || vocabularyCount <= 0) return false;

            return results.All(x => x.Key == "production"
                ? x.Value.Count >= 1 && x.Value.Accuracy == 100
                : x.Value.Count >= Math.Max(5, vocabularyCount)
                    && x.Value.Covered >= vocabularyCount
                    && x.Value.Accuracy >= 80);
        }

        public static bool HasPassedDelayedReview(
            IEnumerable<MasteryAttemptData> attempts,
            IReadOnlyCollection<int> vocabularyIds,
            DateTime reviewDue)
        {
            if (vocabularyIds.Count == 0) return false;
            var delayed = attempts.Where(a => a.AttemptedAt >= reviewDue).ToList();
            var requiredCoverage = Math.Min(3, vocabularyIds.Count);

            return CoreTypes.All(type =>
            {
                var group = delayed.Where(a => a.ExerciseType == type &&
                    a.VocabularyId.HasValue && vocabularyIds.Contains(a.VocabularyId.Value)).ToList();
                var accuracy = group.Count == 0
                    ? 0
                    : (int)Math.Round(group.Count(a => a.IsCorrect) * 100d / group.Count);
                return group.Count >= requiredCoverage
                    && group.Where(a => a.VocabularyId.HasValue)
                        .Select(a => a.VocabularyId!.Value).Distinct().Count() >= requiredCoverage
                    && accuracy >= 80;
            });
        }
    }
}
