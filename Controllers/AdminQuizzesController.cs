using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminQuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppClock _clock;
        private readonly AuditLogService _audit;

        public AdminQuizzesController(ApplicationDbContext context, IAppClock clock, AuditLogService audit)
        {
            _context = context;
            _clock = clock;
            _audit = audit;
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
        public async Task<IActionResult> Create([Bind("CategoryId,Title,JapaneseTitle,Description,TimeLimitMinutes,PassingScore,IsPublished,DisplayOrder")] Quiz quiz)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == quiz.CategoryId))
                ModelState.AddModelError(nameof(quiz.CategoryId), "Select a valid category.");
            if (ModelState.IsValid)
            {
                quiz.CreatedDate = _clock.Now;
                _context.Add(quiz);
                await _context.SaveChangesAsync();
                await _audit.RecordOperationAsync("CreateQuiz", "Quiz", quiz.Id.ToString(), $"Created quiz '{quiz.Title}'.");
                TempData["Success"] = "Quiz created. Now add questions to it.";
                return RedirectToAction("Editor", "AdminContent", new { type = "quiz", mode = "edit", id = quiz.Id });
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,Title,JapaneseTitle,Description,TimeLimitMinutes,PassingScore,IsPublished,DisplayOrder")] Quiz quiz)
        {
            if (id != quiz.Id) return NotFound();
            if (!await _context.Categories.AnyAsync(c => c.Id == quiz.CategoryId))
                ModelState.AddModelError(nameof(quiz.CategoryId), "Select a valid category.");

            var existingQuiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existingQuiz == null) return NotFound();

            if (ModelState.IsValid)
            {
                // The quiz is already tracked with its questions loaded, so mutating the
                // scalar properties is enough. Calling _context.Update() here would mark
                // every loaded question as Modified too and rewrite them on each save.
                existingQuiz.Title = quiz.Title;
                existingQuiz.JapaneseTitle = quiz.JapaneseTitle;
                existingQuiz.CategoryId = quiz.CategoryId;
                existingQuiz.Description = quiz.Description;
                existingQuiz.TimeLimitMinutes = quiz.TimeLimitMinutes;
                existingQuiz.PassingScore = quiz.PassingScore;
                existingQuiz.IsPublished = quiz.IsPublished;
                existingQuiz.DisplayOrder = quiz.DisplayOrder;

                await _context.SaveChangesAsync();
                await _audit.RecordOperationAsync("EditQuiz", "Quiz", existingQuiz.Id.ToString(), $"Updated quiz '{existingQuiz.Title}'.");
                TempData["Success"] = "Quiz updated successfully.";
                return RedirectToAction("Index", "AdminContent", new { type = "quiz" });
            }
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion([Bind("QuizId,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption,Explanation,DisplayOrder,AudioPath")] QuizQuestion question)
        {
            if (!await _context.Quizzes.AnyAsync(q => q.Id == question.QuizId)) return NotFound();
            if (ModelState.IsValid)
            {
                // Always append after the current last question. The old default of
                // "count + 1" collided after deletions, and the learner view orders
                // questions by DisplayOrder only — duplicates made the order ambiguous.
                var lastOrder = await _context.QuizQuestions
                    .Where(q => q.QuizId == question.QuizId)
                    .MaxAsync(q => (int?)q.DisplayOrder) ?? 0;
                question.DisplayOrder = lastOrder + 1;
                _context.Add(question);
                await _context.SaveChangesAsync();
                await _audit.RecordOperationAsync("AddQuizQuestion", "QuizQuestion", question.Id.ToString(), $"Added question to quiz #{question.QuizId}.");
                TempData["Success"] = "Question added successfully.";
            }
            else
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m)));
            }
            return RedirectToAction("Editor", "AdminContent", new { type = "quiz", mode = "edit", id = question.QuizId });
        }

        public async Task<IActionResult> EditQuestion(int? id)
        {
            if (id == null) return NotFound();
            var question = await _context.QuizQuestions.FindAsync(id);
            if (question == null) return NotFound();
            var quiz = await _context.Quizzes.FindAsync(question.QuizId);
            if (quiz == null) return NotFound();
            ViewBag.QuizId = quiz.Id;
            ViewBag.QuizTitle = quiz.Title;
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(int id, [Bind("Id,QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption,Explanation,DisplayOrder,AudioPath")] QuizQuestion question)
        {
            if (id != question.Id) return NotFound();
            var existing = await _context.QuizQuestions.FindAsync(id);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                // QuizId is intentionally not bindable here — moving a question to another
                // quiz would silently detach it from the attempts already recorded.
                existing.QuestionText = question.QuestionText;
                existing.OptionA = question.OptionA;
                existing.OptionB = question.OptionB;
                existing.OptionC = question.OptionC;
                existing.OptionD = question.OptionD;
                existing.CorrectOption = question.CorrectOption;
                existing.Explanation = question.Explanation;
                existing.DisplayOrder = question.DisplayOrder;
                existing.AudioPath = question.AudioPath;
                await _context.SaveChangesAsync();
                await _audit.RecordOperationAsync("EditQuizQuestion", "QuizQuestion", existing.Id.ToString(), $"Updated question #{existing.Id} in quiz #{existing.QuizId}.");
                TempData["Success"] = "Question updated successfully.";
                return RedirectToAction("Editor", "AdminContent", new { type = "quiz", mode = "edit", id = existing.QuizId });
            }

            var quiz = await _context.Quizzes.FindAsync(existing.QuizId);
            ViewBag.QuizId = existing.QuizId;
            ViewBag.QuizTitle = quiz?.Title;
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int questionId, int quizId)
        {
            var question = await _context.QuizQuestions.FindAsync(questionId);
            if (question != null)
            {
                quizId = question.QuizId;
                _context.QuizQuestions.Remove(question);
                await _context.SaveChangesAsync();
                await _audit.RecordOperationAsync("DeleteQuizQuestion", "QuizQuestion", questionId.ToString(), $"Deleted question #{questionId} from quiz #{quizId}.");
                TempData["Success"] = "Question deleted.";
            }
            return RedirectToAction("Editor", "AdminContent", new { type = "quiz", mode = "edit", id = quizId });
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
                // QuizAttempt cascades from Quiz, so deleting a quiz would take every
                // learner's history from ProgressSummary with it. Block on that first.
                var attemptCount = await _context.QuizAttempts.CountAsync(a => a.QuizId == id);
                if (attemptCount > 0)
                {
                    TempData["Error"] = $"This quiz has {attemptCount} learner attempt(s). Unpublish it instead so those results are preserved.";
                    return RedirectToAction("Editor", "AdminContent", new { type = "quiz", mode = "details", id });
                }

                try
                {
                    _context.Quizzes.Remove(quiz);
                    await _context.SaveChangesAsync();
                    await _audit.RecordOperationAsync("DeleteQuiz", "Quiz", id.ToString(), $"Deleted quiz '{quiz.Title}'.");
                    TempData["Success"] = "Quiz deleted successfully.";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "This quiz could not be deleted because other records still reference it.";
                }
            }
            return RedirectToAction("Index", "AdminContent", new { type = "quiz" });
        }

        private bool QuizExists(int id) => _context.Quizzes.Any(e => e.Id == id);
    }
}
