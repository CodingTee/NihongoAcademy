using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private static async Task ValidateIntegratedSkillsCurriculumAsync(ApplicationDbContext context)
        {
            var listening = await context.ListeningTasks.Include(task => task.Questions).ToListAsync();
            if (listening.Count(task => task.Level == "A1") < 6 || listening.Count(task => task.Level == "A2") < 6)
                throw new InvalidOperationException("Listening Studio must contain at least six A1 and six A2 tasks.");
            if (listening.Any(task => task.LessonId == null || task.Questions.Count != 3 ||
                task.Questions.Count(question => question.QuestionType == "choice") != 2 ||
                task.Questions.Count(question => question.QuestionType == "dictation") != 1))
                throw new InvalidOperationException("Every listening task must link to a lesson and contain two choices plus one dictation question.");
            foreach (var question in listening.SelectMany(task => task.Questions).Where(question => question.QuestionType == "choice"))
            {
                var options = JsonSerializer.Deserialize<string[]>(question.OptionsJson ?? "[]") ?? Array.Empty<string>();
                if (options.Length != 4 || !options.Contains(question.CorrectAnswer, StringComparer.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Listening question '{question.Prompt}' must have four options including its answer.");
            }

            var reading = await context.ReadingPassages.Include(passage => passage.Questions).ToListAsync();
            if (reading.Count(item => item.Level == "A1") < 6 || reading.Count(item => item.Level == "A2") < 6 ||
                reading.Any(item => item.LessonId == null || item.Questions.Count != 2 || string.IsNullOrWhiteSpace(item.KanaText)))
                throw new InvalidOperationException("Graded Reading must contain six linked two-question texts at each of A1 and A2.");
            if (reading.Any(item => System.Text.RegularExpressions.Regex.IsMatch(item.KanaText, "[一-龯々]")))
                throw new InvalidOperationException("Graded Reading kana support must not contain kanji.");
            foreach (var question in reading.SelectMany(item => item.Questions).Where(question => question.QuestionType == "choice"))
            {
                var options = JsonSerializer.Deserialize<string[]>(question.OptionsJson ?? "[]") ?? Array.Empty<string>();
                var accepted = question.CorrectAnswer.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (options.Length != 4 || !accepted.Any(answer => options.Contains(answer, StringComparer.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"Reading question '{question.Prompt}' must have four options including its answer.");
            }

            var speaking = await context.SpeakingTasks.ToListAsync();
            if (speaking.Count(item => item.Level == "A1") < 6 || speaking.Count(item => item.Level == "A2") < 6 ||
                speaking.Any(item => item.LessonId == null || string.IsNullOrWhiteSpace(item.ModelResponse) ||
                    string.IsNullOrWhiteSpace(item.PromptReading) || string.IsNullOrWhiteSpace(item.ModelResponseReading)))
                throw new InvalidOperationException("Speaking Studio must contain six complete lesson-linked tasks at each of A1 and A2.");
            if (speaking.Any(item => System.Text.RegularExpressions.Regex.IsMatch(item.PromptReading + item.ModelResponseReading, "[一-龯々]")))
                throw new InvalidOperationException("Speaking task readings must not contain kanji.");

            var writing = await context.WritingTasks.ToListAsync();
            if (writing.Count(item => item.Level == "A1") < 6 || writing.Count(item => item.Level == "A2") < 6 ||
                writing.Any(item => item.LessonId == null || string.IsNullOrWhiteSpace(item.ModelAnswer) || string.IsNullOrWhiteSpace(item.ModelAnswerReading)))
                throw new InvalidOperationException("Writing Studio must contain six complete lesson-linked tasks at each of A1 and A2.");
            if (writing.Any(item => System.Text.RegularExpressions.Regex.IsMatch(item.ModelAnswerReading, "[一-龯々]")))
                throw new InvalidOperationException("Writing model readings must not contain kanji.");
        }
    }
}
