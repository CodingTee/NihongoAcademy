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
    }),
    ("Japanese answers ignore spacing and basic punctuation", () =>
    {
        Expect(AnswerEvaluationPolicy.MatchesJapanese(" 田中さん は、学生です。 ", "田中さんは学生です"));
    }),
    ("incorrect Japanese remains incorrect", () =>
    {
        Expect(!AnswerEvaluationPolicy.MatchesJapanese("学校を行きます", "学校に行きます"));
    }),
    ("accepted romanized answers are case insensitive", () =>
    {
        Expect(AnswerEvaluationPolicy.MatchesAny(" TOKYO. ", new[] { "Tokyo", "とうきょう" }));
    }),
    ("empty answers and zero attempts are safe", () =>
    {
        Expect(!AnswerEvaluationPolicy.MatchesAny(null, new[] { "はい" }));
        Expect(AnswerEvaluationPolicy.Accuracy(0, 0) == 0);
    }),
    ("integrated question mastery rejects same-day repetition", () =>
    {
        var attempts = new[] { 1, 2 }.SelectMany(id => new[]
        {
            new QuestionAttemptData(id, true, DateTime.Today.AddHours(1)),
            new QuestionAttemptData(id, true, DateTime.Today.AddHours(2))
        });
        Expect(!IntegratedSkillMasteryPolicy.HasStableQuestionMastery(attempts, new[] { 1, 2 }));
    }),
    ("integrated question mastery requires every item on latest date", () =>
    {
        var attempts = new[]
        {
            new QuestionAttemptData(1, true, DateTime.Today.AddDays(-1)),
            new QuestionAttemptData(2, true, DateTime.Today.AddDays(-1)),
            new QuestionAttemptData(1, true, DateTime.Today),
            new QuestionAttemptData(2, false, DateTime.Today),
            new QuestionAttemptData(2, true, DateTime.Today.AddDays(-1).AddHours(1))
        };
        Expect(!IntegratedSkillMasteryPolicy.HasStableQuestionMastery(attempts, new[] { 1, 2 }));
    }),
    ("integrated question mastery passes a complete delayed retrieval", () =>
    {
        var attempts = new[]
        {
            new QuestionAttemptData(1, true, DateTime.Today.AddDays(-1)),
            new QuestionAttemptData(2, true, DateTime.Today.AddDays(-1)),
            new QuestionAttemptData(1, true, DateTime.Today),
            new QuestionAttemptData(2, true, DateTime.Today)
        };
        Expect(IntegratedSkillMasteryPolicy.HasStableQuestionMastery(attempts, new[] { 1, 2 }));
    }),
    ("A2 listening requires slow and natural playback", () =>
    {
        var oneMode = new[]
        {
            new QuestionAttemptData(1, true, DateTime.Today.AddDays(-1), "natural"),
            new QuestionAttemptData(2, true, DateTime.Today.AddDays(-1), "natural"),
            new QuestionAttemptData(1, true, DateTime.Today, "natural"),
            new QuestionAttemptData(2, true, DateTime.Today, "natural")
        };
        Expect(!IntegratedSkillMasteryPolicy.HasStableQuestionMastery(oneMode, new[] { 1, 2 }, true));
        Expect(IntegratedSkillMasteryPolicy.HasStableQuestionMastery(oneMode.Append(
            new QuestionAttemptData(1, true, DateTime.Today, "slow")), new[] { 1, 2 }, true));
    }),
    ("speaking and writing require passing output on two dates", () =>
    {
        Expect(!IntegratedSkillMasteryPolicy.HasStableOutputMastery(new[]
        {
            new OutputAttemptData(true, 90, DateTime.Today),
            new OutputAttemptData(true, 95, DateTime.Today.AddHours(1))
        }));
        Expect(IntegratedSkillMasteryPolicy.HasStableOutputMastery(new[]
        {
            new OutputAttemptData(true, 80, DateTime.Today.AddDays(-1)),
            new OutputAttemptData(true, 90, DateTime.Today)
        }));
    }),
    ("same-day grammar drilling is not stable mastery", () =>
    {
        var day = DateTime.Today;
        var attempts = new[] { "grammar-meaning", "grammar-correction", "grammar-order" }
            .SelectMany(type => new[] { (type, true, day), (type, true, day.AddHours(1)) });
        Expect(!AnswerEvaluationPolicy.HasStableGrammarMastery(attempts));
    }),
    ("grammar mastery requires every exercise type", () =>
    {
        var day = DateTime.Today;
        var attempts = Enumerable.Range(0, 3).SelectMany(index => new[]
        {
            ("grammar-meaning", true, day.AddDays(index > 0 ? 1 : 0)),
            ("grammar-correction", true, day.AddDays(index > 0 ? 1 : 0))
        });
        Expect(!AnswerEvaluationPolicy.HasStableGrammarMastery(attempts));
    }),
    ("mixed grammar retrieval across dates can become stable", () =>
    {
        var day = DateTime.Today;
        var attempts = new[] { "grammar-meaning", "grammar-correction", "grammar-order" }
            .SelectMany(type => new[] { (type, true, day), (type, true, day.AddDays(1)) });
        Expect(AnswerEvaluationPolicy.HasStableGrammarMastery(attempts));
    }),
    ("grammar accuracy below eighty percent is rejected", () =>
    {
        var day = DateTime.Today;
        var attempts = new List<(string, bool, DateTime)>
        {
            ("grammar-meaning", true, day), ("grammar-correction", true, day), ("grammar-order", true, day),
            ("grammar-meaning", true, day.AddDays(1)), ("grammar-correction", true, day.AddDays(1)),
            ("grammar-order", false, day.AddDays(1)), ("grammar-order", false, day.AddDays(1))
        };
        Expect(!AnswerEvaluationPolicy.HasStableGrammarMastery(attempts));
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
