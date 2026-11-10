using Microsoft.EntityFrameworkCore;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task ValidateCompleteLearningPathAsync(ApplicationDbContext context)
        {
            var categoryOrder = new[] { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course" };
            var lessons = (await context.Lessons
                .Where(lesson => lesson.IsPublished && categoryOrder.Contains(lesson.Category!.Name))
                .Include(lesson => lesson.Category)
                .Include(lesson => lesson.Prerequisites)
                .ToListAsync())
                // Array.IndexOf is an in-memory ordering rule and cannot be
                // translated by the SQL Server EF provider.
                .OrderBy(lesson => Array.IndexOf(categoryOrder, lesson.Category!.Name))
                .ThenBy(lesson => lesson.DisplayOrder)
                .ThenBy(lesson => lesson.Id)
                .ToList();

            if (lessons.Count == 0)
                throw new InvalidOperationException("The complete learning path contains no published lessons.");

            var lessonIds = lessons.Select(lesson => lesson.Id).ToHashSet();
            for (var index = 0; index < lessons.Count; index++)
            {
                var lesson = lessons[index];
                var corePrerequisites = lesson.Prerequisites
                    .Where(link => lessonIds.Contains(link.PrerequisiteLessonId))
                    .Select(link => link.PrerequisiteLessonId)
                    .ToList();

                if (index == 0 && corePrerequisites.Count != 0)
                    throw new InvalidOperationException($"The first lesson '{lesson.Title}' must not have a core prerequisite.");
                if (index > 0 && (corePrerequisites.Count != 1 || corePrerequisites[0] != lessons[index - 1].Id))
                    throw new InvalidOperationException($"Lesson '{lesson.Title}' must be unlocked only by the immediately preceding core lesson '{lessons[index - 1].Title}'.");
            }

            if (lessons.GroupBy(lesson => new { Category = lesson.Category!.Name, lesson.DisplayOrder }).Any(group => group.Count() > 1))
                throw new InvalidOperationException("Published core lessons must have unique display orders inside each category.");

            await ValidateSkillLinksAsync(
                "Listening", await context.ListeningTasks.Include(task => task.Lesson).ThenInclude(lesson => lesson!.Category).ToListAsync(),
                task => task.Code, task => task.Level, task => task.LessonId, task => task.Lesson?.Category?.Name);
            await ValidateSkillLinksAsync(
                "Reading", await context.ReadingPassages.Include(task => task.Lesson).ThenInclude(lesson => lesson!.Category).ToListAsync(),
                task => task.Code, task => task.Level, task => task.LessonId, task => task.Lesson?.Category?.Name);
            await ValidateSkillLinksAsync(
                "Speaking", await context.SpeakingTasks.Include(task => task.Lesson).ThenInclude(lesson => lesson!.Category).ToListAsync(),
                task => task.Code, task => task.Level, task => task.LessonId, task => task.Lesson?.Category?.Name);
            await ValidateSkillLinksAsync(
                "Writing", await context.WritingTasks.Include(task => task.Lesson).ThenInclude(lesson => lesson!.Category).ToListAsync(),
                task => task.Code, task => task.Level, task => task.LessonId, task => task.Lesson?.Category?.Name);
        }

        private static Task ValidateSkillLinksAsync<T>(string skill, List<T> tasks,
            Func<T, string> code, Func<T, string> level, Func<T, int?> lessonId, Func<T, string?> category)
        {
            if (tasks.Count != 12 || tasks.Count(item => level(item) == "A1") != 6 || tasks.Count(item => level(item) == "A2") != 6)
                throw new InvalidOperationException($"{skill} must contain exactly six A1 and six A2 tasks.");
            if (tasks.GroupBy(code, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
                throw new InvalidOperationException($"{skill} contains duplicate task codes.");
            if (tasks.GroupBy(lessonId).Any(group => group.Key != null && group.Count() > 1))
                throw new InvalidOperationException($"{skill} must not attach multiple graded tasks to the same lesson.");

            foreach (var task in tasks)
            {
                var expectedCategory = level(task) switch
                {
                    "A1" => "A1 Course",
                    "A2" => "A2 Course",
                    _ => throw new InvalidOperationException($"{skill} task '{code(task)}' has unsupported level '{level(task)}'.")
                };
                if (lessonId(task) == null || !string.Equals(category(task), expectedCategory, StringComparison.Ordinal))
                    throw new InvalidOperationException($"{skill} task '{code(task)}' must link to a published {expectedCategory} lesson.");
            }

            return Task.CompletedTask;
        }
    }
}
