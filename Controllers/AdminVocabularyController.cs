using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminVocabularyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminVocabularyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vocab = await _context.Vocabulary
                .Include(v => v.Category)
                .OrderBy(v => v.Category!.DisplayOrder)
                .ThenBy(v => v.DisplayOrder)
                .ToListAsync();
            return View(vocab);
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
                vocabulary.CreatedDate = DateTime.Now;
                _context.Add(vocabulary);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vocabulary card created successfully.";
                return RedirectToAction(nameof(Index));
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
                    TempData["Success"] = "Vocabulary card updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VocabExists(id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
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
                try
                {
                    _context.Vocabulary.Remove(vocab);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Vocabulary card deleted successfully.";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "This vocabulary item is used by lessons or learner attempts and cannot be deleted safely. Reassign or unpublish the related content first.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VocabExists(int id) => _context.Vocabulary.Any(e => e.Id == id);
    }
}
