using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private record KanjiQuestion(string Prompt, string A, string B, string C, string D, int Correct, string Explanation);

        private static async Task SeedKanjiFinalAssessmentAsync(ApplicationDbContext context)
        {
            var categoryId = await context.Categories.Where(c => c.Name == "Kanji").Select(c => c.Id).FirstAsync();
            const string title = "N5 Kanji Final Assessment";
            var quiz = await context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Title == title);
            if (quiz == null)
            {
                quiz = new Quiz { Title = title, CreatedDate = DateTime.Now };
                context.Quizzes.Add(quiz);
            }

            quiz.CategoryId = categoryId;
            quiz.JapaneseTitle = "N5漢字修了テスト";
            quiz.Description = "A comprehensive recognition, meaning and word-reading check for the 100 foundational kanji.";
            quiz.TimeLimitMinutes = 30;
            quiz.PassingScore = 80;
            quiz.IsPublished = true;
            quiz.DisplayOrder = 110;
            await context.SaveChangesAsync();

            var seeds = GetKanjiFinalQuestions();
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

        private static KanjiQuestion[] GetKanjiFinalQuestions() => new[]
        {
            new KanjiQuestion("Which kanji means 'three'?", "二", "三", "五", "八", 2, "三 means three."),
            new KanjiQuestion("Which word means Sunday?", "月曜日", "火曜日", "日曜日", "金曜日", 3, "日曜日 is Sunday."),
            new KanjiQuestion("How is 七時 read?", "ななじ", "しちじ", "しじ", "ななとき", 2, "七時 is read しちじ."),
            new KanjiQuestion("Which expression means every week?", "毎日", "毎年", "毎週", "午前", 3, "毎週 means every week."),
            new KanjiQuestion("What does 午後 mean?", "morning", "afternoon", "this week", "half past", 2, "午後 means p.m. or afternoon."),
            new KanjiQuestion("Which kanji means person?", "人", "入", "大", "天", 1, "人 means person; compare it carefully with 入."),
            new KanjiQuestion("Which word means student?", "先生", "学校", "学生", "会社", 3, "学生 is student."),
            new KanjiQuestion("Which word means Japanese language?", "日本人", "日本語", "外国", "学校", 2, "日本語 means Japanese language."),
            new KanjiQuestion("Which kanji means inside or middle?", "中", "上", "下", "外", 1, "中 means inside or middle."),
            new KanjiQuestion("Which sign means entrance?", "出口", "入口", "東口", "北口", 2, "入口 is an entrance."),
            new KanjiQuestion("Which sign means west exit?", "東口", "南口", "北口", "西口", 4, "西口 means west exit."),
            new KanjiQuestion("Which word means train?", "電車", "会社", "駅前", "毎週", 1, "電車 means train."),
            new KanjiQuestion("Complete: 学校に___きます。", "食", "読", "行", "書", 3, "行きます means go."),
            new KanjiQuestion("Which verb means to return home?", "来ます", "帰ります", "聞きます", "会います", 2, "帰ります means return."),
            new KanjiQuestion("Which kanji completes 水を__みます?", "飲", "読", "見", "聞", 1, "飲みます means drink."),
            new KanjiQuestion("Which sentence means 'I read a book'?", "本を見ます", "本を読みます", "本を書きます", "本を聞きます", 2, "本を読みます means read a book."),
            new KanjiQuestion("Which word means name?", "名前", "何前", "午前", "駅前", 1, "名前 means name."),
            new KanjiQuestion("What does 何ですか mean?", "Where is it?", "Who is it?", "What is it?", "When is it?", 3, "何 means what."),
            new KanjiQuestion("Which word means company?", "学校", "会社", "教室", "外国", 2, "会社 means company."),
            new KanjiQuestion("Which phrase means a new book?", "古い本", "高い本", "長い本", "新しい本", 4, "新しい means new."),
            new KanjiQuestion("Which sentence means there are many people?", "人が少ないです", "人が多いです", "人が高いです", "人が白いです", 2, "多い means many."),
            new KanjiQuestion("Which kanji means hand?", "足", "耳", "手", "目", 3, "手 means hand."),
            new KanjiQuestion("How is 五百円 read?", "ごひゃくえん", "ごびゃくねん", "ごひゃくにん", "ごせんえん", 1, "五百円 is 500 yen."),
            new KanjiQuestion("Which word means weather?", "元気", "天気", "空気", "電気", 2, "天気 means weather."),
            new KanjiQuestion("Which sentence means 'I am well'?", "天気です", "空です", "元気です", "人気です", 3, "元気です means I am well or energetic.")
        };
    }
}
