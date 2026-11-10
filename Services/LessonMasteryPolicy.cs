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

        public record TypeMasteryStatus(
            bool Met,
            int Accuracy,
            int Covered,
            int RequiredCoverage,
            int Count,
            int RequiredCount,
            bool IsProduction);

        // Per-type first-pass status for the partial-progress UI. Production keeps its
        // original gate (one valid submission == 100%); the three core types follow the
        // middle route: each reaches "met" at >=80% accuracy with full coverage.
        public static Dictionary<string, TypeMasteryStatus> GetFirstPassTypeStatus(
            IReadOnlyDictionary<string, ExerciseMasteryResult> results)
        {
            var status = new Dictionary<string, TypeMasteryStatus>();
            foreach (var type in results.Keys)
            {
                results.TryGetValue(type, out var r);
                var count = r?.Count ?? 0;
                var accuracy = r?.Accuracy ?? 0;
                var covered = r?.Covered ?? 0;
                if (type == "production")
                {
                    status[type] = new TypeMasteryStatus(
                        Met: count >= 1 && accuracy == 100,
                        Accuracy: accuracy, Covered: covered, RequiredCoverage: 1,
                        Count: count, RequiredCount: 1, IsProduction: true);
                }
                else
                {
                    var vocabCount = r?.Required ?? 0;
                    var requiredCount = Math.Max(5, vocabCount);
                    status[type] = new TypeMasteryStatus(
                        Met: count >= requiredCount && covered >= vocabCount && accuracy >= 80,
                        Accuracy: accuracy, Covered: covered, RequiredCoverage: vocabCount,
                        Count: count, RequiredCount: requiredCount, IsProduction: false);
                }
            }
            return status;
        }

        // Derive a 70-90 display percent: 70 baseline, +20 spread across the required types
        // (each type that is "met" adds an equal slice). 90 means every required type passed.
        public static int GetFirstPassDisplayPercent(
            IReadOnlyDictionary<string, TypeMasteryStatus> status)
        {
            if (status.Count == 0) return 70;
            var met = status.Values.Count(s => s.Met);
            return 70 + (int)Math.Round(met * 20d / status.Count);
        }
    }
}
