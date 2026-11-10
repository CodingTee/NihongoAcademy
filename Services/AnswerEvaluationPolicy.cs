namespace NihongoAcademy.Services
{
    public static class AnswerEvaluationPolicy
    {
        public static string NormalizeJapanese(string? value) => string.Concat((value ?? string.Empty).Trim()
            .Where(character => !char.IsWhiteSpace(character) && character is not '。' and not '、' and not '.' and not ','));

        public static string NormalizeFlexible(string? value) => NormalizeJapanese(value).ToLowerInvariant();

        public static bool MatchesJapanese(string? answer, string? expected) =>
            NormalizeJapanese(answer) == NormalizeJapanese(expected);

        public static bool MatchesAny(string? answer, IEnumerable<string> acceptedAnswers) =>
            acceptedAnswers.Any(expected => NormalizeFlexible(answer) == NormalizeFlexible(expected));

        public static int Accuracy(int correctCount, int totalCount) => totalCount <= 0
            ? 0 : (int)Math.Round(correctCount * 100d / totalCount);

        public static bool HasStableGrammarMastery(IEnumerable<(string ExerciseType, bool IsCorrect, DateTime AttemptedAt)> attempts)
        {
            var list = attempts.ToList();
            var passedTypes = list.Where(attempt => attempt.IsCorrect).Select(attempt => attempt.ExerciseType).Distinct().Count();
            var dates = list.Where(attempt => attempt.IsCorrect).Select(attempt => attempt.AttemptedAt.Date).Distinct().OrderBy(date => date).ToList();
            return list.Count >= 6 && passedTypes >= 3 && Accuracy(list.Count(attempt => attempt.IsCorrect), list.Count) >= 80 &&
                dates.Count >= 2 && dates[^1] > dates[0];
        }
    }
}
