using Microsoft.EntityFrameworkCore;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task SeedVocabularyExampleReadingsAsync(ApplicationDbContext context)
        {
            var examples = await context.Vocabulary
                .Where(item => item.ExampleJapanese != null && item.ExampleJapanese != "")
                .ToListAsync();

            foreach (var item in examples.Where(item => string.IsNullOrWhiteSpace(item.ExampleReading) &&
                !System.Text.RegularExpressions.Regex.IsMatch(item.ExampleJapanese!, "[一-龯々]")))
                item.ExampleReading = item.ExampleJapanese;

            await context.SaveChangesAsync();

            if (examples.Any(item => string.IsNullOrWhiteSpace(item.ExampleReading)))
                throw new InvalidOperationException("Every vocabulary example must have an explicit kana reading.");
            if (examples.Any(item => System.Text.RegularExpressions.Regex.IsMatch(item.ExampleReading!, "[一-龯々]")))
                throw new InvalidOperationException("Vocabulary example readings must not contain kanji.");
        }
    }
}
