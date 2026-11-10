namespace NihongoAcademy.Services
{
    public sealed record QuestionAttemptData(int QuestionId, bool IsCorrect, DateTime AttemptedAt, string? PlaybackMode = null);
    public sealed record OutputAttemptData(bool IsPassing, int Score, DateTime AttemptedAt);

    public static class IntegratedSkillMasteryPolicy
    {
        public static bool HasStableQuestionMastery(IEnumerable<QuestionAttemptData> source,
            IEnumerable<int> requiredQuestionIds, bool requireBothPlaybackModes = false)
        {
            var attempts = source.ToList();
            var ids = requiredQuestionIds.Distinct().ToList();
            if (ids.Count == 0 || attempts.Count < ids.Count * 2) return false;
            if (attempts.Count(attempt => attempt.IsCorrect) * 100d / attempts.Count < 80) return false;

            var correctDates = attempts.Where(attempt => attempt.IsCorrect)
                .Select(attempt => attempt.AttemptedAt.Date).Distinct().OrderBy(date => date).ToList();
            if (correctDates.Count < 2) return false;

            if (requireBothPlaybackModes)
            {
                var modes = attempts.Where(attempt => attempt.IsCorrect && !string.IsNullOrWhiteSpace(attempt.PlaybackMode))
                    .Select(attempt => attempt.PlaybackMode!.ToLowerInvariant()).ToHashSet();
                if (!modes.Contains("slow") || !modes.Contains("natural")) return false;
            }

            var latestDate = correctDates[^1];
            return ids.All(id => attempts.Any(attempt => attempt.QuestionId == id && attempt.IsCorrect && attempt.AttemptedAt.Date == latestDate));
        }

        public static bool HasStableOutputMastery(IEnumerable<OutputAttemptData> source)
        {
            var passing = source.Where(attempt => attempt.IsPassing && attempt.Score >= 80)
                .OrderBy(attempt => attempt.AttemptedAt).ToList();
            return passing.Select(attempt => attempt.AttemptedAt.Date).Distinct().Count() >= 2;
        }
    }
}
