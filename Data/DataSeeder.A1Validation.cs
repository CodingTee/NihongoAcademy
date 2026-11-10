using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task ValidateA1CurriculumAsync(ApplicationDbContext context)
        {
            var lessons = await context.Lessons
                .Where(l => l.Category!.Name == "A1 Course" && l.IsPublished)
                .Include(l => l.LessonVocabulary)
                .Include(l => l.Activities)
                .Include(l => l.Prerequisites)
                .OrderBy(l => l.DisplayOrder)
                .ToListAsync();

            if (lessons.Count != 18 || !lessons.Select(l => l.DisplayOrder).SequenceEqual(Enumerable.Range(1, 18)))
                throw new InvalidOperationException("A1 curriculum must contain exactly 18 published lessons in orders 1-18.");

            foreach (var lesson in lessons)
            {
                if (lesson.LessonVocabulary.Count == 0)
                    throw new InvalidOperationException($"A1 lesson '{lesson.Title}' has no lesson-specific vocabulary.");
                if (!lesson.Activities.Any(a => a.ActivityType == "dialogue") ||
                    !lesson.Activities.Any(a => a.ActivityType == "comprehension") ||
                    !lesson.Activities.Any(a => a.ActivityType == "sentence-building"))
                    throw new InvalidOperationException($"A1 lesson '{lesson.Title}' is missing a required activity type.");
                if (lesson.Activities.GroupBy(a => a.DisplayOrder).Any(group => group.Count() > 1))
                    throw new InvalidOperationException($"A1 lesson '{lesson.Title}' contains duplicate activity ordering.");
                if (lesson.Activities.Any(a => string.IsNullOrWhiteSpace(a.Prompt) || string.IsNullOrWhiteSpace(a.Explanation)))
                    throw new InvalidOperationException($"A1 lesson '{lesson.Title}' contains an activity without a prompt or explanation.");
                if (lesson.Activities.Count(a => !string.IsNullOrWhiteSpace(a.ReadingSupportText)) < 3)
                    throw new InvalidOperationException($"A1 lesson '{lesson.Title}' must provide reading help for at least three core activities.");
                if (lesson.Activities.Any(a => !string.IsNullOrWhiteSpace(a.ReadingSupportText) &&
                    System.Text.RegularExpressions.Regex.IsMatch(a.ReadingSupportText, "[一-龯々]")))
                    throw new InvalidOperationException($"A1 lesson '{lesson.Title}' contains kanji inside its kana reading help.");

                foreach (var activity in lesson.Activities)
                {
                    if (activity.ActivityType == "dialogue" &&
                        (string.IsNullOrWhiteSpace(activity.JapaneseText) || string.IsNullOrWhiteSpace(activity.EnglishText)))
                        throw new InvalidOperationException($"A1 lesson '{lesson.Title}' contains an incomplete model dialogue.");

                    if (activity.ActivityType == "comprehension")
                    {
                        var options = JsonSerializer.Deserialize<string[]>(activity.OptionsJson ?? "[]") ?? Array.Empty<string>();
                        if (options.Length != 4 || options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4 ||
                            !options.Contains(activity.CorrectAnswer ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                            throw new InvalidOperationException($"A1 lesson '{lesson.Title}' contains a comprehension answer that is missing from four distinct options.");
                    }

                    if (activity.ActivityType is "sentence-building" or "correction" && string.IsNullOrWhiteSpace(activity.CorrectAnswer))
                        throw new InvalidOperationException($"A1 lesson '{lesson.Title}' contains a production task without a model answer.");
                }
            }

            var describePeople = lessons.Single(lesson => lesson.Title == "A1: Describe people simply");
            var shopping = lessons.Single(lesson => lesson.Title == "A1: Shop for an item");
            var requiredVocabulary = await context.LessonVocabulary
                .Where(link => link.LessonId == describePeople.Id || link.LessonId == shopping.Id)
                .Include(link => link.Vocabulary)
                .ToListAsync();
            if (!requiredVocabulary.Any(link => link.LessonId == describePeople.Id && link.Vocabulary!.Japanese == "しずか") ||
                !requiredVocabulary.Any(link => link.LessonId == shopping.Id && link.Vocabulary!.Japanese == "はらいます"))
                throw new InvalidOperationException("A1 lesson vocabulary must cover the language explicitly taught in its examples.");

            foreach (var lesson in lessons)
            {
                if (lesson.Activities.Count < 8 ||
                    lesson.Activities.Count(a => a.ActivityType == "comprehension") < 4 ||
                    lesson.Activities.Count(a => a.ActivityType == "sentence-building") < 2 ||
                    lesson.Activities.Count(a => a.ActivityType == "correction") != 1)
                    throw new InvalidOperationException($"A1 lesson '{lesson.Title}' must contain a dialogue, four checks, two construction tasks and one typed correction.");
            }

            for (var i = 1; i < lessons.Count; i++)
            {
                if (!lessons[i].Prerequisites.Any(p => p.PrerequisiteLessonId == lessons[i - 1].Id))
                    throw new InvalidOperationException($"A1 lesson '{lessons[i].Title}' is not unlocked by the previous A1 lesson.");
            }

            var finalQuiz = await context.Quizzes.Include(q => q.Questions)
                .SingleOrDefaultAsync(q => q.Title == "A1 Final Can-do Assessment" && q.Category!.Name == "A1 Course");
            if (finalQuiz == null || !finalQuiz.IsPublished || finalQuiz.PassingScore != 80 || finalQuiz.Questions.Count != 20)
                throw new InvalidOperationException("The published A1 final assessment must contain 20 questions and require 80 percent.");
        }
    }
}
