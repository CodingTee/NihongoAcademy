using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Controllers
{
    [Authorize]
    public class FlashcardsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAppClock _clock;

        public FlashcardsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IAppClock clock)
        {
            _context = context;
            _userManager = userManager;
            _clock = clock;
        }

        public async Task<IActionResult> Session(string mode = "browse", int? categoryId = null)
            => mode.Equals("practice", StringComparison.OrdinalIgnoreCase)
                ? await Practice(categoryId)
                : await Index(categoryId);

        public async Task<IActionResult> Index(int? categoryId)
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();

            var query = _context.Vocabulary.Include(v => v.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(v => v.CategoryId == categoryId.Value);
                ViewBag.SelectedCategory = categoryId.Value;
            }

            var cards = await query.OrderBy(v => v.DisplayOrder).ToListAsync();
            return View(nameof(Index), cards);
        }

        public async Task<IActionResult> Practice(int? categoryId)
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Vocabulary.Include(v => v.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(v => v.CategoryId == categoryId.Value);
                ViewBag.CategoryName = await _context.Categories
                    .Where(c => c.Id == categoryId.Value)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
            }

            // Load all cards first; SRS due-first ordering is applied further below,
            // after each card's progress is loaded.
            var cards = await query.OrderBy(v => v.DisplayOrder).ToListAsync();

            if (!cards.Any())
            {
                TempData["Message"] = "No cards are due for this category right now. Great job! Come back later or pick another category.";
                return RedirectToAction(nameof(Index));
            }

            // Load user's SRS progress for these cards
            var vocabIds = cards.Select(c => c.Id).ToList();
            var progress = userId != null
                ? await _context.FlashcardProgress
                    .Where(fp => fp.UserId == userId && vocabIds.Contains(fp.VocabularyId))
                    .ToListAsync()
                : new List<FlashcardProgress>();

            // SRS due-first ordering: include only cards that are due (NextReview <= now)
            // or brand-new (no progress yet), then sort due cards ahead of new ones.
            // Reviewed-but-not-due cards are excluded until they come due again.
            var now = _clock.Now;
            var progressMap = progress.ToDictionary(fp => fp.VocabularyId);
            cards = cards
                .Where(c => !progressMap.ContainsKey(c.Id) || progressMap[c.Id].NextReview <= now)
                .OrderBy(c =>
                {
                    if (!progressMap.ContainsKey(c.Id)) return 1;          // new cards after due
                    return progressMap[c.Id].NextReview <= now ? 0 : 2;    // due first
                })
                .ThenBy(c => progressMap.ContainsKey(c.Id) ? progressMap[c.Id].NextReview : DateTime.MinValue)
                .ThenBy(c => c.DisplayOrder)
                .ToList();

            if (!cards.Any())
            {
                TempData["Message"] = "All caught up! No cards are currently due for review in this category.";
                return RedirectToAction(nameof(Index), new { categoryId });
            }

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.SRSProgress = progressMap;
            ViewBag.DueCount = progress.Count(fp => fp.NextReview <= now);

            return View(nameof(Practice), cards);
        }

        /// <summary>
        /// Record SRS review result for a flashcard (AJAX endpoint).
        /// quality: 1=Again, 2=Hard, 3=Good, 4=Easy
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordReview(int vocabularyId, int quality)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var progress = await _context.FlashcardProgress
                .FirstOrDefaultAsync(fp => fp.UserId == userId && fp.VocabularyId == vocabularyId);

            if (progress == null)
            {
                progress = new FlashcardProgress
                {
                    UserId = userId,
                    VocabularyId = vocabularyId,
                    State = 0, // New
                    EaseFactor = 2.5,
                    Repetitions = 0,
                    CreatedDate = _clock.Now
                };
                _context.FlashcardProgress.Add(progress);
            }

            // SM-2 algorithm
            if (quality < 2)
            {
                // Again — reset
                progress.Repetitions = 0;
                progress.IntervalDays = 0;
                progress.State = 1; // Learning
                progress.EaseFactor = Math.Max(1.3, progress.EaseFactor - 0.2);
                progress.NextReview = _clock.Now.AddMinutes(10);
            }
            else if (quality == 2)
            {
                // Hard
                progress.IntervalDays = Math.Max(1, (int)(progress.IntervalDays * 1.2));
                progress.EaseFactor = Math.Max(1.3, progress.EaseFactor - 0.15);
                progress.Repetitions++;
                progress.State = 2; // Review
                progress.NextReview = _clock.Now.AddDays(progress.IntervalDays);
            }
            else if (quality == 3)
            {
                // Good — standard SM-2
                if (progress.Repetitions == 0)
                    progress.IntervalDays = 1;
                else if (progress.Repetitions == 1)
                    progress.IntervalDays = 6;
                else
                    progress.IntervalDays = (int)(progress.IntervalDays * progress.EaseFactor);

                progress.Repetitions++;
                progress.State = 2; // Review
                progress.NextReview = _clock.Now.AddDays(progress.IntervalDays);
            }
            else
            {
                // Easy
                progress.IntervalDays = progress.Repetitions == 0
                    ? 4
                    : (int)(progress.IntervalDays * progress.EaseFactor * 1.3);
                progress.EaseFactor += 0.15;
                progress.Repetitions++;
                progress.State = 2; // Review
                progress.NextReview = _clock.Now.AddDays(progress.IntervalDays);
            }

            progress.LastReview = _clock.Now;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                interval = progress.IntervalDays,
                nextReview = progress.NextReview.ToString("MMM dd"),
                state = progress.State
            });
        }

        // Flashcard review no longer mutates LessonProgress.CompletionPercentage. Under the
        // current mastery model, lesson completion is driven solely by LessonMasteryService
        // (reading scroll caps at 70%, first pass at 90%, stable at 100%), and flashcards are
        // tracked by their own SRS (FlashcardProgress). Writing a capped 30% baseline here only
        // created phantom progress on lessons the user had not opened, so the cross-link is gone.
    }
}
