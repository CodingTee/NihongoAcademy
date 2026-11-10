using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminVocabularyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppClock _clock;
        private readonly AuditLogService _audit;

        public AdminVocabularyController(ApplicationDbContext context, IAppClock clock, AuditLogService audit)
        {
            _context = context;
            _clock = clock;
            _audit = audit;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var model = await AdminVocabularyListViewModel.CreateAsync(
                _context.Vocabulary
                    .Include(v => v.Category)
                    .OrderBy(v => v.Category!.DisplayOrder)
                    .ThenBy(v => v.DisplayOrder),
                page);
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Japanese,Romaji,English,PartOfSpeech,ExampleJapanese,ExampleEnglish,AudioPath,ImagePath,JlptLevel,DisplayOrder")] Vocabulary vocabulary)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == vocabulary.CategoryId))
                ModelState.AddModelError(nameof(vocabulary.CategoryId), "Select a valid category.");
            if (ModelState.IsValid)
            {
                vocabulary.CreatedDate = _clock.Now;
                _context.Add(vocabulary);
                await _context.SaveChangesAsync();
                await _audit.RecordOperationAsync("CreateVocabulary", "Vocabulary", vocabulary.Id.ToString(), $"Created vocabulary card '{vocabulary.Japanese}' ({vocabulary.English}).");
                TempData["Success"] = "Vocabulary card created successfully.";
                return RedirectToAction("Index", "AdminContent", new { type = "vocabulary" });
            }
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(vocabulary);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var vocab = await _context.Vocabulary.FindAsync(id);
            if (vocab == null) return NotFound();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(vocab);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,Japanese,Romaji,English,PartOfSpeech,ExampleJapanese,ExampleEnglish,AudioPath,ImagePath,JlptLevel,DisplayOrder")] Vocabulary vocabulary)
        {
            if (id != vocabulary.Id) return NotFound();
            if (!await _context.Categories.AnyAsync(c => c.Id == vocabulary.CategoryId))
                ModelState.AddModelError(nameof(vocabulary.CategoryId), "Select a valid category.");

            if (ModelState.IsValid)
            {
                var existing = await _context.Vocabulary.FindAsync(id);
                if (existing == null) return NotFound();
                try
                {
                    existing.CategoryId = vocabulary.CategoryId;
                    existing.Japanese = vocabulary.Japanese;
                    existing.Romaji = vocabulary.Romaji;
                    existing.English = vocabulary.English;
                    existing.PartOfSpeech = vocabulary.PartOfSpeech;
                    existing.ExampleJapanese = vocabulary.ExampleJapanese;
                    existing.ExampleEnglish = vocabulary.ExampleEnglish;
                    existing.AudioPath = vocabulary.AudioPath;
                    existing.ImagePath = vocabulary.ImagePath;
                    existing.JlptLevel = vocabulary.JlptLevel;
                    existing.DisplayOrder = vocabulary.DisplayOrder;
                    await _context.SaveChangesAsync();
                    await _audit.RecordOperationAsync("EditVocabulary", "Vocabulary", existing.Id.ToString(), $"Updated vocabulary card '{existing.Japanese}'.");
                    TempData["Success"] = "Vocabulary card updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VocabExists(id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Index", "AdminContent", new { type = "vocabulary" });
            }
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(vocabulary);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var vocab = await _context.Vocabulary
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vocab == null) return NotFound();
            return View(vocab);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var vocab = await _context.Vocabulary
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vocab == null) return NotFound();
            return View(vocab);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vocab = await _context.Vocabulary.FindAsync(id);
            if (vocab != null)
            {
                // FlashcardProgress cascades from Vocabulary, so this check is what
                // stops the delete from wiping everyone's SRS state for this card.
                var learnerRecords = await _context.FlashcardProgress.CountAsync(f => f.VocabularyId == id);
                var lessonLinks = await _context.LessonVocabulary.CountAsync(l => l.VocabularyId == id);

                if (learnerRecords > 0 || lessonLinks > 0)
                {
                    TempData["Error"] = $"This word has {learnerRecords} review record(s) and is used by {lessonLinks} lesson(s). Remove it from those lessons first, or edit it instead of deleting.";
                    return RedirectToAction("Editor", "AdminContent", new { type = "vocabulary", mode = "details", id });
                }

                try
                {
                    _context.Vocabulary.Remove(vocab);
                    await _context.SaveChangesAsync();
                    await _audit.RecordOperationAsync("DeleteVocabulary", "Vocabulary", id.ToString(), $"Deleted vocabulary card '{vocab.Japanese}'.");
                    TempData["Success"] = "Vocabulary card deleted successfully.";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "This vocabulary item is still referenced by learner attempts and cannot be deleted safely.";
                }
            }
            return RedirectToAction("Index", "AdminContent", new { type = "vocabulary" });
        }

        private bool VocabExists(int id) => _context.Vocabulary.Any(e => e.Id == id);
    }
}
