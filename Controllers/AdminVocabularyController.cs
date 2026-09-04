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
        public async Task<IActionResult> Create(Vocabulary vocabulary)
        {
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
        public async Task<IActionResult> Edit(int id, Vocabulary vocabulary)
        {
            if (id != vocabulary.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vocabulary);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Vocabulary card updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VocabExists(vocabulary.Id)) return NotFound();
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
                _context.Vocabulary.Remove(vocab);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vocabulary card deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VocabExists(int id) => _context.Vocabulary.Any(e => e.Id == id);
    }
}
