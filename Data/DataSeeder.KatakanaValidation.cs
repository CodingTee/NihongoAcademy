using Microsoft.EntityFrameworkCore;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task ValidateKatakanaCurriculumAsync(ApplicationDbContext context)
        {
            var lessons = await context.Lessons.Where(lesson => lesson.IsPublished && lesson.Category!.Name == "Katakana")
                .Include(lesson => lesson.LessonVocabulary).ThenInclude(link => link.Vocabulary)
                .Include(lesson => lesson.Prerequisites).ThenInclude(prerequisite => prerequisite.PrerequisiteLesson)
                .OrderBy(lesson => lesson.DisplayOrder).ThenBy(lesson => lesson.Id).ToListAsync();
            var expectedCounts = new[] { 5, 5, 5, 5, 5, 5, 8, 8, 25, 33, 10, 17, 12, 13 };
            if (lessons.Count != expectedCounts.Length)
                throw new InvalidOperationException($"Katakana curriculum must contain {expectedCounts.Length} published lessons; found {lessons.Count}.");

            for (var index = 0; index < lessons.Count; index++)
            {
                var lesson = lessons[index];
                if (lesson.DisplayOrder != index || lesson.LessonVocabulary.Count != expectedCounts[index])
                    throw new InvalidOperationException($"Katakana lesson '{lesson.Title}' has an invalid order or target count.");
                if (lesson.LessonVocabulary.Any(link => link.Vocabulary?.CategoryId != lesson.CategoryId ||
                    string.IsNullOrWhiteSpace(link.Vocabulary.Romaji)))
                    throw new InvalidOperationException($"Katakana lesson '{lesson.Title}' contains an invalid target or reading.");
                if (index == 0)
                {
                    var requiredTitle = lesson.Prerequisites.SingleOrDefault()?.PrerequisiteLesson?.Title;
                    if (requiredTitle != "Hiragana: Integrated reading")
                        throw new InvalidOperationException("The first Katakana lesson must follow the completed Hiragana pathway.");
                }
                else if (lesson.Prerequisites.Count != 1 || lesson.Prerequisites.Single().PrerequisiteLessonId != lessons[index - 1].Id)
                    throw new InvalidOperationException($"Katakana lesson '{lesson.Title}' must require only the previous Katakana lesson.");
            }

            const string expectedBasic = "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン";
            var actualBasic = string.Concat(lessons.Take(8).SelectMany(lesson => lesson.LessonVocabulary.OrderBy(link => link.DisplayOrder))
                .Select(link => link.Vocabulary!.Japanese));
            if (!string.Equals(actualBasic, expectedBasic, StringComparison.Ordinal))
                throw new InvalidOperationException($"Basic Katakana sequence is invalid. Expected '{expectedBasic}', found '{actualBasic}'.");
        }
    }
}
