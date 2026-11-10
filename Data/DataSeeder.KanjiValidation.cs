using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task ValidateKanjiCurriculumAsync(ApplicationDbContext context)
        {
            var modules = GetKanjiModules();
            var expectedTitles = modules.Select(m => m.Title).ToArray();
            var targetItems = modules.SelectMany(module => module.Items).ToList();
            if (targetItems.Count != 100 || targetItems.Select(item => item.Character).Distinct().Count() != 100)
                throw new InvalidOperationException("The kanji source curriculum must define 100 unique target characters.");
            if (targetItems.Any(item => string.IsNullOrWhiteSpace(item.Reading) || string.IsNullOrWhiteSpace(item.Meaning) ||
                string.IsNullOrWhiteSpace(item.Example) || !item.ExampleReading.Contains(item.Reading, StringComparison.Ordinal)))
                throw new InvalidOperationException("Every target kanji reading must appear in its example-word reading.");
            var lessons = await context.Lessons.Where(l => l.Category!.Name == "Kanji" && l.IsPublished && expectedTitles.Contains(l.Title))
                .Include(l => l.LessonVocabulary).Include(l => l.Activities).Include(l => l.Prerequisites)
                .OrderBy(l => l.DisplayOrder).ToListAsync();

            if (lessons.Count != 20 || !lessons.Select(l => l.DisplayOrder).SequenceEqual(Enumerable.Range(1, 20)))
                throw new InvalidOperationException("Kanji curriculum must contain exactly 20 published lessons in orders 1-20.");
            if (lessons.Any(l => l.LessonVocabulary.Count != 5))
                throw new InvalidOperationException("Every foundational kanji lesson must contain exactly five target kanji.");
            if (lessons.SelectMany(l => l.LessonVocabulary).Select(x => x.VocabularyId).Distinct().Count() != 100)
                throw new InvalidOperationException("Kanji curriculum must contain 100 unique target kanji.");

            foreach (var lesson in lessons)
            {
                if (lesson.Activities.Count != 16 || lesson.Activities.Count(activity => activity.ActivityType == "dialogue") != 1 ||
                    lesson.Activities.Count(activity => activity.ActivityType == "comprehension") != 10 ||
                    lesson.Activities.Count(activity => activity.ActivityType == "sentence-building") != 5)
                    throw new InvalidOperationException($"Kanji lesson '{lesson.Title}' must directly test meaning, reading and recall for all five targets.");
                if (lesson.Activities.GroupBy(activity => activity.DisplayOrder).Any(group => group.Count() > 1))
                    throw new InvalidOperationException($"Kanji lesson '{lesson.Title}' contains duplicate activity ordering.");
                foreach (var activity in lesson.Activities.Where(activity => activity.ActivityType == "comprehension"))
                {
                    var options = JsonSerializer.Deserialize<string[]>(activity.OptionsJson ?? "[]") ?? Array.Empty<string>();
                    if (options.Length != 4 || options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4 ||
                        !options.Contains(activity.CorrectAnswer ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                        throw new InvalidOperationException($"Kanji lesson '{lesson.Title}' contains an invalid four-option comprehension check.");
                }
            }
            for (var i = 1; i < lessons.Count; i++)
            {
                if (!lessons[i].Prerequisites.Any(p => p.PrerequisiteLessonId == lessons[i - 1].Id))
                    throw new InvalidOperationException($"Kanji lesson '{lessons[i].Title}' is not unlocked by the previous kanji lesson.");
            }

            var quiz = await context.Quizzes.Include(q => q.Questions)
                .SingleOrDefaultAsync(q => q.Title == "N5 Kanji Final Assessment" && q.Category!.Name == "Kanji");
            if (quiz == null || !quiz.IsPublished || quiz.PassingScore != 80 || quiz.Questions.Count != 25)
                throw new InvalidOperationException("The N5 kanji final assessment must contain 25 questions and require 80 percent.");
            if (quiz.Questions.Any(question => question.CorrectOption is < 1 or > 4 ||
                new[] { question.OptionA, question.OptionB, question.OptionC, question.OptionD }
                    .Where(option => !string.IsNullOrWhiteSpace(option)).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4))
                throw new InvalidOperationException("Every N5 kanji assessment question must have four distinct options and one valid answer index.");
        }
    }
}
