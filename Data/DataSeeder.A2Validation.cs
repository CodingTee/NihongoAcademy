using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task ValidateA2CurriculumAsync(ApplicationDbContext context)
        {
            var expectedTitles = GetA2Modules().Select(m => m.Title).ToArray();
            var lessons = await context.Lessons
                .Where(l => l.Category!.Name == "A2 Course" && l.IsPublished && expectedTitles.Contains(l.Title))
                .Include(l => l.LessonVocabulary).Include(l => l.Activities).Include(l => l.Prerequisites)
                .OrderBy(l => l.DisplayOrder).ToListAsync();

            if (lessons.Count != 15 || !lessons.Select(l => l.DisplayOrder).SequenceEqual(Enumerable.Range(1, 15)))
                throw new InvalidOperationException("A2 curriculum must contain exactly 15 published lessons in orders 1-15.");
            if (lessons.Any(l => l.LessonVocabulary.Count != 5))
                throw new InvalidOperationException("Every A2 lesson must contain exactly five lesson-specific learning items.");
            foreach (var lesson in lessons)
            {
                if (lesson.Activities.Count(a => a.ActivityType == "dialogue") != 1 ||
                    lesson.Activities.Count(a => a.ActivityType == "comprehension") != 4 ||
                    lesson.Activities.Count(a => a.ActivityType == "sentence-building") != 2 ||
                    lesson.Activities.Count(a => a.ActivityType == "correction") != 1)
                    throw new InvalidOperationException($"A2 lesson '{lesson.Title}' must contain a dialogue, four checks, two construction tasks and one typed correction.");
                if (lesson.Activities.GroupBy(activity => activity.DisplayOrder).Any(group => group.Count() > 1))
                    throw new InvalidOperationException($"A2 lesson '{lesson.Title}' contains duplicate activity ordering.");
                if (lesson.Activities.Count(activity => !string.IsNullOrWhiteSpace(activity.ReadingSupportText)) < 3)
                    throw new InvalidOperationException($"A2 lesson '{lesson.Title}' must provide reading help for at least three core activities.");
                if (lesson.Activities.Any(activity => !string.IsNullOrWhiteSpace(activity.ReadingSupportText) &&
                    System.Text.RegularExpressions.Regex.IsMatch(activity.ReadingSupportText, "[一-龯々]")))
                    throw new InvalidOperationException($"A2 lesson '{lesson.Title}' contains kanji inside its kana reading help.");

                var dialogue = lesson.Activities.Single(activity => activity.ActivityType == "dialogue");
                if (string.IsNullOrWhiteSpace(dialogue.JapaneseText) || string.IsNullOrWhiteSpace(dialogue.EnglishText))
                    throw new InvalidOperationException($"A2 lesson '{lesson.Title}' contains an incomplete model dialogue.");

                foreach (var activity in lesson.Activities.Where(activity => activity.ActivityType == "comprehension"))
                {
                    var options = JsonSerializer.Deserialize<string[]>(activity.OptionsJson ?? "[]") ?? Array.Empty<string>();
                    if (options.Length != 4 || options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4 ||
                        !options.Contains(activity.CorrectAnswer ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                        throw new InvalidOperationException($"A2 lesson '{lesson.Title}' contains a comprehension answer that is missing from four distinct options.");
                }

                if (lesson.Activities.Any(activity => string.IsNullOrWhiteSpace(activity.Prompt) ||
                    string.IsNullOrWhiteSpace(activity.Explanation)) ||
                    lesson.Activities.Where(activity => activity.ActivityType is "sentence-building" or "correction")
                        .Any(activity => string.IsNullOrWhiteSpace(activity.CorrectAnswer)))
                    throw new InvalidOperationException($"A2 lesson '{lesson.Title}' contains an incomplete prompt, explanation or model answer.");
            }
            for (var i = 1; i < lessons.Count; i++)
            {
                if (!lessons[i].Prerequisites.Any(p => p.PrerequisiteLessonId == lessons[i - 1].Id))
                    throw new InvalidOperationException($"A2 lesson '{lessons[i].Title}' is not unlocked by the previous A2 lesson.");
            }

            var quiz = await context.Quizzes.Include(q => q.Questions)
                .SingleOrDefaultAsync(q => q.Title == "A2 Final Can-do Assessment" && q.Category!.Name == "A2 Course");
            if (quiz == null || !quiz.IsPublished || quiz.PassingScore != 80 || quiz.Questions.Count != 25)
                throw new InvalidOperationException("The A2 final assessment must contain 25 questions and require 80 percent.");
            if (quiz.Questions.Any(question => question.CorrectOption is < 1 or > 4 ||
                new[] { question.OptionA, question.OptionB, question.OptionC, question.OptionD }
                    .Where(option => !string.IsNullOrWhiteSpace(option)).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4))
                throw new InvalidOperationException("Every A2 final-assessment question must have four distinct options and one valid answer index.");
        }
    }
}
