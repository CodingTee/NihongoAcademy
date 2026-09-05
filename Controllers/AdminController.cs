using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var totalLessons = await _context.Lessons.CountAsync();
            var publishedLessons = await _context.Lessons.CountAsync(l => l.IsPublished);
            var totalVocab = await _context.Vocabulary.CountAsync();
            var totalQuizzes = await _context.Quizzes.CountAsync();
            var totalQuizQuestions = await _context.QuizQuestions.CountAsync();
            var totalUsers = await _userManager.Users.CountAsync();
            var totalAttempts = await _context.QuizAttempts.CountAsync();
            var totalProgress = await _context.LessonProgress.CountAsync();

            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            var recentAttempts = await _context.QuizAttempts
                .Include(a => a.Quiz)
                .Include(a => a.User)
                .OrderByDescending(a => a.AttemptDate)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalLessons = totalLessons;
            ViewBag.PublishedLessons = publishedLessons;
            ViewBag.TotalVocab = totalVocab;
            ViewBag.TotalQuizzes = totalQuizzes;
            ViewBag.TotalQuizQuestions = totalQuizQuestions;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalAttempts = totalAttempts;
            ViewBag.TotalProgress = totalProgress;
            ViewBag.UserRoles = userRoles;
            ViewBag.RecentAttempts = recentAttempts;

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "The final administrator account cannot be demoted.";
                    return RedirectToAction(nameof(Index));
                }

                var removeResult = await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = string.Join(" ", removeResult.Errors.Select(error => error.Description));
                    return RedirectToAction(nameof(Index));
                }

                if (!roles.Contains("Student"))
                {
                    var studentResult = await _userManager.AddToRoleAsync(user, "Student");
                    if (!studentResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                        TempData["Error"] = string.Join(" ", studentResult.Errors.Select(error => error.Description));
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Success"] = $"Removed Admin role from {user.Email}.";
            }
            else
            {
                var result = await _userManager.AddToRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Description));
                    return RedirectToAction(nameof(Index));
                }
                TempData["Success"] = $"Promoted {user.Email} to Admin.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
