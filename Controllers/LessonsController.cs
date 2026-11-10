using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAppClock _clock;

        public LessonsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IAppClock clock)
        {
            _context = context;
            _userManager = userManager;
            _clock = clock;
        }

        public async Task<IActionResult> Index(int? categoryId, string searchString)
        {
            var lessons = _context.Lessons
                .Where(l => l.IsPublished)
                .Include(l => l.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                lessons = lessons.Where(l => l.CategoryId == categoryId.Value);
                ViewBag.SelectedCategory = categoryId.Value;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                lessons = lessons.Where(l =>
                    l.Title.Contains(searchString) ||
                    (l.Description != null && l.Description.Contains(searchString)) ||
                    (l.JapaneseTitle != null && l.JapaneseTitle.Contains(searchString)));
                ViewBag.SearchString = searchString;
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.Name != "Culture")
                .OrderBy(c => c.DisplayOrder).ToListAsync();

            // Learning path categories in order
            var pathCategoryNames = new[] { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course", "Vocabulary", "Grammar" };
            var allCategories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            var pathCategories = allCategories.Where(c => pathCategoryNames.Contains(c.Name)).OrderBy(c => Array.IndexOf(pathCategoryNames, c.Name)).ToList();
            ViewBag.LearningPathCategories = pathCategories;

            // User progress for learning path + continue learning + lesson cards
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                var userProgress = await _context.LessonProgress
                    .Where(p => p.UserId == userId)
                    .Include(p => p.Lesson)
                        .ThenInclude(l => l!.Category)
                    .ToListAsync();

                ViewBag.UserProgress = userProgress;

                var prerequisiteRows = await _context.LessonPrerequisites
                    .Include(p => p.PrerequisiteLesson)
                    .ToListAsync();
                var stablyMasteredIds = userProgress
                    .Where(p => p.FirstMasteryPassedDate.HasValue)
                    .Select(p => p.LessonId)
                    .ToHashSet();
                ViewBag.LockedPrerequisites = prerequisiteRows
                    .Where(p => !stablyMasteredIds.Contains(p.PrerequisiteLessonId))
                    .GroupBy(p => p.LessonId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(p => p.PrerequisiteLesson?.Title ?? "previous lesson").ToList());

                // Category-level progress for learning path: one GROUP BY instead of one COUNT per category.
                var lessonCountsByCategory = await _context.Lessons
                    .AsNoTracking()
                    .Where(l => l.IsPublished)
                    .GroupBy(l => l.CategoryId)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

                var pathProgress = new Dictionary<string, string>();
                foreach (var cat in pathCategories)
                {
                    var catLessons = lessonCountsByCategory.GetValueOrDefault(cat.Id);
                    var catCompleted = userProgress.Count(p => p.Lesson?.CategoryId == cat.Id && p.IsCompleted);
                    var catStarted = userProgress.Any(p => p.Lesson?.CategoryId == cat.Id && !p.IsCompleted && p.CompletionPercentage > 0);

                    if (catLessons == 0) pathProgress[cat.Name] = "empty";
                    else if (catCompleted >= catLessons) pathProgress[cat.Name] = "completed";
                    else if (catStarted || catCompleted > 0) pathProgress[cat.Name] = "in-progress";
                    else pathProgress[cat.Name] = "not-started";
                }
                ViewBag.LearningPathProgress = pathProgress;

                // Continue learning: most recent in-progress lesson
                var continueLesson = userProgress
                    .Where(p => !p.IsCompleted && p.CompletionPercentage > 0 && p.StartedDate.HasValue)
                    .OrderByDescending(p => p.StartedDate)
                    .FirstOrDefault();

                if (continueLesson != null && continueLesson.Lesson != null)
                {
                    ViewBag.ContinueLesson = continueLesson.Lesson;
                    ViewBag.ContinueProgress = continueLesson.CompletionPercentage;
                }
            }

            var result = await lessons
                .OrderBy(l => l.Category!.DisplayOrder)
                .ThenBy(l => l.DisplayOrder)
                .ToListAsync();
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Category)
                .Include(l => l.LessonVocabulary)
                .Include(l => l.LessonGrammarPoints).ThenInclude(link => link.GrammarPoint)
                .Include(l => l.Activities)
                .FirstOrDefaultAsync(l => l.Id == id && l.IsPublished);

            if (lesson == null) return NotFound();

            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUserId = _userManager.GetUserId(User);
                var unmet = await _context.LessonPrerequisites
                    .Where(p => p.LessonId == id)
                    .Where(p => !_context.LessonProgress.Any(progress =>
                        progress.UserId == currentUserId &&
                        progress.LessonId == p.PrerequisiteLessonId &&
                        progress.FirstMasteryPassedDate.HasValue))
                    .Select(p => p.PrerequisiteLesson!.Title)
                    .ToListAsync();
                if (unmet.Count > 0)
                {
                    TempData["Message"] = $"Master {string.Join(", ", unmet)} before starting this lesson.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var viewModel = new LessonDetailsViewModel { Lesson = lesson, RequiresMastery = lesson.LessonVocabulary.Any() };

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                viewModel.Progress = await _context.LessonProgress
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == id);

                // Progress is created lazily by LessonMasteryService.EvaluateAsync on the
                // first mastery pass, so a plain GET must not write to the database.

                // Follow the explicit curriculum graph instead of guessing from
                // category/display ordering.
                viewModel.NextLesson = await _context.LessonPrerequisites
                    .Where(p => p.PrerequisiteLessonId == id && p.Lesson!.IsPublished)
                    .Select(p => p.Lesson)
                    .FirstOrDefaultAsync();

                viewModel.PreviousLesson = await _context.LessonPrerequisites
                    .Where(p => p.LessonId == id && p.PrerequisiteLesson!.IsPublished)
                    .Select(p => p.PrerequisiteLesson)
                    .FirstOrDefaultAsync();

                // Read-only snapshot of first-pass progress for the UI. Intentionally does
                // not call EvaluateAsync so a GET never mutates LessonProgress.
                if (lesson.LessonVocabulary.Any())
                {
                    var vocabIds = lesson.LessonVocabulary.Select(x => x.VocabularyId).ToList();
                    var attempts = await _context.PracticeAttempts
                        .Where(a => a.UserId == userId && a.LessonId == id).ToListAsync();
                    var requiredTypes = lesson.Title.StartsWith("A1:") || lesson.Title.StartsWith("A2:")
                        ? LessonMasteryPolicy.CoreTypes.Concat(new[] { "production" }).ToArray()
                        : LessonMasteryPolicy.CoreTypes;
                    var results = requiredTypes.ToDictionary(
                        type => type,
                        type =>
                        {
                            var grp = attempts.Where(a => a.ExerciseType == type &&
                                a.VocabularyId.HasValue && vocabIds.Contains(a.VocabularyId.Value)).ToList();
                            return new ExerciseMasteryResult(
                                grp.Count,
                                grp.Count == 0 ? 0 : (int)Math.Round(grp.Count(a => a.IsCorrect) * 100d / grp.Count),
                                grp.Where(a => a.VocabularyId.HasValue).Select(a => a.VocabularyId).Distinct().Count(),
                                type == "production" ? 1 : vocabIds.Count);
                        });
                    var reqActivityIds = lesson.Activities
                        .Where(a => a.ActivityType != "dialogue")
                        .Select(a => a.Id).ToList();
                    var actDone = reqActivityIds.Count(id => attempts.Any(a =>
                        a.LessonActivityId == id && a.IsCorrect));
                    viewModel.MasteryEvaluation = new MasteryEvaluation(
                        false, false, false, viewModel.Progress?.MasteryReviewDueDate, results, actDone, reqActivityIds.Count);
                }
            }

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int lessonId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var lesson = await _context.Lessons
                .Where(l => l.Id == lessonId && l.IsPublished)
                .Select(l => new
                {
                    HasMasteryContent = l.LessonVocabulary.Any()
                })
                .FirstOrDefaultAsync();

            if (lesson == null) return NotFound();

            // Lessons with explicit learning items must be completed through the
            // assessed practice flow. Do not allow the generic button/API to
            // bypass recognition, listening and recall mastery.
            if (lesson.HasMasteryContent)
            {
                return BadRequest(new
                {
                    success = false,
                    requiresMastery = true,
                    message = "Complete this lesson's mastery check to finish it."
                });
            }

            var progress = await _context.LessonProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletionPercentage = 100,
                    StartedDate = _clock.Now,
                    CompletedDate = _clock.Now,
                    StableMasteryDate = _clock.Now
                };
                _context.LessonProgress.Add(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletionPercentage = 100;
                progress.CompletedDate = _clock.Now;
                progress.StableMasteryDate ??= _clock.Now;
                _context.Update(progress);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(int lessonId, int percentage)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var lesson = await _context.Lessons
                .Where(l => l.Id == lessonId && l.IsPublished)
                .Select(l => new { HasMasteryContent = l.LessonVocabulary.Any() })
                .FirstOrDefaultAsync();
            if (lesson == null) return NotFound();

            // Reading progress is exposure only. Assessed lessons can reach 100%
            // exclusively through PracticeController.RecordLessonAttempt.
            percentage = Math.Clamp(percentage, 0, lesson.HasMasteryContent ? 70 : 100);

            var progress = await _context.LessonProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    CompletionPercentage = percentage,
                    IsCompleted = percentage >= 100,
                    StartedDate = _clock.Now,
                    CompletedDate = percentage >= 100 ? _clock.Now : null
                };
                if (percentage >= 100) progress.StableMasteryDate = _clock.Now;
                _context.LessonProgress.Add(progress);
            }
            else
            {
                progress.CompletionPercentage = percentage;
                if (percentage >= 100)
                {
                    progress.IsCompleted = true;
                    progress.CompletedDate = _clock.Now;
                    progress.StableMasteryDate ??= _clock.Now;
                }
                _context.Update(progress);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class LessonDetailsViewModel
    {
        public Lesson Lesson { get; set; } = new();
        public LessonProgress? Progress { get; set; }
        public Lesson? NextLesson { get; set; }
        public Lesson? PreviousLesson { get; set; }
        public bool RequiresMastery { get; set; }
        public Services.MasteryEvaluation? MasteryEvaluation { get; set; }
    }
}
