using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ProgressSummaryService _progressSummary;

        public DashboardController(UserManager<IdentityUser> userManager, ProgressSummaryService progressSummary)
        {
            _userManager = userManager;
            _progressSummary = progressSummary;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            return View(await _progressSummary.BuildAsync(userId));
        }
    }
}
