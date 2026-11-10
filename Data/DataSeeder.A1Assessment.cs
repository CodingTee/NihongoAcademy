using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private record A1Question(string Prompt, string A, string B, string C, string D, int Correct, string Explanation);

        private static async Task SeedA1FinalAssessmentAsync(ApplicationDbContext context)
        {
            var categoryId = await context.Categories.Where(c => c.Name == "A1 Course")
                .Select(c => c.Id).FirstAsync();
            const string title = "A1 Final Can-do Assessment";
            var quiz = await context.Quizzes.Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Title == title);

            if (quiz == null)
            {
                quiz = new Quiz { Title = title, CreatedDate = DateTime.Now };
                context.Quizzes.Add(quiz);
            }

            quiz.CategoryId = categoryId;
            quiz.JapaneseTitle = "A1修了テスト";
            quiz.Description = "A comprehensive check of the practical vocabulary, grammar and situations taught across the A1 pathway.";
            quiz.TimeLimitMinutes = 25;
            quiz.PassingScore = 80;
            quiz.IsPublished = true;
            quiz.DisplayOrder = 100;
            await context.SaveChangesAsync();

            var seeds = GetA1FinalQuestions();
            for (var i = 0; i < seeds.Length; i++)
            {
                var seed = seeds[i];
                var order = i + 1;
                var question = quiz.Questions.FirstOrDefault(q => q.DisplayOrder == order);
                if (question == null)
                {
                    question = new QuizQuestion { QuizId = quiz.Id, DisplayOrder = order };
                    context.QuizQuestions.Add(question);
                }

                question.QuestionText = seed.Prompt;
                question.OptionA = seed.A;
                question.OptionB = seed.B;
                question.OptionC = seed.C;
                question.OptionD = seed.D;
                question.CorrectOption = seed.Correct;
                question.Explanation = seed.Explanation;
            }

            var validOrders = Enumerable.Range(1, seeds.Length).ToHashSet();
            context.QuizQuestions.RemoveRange(quiz.Questions.Where(q => !validOrders.Contains(q.DisplayOrder)));
            await context.SaveChangesAsync();
        }

        private static A1Question[] GetA1FinalQuestions() => new[]
        {
            new A1Question("You meet someone for the first time. Which opening is appropriate?", "いただきます", "はじめまして", "おかえり", "さようなら", 2, "はじめまして is used at a first meeting."),
            new A1Question("Complete: わたし___リーです。", "を", "に", "は", "で", 3, "は marks the topic."),
            new A1Question("Which sentence means 'My mother is a teacher'?", "母は先生です。", "母の先生です。", "先生は母のです。", "母を先生です。", 1, "AはBです identifies and describes the topic."),
            new A1Question("Complete: これは だれ___かさですか。", "を", "の", "で", "が", 2, "の expresses possession."),
            new A1Question("Which word means 'that one over there'?", "これ", "それ", "あれ", "どれ", 3, "あれ refers to something away from both people."),
            new A1Question("How do you ask the price?", "なんじですか", "どこですか", "なんさいですか", "いくらですか", 4, "いくらですか asks how much something costs."),
            new A1Question("Which is the correct reading for 7 o'clock?", "ななじ", "しちじ", "ななつじ", "しじ", 2, "Seven o'clock is しちじ."),
            new A1Question("Complete: まいにち 七時___おきます。", "に", "を", "で", "と", 1, "に marks a specific time."),
            new A1Question("Complete: あさごはん___たべます。", "が", "に", "を", "の", 3, "を marks the direct object."),
            new A1Question("Which verb is used for the existence of a cat?", "あります", "います", "します", "なります", 2, "います is used for people and animals."),
            new A1Question("Choose the correct sentence: The book is on the desk.", "本は机の上にあります。", "本は机の上をいます。", "本に机は上です。", "本で机があります。", 1, "Use place plus にあります for an object."),
            new A1Question("Which phrase means 'a quiet town'?", "しずか町", "しずかの町", "しずかな町", "しずかい町", 3, "A な-adjective takes な before a noun."),
            new A1Question("Complete: おんがく___すきです。", "が", "を", "に", "で", 1, "The liked item normally takes が."),
            new A1Question("How do you ask permission to take a photo?", "しゃしんをとります", "しゃしんをとってください", "しゃしんをとってもいいですか", "しゃしんはありません", 3, "て-form plus もいいですか asks permission."),
            new A1Question("Which is a natural restaurant order?", "ラーメンをひとつおねがいします", "ラーメンにひとついます", "ラーメンがひとつですか", "ラーメンでひとつあります", 1, "Item plus quantity plus おねがいします is a natural order."),
            new A1Question("Which phrase means 'I will take this'?", "これにします", "これをいます", "これがどこです", "これのください", 1, "これにします states a choice."),
            new A1Question("Complete: バス___いきます。", "が", "を", "で", "の", 3, "で marks the means of travel."),
            new A1Question("Which sentence is a polite invitation?", "えいがをみませんか", "えいがをみません", "えいがをみてはいけません", "えいががありません", 1, "ませんか makes a polite invitation."),
            new A1Question("Which sentence says 'I went yesterday'?", "きのういきます", "きのういきません", "きのういきました", "きのういきましょう", 3, "ました is the polite past ending."),
            new A1Question("Which phrase asks someone to repeat?", "もういちどおねがいします", "いただきます", "これにします", "だいじょうぶです", 1, "もういちどおねがいします asks for repetition.")
        };
    }
}
