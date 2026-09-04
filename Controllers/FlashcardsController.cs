using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    [Authorize]
    public class FlashcardsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FlashcardsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

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
            return View(cards);
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

            // Order by SRS: due cards first, then new cards
            var cards = await query.OrderBy(v => v.DisplayOrder).ToListAsync();

            if (!cards.Any())
            {
                TempData["Message"] = "No flashcards available for this category yet.";
                return RedirectToAction(nameof(Index));
            }

            // Load user's SRS progress for these cards
            var vocabIds = cards.Select(c => c.Id).ToList();
            var progress = userId != null
                ? await _context.FlashcardProgress
                    .Where(fp => fp.UserId == userId && vocabIds.Contains(fp.VocabularyId))
                    .ToListAsync()
                : new List<FlashcardProgress>();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.SRSProgress = progress.ToDictionary(fp => fp.VocabularyId);
            ViewBag.DueCount = progress.Count(fp => fp.NextReview <= DateTime.Now);

            return View(cards);
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
                    CreatedDate = DateTime.Now
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
                progress.NextReview = DateTime.Now.AddMinutes(10);
            }
            else if (quality == 2)
            {
                // Hard
                progress.IntervalDays = Math.Max(1, (int)(progress.IntervalDays * 1.2));
                progress.EaseFactor = Math.Max(1.3, progress.EaseFactor - 0.15);
                progress.Repetitions++;
                progress.State = 2; // Review
                progress.NextReview = DateTime.Now.AddDays(progress.IntervalDays);
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
                progress.NextReview = DateTime.Now.AddDays(progress.IntervalDays);
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
                progress.NextReview = DateTime.Now.AddDays(progress.IntervalDays);
            }

            progress.LastReview = DateTime.Now;
            await _context.SaveChangesAsync();

            // Cross-link: bump related lesson progress when reviewing flashcards
            await UpdateRelatedLessonProgress(userId, vocabularyId);

            return Json(new
            {
                success = true,
                interval = progress.IntervalDays,
                nextReview = progress.NextReview.ToString("MMM dd"),
                state = progress.State
            });
        }

        /// <summary>
        /// When a user reviews a flashcard, also update the related lesson's progress.
        /// Flashcard practice counts toward lesson completion (~30% of total progress).
        /// Reading the lesson = 70%, practicing flashcards = 30%.
        /// </summary>
        private async Task UpdateRelatedLessonProgress(string userId, int vocabularyId)
        {
            // Find the category of this vocabulary
            var vocab = await _context.Vocabulary.FindAsync(vocabularyId);
            if (vocab == null) return;

            // Find all published lessons in the same category
            var categoryLessons = await _context.Lessons
                .Where(l => l.CategoryId == vocab.CategoryId && l.IsPublished)
                .ToListAsync();

            if (!categoryLessons.Any()) return;

            // Count total vocabulary in this category and how many the user has reviewed
            var totalVocabInCategory = await _context.Vocabulary
                .CountAsync(v => v.CategoryId == vocab.CategoryId);

            var reviewedVocabInCategory = await _context.FlashcardProgress
                .CountAsync(fp => fp.UserId == userId &&
                    fp.State >= 1 &&
                    _context.Vocabulary.Any(v => v.Id == fp.VocabularyId && v.CategoryId == vocab.CategoryId));

            // Flashcard mastery = how much of the category's vocab the user has learned
            var flashcardMasteryPct = totalVocabInCategory > 0
                ? (int)Math.Round((double)reviewedVocabInCategory / totalVocabInCategory * 100)
                : 0;

            // Cap flashcard contribution at 30% of total lesson progress
            var flashcardContribution = (int)Math.Round(flashcardMasteryPct * 0.3);

            foreach (var lesson in categoryLessons)
            {
                var lessonProgress = await _context.LessonProgress
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lesson.Id);

                if (lessonProgress == null)
                {
                    // Create progress entry for this lesson
                    lessonProgress = new LessonProgress
                    {
                        UserId = userId,
                        LessonId = lesson.Id,
                        StartedDate = DateTime.Now,
                        CompletionPercentage = flashcardContribution,
                        IsCompleted = flashcardContribution >= 100,
                        CompletedDate = flashcardContribution >= 100 ? DateTime.Now : null
                    };
                    _context.LessonProgress.Add(lessonProgress);
                }
                else if (!lessonProgress.IsCompleted)
                {
                    // Reading progress (70% weight) + flashcard progress (30% weight)
                    // The reading portion is tracked by scroll progress in Details.cshtml
                    // We calculate: newPct = max(existing, flashcardContribution)
                    // Since reading tracks its own portion separately, we take the max
                    // to avoid overwriting reading progress
                    var newPct = Math.Max(lessonProgress.CompletionPercentage, flashcardContribution);
                    if (newPct > lessonProgress.CompletionPercentage)
                    {
                        lessonProgress.CompletionPercentage = Math.Min(100, newPct);
                        if (lessonProgress.CompletionPercentage >= 100)
                        {
                            lessonProgress.IsCompleted = true;
                            lessonProgress.CompletedDate = DateTime.Now;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
