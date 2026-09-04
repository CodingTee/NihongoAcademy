using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminLessonsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminLessonsController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                lesson.CreatedDate = DateTime.Now;
                _context.Add(lesson);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lesson created successfully.";
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, Lesson lesson)
        {
            if (id != lesson.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    lesson.ModifiedDate = DateTime.Now;
                    _context.Update(lesson);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Lesson updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(lesson.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
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
            return View(lesson);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lesson deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LessonExists(int id) => _context.Lessons.Any(e => e.Id == id);
    }
}
