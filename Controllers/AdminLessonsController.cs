using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminLessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppClock _clock;
        private readonly AuditLogService _audit;

        public AdminLessonsController(ApplicationDbContext context, IAppClock clock, AuditLogService audit)
        {
            _context = context;
            _clock = clock;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Category)
                .OrderBy(l => l.Category!.DisplayOrder)
                .ThenBy(l => l.DisplayOrder)
                .ToListAsync();
            return View(lessons);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Title,JapaneseTitle,Description,Content,VideoUrl,ImageUrl,DifficultyLevel,EstimatedMinutes,IsPublished,DisplayOrder")] Lesson lesson)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == lesson.CategoryId))
                ModelState.AddModelError(nameof(lesson.CategoryId), "Select a valid category.");
            if (ModelState.IsValid)
            {
                lesson.CreatedDate = _clock.Now;
                _context.Add(lesson);
                await _context.SaveChangesAsync();
                await _audit.RecordOperationAsync("CreateLesson", "Lesson", lesson.Id.ToString(), $"Created lesson '{lesson.Title}'.");
                TempData["Success"] = "Lesson created successfully.";
                return RedirectToAction("Index", "AdminContent", new { type = "lesson" });
            }
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(lesson);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,Title,JapaneseTitle,Description,Content,VideoUrl,ImageUrl,DifficultyLevel,EstimatedMinutes,IsPublished,DisplayOrder")] Lesson lesson)
        {
            if (id != lesson.Id) return NotFound();
            if (!await _context.Categories.AnyAsync(c => c.Id == lesson.CategoryId))
                ModelState.AddModelError(nameof(lesson.CategoryId), "Select a valid category.");

            if (ModelState.IsValid)
            {
                var existing = await _context.Lessons.FindAsync(id);
                if (existing == null) return NotFound();
                try
                {
                    existing.CategoryId = lesson.CategoryId;
                    existing.Title = lesson.Title;
                    existing.JapaneseTitle = lesson.JapaneseTitle;
                    existing.Description = lesson.Description;
                    existing.Content = lesson.Content;
                    existing.VideoUrl = lesson.VideoUrl;
                    existing.ImageUrl = lesson.ImageUrl;
                    existing.DifficultyLevel = lesson.DifficultyLevel;
                    existing.EstimatedMinutes = lesson.EstimatedMinutes;
                    existing.IsPublished = lesson.IsPublished;
                    existing.DisplayOrder = lesson.DisplayOrder;
                    existing.ModifiedDate = _clock.Now;
                    await _context.SaveChangesAsync();
                    await _audit.RecordOperationAsync("EditLesson", "Lesson", existing.Id.ToString(), $"Updated lesson '{existing.Title}'.");
                    TempData["Success"] = "Lesson updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Index", "AdminContent", new { type = "lesson" });
            }
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(lesson);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var lesson = await _context.Lessons
                .Include(l => l.Category)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var lesson = await _context.Lessons
                .Include(l => l.Category)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return NotFound();

            // The delete confirmation page must show everything that cascades away with the
            // lesson. Learner records still hard-block the delete (handled in the POST);
            // structured content is only listed here so the choice is informed, not silent.
            ViewBag.LearnerRecords = await _context.LessonProgress.CountAsync(p => p.LessonId == id)
                + await _context.PracticeAttempts.CountAsync(a => a.LessonId == id);
            ViewBag.ActivityCount = await _context.LessonActivities.CountAsync(a => a.LessonId == id);
            ViewBag.VocabularyLinkCount = await _context.LessonVocabulary.CountAsync(l => l.LessonId == id);
            ViewBag.GrammarLinkCount = await _context.LessonGrammarPoints.CountAsync(g => g.LessonId == id);
            ViewBag.SkillTaskCount = await _context.ListeningTasks.CountAsync(t => t.LessonId == id)
                + await _context.ReadingPassages.CountAsync(t => t.LessonId == id)
                + await _context.SpeakingTasks.CountAsync(t => t.LessonId == id)
                + await _context.WritingTasks.CountAsync(t => t.LessonId == id);

            return View(lesson);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                // LessonProgress cascades from Lesson, so without this check the delete
                // succeeds quietly and erases every learner's progress for this lesson.
                var learnerRecords = await _context.LessonProgress.CountAsync(p => p.LessonId == id)
                    + await _context.PracticeAttempts.CountAsync(a => a.LessonId == id);

                if (learnerRecords > 0)
                {
                    TempData["Error"] = $"This lesson has {learnerRecords} learner record(s) attached. Unpublish it instead so those records are preserved.";
                    return RedirectToAction("Editor", "AdminContent", new { type = "lesson", mode = "details", id });
                }
                try
                {
                    _context.Lessons.Remove(lesson);
                    await _context.SaveChangesAsync();
                    await _audit.RecordOperationAsync("DeleteLesson", "Lesson", id.ToString(), $"Deleted lesson '{lesson.Title}'.");
                    TempData["Success"] = "Lesson deleted successfully.";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "This lesson is still referenced by other content and cannot be deleted safely.";
                }
            }
            return RedirectToAction("Index", "AdminContent", new { type = "lesson" });
        }

        private bool LessonExists(int id) => _context.Lessons.Any(e => e.Id == id);
    }
}
