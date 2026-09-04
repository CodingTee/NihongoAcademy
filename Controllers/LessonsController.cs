using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LessonsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                    .Where(p => p.StableMasteryDate.HasValue)
                    .Select(p => p.LessonId)
                    .ToHashSet();
                ViewBag.LockedPrerequisites = prerequisiteRows
                    .Where(p => !stablyMasteredIds.Contains(p.PrerequisiteLessonId))
                    .GroupBy(p => p.LessonId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(p => p.PrerequisiteLesson?.Title ?? "previous lesson").ToList());

                // Category-level progress for learning path
                var pathProgress = new Dictionary<string, string>();
                foreach (var cat in pathCategories)
                {
                    var catLessons = await _context.Lessons.Where(l => l.CategoryId == cat.Id && l.IsPublished).CountAsync();
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
                        progress.StableMasteryDate != null))
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

                if (viewModel.Progress == null)
                {
                    viewModel.Progress = new LessonProgress
                    {
                        UserId = userId!,
                        LessonId = id,
                        StartedDate = DateTime.Now,
                        CompletionPercentage = 0,
                        IsCompleted = false
                    };
                    _context.LessonProgress.Add(viewModel.Progress);
                    await _context.SaveChangesAsync();
                }

                // Follow the explicit curriculum graph instead of guessing from
                // category/display ordering.
                viewModel.NextLesson = await _context.LessonPrerequisites
                    .Where(p => p.PrerequisiteLessonId == id && p.Lesson!.IsPublished)
                    .Select(p => p.Lesson)
                    .FirstOrDefaultAsync();
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
                    StartedDate = DateTime.Now,
                    CompletedDate = DateTime.Now,
                    StableMasteryDate = DateTime.Now
                };
                _context.LessonProgress.Add(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletionPercentage = 100;
                progress.CompletedDate = DateTime.Now;
                progress.StableMasteryDate ??= DateTime.Now;
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
                    StartedDate = DateTime.Now,
                    CompletedDate = percentage >= 100 ? DateTime.Now : null
                };
                if (percentage >= 100) progress.StableMasteryDate = DateTime.Now;
                _context.LessonProgress.Add(progress);
            }
            else
            {
                progress.CompletionPercentage = percentage;
                if (percentage >= 100)
                {
                    progress.IsCompleted = true;
                    progress.CompletedDate = DateTime.Now;
                    progress.StableMasteryDate ??= DateTime.Now;
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
        public bool RequiresMastery { get; set; }
    }
}
