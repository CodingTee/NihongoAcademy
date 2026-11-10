using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private record A2Question(string Prompt, string A, string B, string C, string D, int Correct, string Explanation);

        private static async Task SeedA2FinalAssessmentAsync(ApplicationDbContext context)
        {
            var categoryId = await context.Categories.Where(c => c.Name == "A2 Course").Select(c => c.Id).FirstAsync();
            const string title = "A2 Final Can-do Assessment";
            var quiz = await context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Title == title);
            if (quiz == null)
            {
                quiz = new Quiz { Title = title, CreatedDate = DateTime.Now };
                context.Quizzes.Add(quiz);
            }
            quiz.CategoryId = categoryId;
            quiz.JapaneseTitle = "A2修了テスト";
            quiz.Description = "A comprehensive grammar, reading and situation-based assessment for the complete A2 pathway.";
            quiz.TimeLimitMinutes = 35;
            quiz.PassingScore = 80;
            quiz.IsPublished = true;
            quiz.DisplayOrder = 120;
            await context.SaveChangesAsync();

            var seeds = GetA2FinalQuestions();
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
                question.OptionA = seed.A; question.OptionB = seed.B;
                question.OptionC = seed.C; question.OptionD = seed.D;
                question.CorrectOption = seed.Correct;
                question.Explanation = seed.Explanation;
            }
            var validOrders = Enumerable.Range(1, seeds.Length).ToHashSet();
            context.QuizQuestions.RemoveRange(quiz.Questions.Where(q => !validOrders.Contains(q.DisplayOrder)));
            await context.SaveChangesAsync();
        }

        private static A2Question[] GetA2FinalQuestions() => new[]
        {
            new A2Question("Choose the natural sequence: I get up, wash my face, and eat breakfast.", "起きて、顔を洗って、朝ごはんを食べます。", "起きる、顔が洗う、朝ごはんに食べます。", "起きてから、顔を洗うでした。", "起きますので顔を食べます。", 1, "The て-form connects actions; only the final verb carries tense."),
            new A2Question("Which sentence asks about an experience?", "京都に行きますか。", "京都に行ったことがありますか。", "京都に行くつもりですか。", "京都に行かなければなりませんか。", 2, "Past plain form plus ことがあります asks about experience."),
            new A2Question("What does 日本へ行くつもりです mean?", "I was in Japan", "I must go to Japan", "I intend to go to Japan", "I can go to Japan", 3, "つもりです expresses an intention."),
            new A2Question("Which expression describes a scheduled plan?", "出発する予定です", "出発したことです", "出発したほうです", "出発してあります", 1, "予定です describes a scheduled plan."),
            new A2Question("Complete: バス___電車のほうが速いです。", "から", "より", "まで", "しか", 2, "AよりBのほうが compares B favourably with A."),
            new A2Question("How do you ask which of two options is better?", "どれですか", "どちらのほうがいいですか", "どこがいいですか", "だれのほうですか", 2, "どちらのほう asks which of two choices."),
            new A2Question("Complete the reason: 熱があった___、休みました。", "まで", "でも", "から", "より", 3, "から follows the reason."),
            new A2Question("What nuance does どうしたんですか have?", "It asks for background or explanation", "It gives permission", "It compares two things", "It reports a rumour", 1, "んです presents or requests explanatory background."),
            new A2Question("Which is a potential-form sentence?", "日本語を話します", "日本語が話せます", "日本語を話しました", "日本語に話させます", 2, "話せます expresses ability; the target often takes が."),
            new A2Question("Choose the natural casual question to a friend.", "明日行きますか。", "明日行く？", "明日行くですか。", "明日行ってございますか。", 2, "Plain forms are natural in an appropriately informal relationship."),
            new A2Question("Your friend did something helpful for you. Which verb centres that favour?", "あげました", "くれました", "売りました", "借りました", 2, "くれる describes someone giving to or helping the speaker's side."),
            new A2Question("Complete: 友だちに本を___。I received a book from a friend.", "あげました", "もらいました", "くれましたを", "しました", 2, "もらう expresses receiving."),
            new A2Question("Complete: 雨が降っ___、家にいます。", "ても", "たり", "たら", "ながら", 3, "～たら expresses a likely if/when condition."),
            new A2Question("Which sentence means 'If I have time, I will study'?", "時間があるより、勉強します。", "時間があったら、勉強します。", "時間があることが、勉強します。", "時間をあげて、勉強します。", 2, "あったら gives the condition."),
            new A2Question("Which phrase means 'the person reading by the window'?", "窓を読む人", "窓のそばで本を読んでいる人", "人が窓の本です", "窓より読んだ人", 2, "A plain-form clause comes directly before the noun 人."),
            new A2Question("Which sentence means 'the book I bought yesterday'?", "きのう買った本", "きのう本を買う人", "本がきのう買いました", "きのうより本", 1, "The modifying clause きのう買った comes before 本."),
            new A2Question("Which expression gives advice?", "休んだほうがいいです", "休まなければなりません", "休むつもりです", "休んだことがあります", 1, "～たほうがいいです gives advice."),
            new A2Question("Which expression states obligation?", "薬を飲んでもいいです", "薬を飲んだことがあります", "薬を飲まなければなりません", "薬を飲むつもりでした", 3, "～なければなりません means must."),
            new A2Question("What does 漢字が読めるようになりました mean?", "I decided not to read kanji", "I became able to read kanji", "I have read this kanji before", "I should read kanji", 2, "～ようになりました describes a developed ability or change."),
            new A2Question("Which sentence expresses a personal decision?", "日本に住むことにしました", "日本に住んだことがあります", "日本に住むほうがいいです", "日本に住んでくれました", 1, "～ことにしました states a decision."),
            new A2Question("Complete the opinion: バスは便利だ___思います。", "に", "を", "と", "が", 3, "Plain form plus と思います expresses an opinion."),
            new A2Question("What does 電車が遅れると言っていました mean?", "The train was already late", "They said the train would be delayed", "They asked whether the train was late", "The train must be delayed", 2, "と言っていました reports what someone said."),
            new A2Question("Read: 雨が降ったら、予定を変えます。 What will happen if it rains?", "The plan will change", "The trip is already cancelled", "The weather will improve", "Nothing will change", 1, "予定を変えます means change the plan."),
            new A2Question("Read: 京都に行ったことがありますが、大阪にはまだ行ったことがありません。 Which is true?", "The speaker has visited both cities", "The speaker has visited neither city", "The speaker has visited Kyoto but not Osaka", "The speaker lives in Osaka", 3, "あります marks the Kyoto experience; まだありません says not yet for Osaka."),
            new A2Question("What does 財布をなくしてしまいました mean?", "I intend to buy a wallet", "I unfortunately lost my wallet", "Someone gave me a wallet", "I must check the wallet", 2, "～てしまいました reports a completed event, often with regret.")
        };
    }
}
