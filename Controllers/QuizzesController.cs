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
            }

            return View(quizzes);
        }

        public async Task<IActionResult> Take(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions.OrderBy(qq => qq.DisplayOrder))
                .Include(q => q.Category)
                .FirstOrDefaultAsync(q => q.Id == id && q.IsPublished);

            if (quiz == null) return NotFound();

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int quizId, int timeTakenSeconds, List<int> answers)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return NotFound();

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
                UserId = _userManager.GetUserId(User)!,
                QuizId = quizId,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                ScorePercentage = scorePercentage,
                IsPassed = isPassed,
                TimeTakenSeconds = timeTakenSeconds,
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

            return View(attempts);
        }
    }

    public class QuizResultViewModel
    {
        public Quiz Quiz { get; set; } = new();
        public QuizAttempt Attempt { get; set; } = new();
        public List<int> UserAnswers { get; set; } = new();
    }
}
