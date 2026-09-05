using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using System.Linq;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Lessons)
                .Include(c => c.Vocabulary)
                .Include(c => c.Quizzes)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,IconClass,DisplayOrder")] Models.Category category)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == category.Name))
                ModelState.AddModelError(nameof(category.Name), "A category with this name already exists.");
            if (ModelState.IsValid)
            {
                category.CreatedDate = DateTime.Now;
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IconClass,DisplayOrder")] Models.Category category)
        {
            if (id != category.Id) return NotFound();
            if (await _context.Categories.AnyAsync(c => c.Id != id && c.Name == category.Name))
                ModelState.AddModelError(nameof(category.Name), "A category with this name already exists.");
            if (ModelState.IsValid)
            {
                var existing = await _context.Categories.FindAsync(id);
                if (existing == null) return NotFound();
                try
                {
                    existing.Name = category.Name;
                    existing.Description = category.Description;
                    existing.IconClass = category.IconClass;
                    existing.DisplayOrder = category.DisplayOrder;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Category updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                var hasLessons = await _context.Lessons.AnyAsync(l => l.CategoryId == id);
                var hasVocab = await _context.Vocabulary.AnyAsync(v => v.CategoryId == id);
                var hasQuizzes = await _context.Quizzes.AnyAsync(q => q.CategoryId == id);

                if (hasLessons || hasVocab || hasQuizzes)
                {
                    TempData["Error"] = "Cannot delete category that contains lessons, vocabulary, or quizzes. Please remove or reassign them first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
