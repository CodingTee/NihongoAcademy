using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    [Authorize]
    public class QuizzesController : Controller
    {
        private const string A1FinalTitle = "A1 Final Can-do Assessment";
        private const string KanjiFinalTitle = "N5 Kanji Final Assessment";
        private const string A2FinalTitle = "A2 Final Can-do Assessment";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuizzesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.IsPublished)
                .Include(q => q.Category)
                .Include(q => q.Questions)
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();

            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var attempts = await _context.QuizAttempts
                    .Where(a => a.UserId == userId)
                    .ToListAsync();
                ViewBag.UserAttempts = attempts;
                ViewBag.LockedQuizIds = await GetLockedQuizIdsAsync(quizzes, userId);
            }

            return View(quizzes);
        }

        // A quiz is one dynamic session page whose state is selected by mode.
        public async Task<IActionResult> Session(string mode = "take", int? id = null)
        {
            if (mode.Equals("history", StringComparison.OrdinalIgnoreCase))
                return await History();
            if (!id.HasValue) return RedirectToAction(nameof(Index));
            return await Take(id.Value);
        }

        public async Task<IActionResult> Take(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions.OrderBy(qq => qq.DisplayOrder))
                .Include(q => q.Category)
                .FirstOrDefaultAsync(q => q.Id == id && q.IsPublished);

            if (quiz == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();
            var lockMessage = await GetLockMessageAsync(quiz.Title, userId);
            if (lockMessage != null)
            {
                TempData["Error"] = lockMessage;
                return RedirectToAction(nameof(Index));
            }

            return View(nameof(Take), quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int quizId, int timeTakenSeconds, List<int> answers)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.IsPublished);

            if (quiz == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();
            if (await GetLockMessageAsync(quiz.Title, userId) != null)
                return Forbid();

            answers ??= new List<int>();
            int totalQuestions = quiz.Questions.Count;
            int correctAnswers = 0;

            for (int i = 0; i < quiz.Questions.Count && i < answers.Count; i++)
            {
                var question = quiz.Questions.OrderBy(q => q.DisplayOrder).ElementAt(i);
                if (answers[i] == question.CorrectOption)
                {
                    correctAnswers++;
                }
            }

            int scorePercentage = totalQuestions > 0 ? (correctAnswers * 100 / totalQuestions) : 0;
            bool isPassed = scorePercentage >= quiz.PassingScore;

            var attempt = new QuizAttempt
            {
                UserId = userId,
                QuizId = quizId,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                ScorePercentage = scorePercentage,
                IsPassed = isPassed,
                TimeTakenSeconds = Math.Clamp(timeTakenSeconds, 0, quiz.TimeLimitMinutes * 60),
                AttemptDate = DateTime.Now
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            var viewModel = new QuizResultViewModel
            {
                Quiz = quiz,
                Attempt = attempt,
                UserAnswers = answers
            };

            return View("Results", viewModel);
        }

        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var attempts = await _context.QuizAttempts
                .Where(a => a.UserId == userId)
                .Include(a => a.Quiz)
                .ThenInclude(q => q!.Category)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();

            return View(nameof(History), attempts);
        }

        private async Task<HashSet<int>> GetLockedQuizIdsAsync(IEnumerable<Quiz> quizzes, string userId)
        {
            var locked = new HashSet<int>();
            if (!await HasCompletedCategoryAsync(userId, "A1 Course", "A1:"))
            {
                var finalQuiz = quizzes.FirstOrDefault(q => q.Title == A1FinalTitle);
                if (finalQuiz != null) locked.Add(finalQuiz.Id);
            }
            if (!await HasCompletedCategoryAsync(userId, "Kanji", "Kanji Foundations:"))
            {
                var finalQuiz = quizzes.FirstOrDefault(q => q.Title == KanjiFinalTitle);
                if (finalQuiz != null) locked.Add(finalQuiz.Id);
            }
            if (!await HasCompletedCategoryAsync(userId, "A2 Course", "A2:"))
            {
                var finalQuiz = quizzes.FirstOrDefault(q => q.Title == A2FinalTitle);
                if (finalQuiz != null) locked.Add(finalQuiz.Id);
            }
            return locked;
        }

        private async Task<string?> GetLockMessageAsync(string quizTitle, string userId)
        {
            if (quizTitle == A1FinalTitle && !await HasCompletedCategoryAsync(userId, "A1 Course", "A1:"))
                return "Complete every A1 course lesson with stable mastery before taking the final assessment.";
            if (quizTitle == KanjiFinalTitle && !await HasCompletedCategoryAsync(userId, "Kanji", "Kanji Foundations:"))
                return "Complete every kanji lesson with stable mastery before taking the N5 kanji assessment.";
            if (quizTitle == A2FinalTitle && !await HasCompletedCategoryAsync(userId, "A2 Course", "A2:"))
                return "Complete every A2 course lesson with stable mastery before taking the A2 final assessment.";
            return null;
        }

        /// <summary>
        /// True when every published lesson in the category has reached stable mastery.
        /// The expected count is read from the database rather than hardcoded: a baked-in
        /// number would silently lock the final assessment forever once content changed.
        /// </summary>
        private async Task<bool> HasCompletedCategoryAsync(string userId, string categoryName, string titlePrefix)
        {
            var lessonIds = await _context.Lessons
                .Where(l => l.IsPublished && l.Category!.Name == categoryName && l.Title.StartsWith(titlePrefix))
                .Select(l => l.Id)
                .ToListAsync();
            if (lessonIds.Count == 0) return false;
            var masteredCount = await _context.LessonProgress.CountAsync(p =>
                p.UserId == userId && p.IsCompleted && lessonIds.Contains(p.LessonId));
            return masteredCount == lessonIds.Count;
        }
    }

    public class QuizResultViewModel
    {
        public Quiz Quiz { get; set; } = new();
        public QuizAttempt Attempt { get; set; } = new();
        public List<int> UserAnswers { get; set; } = new();
    }
}
