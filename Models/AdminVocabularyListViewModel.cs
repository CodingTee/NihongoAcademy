using Microsoft.EntityFrameworkCore;

namespace NihongoAcademy.Models
{
    /// <summary>
    /// Paginated vocabulary list for the admin screens. The raw table holds
    /// 650+ rows; rendering all of them at once costs ~11k DOM nodes and
    /// several 60ms long tasks, which makes the page feel stuck on slower
    /// machines (laggy mouse, unresponsive clicks). 50 rows per page keeps
    /// the DOM small and the page snappy.
    /// </summary>
    public class AdminVocabularyListViewModel
    {
        public const int PageSize = 50;

        public List<Vocabulary> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }

        public static async Task<AdminVocabularyListViewModel> CreateAsync(IQueryable<Vocabulary> query, int page)
        {
            var model = new AdminVocabularyListViewModel
            {
                TotalCount = await query.CountAsync()
            };
            model.TotalPages = Math.Max(1, (int)Math.Ceiling(model.TotalCount / (double)PageSize));
            model.Page = Math.Min(Math.Max(page, 1), model.TotalPages);
            model.Items = await query
                .Skip((model.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
            return model;
        }
    }
}
