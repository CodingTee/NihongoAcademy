using Microsoft.EntityFrameworkCore;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task ValidateHiraganaCurriculumAsync(ApplicationDbContext context)
        {
            var lessons = await context.Lessons
                .Where(l => l.IsPublished && l.Category!.Name == "Hiragana")
                .Include(l => l.LessonVocabulary)
                .ThenInclude(lv => lv.Vocabulary)
                .Include(l => l.Prerequisites)
                .OrderBy(l => l.DisplayOrder)
                .ThenBy(l => l.Id)
                .ToListAsync();

            var expectedCounts = new[] { 5, 5, 5, 5, 5, 5, 8, 8, 25, 25, 8, 10, 15 };
            if (lessons.Count != expectedCounts.Length)
                throw new InvalidOperationException($"Hiragana curriculum must contain {expectedCounts.Length} published lessons; found {lessons.Count}.");

            for (var i = 0; i < lessons.Count; i++)
            {
                var lesson = lessons[i];
                if (lesson.DisplayOrder != i)
                    throw new InvalidOperationException($"Hiragana lesson order {i} is occupied by '{lesson.Title}' with DisplayOrder {lesson.DisplayOrder}.");
                if (lesson.LessonVocabulary.Count != expectedCounts[i])
                    throw new InvalidOperationException($"'{lesson.Title}' must contain {expectedCounts[i]} target items; found {lesson.LessonVocabulary.Count}.");
                if (lesson.LessonVocabulary.Any(x => x.Vocabulary?.CategoryId != lesson.CategoryId))
                    throw new InvalidOperationException($"'{lesson.Title}' contains an item from another category.");
                if (lesson.LessonVocabulary.Any(x => string.IsNullOrWhiteSpace(x.Vocabulary?.Romaji)))
                    throw new InvalidOperationException($"'{lesson.Title}' contains an item without a reading.");

                if (i == 0)
                {
                    if (lesson.Prerequisites.Count != 0)
                        throw new InvalidOperationException("The first Hiragana lesson must not have a prerequisite.");
                }
                else if (lesson.Prerequisites.Count != 1 ||
                    lesson.Prerequisites.Single().PrerequisiteLessonId != lessons[i - 1].Id)
                {
                    throw new InvalidOperationException($"'{lesson.Title}' must require only '{lessons[i - 1].Title}'.");
                }
            }

            var expectedBasicKana = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん";
            var actualBasicKana = string.Concat(lessons.Take(8)
                .SelectMany(l => l.LessonVocabulary.OrderBy(x => x.DisplayOrder))
                .Select(x => x.Vocabulary!.Japanese));
            if (!string.Equals(actualBasicKana, expectedBasicKana, StringComparison.Ordinal))
                throw new InvalidOperationException($"Basic Hiragana sequence is invalid. Expected '{expectedBasicKana}', found '{actualBasicKana}'.");
        }
    }
}
