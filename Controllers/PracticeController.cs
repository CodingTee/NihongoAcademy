using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    [Authorize]
    public class PracticeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LessonMasteryService _masteryService;
        private readonly AudioAssetService _audioAssets;
        private readonly XpService _xpService;
        private readonly IAppClock _clock;

        public PracticeController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            LessonMasteryService masteryService,
            AudioAssetService audioAssets,
            XpService xpService, IAppClock clock)
        {
            _context = context;
            _userManager = userManager;
            _masteryService = masteryService;
            _audioAssets = audioAssets;
            _xpService = xpService;
            _clock = clock;
        }

        // ===== PRACTICE HUB =====
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // SRS due count
            var dueCount = 0;
            var weakAreas = new List<string>();
            var dueMasteryReviews = new List<LessonProgress>();
            var pendingMasteryReviewCount = 0;
            if (userId != null)
            {
                var now = _clock.Now;
                dueCount = await _context.FlashcardProgress
                    .CountAsync(fp => fp.UserId == userId && fp.NextReview <= now);

                dueMasteryReviews = await _context.LessonProgress
                    .Where(p => p.UserId == userId && !p.IsCompleted &&
                        p.FirstMasteryPassedDate.HasValue &&
                        p.MasteryReviewDueDate.HasValue && p.MasteryReviewDueDate <= now)
                    .Include(p => p.Lesson)
                    .ThenInclude(l => l!.Category)
                    .OrderBy(p => p.MasteryReviewDueDate)
                    .Take(5)
                    .ToListAsync();

                pendingMasteryReviewCount = await _context.LessonProgress
                    .CountAsync(p => p.UserId == userId && !p.IsCompleted &&
                        p.FirstMasteryPassedDate.HasValue &&
                        p.MasteryReviewDueDate.HasValue && p.MasteryReviewDueDate > now);

                // Weak areas: the five categories with the lowest average ease factor.
                // Aggregation runs in memory because only this learner's weak cards are loaded.
                var weakRows = await _context.FlashcardProgress
                    .AsNoTracking()
                    .Where(fp => fp.UserId == userId && fp.EaseFactor < 2.0)
                    .Join(_context.Vocabulary, fp => fp.VocabularyId, v => v.Id,
                        (fp, v) => new { v.CategoryId, fp.EaseFactor })
                    .ToListAsync();

                var weakCategories = weakRows
                    .GroupBy(x => x.CategoryId)
                    .OrderBy(g => g.Average(x => x.EaseFactor))
                    .Take(5)
                    .Select(g => g.Key)
                    .ToList();

                weakAreas = await _context.Categories
                    .AsNoTracking()
                    .Where(c => weakCategories.Contains(c.Id))
                    .Select(c => c.Name)
                    .ToListAsync();
                weakAreas.RemoveAll(n => string.IsNullOrWhiteSpace(n));
            }

            ViewBag.DueCount = dueCount;
            ViewBag.WeakAreas = weakAreas;
            ViewBag.DueMasteryReviews = dueMasteryReviews;
            ViewBag.PendingMasteryReviewCount = pendingMasteryReviewCount;

            // Topics (non-Culture categories that have vocabulary)
            var topics = await _context.Categories
                .Where(c => c.Name != "Culture")
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            ViewBag.Topics = topics;

            // Vocabulary count per topic: one GROUP BY, keeping a key for every topic (including zero).
            var vocabRows = await _context.Vocabulary
                .AsNoTracking()
                .GroupBy(v => v.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync();
            var topicVocabCounts = topics.ToDictionary(t => t.Id, t => 0);
            foreach (var row in vocabRows)
            {
                if (topicVocabCounts.ContainsKey(row.CategoryId))
                    topicVocabCounts[row.CategoryId] = row.Count;
            }
            ViewBag.TopicVocabCounts = topicVocabCounts;

            return View();
        }

        // ===== LESSON PRACTICE (inside Learn) =====
        public async Task<IActionResult> LessonPractice(int lessonId)
        {
            var userId = _userManager.GetUserId(User);
            var lesson = await _context.Lessons
                .Include(l => l.Category)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.IsPublished);

            if (lesson == null) return NotFound();

            var hasUnmetPrerequisite = await _context.LessonPrerequisites
                .Where(p => p.LessonId == lessonId)
                .AnyAsync(p => !_context.LessonProgress.Any(progress =>
                    progress.UserId == userId &&
                    progress.LessonId == p.PrerequisiteLessonId &&
                    progress.FirstMasteryPassedDate.HasValue));
            if (hasUnmetPrerequisite)
            {
                TempData["Message"] = "Complete the required previous lesson before opening this practice.";
                return RedirectToAction("Index", "Lessons");
            }

            // Prefer explicit lesson content. Older lessons fall back to category data
            // until they have been migrated to lesson-specific mappings.
            var vocab = await _context.LessonVocabulary
                .Where(lv => lv.LessonId == lessonId)
                .OrderBy(lv => lv.DisplayOrder)
                .Select(lv => lv.Vocabulary!)
                .ToListAsync();
            var hasExplicitContent = vocab.Any();
            if (!hasExplicitContent)
            {
                vocab = await _context.Vocabulary
                    .Where(v => v.CategoryId == lesson.CategoryId)
                    .OrderBy(v => v.DisplayOrder)
                    .Take(20)
                    .ToListAsync();
            }

            // Get quizzes for this category
            var quizzes = hasExplicitContent
                ? new List<Quiz>()
                : await _context.Quizzes
                    .Where(q => q.CategoryId == lesson.CategoryId && q.IsPublished)
                    .OrderBy(q => q.DisplayOrder)
                    .Take(3)
                    .ToListAsync();

            ViewBag.Lesson = lesson;
            ViewBag.Vocabulary = vocab;
            var targetVocabularyIds = vocab.Select(v => v.Id).ToList();
            ViewBag.ReviewVocabulary = userId == null || lesson.Category?.Name != "Hiragana"
                ? new List<Vocabulary>()
                : await _context.LessonVocabulary
                    .Where(lv => lv.LessonId != lessonId && !targetVocabularyIds.Contains(lv.VocabularyId) &&
                        lv.Lesson!.CategoryId == lesson.CategoryId &&
                        lv.Lesson.ProgressRecords.Any(p => p.UserId == userId && p.StableMasteryDate != null))
                    .OrderByDescending(lv => lv.Lesson!.DisplayOrder)
                    .ThenBy(lv => lv.DisplayOrder)
                    .Select(lv => lv.Vocabulary!)
                    .Distinct()
                    .Take(20)
                    .ToListAsync();
            ViewBag.Quizzes = quizzes;
            ViewBag.RequiresProduction = lesson.Title.StartsWith("A1:") || lesson.Title.StartsWith("A2:");
            var activities = await _context.LessonActivities
                .Where(a => a.LessonId == lessonId)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();
            foreach (var activity in activities) _audioAssets.ApplyConventionSources(activity);
            ViewBag.Activities = activities;

            var progress = userId == null ? null : await _context.LessonProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);
            ViewBag.IsMastered = progress?.IsCompleted == true;
            ViewBag.MasteryReviewDueDate = progress?.MasteryReviewDueDate;
            ViewBag.MasteryEvaluation = userId != null && hasExplicitContent
                ? await _masteryService.EvaluateAsync(userId, lessonId)
                : null;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordLessonAttempt(int lessonId, int vocabularyId, string exerciseType, string answer)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var allowedTypes = new[] { "recognition", "listening", "recall", "production" };
            if (answer?.Length > 2000) return BadRequest();
            var expectedAnswer = await _context.LessonVocabulary
                .Where(x => x.LessonId == lessonId && x.VocabularyId == vocabularyId && x.Lesson!.IsPublished)
                .Select(x => x.Vocabulary!.Japanese)
                .FirstOrDefaultAsync();
            var countsTowardMastery = expectedAnswer != null;
            if (expectedAnswer == null)
            {
                var lessonCategoryId = await _context.Lessons
                    .Where(l => l.Id == lessonId && l.IsPublished)
                    .Select(l => (int?)l.CategoryId)
                    .FirstOrDefaultAsync();
                if (lessonCategoryId.HasValue)
                {
                    expectedAnswer = await _context.LessonVocabulary
                        .Where(x => x.VocabularyId == vocabularyId && x.Lesson!.CategoryId == lessonCategoryId &&
                            x.Lesson.ProgressRecords.Any(p => p.UserId == userId && p.StableMasteryDate != null))
                        .Select(x => x.Vocabulary!.Japanese)
                        .FirstOrDefaultAsync();
                }
            }
            // Production scoring uses the lesson's vocabulary forms directly, not expectedAnswer,
            // so a null expectedAnswer must not reject a valid production submission.
            if (!allowedTypes.Contains(exerciseType) ||
                (expectedAnswer == null && exerciseType != "production"))
                return BadRequest();

            if (await _context.LessonPrerequisites
                .Where(p => p.LessonId == lessonId)
                .AnyAsync(p => !_context.LessonProgress.Any(progress =>
                    progress.UserId == userId &&
                    progress.LessonId == p.PrerequisiteLessonId &&
                    progress.FirstMasteryPassedDate.HasValue)))
                return Forbid();

            static string NormalizeAnswer(string value) =>
                string.Concat(value.Where(c => !char.IsWhiteSpace(c))).Trim();

            bool isCorrect;
            if (exerciseType == "production")
            {
                // The learner has to tick three self-review checkboxes, but those live in
                // the browser, so trusting length alone meant any eight characters scored
                // correct. Require the response to actually use this lesson's vocabulary.
                var lessonForms = await _context.LessonVocabulary
                    .Where(x => x.LessonId == lessonId)
                    .Select(x => x.Vocabulary!.Japanese)
                    .ToListAsync();
                var written = answer?.Trim() ?? string.Empty;
                isCorrect = written.Length >= 8 && lessonForms.Any(form =>
                    !string.IsNullOrWhiteSpace(form) &&
                    written.Contains(form.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                isCorrect = NormalizeAnswer(answer ?? string.Empty) == NormalizeAnswer(expectedAnswer ?? string.Empty);
            }

            _context.PracticeAttempts.Add(new PracticeAttempt
            {
                UserId = userId,
                LessonId = lessonId,
                VocabularyId = vocabularyId,
                ExerciseType = exerciseType,
                IsCorrect = isCorrect
            });
            await _context.SaveChangesAsync();

            var evaluation = await _masteryService.EvaluateAsync(userId, lessonId);
            var typeStatus = LessonMasteryPolicy.GetFirstPassTypeStatus(evaluation.Results);
            return Json(new
            {
                success = true,
                isCorrect,
                correctAnswer = expectedAnswer,
                countsTowardMastery,
                mastered = evaluation.Mastered,
                firstPassComplete = evaluation.FirstPassComplete,
                awaitingDelayedReview = evaluation.AwaitingDelayedReview,
                reviewDueDate = evaluation.ReviewDueDate,
                results = evaluation.Results,
                firstPassProgress = evaluation.Mastered
                    ? 100
                    : LessonMasteryPolicy.GetFirstPassDisplayPercent(typeStatus),
                typeMet = typeStatus.ToDictionary(k => k.Key, k => k.Value.Met)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordActivityAttempt(int lessonId, int activityId, string answer)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var activity = await _context.LessonActivities
                .FirstOrDefaultAsync(a => a.Id == activityId && a.LessonId == lessonId &&
                    a.Lesson!.IsPublished && a.ActivityType != "dialogue");
            if (activity == null) return BadRequest();
            if (answer?.Length > 2000) return BadRequest();

            if (await _context.LessonPrerequisites
                .Where(p => p.LessonId == lessonId)
                .AnyAsync(p => !_context.LessonProgress.Any(progress =>
                    progress.UserId == userId &&
                    progress.LessonId == p.PrerequisiteLessonId &&
                    progress.FirstMasteryPassedDate.HasValue)))
                return Forbid();

            var normalize = (string value) => string.Concat(value.Where(c => !char.IsWhiteSpace(c))).Trim();
            var isCorrect = normalize(answer ?? string.Empty) == normalize(activity.CorrectAnswer ?? string.Empty);
            _context.PracticeAttempts.Add(new PracticeAttempt
            {
                UserId = userId,
                LessonId = lessonId,
                LessonActivityId = activityId,
                ExerciseType = activity.ActivityType,
                IsCorrect = isCorrect
            });
            await _context.SaveChangesAsync();
            var evaluation = await _masteryService.EvaluateAsync(userId, lessonId);
            var actTypeStatus = LessonMasteryPolicy.GetFirstPassTypeStatus(evaluation.Results);

            return Json(new
            {
                success = true,
                isCorrect,
                correctAnswer = activity.CorrectAnswer,
                activity.Explanation,
                mastered = evaluation.Mastered,
                firstPassComplete = evaluation.FirstPassComplete,
                awaitingDelayedReview = evaluation.AwaitingDelayedReview,
                reviewDueDate = evaluation.ReviewDueDate,
                firstPassProgress = evaluation.Mastered
                    ? 100
                    : LessonMasteryPolicy.GetFirstPassDisplayPercent(actTypeStatus),
                typeMet = actTypeStatus.ToDictionary(k => k.Key, k => k.Value.Met),
                activitiesComplete = evaluation.ActivitiesComplete,
                activitiesTotal = evaluation.ActivitiesTotal
            });
        }

        // ===== MINI QUIZ =====
        // A lesson that owns its vocabulary gets a short recap built from that
        // vocabulary, so the quiz stays tied to what the learner just read. The
        // recap is a self-check: it grades in the browser and writes no practice
        // attempts, which keeps the mastery metrics on the practice page clean.
        private const int MinQuizItems = 4;      // one answer plus three options
        private const int QuizQuestionLimit = 5;

        public async Task<IActionResult> LessonQuiz(int lessonId)
        {
            var userId = _userManager.GetUserId(User);
            var lesson = await _context.Lessons
                .Include(l => l.Category)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.IsPublished);

            if (lesson == null) return NotFound();

            if (await _context.LessonPrerequisites
                .Where(p => p.LessonId == lessonId)
                .AnyAsync(p => !_context.LessonProgress.Any(progress =>
                    progress.UserId == userId &&
                    progress.LessonId == p.PrerequisiteLessonId &&
                    progress.FirstMasteryPassedDate.HasValue)))
            {
                TempData["Message"] = "Complete the required previous lesson before opening this quiz.";
                return RedirectToAction("Index", "Lessons");
            }

            var items = await _context.LessonVocabulary
                .Where(lv => lv.LessonId == lessonId)
                .OrderBy(lv => lv.DisplayOrder)
                .Select(lv => lv.Vocabulary!)
                .ToListAsync();

            if (items.Count >= MinQuizItems)
            {
                var itemIds = items.Select(item => item.Id).ToList();
                var distractors = await _context.Vocabulary
                    .Where(v => v.CategoryId == lesson.CategoryId && !itemIds.Contains(v.Id))
                    .OrderBy(v => v.DisplayOrder)
                    .Take(40)
                    .ToListAsync();

                ViewBag.Lesson = lesson;
                ViewBag.QuizTargets = items;
                // Distractors may come from the wider category, but a Japanese form
                // must never appear twice or the quiz would show two right answers.
                ViewBag.QuizPool = items.Concat(distractors)
                    .GroupBy(v => v.Japanese)
                    .Select(group => group.First())
                    .ToList();
                ViewBag.QuizQuestionCount = Math.Min(QuizQuestionLimit, items.Count);
                return View();
            }

            // Lessons without their own items keep using the wider category quiz.
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .Where(q => q.CategoryId == lesson.CategoryId && q.IsPublished)
                .OrderBy(q => q.DisplayOrder)
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                TempData["Message"] = "No quiz available for this lesson yet.";
                return RedirectToAction("Details", "Lessons", new { id = lessonId });
            }

            return RedirectToAction("Take", "Quizzes", new { id = quiz.Id });
        }

        // ===== STRUCTURED GRAMMAR PRACTICE =====
        public async Task<IActionResult> Grammar()
        {
            var userId = _userManager.GetUserId(User)!;
            var links = await _context.LessonGrammarPoints
                .Where(link => link.Lesson!.IsPublished &&
                    !link.Lesson.Prerequisites.Any(prerequisite =>
                        !prerequisite.PrerequisiteLesson!.ProgressRecords.Any(progress =>
                            progress.UserId == userId && progress.FirstMasteryPassedDate.HasValue)))
                .Include(link => link.Lesson)
                .Include(link => link.GrammarPoint)
                .OrderBy(link => link.Lesson!.DisplayOrder)
                .ThenBy(link => link.DisplayOrder)
                .ToListAsync();

            var grammarIds = links.Select(link => link.GrammarPointId).Distinct().ToList();
            var attempts = await _context.PracticeAttempts
                .Where(attempt => attempt.UserId == userId && attempt.GrammarPointId.HasValue &&
                    grammarIds.Contains(attempt.GrammarPointId.Value))
                .ToListAsync();

            // Locked grammar points may safely supply distractor meanings without
            // exposing their lesson content or making them answerable exercises.
            var meaningPool = await _context.GrammarPoints.Where(point => point.Level == "A1" || point.Level == "A2")
                .Select(point => point.Meaning).Distinct().ToListAsync();
            var cards = links.GroupBy(link => link.GrammarPointId).Select(group =>
            {
                var link = group.First();
                var grammar = link.GrammarPoint!;
                var grammarAttempts = attempts.Where(attempt => attempt.GrammarPointId == grammar.Id).ToList();
                var passedTypes = grammarAttempts.Where(attempt => attempt.IsCorrect)
                    .Select(attempt => attempt.ExerciseType).Distinct().Count();
                var accuracy = grammarAttempts.Count == 0 ? 0 :
                    (int)Math.Round(grammarAttempts.Count(attempt => attempt.IsCorrect) * 100d / grammarAttempts.Count);
                // Sorting the whole pool alphabetically gave every card the same three
                // distractors and always parked the right answer in the same slot, so the
                // exercise could be passed by position. Seeding from the grammar id keeps
                // one card's options stable while varying them from card to card.
                var random = new Random(grammar.Id);
                var alternatives = meaningPool.Where(meaning => meaning != grammar.Meaning)
                    .OrderBy(meaning => random.Next())
                    .Take(3);

                return new GrammarPracticeCardViewModel
                {
                    GrammarPoint = grammar,
                    LessonTitle = link.Lesson!.Title,
                    MeaningOptions = alternatives.Append(grammar.Meaning)
                        .OrderBy(meaning => random.Next()).ToList(),
                    IncorrectExample = GrammarIncorrectExamples.GetValueOrDefault(grammar.Code, grammar.CommonMistake ?? string.Empty),
                    SentenceParts = GrammarSentenceParts.GetValueOrDefault(grammar.Code, new[] { grammar.ExampleJapanese }).ToList(),
                    AttemptCount = grammarAttempts.Count,
                    Accuracy = accuracy,
                    IsMastered = HasStableGrammarMastery(grammarAttempts, passedTypes, accuracy)
                };
            }).ToList();

            return View(cards);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordGrammarAttempt(int grammarPointId, string exerciseType, string answer)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();
            if (answer?.Length > 1000) return BadRequest();
            var allowedTypes = new[] { "grammar-meaning", "grammar-correction", "grammar-order" };
            if (!allowedTypes.Contains(exerciseType)) return BadRequest();

            var link = await _context.LessonGrammarPoints
                .Include(item => item.GrammarPoint)
                .Include(item => item.Lesson).ThenInclude(lesson => lesson!.Prerequisites)
                .ThenInclude(prerequisite => prerequisite.PrerequisiteLesson)
                .FirstOrDefaultAsync(item => item.GrammarPointId == grammarPointId && item.Lesson!.IsPublished);
            if (link?.GrammarPoint == null || link.Lesson == null) return BadRequest();

            var prerequisiteIds = link.Lesson.Prerequisites.Select(item => item.PrerequisiteLessonId).ToList();
            var masteredPrerequisiteCount = await _context.LessonProgress.CountAsync(progress =>
                progress.UserId == userId && prerequisiteIds.Contains(progress.LessonId) && progress.FirstMasteryPassedDate.HasValue);
            if (masteredPrerequisiteCount != prerequisiteIds.Count)
                return Forbid();

            var expected = exerciseType == "grammar-meaning"
                ? link.GrammarPoint.Meaning
                : link.GrammarPoint.ExampleJapanese;
            var isCorrect = AnswerEvaluationPolicy.MatchesJapanese(answer, expected);

            _context.PracticeAttempts.Add(new PracticeAttempt
            {
                UserId = userId,
                LessonId = link.LessonId,
                GrammarPointId = grammarPointId,
                ExerciseType = exerciseType,
                IsCorrect = isCorrect
            });
            await _context.SaveChangesAsync();

            var attempts = await _context.PracticeAttempts
                .Where(item => item.UserId == userId && item.GrammarPointId == grammarPointId).ToListAsync();
            var accuracy = (int)Math.Round(attempts.Count(item => item.IsCorrect) * 100d / attempts.Count);
            var passedTypes = attempts.Where(item => item.IsCorrect).Select(item => item.ExerciseType).Distinct().Count();
            return Json(new { success = true, isCorrect, correctAnswer = expected,
                explanation = link.GrammarPoint.UsageNotes, attemptCount = attempts.Count, accuracy,
                mastered = HasStableGrammarMastery(attempts, passedTypes, accuracy) });
        }

        private static bool HasStableGrammarMastery(List<PracticeAttempt> attempts, int passedTypes, int accuracy)
        {
            return AnswerEvaluationPolicy.HasStableGrammarMastery(attempts.Select(attempt =>
                (attempt.ExerciseType, attempt.IsCorrect, attempt.AttemptedAt)));
        }

        private static readonly Dictionary<string, string> GrammarIncorrectExamples = new()
        {
            ["a1-greetings"] = "夜、先生に『おはようございます』と言います。", ["a1-topic-wa"] = "わたしをリーです。",
            ["a1-possession-no"] = "これはわたし母です。", ["a1-na-adjective"] = "マリアさんはしずかい人です。",
            ["a1-kore-kono"] = "このは何ですか。", ["a1-number-desu"] = "わたしは二十歳があります。",
            ["a1-time-ni"] = "七時をおきます。", ["a1-destination-ni"] = "学校を行きます。",
            ["a1-arimasu-imasu"] = "教室に先生があります。", ["a1-adjective-link"] = "この町はしずかくてきれいです。",
            ["a1-suki-ga"] = "音楽を好きです。", ["a1-permission"] = "写真をとるてもいいですか。",
            ["a1-order-onegaishimasu"] = "ラーメンが一つお願いします。", ["a1-availability-wa"] = "赤をありますか。",
            ["a1-means-de"] = "電車に行きます。", ["a1-invitation-masenka"] = "いっしょに映画を見ますか。",
            ["a1-polite-past"] = "きのう公園に行きます。", ["a1-clarification"] = "もう一つゆっくりお願いします。",
            ["a2-te-sequence"] = "顔を洗うて、朝ごはんを食べます。", ["a2-experience-koto"] = "京都に行くことがあります。",
            ["a2-plan-intention"] = "来月日本へ行く予定します。", ["a2-comparison"] = "バスより電車は速いのほうです。",
            ["a2-reason-ndesu"] = "熱があったですから。", ["a2-potential"] = "日本語を話すできます。",
            ["a2-plain-forms"] = "明日映画に行くですか。", ["a2-shimau-request"] = "かばんを電車に忘れてしまうました。",
            ["a2-giving-receiving"] = "友だちに手伝ってくれました。", ["a2-tara"] = "時間があるたら、勉強します。",
            ["a2-relative-clause"] = "これはきのう買いました本です。", ["a2-obligation-advice"] = "今日は休んでほうがいいです。",
            ["a2-change-decision"] = "漢字が読めるようになりしました。", ["a2-report-opinion"] = "田中さんは電車が遅れるを言っていました。",
            ["a2-integrated-repair"] = "漢字が読めません、ゆっくり説明してください。"
        };

        private static readonly Dictionary<string, string[]> GrammarSentenceParts = new()
        {
            ["a1-greetings"] = new[] { "こんばんは、", "田中先生。" }, ["a1-topic-wa"] = new[] { "わたしは", "リー", "です。" },
            ["a1-possession-no"] = new[] { "これは", "わたしの", "母", "です。" }, ["a1-na-adjective"] = new[] { "マリアさんは", "しずかな", "人", "です。" },
            ["a1-kore-kono"] = new[] { "これは", "何", "ですか。" }, ["a1-number-desu"] = new[] { "二十歳", "です。" },
            ["a1-time-ni"] = new[] { "七時に", "おきます。" }, ["a1-destination-ni"] = new[] { "学校に", "行きます。" },
            ["a1-arimasu-imasu"] = new[] { "教室に", "先生が", "います。" }, ["a1-adjective-link"] = new[] { "この町は", "しずかで", "きれいです。" },
            ["a1-suki-ga"] = new[] { "音楽が", "好きです。" }, ["a1-permission"] = new[] { "写真を", "とっても", "いいですか。" },
            ["a1-order-onegaishimasu"] = new[] { "ラーメンを", "一つ", "お願いします。" }, ["a1-availability-wa"] = new[] { "赤は", "ありますか。" },
            ["a1-means-de"] = new[] { "電車で", "行きます。" }, ["a1-invitation-masenka"] = new[] { "いっしょに", "映画を", "見ませんか。" },
            ["a1-polite-past"] = new[] { "きのう", "公園に", "行きました。" }, ["a1-clarification"] = new[] { "もう一度", "ゆっくり", "お願いします。" },
            ["a2-te-sequence"] = new[] { "朝起きて、", "顔を洗って、", "朝ごはんを食べます。" },
            ["a2-experience-koto"] = new[] { "京都に", "行ったことが", "あります。" }, ["a2-plan-intention"] = new[] { "来月", "日本へ行く", "予定です。" },
            ["a2-comparison"] = new[] { "バスより", "電車のほうが", "速いです。" }, ["a2-reason-ndesu"] = new[] { "熱があった", "からです。" },
            ["a2-potential"] = new[] { "日本語が", "少し", "話せます。" }, ["a2-plain-forms"] = new[] { "明日", "映画に", "行く？" },
            ["a2-shimau-request"] = new[] { "かばんを", "電車に", "忘れてしまいました。" }, ["a2-giving-receiving"] = new[] { "友だちが", "手伝って", "くれました。" },
            ["a2-tara"] = new[] { "時間があったら、", "勉強します。" }, ["a2-relative-clause"] = new[] { "これは", "きのう買った", "本です。" },
            ["a2-obligation-advice"] = new[] { "今日は", "休んだほうが", "いいです。" }, ["a2-change-decision"] = new[] { "漢字が", "読めるように", "なりました。" },
            ["a2-report-opinion"] = new[] { "田中さんは", "電車が遅れると", "言っていました。" }, ["a2-integrated-repair"] = new[] { "漢字が読めませんから、", "ゆっくり", "説明してください。" }
        };

        // ===== PRACTICE BY SKILL =====
        public IActionResult Listening() => View("Skill", new SkillViewModel
        {
            SkillKey = "listening",
            Title = "Listening",
            JapaneseTitle = "リスニング",
            Description = "Train your ears with everyday Japanese.",
            Icon = "🎧",
            Modes = new List<SkillMode>
            {
                new() { Key = "listen-choose", Title = "Listen & Choose", Description = "Listen and pick the correct answer", Icon = "🎧" },
                new() { Key = "dictation", Title = "Dictation", Description = "Type what you hear", Icon = "✏️" },
                new() { Key = "conversation", Title = "Conversation Listening", Description = "Understand short dialogues", Icon = "💬" },
                new() { Key = "speed-challenge", Title = "Natural Speed Challenge", Description = "Train with faster audio", Icon = "⚡" }
            }
        });

        public IActionResult Reading() => View("Skill", new SkillViewModel
        {
            SkillKey = "reading",
            Title = "Reading",
            JapaneseTitle = "リーディング",
            Description = "Read and understand Japanese text.",
            Icon = "📖",
            Modes = new List<SkillMode>
            {
                new() { Key = "reading-comprehension", Title = "Reading Comprehension", Description = "Read passages and answer questions", Icon = "📖" },
                new() { Key = "read-aloud", Title = "Read Aloud", Description = "Practice pronunciation by reading aloud", Icon = "🎙" },
                new() { Key = "short-stories", Title = "Short Stories", Description = "Graded reading passages", Icon = "📰" },
                new() { Key = "speed-reading", Title = "Speed Reading", Description = "Timed reading practice", Icon = "⚡" }
            }
        });

        public IActionResult Speaking() => View("Skill", new SkillViewModel
        {
            SkillKey = "speaking",
            Title = "Speaking",
            JapaneseTitle = "スピーキング",
            Description = "Express yourself in Japanese.",
            Icon = "🎤",
            Modes = new List<SkillMode>
            {
                new() { Key = "free-conversation", Title = "Free Conversation", Description = "Chat naturally with AI", Icon = "💬" },
                new() { Key = "role-play", Title = "Role Play", Description = "Practice real-life situations", Icon = "🎭" },
                new() { Key = "topic-challenge", Title = "Topic Challenge", Description = "Speak on a given topic", Icon = "🎤" },
                new() { Key = "pronunciation", Title = "Pronunciation Practice", Description = "Listen, repeat, compare", Icon = "🗣" }
            }
        });

        public IActionResult Writing() => View("Skill", new SkillViewModel
        {
            SkillKey = "writing",
            Title = "Writing",
            JapaneseTitle = "ライティング",
            Description = "Produce and organise Japanese.",
            Icon = "✍️",
            Modes = new List<SkillMode>
            {
                new() { Key = "handwriting", Title = "Handwriting", Description = "Trace and recall kana and kanji forms", Icon = "✍️" },
                new() { Key = "sentence-writing", Title = "Sentence Writing", Description = "Translate and write sentences", Icon = "📝" },
                new() { Key = "guided-writing", Title = "Guided Writing", Description = "Write with vocabulary & grammar support", Icon = "📄" },
                new() { Key = "free-writing", Title = "Free Writing", Description = "Write freely on a prompt", Icon = "🌸" }
            }
        });

        // ===== UNIFIED SKILL ROUTES =====
        // One dynamic library/workspace route keeps four skills inside two logical
        // pages while the specialised views remain reusable presentation partials.
        public async Task<IActionResult> Library(string skill, string? level = null, string? type = null)
        {
            return skill?.ToLowerInvariant() switch
            {
                "listening" => await ListeningLibrary(level, type),
                "reading" => await ReadingLibrary(level),
                "speaking" => await SpeakingLibrary(level, type),
                "writing" => await WritingLibrary(level, type),
                _ => RedirectToAction(nameof(Index))
            };
        }

        public async Task<IActionResult> Workspace(string skill, int id)
        {
            return skill?.ToLowerInvariant() switch
            {
                "listening" => await ListeningWorkspace(id),
                "reading" => await ReadingPassage(id),
                "speaking" => await SpeakingWorkspace(id),
                "writing" => await WritingWorkspace(id),
                _ => RedirectToAction(nameof(Index))
            };
        }

        // ===== GRADED READING =====
        public async Task<IActionResult> ReadingLibrary(string? level = null)
        {
            var userId = _userManager.GetUserId(User)!;
            level = string.IsNullOrWhiteSpace(level) ? null : level.ToUpperInvariant();
            var passages = await _context.ReadingPassages
                .Include(passage => passage.Lesson).ThenInclude(lesson => lesson!.Prerequisites)
                .Include(passage => passage.Questions).Include(passage => passage.Vocabulary)
                .OrderBy(passage => passage.DisplayOrder).ToListAsync();
            if (!string.IsNullOrWhiteSpace(level)) passages = passages.Where(passage => passage.Level == level.ToUpperInvariant()).ToList();

            var stableLessons = await _context.LessonProgress.Where(progress => progress.UserId == userId && progress.FirstMasteryPassedDate.HasValue)
                .Select(progress => progress.LessonId).ToListAsync();
            var knownVocabulary = await _context.LessonVocabulary.Where(link =>
                link.Lesson!.ProgressRecords.Any(progress => progress.UserId == userId && progress.FirstMasteryPassedDate.HasValue))
                .Select(link => link.VocabularyId).Distinct().ToListAsync();
            var passageIds = passages.Select(passage => passage.Id).ToList();
            var attempts = await _context.ReadingAttempts.Where(attempt => attempt.UserId == userId && passageIds.Contains(attempt.ReadingPassageId)).ToListAsync();

            var model = passages.Select(passage =>
            {
                var passageAttempts = attempts.Where(attempt => attempt.ReadingPassageId == passage.Id).ToList();
                var accuracy = passageAttempts.Count == 0 ? 0 : (int)Math.Round(passageAttempts.Count(a => a.IsCorrect) * 100d / passageAttempts.Count);
                var targetIds = passage.Vocabulary.Select(link => link.VocabularyId).ToList();
                var unknown = targetIds.Count == 0 ? 0 : (int)Math.Round(targetIds.Count(id => !knownVocabulary.Contains(id)) * 100d / targetIds.Count);
                var prerequisiteIds = passage.Lesson?.Prerequisites.Select(item => item.PrerequisiteLessonId).ToList() ?? new();
                return new ReadingLibraryItemViewModel { Passage = passage, Accuracy = accuracy, UnknownPercentage = unknown,
                    IsUnlocked = prerequisiteIds.All(stableLessons.Contains), IsMastered = HasStableReadingMastery(passageAttempts, passage.Questions.Select(q => q.Id)) };
            }).ToList();
            ViewBag.Level = level;
            return View(nameof(ReadingLibrary), model);
        }

        public async Task<IActionResult> ReadingPassage(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var passage = await _context.ReadingPassages.Include(p => p.Lesson).ThenInclude(l => l!.Prerequisites)
                .Include(p => p.GrammarPoint).Include(p => p.Questions).Include(p => p.Vocabulary).ThenInclude(link => link.Vocabulary)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (passage == null) return NotFound();
            var stableLessons = await _context.LessonProgress.Where(p => p.UserId == userId && p.FirstMasteryPassedDate.HasValue).Select(p => p.LessonId).ToListAsync();
            if (passage.Lesson?.Prerequisites.Any(p => !stableLessons.Contains(p.PrerequisiteLessonId)) == true)
                return RedirectToAction(nameof(ReadingLibrary));

            var knownVocabulary = await _context.LessonVocabulary.Where(link => link.Lesson!.ProgressRecords.Any(p => p.UserId == userId && p.FirstMasteryPassedDate.HasValue))
                .Select(link => link.VocabularyId).Distinct().ToListAsync();
            var targetIds = passage.Vocabulary.Select(link => link.VocabularyId).ToList();
            var attempts = await _context.ReadingAttempts.Where(a => a.UserId == userId && a.ReadingPassageId == id).ToListAsync();
            var accuracy = attempts.Count == 0 ? 0 : (int)Math.Round(attempts.Count(a => a.IsCorrect) * 100d / attempts.Count);
            return View(nameof(ReadingPassage), new ReadingPassageViewModel { Passage = passage, Accuracy = accuracy,
                UnknownPercentage = targetIds.Count == 0 ? 0 : (int)Math.Round(targetIds.Count(wordId => !knownVocabulary.Contains(wordId)) * 100d / targetIds.Count),
                IsMastered = HasStableReadingMastery(attempts, passage.Questions.Select(q => q.Id)),
                Notes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(passage.SentenceNotesJson ?? "[]") ?? new(),
                Glossary = System.Text.Json.JsonSerializer.Deserialize<List<string>>(passage.GlossaryJson ?? "[]") ?? new() });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordReadingAttempt(int passageId, int questionId, string answer)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();
            if (answer?.Length > 1000) return BadRequest();
            var question = await _context.ReadingQuestions.Include(q => q.ReadingPassage).ThenInclude(p => p!.Lesson)
                .ThenInclude(l => l!.Prerequisites).SingleOrDefaultAsync(q => q.Id == questionId && q.ReadingPassageId == passageId);
            if (question?.ReadingPassage == null) return BadRequest();
            var prerequisiteIds = question.ReadingPassage.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId).ToList() ?? new();
            var masteredPrerequisites = await _context.LessonProgress.CountAsync(p => p.UserId == userId && prerequisiteIds.Contains(p.LessonId) && p.FirstMasteryPassedDate.HasValue);
            if (masteredPrerequisites != prerequisiteIds.Count) return Forbid();

            var acceptedAnswers = question.CorrectAnswer.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var isCorrect = AnswerEvaluationPolicy.MatchesAny(answer, acceptedAnswers);
            _context.ReadingAttempts.Add(new ReadingAttempt { UserId = userId, ReadingPassageId = passageId,
                ReadingQuestionId = questionId, Answer = answer, IsCorrect = isCorrect });
            await _context.SaveChangesAsync();
            var attempts = await _context.ReadingAttempts.Where(a => a.UserId == userId && a.ReadingPassageId == passageId).ToListAsync();
            var questionIds = await _context.ReadingQuestions.Where(q => q.ReadingPassageId == passageId).Select(q => q.Id).ToListAsync();
            var accuracy = (int)Math.Round(attempts.Count(a => a.IsCorrect) * 100d / attempts.Count);
            return Json(new { success = true, isCorrect, correctAnswer = acceptedAnswers.First(), question.Explanation,
                accuracy, mastered = HasStableReadingMastery(attempts, questionIds) });
        }

        private static bool HasStableReadingMastery(IEnumerable<ReadingAttempt> source, IEnumerable<int> requiredQuestionIds)
            => IntegratedSkillMasteryPolicy.HasStableQuestionMastery(
                source.Select(a => new QuestionAttemptData(a.ReadingQuestionId, a.IsCorrect, a.AttemptedAt)), requiredQuestionIds);

        // ===== STRUCTURED WRITING =====
        public async Task<IActionResult> WritingLibrary(string? level = null, string? type = null)
        {
            var userId = _userManager.GetUserId(User)!;
            level = string.IsNullOrWhiteSpace(level) ? null : level.ToUpperInvariant();
            var tasks = await _context.WritingTasks.Include(task => task.Lesson).ThenInclude(lesson => lesson!.Prerequisites)
                .OrderBy(task => task.DisplayOrder).ToListAsync();
            if (level != null) tasks = tasks.Where(task => task.Level == level).ToList();
            if (!string.IsNullOrWhiteSpace(type)) tasks = tasks.Where(task => task.TaskType == type).ToList();
            var stableLessons = await _context.LessonProgress.Where(p => p.UserId == userId && p.FirstMasteryPassedDate.HasValue).Select(p => p.LessonId).ToListAsync();
            var taskIds = tasks.Select(task => task.Id).ToList();
            var submissions = await _context.WritingSubmissions.Where(s => s.UserId == userId && taskIds.Contains(s.WritingTaskId)).ToListAsync();
            var model = tasks.Select(task =>
            {
                var history = submissions.Where(s => s.WritingTaskId == task.Id).ToList();
                var prerequisiteIds = task.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId) ?? Enumerable.Empty<int>();
                return new WritingLibraryItemViewModel { Task = task, IsUnlocked = prerequisiteIds.All(stableLessons.Contains),
                    SubmissionCount = history.Count, BestScore = history.Count == 0 ? 0 : history.Max(s => s.Score), IsMastered = HasStableWritingMastery(history) };
            }).ToList();
            ViewBag.Level = level; ViewBag.Type = type;
            return View(nameof(WritingLibrary), model);
        }

        public async Task<IActionResult> WritingWorkspace(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var task = await _context.WritingTasks.Include(t => t.Lesson).ThenInclude(l => l!.Prerequisites).Include(t => t.GrammarPoint).SingleOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            var prerequisiteIds = task.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId).ToList() ?? new();
            var masteredCount = await _context.LessonProgress.CountAsync(p => p.UserId == userId && prerequisiteIds.Contains(p.LessonId) && p.FirstMasteryPassedDate.HasValue);
            if (masteredCount != prerequisiteIds.Count) return RedirectToAction(nameof(WritingLibrary));
            var history = await _context.WritingSubmissions.Where(s => s.UserId == userId && s.WritingTaskId == id).OrderByDescending(s => s.SubmittedAt).ToListAsync();
            return View(nameof(WritingWorkspace), new WritingWorkspaceViewModel { Task = task, History = history, IsMastered = HasStableWritingMastery(history),
                Steps = DeserializeList(task.StepsJson), Requirements = DeserializeList(task.RequirementsJson), Checklist = DeserializeList(task.ChecklistJson) });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWritingSubmission(int taskId, string content)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();
            if (string.IsNullOrWhiteSpace(content) || content.Length > 4000) return BadRequest();
            var task = await _context.WritingTasks.Include(t => t.Lesson).ThenInclude(l => l!.Prerequisites).SingleOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return BadRequest();
            var prerequisiteIds = task.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId).ToList() ?? new();
            if (await _context.LessonProgress.CountAsync(p => p.UserId == userId && prerequisiteIds.Contains(p.LessonId) && p.FirstMasteryPassedDate.HasValue) != prerequisiteIds.Count) return Forbid();

            var evaluation = EvaluateWriting(task, content);
            var previousId = await _context.WritingSubmissions.Where(s => s.UserId == userId && s.WritingTaskId == taskId)
                .OrderByDescending(s => s.SubmittedAt).Select(s => (int?)s.Id).FirstOrDefaultAsync();
            var submission = new WritingSubmission { UserId = userId, WritingTaskId = taskId, PreviousSubmissionId = previousId,
                Content = content.Trim(), Score = evaluation.Score, IsPassing = evaluation.Score >= 80,
                FeedbackJson = System.Text.Json.JsonSerializer.Serialize(evaluation.Feedback) };
            _context.WritingSubmissions.Add(submission); await _context.SaveChangesAsync();
            var history = await _context.WritingSubmissions.Where(s => s.UserId == userId && s.WritingTaskId == taskId).ToListAsync();
            return Json(new { success = true, submission.Id, evaluation.Score, passing = submission.IsPassing,
                evaluation.JapaneseCharacters, evaluation.SentenceCount, evaluation.RequirementsMet, evaluation.RequirementCount,
                evaluation.Feedback, mastered = HasStableWritingMastery(history), submittedAt = submission.SubmittedAt.ToString("g") });
        }

        private static (int Score, int JapaneseCharacters, int SentenceCount, int RequirementsMet, int RequirementCount, List<string> Feedback) EvaluateWriting(WritingTask task, string content)
        {
            var japaneseCharacters = content.Count(c => c is >= '\u3040' and <= '\u30ff' or >= '\u3400' and <= '\u9fff');
            var sentenceCount = content.Split(new[] { '。', '！', '？', '\n' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length;
            var requirements = DeserializeList(task.RequirementsJson); var met = 0; var feedback = new List<string>();
            foreach (var requirement in requirements)
            {
                var parts = requirement.Split('|', 2); var alternatives = parts.Length > 1 ? parts[1].Split('/', StringSplitOptions.TrimEntries) : Array.Empty<string>();
                var found = alternatives.Any(content.Contains); if (found) met++; else feedback.Add($"Add: {parts[0]}");
            }
            var characterScore = Math.Min(25, (int)Math.Round(japaneseCharacters * 25d / Math.Max(1, task.MinimumJapaneseCharacters)));
            var sentenceScore = Math.Min(25, (int)Math.Round(sentenceCount * 25d / Math.Max(1, task.MinimumSentences)));
            var requirementScore = requirements.Count == 0 ? 50 : (int)Math.Round(met * 50d / requirements.Count);
            if (japaneseCharacters < task.MinimumJapaneseCharacters) feedback.Add($"Write at least {task.MinimumJapaneseCharacters} Japanese characters ({japaneseCharacters} now).");
            if (sentenceCount < task.MinimumSentences) feedback.Add($"Write at least {task.MinimumSentences} sentences ({sentenceCount} now).");
            if (feedback.Count == 0) feedback.Add("All automated structure checks passed. Use the self-checklist for meaning and naturalness.");
            return (characterScore + sentenceScore + requirementScore, japaneseCharacters, sentenceCount, met, requirements.Count, feedback);
        }

        private static List<string> DeserializeList(string? json) =>
            System.Text.Json.JsonSerializer.Deserialize<List<string>>(json ?? "[]") ?? new();

        private static bool HasStableWritingMastery(IEnumerable<WritingSubmission> source)
            => IntegratedSkillMasteryPolicy.HasStableOutputMastery(
                source.Select(s => new OutputAttemptData(s.IsPassing, s.Score, s.SubmittedAt)));

        // ===== STRUCTURED SPEAKING =====
        public async Task<IActionResult> SpeakingLibrary(string? level = null, string? type = null)
        {
            var userId = _userManager.GetUserId(User)!;
            if (!await _context.SpeakingTasks.AnyAsync())
                await DataSeeder.SeedSpeakingCurriculumAsync(_context);
            level = string.IsNullOrWhiteSpace(level) ? null : level.ToUpperInvariant();
            var tasks = await _context.SpeakingTasks.Include(task => task.Lesson).ThenInclude(lesson => lesson!.Prerequisites)
                .OrderBy(task => task.DisplayOrder).ToListAsync();
            if (level != null) tasks = tasks.Where(task => task.Level == level).ToList();
            if (!string.IsNullOrWhiteSpace(type)) tasks = tasks.Where(task => task.TaskType == type).ToList();
            var stableLessons = await _context.LessonProgress.Where(p => p.UserId == userId && p.FirstMasteryPassedDate.HasValue).Select(p => p.LessonId).ToListAsync();
            var taskIds = tasks.Select(task => task.Id).ToList();
            var attempts = await _context.SpeakingAttempts.Where(a => a.UserId == userId && taskIds.Contains(a.SpeakingTaskId)).ToListAsync();
            var model = tasks.Select(task =>
            {
                var history = attempts.Where(a => a.SpeakingTaskId == task.Id).ToList();
                var prerequisiteIds = task.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId) ?? Enumerable.Empty<int>();
                return new SpeakingLibraryItemViewModel { Task = task, IsUnlocked = prerequisiteIds.All(stableLessons.Contains),
                    AttemptCount = history.Count, BestScore = history.Count == 0 ? 0 : history.Max(a => a.Score), IsMastered = HasStableSpeakingMastery(history) };
            }).ToList();
            ViewBag.Level = level; ViewBag.Type = type;
            return View(nameof(SpeakingLibrary), model);
        }

        public async Task<IActionResult> SpeakingWorkspace(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var task = await _context.SpeakingTasks.Include(t => t.Lesson).ThenInclude(l => l!.Prerequisites).SingleOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            var prerequisiteIds = task.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId).ToList() ?? new();
            var masteredCount = await _context.LessonProgress.CountAsync(p => p.UserId == userId && prerequisiteIds.Contains(p.LessonId) && p.FirstMasteryPassedDate.HasValue);
            if (masteredCount != prerequisiteIds.Count) return RedirectToAction(nameof(SpeakingLibrary));
            var history = await _context.SpeakingAttempts.Where(a => a.UserId == userId && a.SpeakingTaskId == id).OrderByDescending(a => a.AttemptedAt).ToListAsync();
            return View(nameof(SpeakingWorkspace), new SpeakingWorkspaceViewModel { Task = task, History = history, IsMastered = HasStableSpeakingMastery(history),
                Steps = DeserializeList(task.ResponseStepsJson), Requirements = DeserializeList(task.RequirementsJson), Checklist = DeserializeList(task.ChecklistJson) });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSpeakingAttempt(int taskId, string transcript, string inputMode = "typed")
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();
            if (string.IsNullOrWhiteSpace(transcript) || transcript.Length > 4000) return BadRequest();
            inputMode = inputMode.Equals("voice", StringComparison.OrdinalIgnoreCase) ? "voice" : "typed";
            var task = await _context.SpeakingTasks.Include(t => t.Lesson).ThenInclude(l => l!.Prerequisites).SingleOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return BadRequest();
            var prerequisiteIds = task.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId).ToList() ?? new();
            if (await _context.LessonProgress.CountAsync(p => p.UserId == userId && prerequisiteIds.Contains(p.LessonId) && p.FirstMasteryPassedDate.HasValue) != prerequisiteIds.Count) return Forbid();

            var evaluation = EvaluateSpeaking(task, transcript);
            var attempt = new SpeakingAttempt { UserId = userId, SpeakingTaskId = taskId, Transcript = transcript.Trim(), InputMode = inputMode,
                Score = evaluation.Score, IsPassing = evaluation.Score >= 80, FeedbackJson = System.Text.Json.JsonSerializer.Serialize(evaluation.Feedback) };
            _context.SpeakingAttempts.Add(attempt); await _context.SaveChangesAsync();
            var history = await _context.SpeakingAttempts.Where(a => a.UserId == userId && a.SpeakingTaskId == taskId).ToListAsync();
            return Json(new { success = true, attempt.Id, evaluation.Score, passing = attempt.IsPassing, evaluation.JapaneseCharacters,
                evaluation.RequirementsMet, evaluation.RequirementCount, evaluation.Feedback, mastered = HasStableSpeakingMastery(history),
                attemptedAt = attempt.AttemptedAt.ToString("g") });
        }

        private static (int Score, int JapaneseCharacters, int RequirementsMet, int RequirementCount, List<string> Feedback) EvaluateSpeaking(SpeakingTask task, string transcript)
        {
            var japaneseCharacters = transcript.Count(c => c is >= '\u3040' and <= '\u30ff' or >= '\u3400' and <= '\u9fff');
            var requirements = DeserializeList(task.RequirementsJson); var met = 0; var feedback = new List<string>();
            foreach (var requirement in requirements)
            {
                var parts = requirement.Split('|', 2); var alternatives = parts.Length > 1 ? parts[1].Split('/', StringSplitOptions.TrimEntries) : Array.Empty<string>();
                if (alternatives.Any(transcript.Contains)) met++; else feedback.Add($"Try adding: {parts[0]}");
            }
            var lengthScore = Math.Min(35, (int)Math.Round(japaneseCharacters * 35d / Math.Max(1, task.MinimumJapaneseCharacters)));
            var requirementScore = requirements.Count == 0 ? 65 : (int)Math.Round(met * 65d / requirements.Count);
            if (japaneseCharacters < task.MinimumJapaneseCharacters) feedback.Add($"Respond with at least {task.MinimumJapaneseCharacters} Japanese characters ({japaneseCharacters} now).");
            if (feedback.Count == 0) feedback.Add("All transcript checks passed. Use the self-checklist to judge pace, clarity and pronunciation.");
            return (lengthScore + requirementScore, japaneseCharacters, met, requirements.Count, feedback);
        }

        private static bool HasStableSpeakingMastery(IEnumerable<SpeakingAttempt> source)
            => IntegratedSkillMasteryPolicy.HasStableOutputMastery(
                source.Select(a => new OutputAttemptData(a.IsPassing, a.Score, a.AttemptedAt)));

        // ===== STRUCTURED LISTENING =====
        public async Task<IActionResult> ListeningLibrary(string? level = null, string? type = null)
        {
            var userId = _userManager.GetUserId(User)!;
            if (!await _context.ListeningTasks.AnyAsync()) await DataSeeder.SeedListeningCurriculumAsync(_context);
            level = string.IsNullOrWhiteSpace(level) ? null : level.ToUpperInvariant();
            var tasks = await _context.ListeningTasks.Include(t => t.Lesson).ThenInclude(l => l!.Prerequisites).Include(t => t.Questions)
                .OrderBy(t => t.DisplayOrder).ToListAsync();
            if (level != null) tasks = tasks.Where(t => t.Level == level).ToList();
            if (!string.IsNullOrWhiteSpace(type)) tasks = tasks.Where(t => t.TaskType == type).ToList();
            var stableLessons = await _context.LessonProgress.Where(p => p.UserId == userId && p.FirstMasteryPassedDate.HasValue).Select(p => p.LessonId).ToListAsync();
            var taskIds = tasks.Select(t => t.Id).ToList();
            var attempts = await _context.ListeningAttempts.Where(a => a.UserId == userId && taskIds.Contains(a.ListeningTaskId)).ToListAsync();
            var model = tasks.Select(task =>
            {
                var history = attempts.Where(a => a.ListeningTaskId == task.Id).ToList();
                var accuracy = history.Count == 0 ? 0 : (int)Math.Round(history.Count(a => a.IsCorrect) * 100d / history.Count);
                var prerequisiteIds = task.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId) ?? Enumerable.Empty<int>();
                return new ListeningLibraryItemViewModel { Task = task, Accuracy = accuracy, AttemptCount = history.Count,
                    IsUnlocked = prerequisiteIds.All(stableLessons.Contains), IsMastered = HasStableListeningMastery(history, task.Questions.Select(q => q.Id), task.Level == "A2") };
            }).ToList();
            ViewBag.Level = level; ViewBag.Type = type;
            return View(nameof(ListeningLibrary), model);
        }

        public async Task<IActionResult> ListeningWorkspace(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var task = await _context.ListeningTasks.Include(t => t.Lesson).ThenInclude(l => l!.Prerequisites).Include(t => t.Questions)
                .SingleOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            var prerequisiteIds = task.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId).ToList() ?? new();
            if (await _context.LessonProgress.CountAsync(p => p.UserId == userId && prerequisiteIds.Contains(p.LessonId) && p.FirstMasteryPassedDate.HasValue) != prerequisiteIds.Count)
                return RedirectToAction(nameof(ListeningLibrary));
            var history = await _context.ListeningAttempts.Where(a => a.UserId == userId && a.ListeningTaskId == id).OrderByDescending(a => a.AttemptedAt).ToListAsync();
            var accuracy = history.Count == 0 ? 0 : (int)Math.Round(history.Count(a => a.IsCorrect) * 100d / history.Count);
            return View(nameof(ListeningWorkspace), new ListeningWorkspaceViewModel { Task = task, Tips = DeserializeList(task.ListeningTipsJson), History = history,
                Accuracy = accuracy, IsMastered = HasStableListeningMastery(history, task.Questions.Select(q => q.Id), task.Level == "A2") });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordListeningAttempt(int taskId, int questionId, string answer, string playbackMode = "natural")
        {
            var userId = _userManager.GetUserId(User); if (userId == null) return Unauthorized();
            if (answer?.Length > 1000) return BadRequest();
            var question = await _context.ListeningQuestions.Include(q => q.ListeningTask).ThenInclude(t => t!.Lesson).ThenInclude(l => l!.Prerequisites)
                .SingleOrDefaultAsync(q => q.Id == questionId && q.ListeningTaskId == taskId);
            if (question?.ListeningTask == null) return BadRequest();
            var prerequisiteIds = question.ListeningTask.Lesson?.Prerequisites.Select(p => p.PrerequisiteLessonId).ToList() ?? new();
            if (await _context.LessonProgress.CountAsync(p => p.UserId == userId && prerequisiteIds.Contains(p.LessonId) && p.FirstMasteryPassedDate.HasValue) != prerequisiteIds.Count) return Forbid();
            var accepted = question.CorrectAnswer.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var isCorrect = AnswerEvaluationPolicy.MatchesAny(answer, accepted);
            playbackMode = playbackMode.Equals("slow", StringComparison.OrdinalIgnoreCase) ? "slow" : "natural";
            _context.ListeningAttempts.Add(new ListeningAttempt { UserId = userId, ListeningTaskId = taskId, ListeningQuestionId = questionId,
                Answer = answer, PlaybackMode = playbackMode, IsCorrect = isCorrect });
            await _context.SaveChangesAsync();
            var history = await _context.ListeningAttempts.Where(a => a.UserId == userId && a.ListeningTaskId == taskId).ToListAsync();
            var questionIds = await _context.ListeningQuestions.Where(q => q.ListeningTaskId == taskId).Select(q => q.Id).ToListAsync();
            var accuracy = (int)Math.Round(history.Count(a => a.IsCorrect) * 100d / history.Count);
            return Json(new { success=true, isCorrect, correctAnswer=accepted.First(), question.Explanation, accuracy,
                mastered=HasStableListeningMastery(history, questionIds, question.ListeningTask.Level == "A2") });
        }

        private static bool HasStableListeningMastery(IEnumerable<ListeningAttempt> source, IEnumerable<int> requiredQuestionIds, bool requireBothPlaybackModes = false)
            => IntegratedSkillMasteryPolicy.HasStableQuestionMastery(
                source.Select(a => new QuestionAttemptData(a.ListeningQuestionId, a.IsCorrect, a.AttemptedAt, a.PlaybackMode)),
                requiredQuestionIds, requireBothPlaybackModes);

        public async Task<IActionResult> Capstone(string level = "A1")
            => level.Equals("A2", StringComparison.OrdinalIgnoreCase)
                ? await A2Capstone()
                : await A1Capstone();

        // ===== A1 INTEGRATED CAPSTONE =====
        public async Task<IActionResult> A1Capstone()
        {
            var userId = _userManager.GetUserId(User)!;
            var a1LessonIds = await _context.Lessons.Where(l => l.IsPublished && l.Category!.Name == "A1 Course" && l.Title.StartsWith("A1:"))
                .Select(l => l.Id).ToListAsync();
            var stableLessonCount = await _context.LessonProgress.CountAsync(p => p.UserId == userId && p.StableMasteryDate != null && a1LessonIds.Contains(p.LessonId));
            var bestFinalQuiz = await _context.QuizAttempts.Where(a => a.UserId == userId && a.Quiz!.Title == "A1 Final Can-do Assessment")
                .Select(a => (int?)a.ScorePercentage).MaxAsync() ?? 0;

            var listeningTasks = await _context.ListeningTasks.Where(t => t.Level == "A1").Include(t => t.Questions).ToListAsync();
            var listeningTaskIds = listeningTasks.Select(t => t.Id).ToList();
            var listening = await _context.ListeningAttempts.Where(a => a.UserId == userId && listeningTaskIds.Contains(a.ListeningTaskId)).ToListAsync();
            var masteredListening = listeningTasks.Count(t => HasStableListeningMastery(listening.Where(a => a.ListeningTaskId == t.Id), t.Questions.Select(q => q.Id)));

            var passages = await _context.ReadingPassages.Where(p => p.Level == "A1").Include(p => p.Questions).ToListAsync();
            var passageIds = passages.Select(p => p.Id).ToList();
            var readingAttempts = await _context.ReadingAttempts.Where(a => a.UserId == userId && passageIds.Contains(a.ReadingPassageId)).ToListAsync();
            var masteredReading = passages.Count(p => HasStableReadingMastery(readingAttempts.Where(a => a.ReadingPassageId == p.Id), p.Questions.Select(q => q.Id)));

            var writingTasks = await _context.WritingTasks.Where(t => t.Level == "A1").Select(t => t.Id).ToListAsync();
            var writingAttempts = await _context.WritingSubmissions.Where(s => s.UserId == userId && writingTasks.Contains(s.WritingTaskId)).ToListAsync();
            var masteredWriting = writingTasks.Count(id => HasStableWritingMastery(writingAttempts.Where(s => s.WritingTaskId == id)));

            var speakingTasks = await _context.SpeakingTasks.Where(t => t.Level == "A1").Select(t => t.Id).ToListAsync();
            var speakingAttempts = await _context.SpeakingAttempts.Where(a => a.UserId == userId && speakingTasks.Contains(a.SpeakingTaskId)).ToListAsync();
            var masteredSpeaking = speakingTasks.Count(id => HasStableSpeakingMastery(speakingAttempts.Where(a => a.SpeakingTaskId == id)));

            var model = new A1CapstoneViewModel
            {
                LearnerName = User.Identity?.Name ?? "NihongoAcademy learner",
                GeneratedAt = _clock.Now,
                StableLessonCount = stableLessonCount,
                BestFinalQuizScore = bestFinalQuiz,
                // Derived from the curriculum rather than a hardcoded lesson count, which
                // would silently make the capstone unreachable after any content change.
                FoundationComplete = a1LessonIds.Count > 0 && stableLessonCount == a1LessonIds.Count && bestFinalQuiz >= 80,
                Skills = new()
                {
                    new() { Key="listening", Title="Listening", JapaneseTitle="聞く", Icon="🎧", Requirement="Reach stable mastery in at least two A1 listening tasks.", ProgressText=$"{masteredListening}/2 listening tasks mastered", ActionName=nameof(ListeningLibrary), ActionLabel="Open Listening Studio", IsComplete=masteredListening >= 2, Score=Math.Min(100, masteredListening * 50) },
                    new() { Key="reading", Title="Reading", JapaneseTitle="読む", Icon="📖", Requirement="Reach stable mastery in at least two A1 graded readings.", ProgressText=$"{masteredReading}/2 readings mastered", ActionName=nameof(ReadingLibrary), ActionLabel="Open graded reading", IsComplete=masteredReading >= 2, Score=Math.Min(100, masteredReading * 50) },
                    new() { Key="speaking", Title="Speaking", JapaneseTitle="話す", Icon="🎤", Requirement="Pass at least two A1 speaking tasks on two different dates.", ProgressText=$"{masteredSpeaking}/2 speaking tasks mastered", ActionName=nameof(SpeakingLibrary), ActionLabel="Open Speaking Studio", IsComplete=masteredSpeaking >= 2, Score=Math.Min(100, masteredSpeaking * 50) },
                    new() { Key="writing", Title="Writing", JapaneseTitle="書く", Icon="✍️", Requirement="Pass at least two A1 writing tasks on two different dates.", ProgressText=$"{masteredWriting}/2 writing tasks mastered", ActionName=nameof(WritingLibrary), ActionLabel="Open Writing Studio", IsComplete=masteredWriting >= 2, Score=Math.Min(100, masteredWriting * 50) }
                }
            };
            return View(nameof(A1Capstone), model);
        }

        public async Task<IActionResult> A2Capstone()
        {
            var userId = _userManager.GetUserId(User)!;
            var a2LessonIds = await _context.Lessons.Where(l => l.IsPublished && l.Category!.Name == "A2 Course" && l.Title.StartsWith("A2:"))
                .Select(l => l.Id).ToListAsync();
            var stableLessonCount = await _context.LessonProgress.CountAsync(p => p.UserId == userId && p.StableMasteryDate != null && a2LessonIds.Contains(p.LessonId));
            var bestFinalQuiz = await _context.QuizAttempts.Where(a => a.UserId == userId && a.Quiz!.Title == "A2 Final Can-do Assessment")
                .Select(a => (int?)a.ScorePercentage).MaxAsync() ?? 0;

            var listeningTasks = await _context.ListeningTasks.Where(t => t.Level == "A2").Include(t => t.Questions).ToListAsync();
            var listeningIds = listeningTasks.Select(t => t.Id).ToList();
            var listeningAttempts = await _context.ListeningAttempts.Where(a => a.UserId == userId && listeningIds.Contains(a.ListeningTaskId)).ToListAsync();
            var masteredListening = listeningTasks.Count(t => HasStableListeningMastery(listeningAttempts.Where(a => a.ListeningTaskId == t.Id), t.Questions.Select(q => q.Id), requireBothPlaybackModes: true));

            var passages = await _context.ReadingPassages.Where(p => p.Level == "A2").Include(p => p.Questions).ToListAsync();
            var passageIds = passages.Select(p => p.Id).ToList();
            var readingAttempts = await _context.ReadingAttempts.Where(a => a.UserId == userId && passageIds.Contains(a.ReadingPassageId)).ToListAsync();
            var masteredReading = passages.Count(p => HasStableReadingMastery(readingAttempts.Where(a => a.ReadingPassageId == p.Id), p.Questions.Select(q => q.Id)));

            var speakingIds = await _context.SpeakingTasks.Where(t => t.Level == "A2").Select(t => t.Id).ToListAsync();
            var speakingAttempts = await _context.SpeakingAttempts.Where(a => a.UserId == userId && speakingIds.Contains(a.SpeakingTaskId)).ToListAsync();
            var masteredSpeaking = speakingIds.Count(id => HasStableSpeakingMastery(speakingAttempts.Where(a => a.SpeakingTaskId == id)));

            var writingIds = await _context.WritingTasks.Where(t => t.Level == "A2").Select(t => t.Id).ToListAsync();
            var writingAttempts = await _context.WritingSubmissions.Where(s => s.UserId == userId && writingIds.Contains(s.WritingTaskId)).ToListAsync();
            var masteredWriting = writingIds.Count(id => HasStableWritingMastery(writingAttempts.Where(s => s.WritingTaskId == id)));

            var model = new A2CapstoneViewModel
            {
                LearnerName = User.Identity?.Name ?? "NihongoAcademy learner",
                GeneratedAt = _clock.Now,
                StableLessonCount = stableLessonCount, BestFinalQuizScore = bestFinalQuiz,
                FoundationComplete = a2LessonIds.Count > 0 && stableLessonCount == a2LessonIds.Count && bestFinalQuiz >= 80,
                Skills = new()
                {
                    new() { Key="listening", Title="Listening", JapaneseTitle="聞く", Icon="🎧", Requirement="Master at least two A2 listening tasks at natural and learner speeds across different dates.", ProgressText=$"{masteredListening}/2 listening tasks mastered", ActionName=nameof(ListeningLibrary), ActionLabel="Open A2 listening", IsComplete=masteredListening >= 2, Score=Math.Min(100, masteredListening * 50) },
                    new() { Key="reading", Title="Reading", JapaneseTitle="読む", Icon="📖", Requirement="Reach stable mastery in at least two A2 graded readings.", ProgressText=$"{masteredReading}/2 readings mastered", ActionName=nameof(ReadingLibrary), ActionLabel="Open A2 reading", IsComplete=masteredReading >= 2, Score=Math.Min(100, masteredReading * 50) },
                    new() { Key="speaking", Title="Speaking", JapaneseTitle="話す", Icon="🎤", Requirement="Pass at least two A2 speaking tasks on two different dates.", ProgressText=$"{masteredSpeaking}/2 speaking tasks mastered", ActionName=nameof(SpeakingLibrary), ActionLabel="Open A2 speaking", IsComplete=masteredSpeaking >= 2, Score=Math.Min(100, masteredSpeaking * 50) },
                    new() { Key="writing", Title="Writing", JapaneseTitle="書く", Icon="✍️", Requirement="Pass at least two A2 writing tasks on two different dates.", ProgressText=$"{masteredWriting}/2 writing tasks mastered", ActionName=nameof(WritingLibrary), ActionLabel="Open A2 writing", IsComplete=masteredWriting >= 2, Score=Math.Min(100, masteredWriting * 50) }
                }
            };
            return View(nameof(A2Capstone), model);
        }

        // ===== PRACTICE BY TOPIC =====
        public async Task<IActionResult> Topic(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var vocab = await _context.Vocabulary
                .Where(v => v.CategoryId == id)
                .OrderBy(v => v.DisplayOrder)
                .ToListAsync();

            var quizzes = await _context.Quizzes
                .Where(q => q.CategoryId == id && q.IsPublished)
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();

            ViewBag.Category = category;
            ViewBag.Vocabulary = vocab;
            ViewBag.Quizzes = quizzes;

            return View();
        }

        // ===== ADAPTIVE REVIEW =====
        public async Task<IActionResult> AdaptiveReview()
        {
            var userId = _userManager.GetUserId(User);

            var dueCards = new List<FlashcardProgress>();
            var weakItems = new List<object>();

            if (userId != null)
            {
                dueCards = await _context.FlashcardProgress
                    .Where(fp => fp.UserId == userId && fp.NextReview <= _clock.Now)
                    .Include(fp => fp.Vocabulary)
                    .OrderBy(fp => fp.NextReview)
                    .Take(20)
                    .ToListAsync();

                // Weak items: low ease factor
                var weak = await _context.FlashcardProgress
                    .Where(fp => fp.UserId == userId && fp.EaseFactor < 2.0)
                    .Include(fp => fp.Vocabulary)
                    .OrderBy(fp => fp.EaseFactor)
                    .Take(10)
                    .ToListAsync();

                weakItems = weak.Select(w => (object)new
                {
                    w.Vocabulary?.Japanese,
                    w.Vocabulary?.English,
                    w.EaseFactor
                }).ToList();
            }

            ViewBag.DueCards = dueCards;
            ViewBag.WeakItems = weakItems;

            return View();
        }

        // ===== CHALLENGES =====
        public IActionResult Challenges() => View();

        // ===== CHALLENGE: Speed Round =====
        public IActionResult SpeedRound() => View();

        // ===== CHALLENGE: Sentence Builder =====
        [AllowAnonymous]
        public async Task<IActionResult> SentenceBuilder()
        {
            var userId = User.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            int userTotalXp = 0;
            int userWeeklyXp = 0;
            if (userId != null)
            {
                var summary = await _xpService.GetWeeklyLeaderboardAsync(userId, top: 1);
                userTotalXp = summary.UserSummary.TotalXp;
                userWeeklyXp = summary.UserSummary.WeeklyXp;
            }

            ViewBag.TotalXp = userTotalXp;
            ViewBag.WeeklyXp = userWeeklyXp;
            ViewBag.IsAuthenticated = userId != null;
            return View();
        }

        public class SentenceXpPayload
        {
            public int Amount { get; set; } = 15;
            public string? SentenceAnswer { get; set; }
            public int Streak { get; set; } = 1;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AwardSentenceXp([FromBody] SentenceXpPayload payload)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            // Nothing server-side can confirm the learner actually formed this sentence,
            // so the client's requested amount is ignored: the reward is fixed and capped
            // per day. Trusting it turned this endpoint into an unlimited XP faucet.
            const int sentenceXpReward = 5;
            const int sentenceXpDailyCap = 50;

            var description = $"Sentence: {payload?.SentenceAnswer ?? "Word Scramble"} (Streak {payload?.Streak ?? 1})";
            var (newTotalXp, weeklyXp, rewarded) = await _xpService.AwardCappedXpAsync(
                userId, sentenceXpReward, "Sentence Builder", sentenceXpDailyCap, description);

            return Json(new
            {
                success = true,
                awarded = rewarded,
                totalXp = newTotalXp,
                weeklyXp = weeklyXp
            });
        }

        // ===== CHALLENGE: Dictation Challenge =====
        public IActionResult DictationChallenge() => View();

        // ===== CHALLENGE: Mixed Challenge =====
        public IActionResult MixedChallenge() => View();

        public async Task<IActionResult> Challenge(string mode = "overview")
        {
            // SentenceBuilder() loads the learner's XP state, so route through the action
            // rather than returning its view directly. Returning the view here created a
            // second URL into the same page with every ViewBag unset, which presented
            // signed-in learners as guests.
            if (mode.Equals("sentence-builder", StringComparison.OrdinalIgnoreCase))
                return await SentenceBuilder();

            return mode.ToLowerInvariant() switch
            {
                "speed" => View(nameof(SpeedRound)),
                "dictation" => View(nameof(DictationChallenge)),
                "mixed" => View(nameof(MixedChallenge)),
                _ => View(nameof(Challenges))
            };
        }

        // ===== PRACTICE MODE RUNNER =====
        public async Task<IActionResult> Mode(string skill, string mode)
        {
            if (skill?.Equals("reading", StringComparison.OrdinalIgnoreCase) == true &&
                (mode?.Equals("reading-comprehension", StringComparison.OrdinalIgnoreCase) == true ||
                 mode?.Equals("short-stories", StringComparison.OrdinalIgnoreCase) == true))
                return RedirectToAction(nameof(ReadingLibrary));
            if (skill?.Equals("listening", StringComparison.OrdinalIgnoreCase) == true)
                return RedirectToAction(nameof(ListeningLibrary), new { type = mode?.Equals("conversation", StringComparison.OrdinalIgnoreCase) == true ? "dialogue" : (string?)null });
            if (skill?.Equals("writing", StringComparison.OrdinalIgnoreCase) == true &&
                new[] { "sentence-writing", "guided-writing", "free-writing" }.Contains(mode, StringComparer.OrdinalIgnoreCase))
                return RedirectToAction(nameof(WritingLibrary), new { type = mode switch { "sentence-writing" => "sentence", "guided-writing" => "guided", _ => "free" } });
            if (skill?.Equals("speaking", StringComparison.OrdinalIgnoreCase) == true &&
                new[] { "role-play", "topic-challenge", "pronunciation" }.Contains(mode, StringComparer.OrdinalIgnoreCase))
                return RedirectToAction(nameof(SpeakingLibrary), new { type = mode switch { "role-play" => "role-play", "topic-challenge" => "topic", _ => (string?)null } });

            // Generic practice mode runner
            var skillInfo = skill?.ToLower() switch
            {
                "listening" => ("Listening", "リスニング", "🎧"),
                "reading" => ("Reading", "リーディング", "📖"),
                "speaking" => ("Speaking", "スピーキング", "🎤"),
                "writing" => ("Writing", "ライティング", "✍️"),
                _ => ("Practice", "練習", "🌸")
            };

            // Get vocabulary for the mode
            var vocabQuery = _context.Vocabulary
                .Include(v => v.Category)
                .Where(v => v.Category != null && v.Category.Name != "Culture");
            if (mode?.Equals("handwriting", StringComparison.OrdinalIgnoreCase) == true)
            {
                vocabQuery = vocabQuery.Where(v =>
                    v.PartOfSpeech == "Kanji" ||
                    ((v.Category!.Name == "Hiragana" || v.Category.Name == "Katakana") && v.Japanese.Length == 1));
            }
            var vocab = await vocabQuery
                .OrderBy(v => v.DisplayOrder)
                .Take(mode?.Equals("handwriting", StringComparison.OrdinalIgnoreCase) == true ? 140 : 20)
                .ToListAsync();

            ViewBag.SkillTitle = skillInfo.Item1;
            ViewBag.SkillJp = skillInfo.Item2;
            ViewBag.SkillIcon = skillInfo.Item3;
            ViewBag.ModeKey = mode;
            ViewBag.Vocabulary = vocab;

            return View("ModeRunner");
        }
    }

    // ===== View Models =====
    public class SkillViewModel
    {
        public string SkillKey { get; set; } = "";
        public string Title { get; set; } = "";
        public string JapaneseTitle { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public List<SkillMode> Modes { get; set; } = new();
    }

    public class SkillMode
    {
        public string Key { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
    }
}
