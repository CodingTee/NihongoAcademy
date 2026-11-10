using Microsoft.EntityFrameworkCore;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task ValidateGrammarCurriculumAsync(ApplicationDbContext context)
        {
            var points = await context.GrammarPoints
                .Where(point => point.Level == "A1" || point.Level == "A2")
                .Include(point => point.Lessons)
                    .ThenInclude(link => link.Lesson)
                        .ThenInclude(lesson => lesson!.Category)
                .ToListAsync();

            if (points.Count(point => point.Level == "A1") != 18 || points.Count(point => point.Level == "A2") != 15)
                throw new InvalidOperationException("Grammar curriculum requires exactly 18 A1 and 15 A2 grammar points.");

            if (points.Select(point => point.Code).Distinct(StringComparer.Ordinal).Count() != points.Count)
                throw new InvalidOperationException("Grammar point codes must be unique.");

            foreach (var point in points)
            {
                if (string.IsNullOrWhiteSpace(point.Code) || string.IsNullOrWhiteSpace(point.Pattern) ||
                    string.IsNullOrWhiteSpace(point.Meaning) || string.IsNullOrWhiteSpace(point.Formation) ||
                    string.IsNullOrWhiteSpace(point.UsageNotes) || string.IsNullOrWhiteSpace(point.CommonMistake) ||
                    string.IsNullOrWhiteSpace(point.ExampleJapanese) || string.IsNullOrWhiteSpace(point.ExampleReading) ||
                    string.IsNullOrWhiteSpace(point.ExampleEnglish))
                    throw new InvalidOperationException($"Grammar point '{point.Code}' has incomplete teaching content.");
                if (System.Text.RegularExpressions.Regex.IsMatch(point.ExampleReading, "[一-龯々]"))
                    throw new InvalidOperationException($"Grammar point '{point.Code}' contains kanji inside its kana reading.");

                var expectedCategory = point.Level == "A1" ? "A1 Course" : "A2 Course";
                if (!point.Lessons.Any(link => link.IsPrimary && link.Lesson?.IsPublished == true &&
                    link.Lesson.Category?.Name == expectedCategory))
                    throw new InvalidOperationException($"Grammar point '{point.Code}' needs a published primary {point.Level} lesson.");
            }

            var courseLessons = await context.Lessons
                .Where(lesson => lesson.IsPublished && (lesson.Category!.Name == "A1 Course" || lesson.Category.Name == "A2 Course"))
                .Include(lesson => lesson.LessonGrammarPoints)
                .ToListAsync();
            if (courseLessons.Count != 33 || courseLessons.Any(lesson => !lesson.LessonGrammarPoints.Any(link => link.IsPrimary)))
                throw new InvalidOperationException("Every published A1/A2 lesson must have a primary structured grammar focus.");
        }
    }
}
