using Microsoft.EntityFrameworkCore;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task ValidateContentQualityAsync(ApplicationDbContext context)
        {
            var coreCategories = new[] { "Hiragana", "Katakana", "A1 Course", "Kanji", "A2 Course" };
            var lessons = await context.Lessons
                .Where(lesson => lesson.IsPublished && coreCategories.Contains(lesson.Category!.Name))
                .Include(lesson => lesson.Category)
                .Include(lesson => lesson.LessonVocabulary).ThenInclude(link => link.Vocabulary)
                .Include(lesson => lesson.Activities)
                .Include(lesson => lesson.LessonGrammarPoints)
                .ToListAsync();

            if (lessons.Count != 80)
                throw new InvalidOperationException($"The guided curriculum must contain exactly 80 published lessons; found {lessons.Count}.");

            var placeholderTerms = new[] { "coming soon", "lorem ipsum", "todo", "placeholder" };
            foreach (var lesson in lessons)
            {
                if (string.IsNullOrWhiteSpace(lesson.Title) || string.IsNullOrWhiteSpace(lesson.JapaneseTitle) ||
                    string.IsNullOrWhiteSpace(lesson.Description) || string.IsNullOrWhiteSpace(lesson.Content))
                    throw new InvalidOperationException($"Core lesson '{lesson.Title}' has incomplete title, description or teaching content.");
                if (lesson.Description.Length < 20 || lesson.Content.Length < 120 || lesson.EstimatedMinutes is < 10 or > 90)
                    throw new InvalidOperationException($"Core lesson '{lesson.Title}' does not meet the minimum content-depth or duration standard.");
                if (placeholderTerms.Any(term => ($"{lesson.Title} {lesson.Description} {lesson.Content}")
                    .Contains(term, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"Core lesson '{lesson.Title}' contains unfinished placeholder text.");
                if (lesson.LessonVocabulary.Count == 0 || lesson.LessonVocabulary
                    .GroupBy(link => link.VocabularyId).Any(group => group.Count() > 1))
                    throw new InvalidOperationException($"Core lesson '{lesson.Title}' has missing or duplicated learning targets.");
                if (lesson.LessonVocabulary.Any(link => link.Vocabulary == null ||
                    string.IsNullOrWhiteSpace(link.Vocabulary.Japanese) ||
                    string.IsNullOrWhiteSpace(link.Vocabulary.Romaji) ||
                    string.IsNullOrWhiteSpace(link.Vocabulary.English)))
                    throw new InvalidOperationException($"Core lesson '{lesson.Title}' contains an incomplete learning target.");
            }

            var languageLessons = lessons.Where(lesson => lesson.Category!.Name is "A1 Course" or "A2 Course").ToList();
            foreach (var lesson in languageLessons)
            {
                var categoryName = lesson.Category?.Name ?? "Language course";
                var typeCounts = lesson.Activities.GroupBy(activity => activity.ActivityType)
                    .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
                if (lesson.Activities.Count < 8 ||
                    typeCounts.GetValueOrDefault("dialogue") < 1 ||
                    typeCounts.GetValueOrDefault("comprehension") < 4 ||
                    typeCounts.GetValueOrDefault("sentence-building") < 2 ||
                    typeCounts.GetValueOrDefault("correction") < 1 ||
                    !lesson.LessonGrammarPoints.Any(link => link.IsPrimary))
                    throw new InvalidOperationException($"{categoryName} lesson '{lesson.Title}' lacks balanced listening, reading, grammar or active-output practice.");
                if (lesson.Activities.Any(activity => string.IsNullOrWhiteSpace(activity.Prompt) ||
                    string.IsNullOrWhiteSpace(activity.Explanation)))
                    throw new InvalidOperationException($"{categoryName} lesson '{lesson.Title}' contains an activity without learner guidance.");
            }

            var levelLessons = languageLessons.GroupBy(lesson => lesson.Category!.Name == "A1 Course" ? "A1" : "A2")
                .ToDictionary(group => group.Key, group => group.Select(lesson => lesson.Id).ToHashSet());
            await ValidateSkillDistributionAsync("Listening", await context.ListeningTasks.Select(task => new { task.Level, task.LessonId }).ToListAsync(), levelLessons);
            await ValidateSkillDistributionAsync("Reading", await context.ReadingPassages.Select(task => new { task.Level, task.LessonId }).ToListAsync(), levelLessons);
            await ValidateSkillDistributionAsync("Speaking", await context.SpeakingTasks.Select(task => new { task.Level, task.LessonId }).ToListAsync(), levelLessons);
            await ValidateSkillDistributionAsync("Writing", await context.WritingTasks.Select(task => new { task.Level, task.LessonId }).ToListAsync(), levelLessons);
        }

        private static Task ValidateSkillDistributionAsync<T>(string skill, IEnumerable<T> tasks,
            IReadOnlyDictionary<string, HashSet<int>> levelLessons) where T : class
        {
            var properties = typeof(T).GetProperties();
            var levelProperty = properties.Single(property => property.Name == "Level");
            var lessonProperty = properties.Single(property => property.Name == "LessonId");
            foreach (var level in new[] { "A1", "A2" })
            {
                var linkedIds = tasks.Where(task => string.Equals(levelProperty.GetValue(task)?.ToString(), level, StringComparison.Ordinal))
                    .Select(task => lessonProperty.GetValue(task) as int?)
                    .Where(id => id.HasValue).Select(id => id!.Value).ToList();
                if (linkedIds.Count != 6 || linkedIds.Distinct().Count() != 6 || linkedIds.Any(id => !levelLessons[level].Contains(id)))
                    throw new InvalidOperationException($"{skill} must provide six {level} tasks linked across six distinct appropriate lessons.");
            }
            return Task.CompletedTask;
        }
    }
}
