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

        public PracticeController(ApplicationDbContext context, UserManager<IdentityUser> userManager, LessonMasteryService masteryService)
        {
            _context = context;
            _userManager = userManager;
            _masteryService = masteryService;
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
                var now = DateTime.Now;
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

                // Simple rule-based weak areas: categories with low average ease factor
                var weakCategories = await _context.FlashcardProgress
                    .Where(fp => fp.UserId == userId && fp.EaseFactor < 2.0)
                    .Join(_context.Vocabulary, fp => fp.VocabularyId, v => v.Id, (fp, v) => v.CategoryId)
                    .Distinct()
                    .Take(5)
                    .ToListAsync();

                foreach (var catId in weakCategories)
                {
                    var catName = await _context.Categories.Where(c => c.Id == catId).Select(c => c.Name).FirstOrDefaultAsync();
                    if (catName != null) weakAreas.Add(catName);
                }
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

            // Vocabulary count per topic
            var topicVocabCounts = new Dictionary<int, int>();
            foreach (var t in topics)
            {
                topicVocabCounts[t.Id] = await _context.Vocabulary.CountAsync(v => v.CategoryId == t.Id);
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
                    progress.StableMasteryDate != null));
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
            ViewBag.Activities = await _context.LessonActivities
                .Where(a => a.LessonId == lessonId)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();

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
            if (!allowedTypes.Contains(exerciseType) || expectedAnswer == null)
                return BadRequest();

            if (await _context.LessonPrerequisites
                .Where(p => p.LessonId == lessonId)
                .AnyAsync(p => !_context.LessonProgress.Any(progress =>
                    progress.UserId == userId &&
                    progress.LessonId == p.PrerequisiteLessonId &&
                    progress.StableMasteryDate != null)))
                return Forbid();

            static string NormalizeAnswer(string value) =>
                string.Concat(value.Where(c => !char.IsWhiteSpace(c))).Trim();
            var isCorrect = exerciseType == "production"
                ? !string.IsNullOrWhiteSpace(answer) && answer.Trim().Length >= 8
                : NormalizeAnswer(answer ?? string.Empty) == NormalizeAnswer(expectedAnswer);

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
                results = evaluation.Results
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
                    progress.StableMasteryDate != null)))
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

            return Json(new
            {
                success = true,
                isCorrect,
                correctAnswer = activity.CorrectAnswer,
                activity.Explanation,
                mastered = evaluation.Mastered,
                firstPassComplete = evaluation.FirstPassComplete,
                awaitingDelayedReview = evaluation.AwaitingDelayedReview,
                reviewDueDate = evaluation.ReviewDueDate
            });
        }

        public async Task<IActionResult> LessonQuiz(int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Category)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.IsPublished);

            if (lesson == null) return NotFound();

            if (await _context.LessonVocabulary.AnyAsync(x => x.LessonId == lessonId))
            {
                TempData["Message"] = "Complete this lesson's recognition, listening and recall mastery check instead.";
                return RedirectToAction(nameof(LessonPractice), new { lessonId });
            }

            // Get quizzes for this lesson's category
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
                new() { Key = "handwriting", Title = "Handwriting", Description = "Practice kana & kanji stroke order", Icon = "✍️" },
                new() { Key = "sentence-writing", Title = "Sentence Writing", Description = "Translate and write sentences", Icon = "📝" },
                new() { Key = "guided-writing", Title = "Guided Writing", Description = "Write with vocabulary & grammar support", Icon = "📄" },
                new() { Key = "free-writing", Title = "Free Writing", Description = "Write freely on a prompt", Icon = "🌸" }
            }
        });

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
                    .Where(fp => fp.UserId == userId && fp.NextReview <= DateTime.Now)
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
        public IActionResult SentenceBuilder() => View();

        // ===== CHALLENGE: Dictation Challenge =====
        public IActionResult DictationChallenge() => View();

        // ===== CHALLENGE: Mixed Challenge =====
        public IActionResult MixedChallenge() => View();

        // ===== PRACTICE MODE RUNNER =====
        public async Task<IActionResult> Mode(string skill, string mode)
        {
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
            var vocab = await _context.Vocabulary
                .Include(v => v.Category)
                .Where(v => v.Category != null && v.Category.Name != "Culture")
                .OrderBy(v => v.DisplayOrder)
                .Take(20)
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
