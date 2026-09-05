using NihongoAcademy.Services;

var kanaIds = new[] { 1, 2, 3, 4, 5 };
var checks = new List<(string Name, Action Run)>
{
    ("first check rejects repeated single-item drilling", () =>
    {
        var results = CoreResults(count: 5, accuracy: 100, covered: 1, required: 5);
        Expect(!LessonMasteryPolicy.HasPassedFirstCheck(results, 5, true));
    }),
    ("first check requires every kana in every mode", () =>
    {
        var results = CoreResults(count: 5, accuracy: 80, covered: 5, required: 5);
        Expect(LessonMasteryPolicy.HasPassedFirstCheck(results, 5, true));
        results["listening"] = results["listening"] with { Covered = 4 };
        Expect(!LessonMasteryPolicy.HasPassedFirstCheck(results, 5, true));
    }),
    ("required activities cannot be skipped", () =>
    {
        var results = CoreResults(5, 100, 5, 5);
        Expect(!LessonMasteryPolicy.HasPassedFirstCheck(results, 5, false));
    }),
    ("A1 production must be completed successfully", () =>
    {
        var results = CoreResults(5, 100, 5, 5);
        results["production"] = new ExerciseMasteryResult(0, 0, 0, 1);
        Expect(!LessonMasteryPolicy.HasPassedFirstCheck(results, 5, true));
        results["production"] = new ExerciseMasteryResult(1, 100, 1, 1);
        Expect(LessonMasteryPolicy.HasPassedFirstCheck(results, 5, true));
    }),
    ("same-day work cannot satisfy delayed review", () =>
    {
        var due = DateTime.Today.AddDays(1);
        var attempts = DelayedAttempts(due.AddMinutes(-1));
        Expect(!LessonMasteryPolicy.HasPassedDelayedReview(attempts, kanaIds, due));
    }),
    ("delayed review requires three distinct kana per mode", () =>
    {
        var due = DateTime.Today;
        var attempts = LessonMasteryPolicy.CoreTypes.SelectMany(type =>
            Enumerable.Range(0, 3).Select(i => Attempt(type, 1, true, due.AddMinutes(i)))).ToList();
        Expect(!LessonMasteryPolicy.HasPassedDelayedReview(attempts, kanaIds, due));
    }),
    ("later-day review passes after adequate mixed retrieval", () =>
    {
        var due = DateTime.Today;
        Expect(LessonMasteryPolicy.HasPassedDelayedReview(DelayedAttempts(due), kanaIds, due));
    }),
    ("delayed review rejects accuracy below eighty percent", () =>
    {
        var due = DateTime.Today;
        var attempts = DelayedAttempts(due);
        attempts.AddRange(LessonMasteryPolicy.CoreTypes.Select(type => Attempt(type, 4, false, due.AddMinutes(10))));
        attempts.AddRange(LessonMasteryPolicy.CoreTypes.Select(type => Attempt(type, 5, false, due.AddMinutes(11))));
        Expect(!LessonMasteryPolicy.HasPassedDelayedReview(attempts, kanaIds, due));
    }),
    ("earlier-lesson review items do not distort current mastery", () =>
    {
        var due = DateTime.Today;
        var attempts = DelayedAttempts(due);
        attempts.AddRange(LessonMasteryPolicy.CoreTypes.SelectMany(type =>
            Enumerable.Range(0, 20).Select(i => Attempt(type, 99, false, due.AddMinutes(i)))));
        Expect(LessonMasteryPolicy.HasPassedDelayedReview(attempts, kanaIds, due));
    }),
    ("earlier-lesson items cannot replace current targets", () =>
    {
        var due = DateTime.Today;
        var attempts = LessonMasteryPolicy.CoreTypes.SelectMany(type =>
            new[] { 91, 92, 93, 94, 95 }.Select(id => Attempt(type, id, true, due))).ToList();
        Expect(!LessonMasteryPolicy.HasPassedDelayedReview(attempts, kanaIds, due));
    })
};

var failures = 0;
foreach (var check in checks)
{
    try { check.Run(); Console.WriteLine($"PASS  {check.Name}"); }
    catch (Exception ex) { failures++; Console.WriteLine($"FAIL  {check.Name}: {ex.Message}"); }
}

Console.WriteLine($"{checks.Count - failures}/{checks.Count} mastery policy checks passed.");
return failures == 0 ? 0 : 1;

static Dictionary<string, ExerciseMasteryResult> CoreResults(int count, int accuracy, int covered, int required) =>
    LessonMasteryPolicy.CoreTypes.ToDictionary(
        type => type,
        _ => new ExerciseMasteryResult(count, accuracy, covered, required));

static List<MasteryAttemptData> DelayedAttempts(DateTime time) =>
    LessonMasteryPolicy.CoreTypes.SelectMany(type => new[]
    {
        Attempt(type, 1, true, time),
        Attempt(type, 2, true, time.AddMinutes(1)),
        Attempt(type, 3, true, time.AddMinutes(2))
    }).ToList();

static MasteryAttemptData Attempt(string type, int vocabularyId, bool correct, DateTime time) =>
    new(type, vocabularyId, correct, time);

static void Expect(bool condition)
{
    if (!condition) throw new InvalidOperationException("expectation was not met");
}
