using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminAudioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AudioAssetService _audioAssets;

        public AdminAudioController(ApplicationDbContext context, AudioAssetService audioAssets)
        {
            _context = context;
            _audioAssets = audioAssets;
        }

        public async Task<IActionResult> Index(string? status = null)
        {
            var activities = await _context.LessonActivities.AsNoTracking()
                .Where(activity => activity.Lesson!.IsPublished &&
                    (activity.ActivityType == "dialogue" || activity.ActivityType == "comprehension") &&
                    activity.JapaneseText != null)
                .Include(activity => activity.Lesson)
                .ThenInclude(lesson => lesson!.Category)
                .OrderBy(activity => activity.Lesson!.Category!.DisplayOrder)
                .ThenBy(activity => activity.Lesson!.DisplayOrder)
                .ThenBy(activity => activity.DisplayOrder)
                .ToListAsync();

            var model = new AudioCoverageViewModel
            {
                Items = activities.Select(activity =>
                {
                    var slow = _audioAssets.Resolve(activity, "slow");
                    var natural = _audioAssets.Resolve(activity, "natural");
                    return new AudioCoverageItemViewModel
                    {
                        LessonId = activity.LessonId,
                        ActivityId = activity.Id,
                        LessonTitle = activity.Lesson!.Title,
                        ActivityType = activity.ActivityType,
                        JapanesePreview = activity.JapaneseText!.Length <= 80 ? activity.JapaneseText : activity.JapaneseText[..80] + "…",
                        SlowUrl = slow.Url ?? slow.ExpectedUrl,
                        NaturalUrl = natural.Url ?? natural.ExpectedUrl,
                        SlowExists = slow.Exists,
                        NaturalExists = natural.Exists,
                        SlowExternal = slow.IsExternal,
                        NaturalExternal = natural.IsExternal
                    };
                }).ToList()
            };
            model.TotalRequiredFileCount = model.Items.Count * 2;
            model.TotalAvailableFileCount = model.Items.Sum(item =>
                (item.SlowExists ? 1 : 0) + (item.NaturalExists ? 1 : 0));

            if (string.Equals(status, "missing", StringComparison.OrdinalIgnoreCase))
                model.Items = model.Items.Where(item => !item.SlowExists || !item.NaturalExists).ToList();
            ViewBag.Status = status;
            return View(model);
        }
    }
}
