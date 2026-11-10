using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Services
{
    /// <summary>
    /// Caches the public catalogue shown on the home page.
    /// Entries are no-tracking entities, so sharing them across requests is safe.
    /// The short TTL keeps admin content edits visible without manual invalidation.
    /// </summary>
    public sealed class PublicCatalogCache
    {
        private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);

        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _db;

        public PublicCatalogCache(IMemoryCache cache, ApplicationDbContext db)
        {
            _cache = cache;
            _db = db;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _cache.GetOrCreateAsync("catalog:categories", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Ttl;
                return await _db.Categories
                    .AsNoTracking()
                    .Include(c => c.Lessons.Where(l => l.IsPublished))
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
            }) ?? new List<Category>();
        }

        public async Task<List<Lesson>> GetRecommendedLessonsAsync()
        {
            return await _cache.GetOrCreateAsync("catalog:recommended", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Ttl;
                return await _db.Lessons
                    .AsNoTracking()
                    .Where(l => l.IsPublished)
                    .Include(l => l.Category)
                    .OrderBy(l => l.Category!.DisplayOrder)
                    .ThenBy(l => l.DisplayOrder)
                    .Take(6)
                    .ToListAsync();
            }) ?? new List<Lesson>();
        }

        public async Task<List<Quiz>> GetFeaturedQuizzesAsync()
        {
            return await _cache.GetOrCreateAsync("catalog:quizzes", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Ttl;
                return await _db.Quizzes
                    .AsNoTracking()
                    .Where(q => q.IsPublished)
                    .Include(q => q.Category)
                    .Include(q => q.Questions)
                    .OrderBy(q => q.DisplayOrder)
                    .Take(3)
                    .ToListAsync();
            }) ?? new List<Quiz>();
        }

        public async Task<CatalogCounts> GetCountsAsync()
        {
            return await _cache.GetOrCreateAsync("catalog:counts", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Ttl;
                return new CatalogCounts(
                    await _db.Lessons.CountAsync(l => l.IsPublished),
                    await _db.Vocabulary.CountAsync(),
                    await _db.Quizzes.CountAsync(q => q.IsPublished));
            }) ?? new CatalogCounts(0, 0, 0);
        }
    }

    public sealed record CatalogCounts(int Lessons, int Vocabulary, int Quizzes);
}
