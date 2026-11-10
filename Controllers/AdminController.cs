using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IAppClock _clock;
        private readonly AuditLogService _audit;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, IAppClock clock, AuditLogService audit)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _clock = clock;
            _audit = audit;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Index(string? search)
        {
            var totalLessons = await _context.Lessons.CountAsync();
            var publishedLessons = await _context.Lessons.CountAsync(l => l.IsPublished);
            var totalVocab = await _context.Vocabulary.CountAsync();
            var totalQuizzes = await _context.Quizzes.CountAsync();
            var totalQuizQuestions = await _context.QuizQuestions.CountAsync();
            var totalUsers = await _userManager.Users.CountAsync();
            var totalAttempts = await _context.QuizAttempts.CountAsync();
            var totalProgress = await _context.LessonProgress.CountAsync();
            var coreCategories = new[] { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course" };
            var coreLessons = await _context.Lessons.CountAsync(l => l.IsPublished && coreCategories.Contains(l.Category!.Name));
            var lessonActivities = await _context.LessonActivities.CountAsync();
            var readingHelpActivities = await _context.LessonActivities.CountAsync(a => !string.IsNullOrEmpty(a.ReadingSupportText));
            var integratedTasks = await _context.ListeningTasks.CountAsync() + await _context.ReadingPassages.CountAsync() +
                await _context.SpeakingTasks.CountAsync() + await _context.WritingTasks.CountAsync();
            var linkedIntegratedTasks = await _context.ListeningTasks.CountAsync(t => t.LessonId != null) + await _context.ReadingPassages.CountAsync(t => t.LessonId != null) +
                await _context.SpeakingTasks.CountAsync(t => t.LessonId != null) + await _context.WritingTasks.CountAsync(t => t.LessonId != null);

            ViewBag.PendingReportCount = await _context.ContentReports.CountAsync(r => r.Status == ReportStatus.Pending);

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
            ViewBag.CoreLessons = coreLessons;
            ViewBag.LessonActivities = lessonActivities;
            ViewBag.ReadingHelpActivities = readingHelpActivities;
            ViewBag.IntegratedTasks = integratedTasks;
            ViewBag.LinkedIntegratedTasks = linkedIntegratedTasks;
            ViewBag.RecentAttempts = recentAttempts;

            return View(new List<IdentityUser>());
        }

        [Authorize(Roles = "SuperAdmin")]
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
                await _userManager.UpdateSecurityStampAsync(user);
                await _audit.RecordOperationAsync("DemoteAdmin", "IdentityUser", user.Id, $"Removed Admin role from {user.Email}.");
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
                await _userManager.UpdateSecurityStampAsync(user);
                await _audit.RecordOperationAsync("PromoteAdmin", "IdentityUser", user.Id, $"Promoted {user.Email} to Admin.");
                TempData["Success"] = $"Promoted {user.Email} to Admin.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ProgressTesting(string? userId)
        {
            var users = await _userManager.Users.OrderBy(user => user.Email).ToListAsync();
            var selectedUserId = users.Any(user => user.Id == userId)
                ? userId!
                : _userManager.GetUserId(User) ?? users.FirstOrDefault()?.Id ?? string.Empty;
            var model = await BuildProgressTestingModelAsync(users, selectedUserId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimulateProgress(string userId, int targetLessonId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var orderedLessons = await GetOrderedCoreLessonsAsync();
            var targetIndex = orderedLessons.FindIndex(lesson => lesson.Id == targetLessonId);
            if (targetIndex < 0)
            {
                TempData["Error"] = "The selected course stage is not available.";
                return RedirectToAction(nameof(ProgressTesting), new { userId });
            }

            if (await _context.LessonProgress.AnyAsync(progress => progress.UserId == userId && !progress.IsAdminSimulated))
            {
                TempData["Error"] = "This account already has genuine lesson progress. Use a clean dedicated test account so real study data cannot be overwritten.";
                return RedirectToAction(nameof(ProgressTesting), new { userId });
            }

            var now = _clock.Now;
            var selectedIds = orderedLessons.Take(targetIndex + 1).Select(lesson => lesson.Id).ToHashSet();
            var existing = await _context.LessonProgress
                .Where(progress => progress.UserId == userId && selectedIds.Contains(progress.LessonId))
                .ToDictionaryAsync(progress => progress.LessonId);

            foreach (var lessonId in selectedIds)
            {
                if (existing.TryGetValue(lessonId, out var progress))
                {
                    ApplySimulatedMastery(progress, now);
                }
                else
                {
                    var newProgress = new LessonProgress { UserId = userId, LessonId = lessonId, IsAdminSimulated = true };
                    ApplySimulatedMastery(newProgress, now);
                    _context.LessonProgress.Add(newProgress);
                }
            }

            await _context.SaveChangesAsync();
            await _audit.RecordOperationAsync("SimulateProgress", "LessonProgress", userId,
                $"Simulated stable mastery through {orderedLessons[targetIndex].Title} for {user.Email}.");
            TempData["Success"] = $"Simulated stable mastery through {orderedLessons[targetIndex].Title} for {user.Email}. Existing real progress was preserved.";
            return RedirectToAction(nameof(ProgressTesting), new { userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearSimulatedProgress(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var simulated = await _context.LessonProgress
                .Where(progress => progress.UserId == userId && progress.IsAdminSimulated)
                .ToListAsync();
            _context.LessonProgress.RemoveRange(simulated);
            await _context.SaveChangesAsync();
            await _audit.RecordOperationAsync("ClearSimulatedProgress", "LessonProgress", userId,
                $"Removed {simulated.Count} simulated progress records from {user.Email}.");
            TempData["Success"] = $"Removed {simulated.Count} simulated progress records from {user.Email}. Real study progress was preserved.";
            return RedirectToAction(nameof(ProgressTesting), new { userId });
        }

        private async Task<AdminProgressTestingViewModel> BuildProgressTestingModelAsync(List<IdentityUser> users, string selectedUserId)
        {
            var lessons = await GetOrderedCoreLessonsAsync();
            var progress = await _context.LessonProgress.Where(item => item.UserId == selectedUserId).ToListAsync();
            return new AdminProgressTestingViewModel
            {
                SelectedUserId = selectedUserId,
                Users = users.Select(user => new SelectListItem(user.Email ?? user.UserName ?? user.Id, user.Id,
                    user.Id == selectedUserId)).ToList(),
                Stages = lessons.Select((lesson, index) => new AdminProgressStageViewModel
                {
                    LessonId = lesson.Id,
                    StageLabel = lesson.Title,
                    CategoryName = lesson.Category!.Name,
                    SequenceNumber = index + 1
                }).ToList(),
                SimulatedLessonCount = progress.Count(item => item.IsAdminSimulated),
                RealLessonCount = progress.Count(item => !item.IsAdminSimulated && item.StableMasteryDate != null)
            };
        }

        private async Task<List<Lesson>> GetOrderedCoreLessonsAsync()
        {
            var categoryOrder = new[] { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course" };
            var lessons = await _context.Lessons.Include(lesson => lesson.Category)
                .Where(lesson => lesson.IsPublished && categoryOrder.Contains(lesson.Category!.Name)).ToListAsync();
            return lessons.OrderBy(lesson => Array.IndexOf(categoryOrder, lesson.Category!.Name))
                .ThenBy(lesson => lesson.DisplayOrder).ThenBy(lesson => lesson.Id).ToList();
        }

        private static void ApplySimulatedMastery(LessonProgress progress, DateTime now)
        {
            progress.IsCompleted = true;
            progress.CompletionPercentage = 100;
            progress.StartedDate ??= now.AddDays(-3);
            progress.FirstMasteryPassedDate = now.AddDays(-2);
            progress.MasteryReviewDueDate = now.AddDays(-1);
            progress.CompletedDate = now;
            progress.StableMasteryDate = now;
            progress.IsAdminSimulated = true;
        }
    }
}
