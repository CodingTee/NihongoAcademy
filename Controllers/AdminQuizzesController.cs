using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminQuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminQuizzesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.Category)
                .Include(q => q.Questions)
                .OrderBy(q => q.Category!.DisplayOrder)
                .ThenBy(q => q.DisplayOrder)
                .ToListAsync();
            return View(quizzes);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                quiz.CreatedDate = DateTime.Now;
                _context.Add(quiz);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Quiz created. Now add questions to it.";
                return RedirectToAction(nameof(Edit), new { id = quiz.Id });
            }
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(quiz);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var quiz = await _context.Quizzes
                .Include(q => q.Questions.OrderBy(qq => qq.DisplayOrder))
                .Include(q => q.Category)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return NotFound();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Quiz quiz)
        {
            if (id != quiz.Id) return NotFound();

            var existingQuiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existingQuiz == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingQuiz.Title = quiz.Title;
                existingQuiz.JapaneseTitle = quiz.JapaneseTitle;
                existingQuiz.CategoryId = quiz.CategoryId;
                existingQuiz.Description = quiz.Description;
                existingQuiz.TimeLimitMinutes = quiz.TimeLimitMinutes;
                existingQuiz.PassingScore = quiz.PassingScore;
                existingQuiz.IsPublished = quiz.IsPublished;
                existingQuiz.DisplayOrder = quiz.DisplayOrder;

                _context.Update(existingQuiz);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Quiz updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(QuizQuestion question)
        {
            if (ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Question added successfully.";
            }
            return RedirectToAction(nameof(Edit), new { id = question.QuizId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int questionId, int quizId)
        {
            var question = await _context.QuizQuestions.FindAsync(questionId);
            if (question != null)
            {
                _context.QuizQuestions.Remove(question);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Question deleted.";
            }
            return RedirectToAction(nameof(Edit), new { id = quizId });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var quiz = await _context.Quizzes
                .Include(q => q.Category)
                .Include(q => q.Questions.OrderBy(qq => qq.DisplayOrder))
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return NotFound();
            return View(quiz);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var quiz = await _context.Quizzes
                .Include(q => q.Category)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return NotFound();
            return View(quiz);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Quiz deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool QuizExists(int id) => _context.Quizzes.Any(e => e.Id == id);
    }
}
