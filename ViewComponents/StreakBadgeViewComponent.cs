using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NihongoAcademy.Services;

namespace NihongoAcademy.ViewComponents
{
    /// <summary>
    /// Compact "streak / daily goal" badge shown in the navbar
    /// (replaces the old motivation strip under the home page hero banner).
    /// Shown on every page, for every visitor, so the navbar stays consistent.
    /// </summary>
    public class StreakBadgeViewComponent : ViewComponent
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ProgressSummaryService _progressSummary;

        public StreakBadgeViewComponent(UserManager<IdentityUser> userManager, ProgressSummaryService progressSummary)
        {
            _userManager = userManager;
            _progressSummary = progressSummary;
        }

        public class StreakBadgeViewModel
        {
            public int Streak { get; set; }
            public int Progress { get; set; }
            public int Goal { get; set; } = 10;
            public bool IsAuthenticated { get; set; }
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var isAuthenticated = UserClaimsPrincipal?.Identity?.IsAuthenticated == true;
            var userId = isAuthenticated ? _userManager.GetUserId(UserClaimsPrincipal!) : null;

            // Anonymous visitors see a friendly default badge so the navbar
            // looks consistent on every page (matches the design reference).
            if (!isAuthenticated || userId == null)
            {
                return View(new StreakBadgeViewModel
                {
                    Streak = 0,
                    Progress = 0,
                    Goal = 10,
                    IsAuthenticated = false,
                });
            }

            var summary = await _progressSummary.BuildNavbarAsync(userId);

            var model = new StreakBadgeViewModel
            {
                Streak = summary.Streak,
                Progress = summary.DailyProgress,
                Goal = summary.DailyGoal,
                IsAuthenticated = true,
            };

            return View(model);
        }
    }
}
