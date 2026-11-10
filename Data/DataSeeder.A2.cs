using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private record A2Module(int Order, string Title, string JapaneseTitle, string Goal, string Focus,
            string Dialogue, string Meaning, string[] Words, string Question, string[] Options, string Answer,
            string BuilderPrompt, string[] Tokens, string BuiltSentence);
        private record A2Extension(string Prompt, string Japanese, string[] Options, string Answer, string Explanation,
            string Wrong, string Correct, string CorrectionExplanation, string BuilderPrompt, string[] Tokens, string BuiltSentence);

        private static async Task SeedA2CurriculumAsync(ApplicationDbContext context)
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == "A2 Course");
            if (category == null)
            {
                category = new Category { Name = "A2 Course", Description = "Handle connected everyday situations with elementary Japanese", IconClass = "bi bi-signpost", DisplayOrder = 5 };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }

            var modules = GetA2Modules();
            var readingSupport = GetA2ReadingSupport();
            foreach (var module in modules)
            {
                var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.Title == module.Title);
                if (lesson == null) { lesson = new Lesson { Title = module.Title }; context.Lessons.Add(lesson); }
                lesson.CategoryId = category.Id; lesson.JapaneseTitle = module.JapaneseTitle; lesson.Description = module.Goal;
                lesson.Content = $"<h2>Can-do goal</h2><p>{module.Goal}</p><h2>Language focus</h2><p>{module.Focus}</p><h2>Model in context</h2><p style='white-space:pre-line'>{module.Dialogue.Replace("\n", "<br/>")}</p><h2>Output task</h2><p>{module.BuilderPrompt} Then adapt it with your own information and say it aloud.</p>";
                lesson.DisplayOrder = module.Order; lesson.DifficultyLevel = 2; lesson.EstimatedMinutes = 45; lesson.IsPublished = true;
                await context.SaveChangesAsync();

                var items = new List<Vocabulary>();
                for (var i = 0; i < module.Words.Length; i++)
                {
                    var parts = module.Words[i].Split('|');
                    var item = await context.Vocabulary.FirstOrDefaultAsync(v => v.CategoryId == category.Id && v.Japanese == parts[0]);
                    if (item == null) { item = new Vocabulary { CategoryId = category.Id, Japanese = parts[0] }; context.Vocabulary.Add(item); }
                    item.Romaji = parts[1]; item.English = parts[2]; item.PartOfSpeech = parts[3]; item.DisplayOrder = module.Order * 100 + i; item.JlptLevel = 4;
                    await context.SaveChangesAsync(); items.Add(item);
                    if (!await context.LessonVocabulary.AnyAsync(x => x.LessonId == lesson.Id && x.VocabularyId == item.Id))
                        context.LessonVocabulary.Add(new LessonVocabulary { LessonId = lesson.Id, VocabularyId = item.Id, DisplayOrder = i });
                }

                var itemIds = items.Select(v => v.Id).ToList();
                var staleMappings = await context.LessonVocabulary
                    .Where(x => x.LessonId == lesson.Id && !itemIds.Contains(x.VocabularyId)).ToListAsync();
                context.LessonVocabulary.RemoveRange(staleMappings);

                var readingAnswer = module.BuilderPrompt.Replace("Build:", "", StringComparison.OrdinalIgnoreCase).Trim().TrimEnd('.');
                var readingOptions = new[] { readingAnswer }
                    .Concat(modules.Where(m => m.Title != module.Title)
                        .Select(m => m.BuilderPrompt.Replace("Build:", "", StringComparison.OrdinalIgnoreCase).Trim().TrimEnd('.')))
                    .Distinct().Take(4).ToArray();

                var activities = new List<LessonActivity>
                {
                    new LessonActivity { ActivityType="dialogue", DisplayOrder=0, Prompt="Listen for the main situation and the target pattern.", JapaneseText=module.Dialogue, EnglishText=module.Meaning, Explanation=module.Focus },
                    new LessonActivity { ActivityType="comprehension", DisplayOrder=1, Prompt=module.Question, JapaneseText=module.Dialogue, OptionsJson=JsonSerializer.Serialize(module.Options), CorrectAnswer=module.Answer, Explanation=module.Focus },
                    new LessonActivity { ActivityType="comprehension", DisplayOrder=2, Prompt="Read the short sentence and choose its meaning.", JapaneseText=module.BuiltSentence, OptionsJson=JsonSerializer.Serialize(readingOptions), CorrectAnswer=readingAnswer, Explanation=module.Focus },
                    new LessonActivity { ActivityType="sentence-building", DisplayOrder=3, Prompt=module.BuilderPrompt, OptionsJson=JsonSerializer.Serialize(module.Tokens), CorrectAnswer=module.BuiltSentence, Explanation=module.Focus }
                };
                var extension = GetA2Extensions()[module.Title];
                activities.AddRange(new[]
                {
                    new LessonActivity { ActivityType="comprehension", DisplayOrder=4, Prompt=extension.Prompt, JapaneseText=extension.Japanese, OptionsJson=JsonSerializer.Serialize(extension.Options), CorrectAnswer=extension.Answer, Explanation=extension.Explanation },
                    new LessonActivity { ActivityType="comprehension", DisplayOrder=5, Prompt="Choose the natural correction.", JapaneseText=extension.Wrong, OptionsJson=JsonSerializer.Serialize(new[] { extension.Correct, extension.Wrong, module.BuiltSentence, extension.BuiltSentence }), CorrectAnswer=extension.Correct, Explanation=extension.CorrectionExplanation },
                    new LessonActivity { ActivityType="sentence-building", DisplayOrder=6, Prompt=extension.BuilderPrompt, OptionsJson=JsonSerializer.Serialize(extension.Tokens), CorrectAnswer=extension.BuiltSentence, Explanation=extension.Explanation },
                    new LessonActivity { ActivityType="correction", DisplayOrder=7, Prompt="Rewrite the sentence correctly.", JapaneseText=extension.Wrong, CorrectAnswer=extension.Correct, Explanation=extension.CorrectionExplanation }
                });
                foreach (var seed in activities)
                {
                    var activity = await context.LessonActivities.FirstOrDefaultAsync(a => a.LessonId == lesson.Id && a.ActivityType == seed.ActivityType && a.DisplayOrder == seed.DisplayOrder);
                    if (activity == null) { activity = new LessonActivity { LessonId=lesson.Id, ActivityType=seed.ActivityType, DisplayOrder=seed.DisplayOrder }; context.LessonActivities.Add(activity); }
                    activity.Prompt=seed.Prompt; activity.JapaneseText=seed.JapaneseText; activity.EnglishText=seed.EnglishText; activity.OptionsJson=seed.OptionsJson; activity.CorrectAnswer=seed.CorrectAnswer; activity.Explanation=seed.Explanation;
                    activity.ReadingSupportText = readingSupport.GetValueOrDefault((module.Title, seed.DisplayOrder));
                }
                var activityKeys = activities.Select(a => new { a.ActivityType, a.DisplayOrder }).ToList();
                var staleActivities = await context.LessonActivities.Where(a => a.LessonId == lesson.Id)
                    .ToListAsync();
                context.LessonActivities.RemoveRange(staleActivities.Where(a =>
                    !activityKeys.Any(key => key.ActivityType == a.ActivityType && key.DisplayOrder == a.DisplayOrder)));
                await context.SaveChangesAsync();
            }

            foreach (var c in await context.Categories.ToListAsync())
            {
                var order = c.Name switch { "Hiragana"=>1, "Katakana"=>2, "A1 Course"=>3, "Kanji"=>4, "A2 Course"=>5, "Vocabulary"=>6, "Grammar"=>7, "Culture"=>8, _=>c.DisplayOrder };
                c.DisplayOrder = order;
            }
            await context.SaveChangesAsync();
        }

        private static Dictionary<(string Lesson, int Order), string> GetA2ReadingSupport() => new()
        {
            [("A2: Connect actions and describe a sequence", 0)] = "あさ おきて、かおを あらって、あさごはんを たべます。",
            [("A2: Connect actions and describe a sequence", 2)] = "おきて、あさごはんを たべます。",
            [("A2: Connect actions and describe a sequence", 7)] = "かおを あらうて、あさごはんを たべます。→ かおを あらって、あさごはんを たべます。",
            [("A2: Talk about experiences", 0)] = "A：きょうとに いったことが ありますか。\nB：はい、にかい いったことが あります。おてらを みました。",
            [("A2: Talk about experiences", 2)] = "すしを たべたことが ありますか。",
            [("A2: Talk about experiences", 7)] = "きょうとに いきることが あります。→ きょうとに いったことが あります。",
            [("A2: Explain plans and intentions", 0)] = "A：なつやすみに なにを するつもりですか。\nB：にほんへ いくつもりです。はちがつ とおかに しゅっぱつする よていです。",
            [("A2: Explain plans and intentions", 2)] = "にほんごを べんきょうする つもりです。",
            [("A2: Explain plans and intentions", 7)] = "らいげつ にほんへ いく よていします。→ らいげつ にほんへ いく よていです。",
            [("A2: Compare choices", 0)] = "A：でんしゃと バスと、どちらの ほうが はやいですか。\nB：でんしゃの ほうが バスより はやいです。",
            [("A2: Compare choices", 2)] = "バスより でんしゃの ほうが べんりです。",
            [("A2: Compare choices", 7)] = "バスより でんしゃは はやいの ほうです。→ バスより でんしゃの ほうが はやいです。",
            [("A2: Give reasons and explanations", 0)] = "A：どうして こなかったんですか。\nB：ねつが あったからです。",
            [("A2: Give reasons and explanations", 2)] = "つかれていますから、やすみます。",
            [("A2: Give reasons and explanations", 7)] = "ねつが あったですから。→ ねつが あったからです。",
            [("A2: Express ability with potential forms", 0)] = "A：にほんごが はなせますか。\nB：すこし はなせますが、かんじは あまり よめません。",
            [("A2: Express ability with potential forms", 2)] = "にほんごが すこし はなせます。",
            [("A2: Express ability with potential forms", 7)] = "にほんごを はなすできます。→ にほんごが はなせます。",
            [("A2: Use casual speech with friends", 0)] = "A：あした えいがに いく？\nB：うん、いく。なんじに あう？\nA：ろくじは どう？",
            [("A2: Use casual speech with friends", 2)] = "あした いく？",
            [("A2: Use casual speech with friends", 7)] = "きのう えいがに いくた？→ きのう えいがに いった？",
            [("A2: Explain a problem and ask for help", 0)] = "りょこうしゃ：すみません、かばんを でんしゃに わすれてしまったんですが。\nえきいん：どんな かばんですか。\nりょこうしゃ：くろくて、おおきい かばんです。しらべて いただけますか。\nえきいん：はい、しょうしょう おまちください。",
            [("A2: Explain a problem and ask for help", 2)] = "さいふを なくして しまいました。",
            [("A2: Explain a problem and ask for help", 7)] = "かばんを でんしゃに わすれて しまうました。→ かばんを でんしゃに わすれて しまいました。",
            [("A2: Give and receive help", 0)] = "A：この かんじを おしえて くれますか。\nB：いいですよ。せんせいにも せつめいして もらいました。",
            [("A2: Give and receive help", 2)] = "ともだちが てつだって くれました。",
            [("A2: Give and receive help", 7)] = "ともだちに ほんを かして くれました。→ ともだちが ほんを かして くれました。",
            [("A2: Talk about conditions", 0)] = "A：あめが ふったら、どうしますか。\nB：いえで えいがを みます。はれたら、こうえんへ いきます。",
            [("A2: Talk about conditions", 2)] = "じかんが あったら、べんきょうします。",
            [("A2: Talk about conditions", 7)] = "あめが ふるたら、いえに います。→ あめが ふったら、いえに います。",
            [("A2: Describe nouns with clauses", 0)] = "A：たなかさんは どの ひとですか。\nB：まどの そばで ほんを よんでいる ひとです。",
            [("A2: Describe nouns with clauses", 2)] = "これは きのう かった ほんです。",
            [("A2: Describe nouns with clauses", 7)] = "まどの そばで ほんを よみている ひとです。→ まどの そばで ほんを よんでいる ひとです。",
            [("A2: Express obligation and advice", 0)] = "A：ねつが あります。\nB：きょうは やすんだ ほうが いいですよ。がっこうを やすむなら、せんせいに れんらくしなければ なりません。",
            [("A2: Express obligation and advice", 2)] = "むりを しない ほうが いいです。",
            [("A2: Express obligation and advice", 7)] = "きょうは やすんで ほうが いいです。→ きょうは やすんだ ほうが いいです。",
            [("A2: Describe change and continuing states", 0)] = "A：にほんの せいかつに なれましたか。\nB：はい。でんしゃも つかえるように なりました。らいねんも にほんに すむことに しました。",
            [("A2: Describe change and continuing states", 2)] = "かんじが よめるように なりました。",
            [("A2: Describe change and continuing states", 7)] = "にほんごが はなせるように なりしました。→ にほんごが はなせるように なりました。",
            [("A2: Report information and opinions", 0)] = "A：たなかさんは なんと いっていましたか。\nB：でんしゃが おくれると いっていました。わたしは バスの ほうが いいと おもいます。",
            [("A2: Report information and opinions", 2)] = "バスは べんりだと おもいます。",
            [("A2: Report information and opinions", 7)] = "この まちは べんりと おもいます。→ この まちは べんりだと おもいます。",
            [("A2: Complete an independent travel mission", 0)] = "りょこうしゃ：きょうとに いったことが ありません。らいげつ いくつもりです。\nかかりいん：でんしゃと バスと、どちらが いいですか。\nりょこうしゃ：でんしゃの ほうが はやいですから、でんしゃに します。かんじが よめませんから、ゆっくり せつめいしてください。",
            [("A2: Complete an independent travel mission", 2)] = "かんじが よめませんから、ゆっくり せつめいしてください。",
            [("A2: Complete an independent travel mission", 7)] = "でんしゃの ほうが はやいより、でんしゃに します。→ バスより でんしゃの ほうが はやいですから、でんしゃに します。"
        };

        private static A2Module[] GetA2Modules() => GetA2CoreModules()
            .Select(module => module.Title == "A2: Complete an independent travel mission" ? module with { Order = 15 } : module)
            .Concat(GetA2ExpansionModules())
            .OrderBy(module => module.Order)
            .ToArray();

        private static Dictionary<string, A2Extension> GetA2Extensions() => new()
        {
            ["A2: Connect actions and describe a sequence"] = new("Which word explicitly introduces the next event?", "朝ごはんを食べました。それから学校へ行きました。", new[]{"それから","まだ","より","しか"}, "それから", "それから moves the sequence to the next event.", "顔を洗うて、朝ごはんを食べます。", "顔を洗って、朝ごはんを食べます。", "洗う changes to the て-form 洗って.", "Build: After eating breakfast, I go to school.", new[]{"朝ごはんを食べて、","学校へ","行きます。"}, "朝ごはんを食べて、学校へ行きます。"),
            ["A2: Talk about experiences"] = new("Which expression asks whether an experience has ever happened?", "富士山を見たことがありますか。", new[]{"見たことがありますか","見る予定ですか","見たほうがいいですか","見ていますか"}, "見たことがありますか", "Past plain form plus ことがあります asks about experience.", "京都に行きることがあります。", "京都に行ったことがあります。", "The experience pattern uses the past plain form 行った.", "Build: I have never eaten natto.", new[]{"納豆を","食べたことが","ありません。"}, "納豆を食べたことがありません。"),
            ["A2: Explain plans and intentions"] = new("Which phrase describes a fixed scheduled departure?", "八月十日に出発する予定です。", new[]{"出発する予定です","出発するつもりでした","出発したことがあります","出発したほうがいいです"}, "出発する予定です", "予定です describes an arranged schedule.", "来月日本へ行く予定します。", "来月日本へ行く予定です。", "Dictionary form plus 予定です states a scheduled plan.", "Build: I intend to travel next month.", new[]{"来月","旅行する","つもりです。"}, "来月旅行するつもりです。"),
            ["A2: Compare choices"] = new("What is being used as the comparison baseline?", "バスより電車のほうが速いです。", new[]{"The bus","The train","Speed","The station"}, "The bus", "The noun before より is the comparison baseline.", "バスより電車は速いのほうです。", "バスより電車のほうが速いです。", "Use AよりBのほうが plus the shared quality.", "Build: Kyoto is quieter than Tokyo.", new[]{"東京より","京都のほうが","静かです。"}, "東京より京都のほうが静かです。"),
            ["A2: Give reasons and explanations"] = new("Which form softly asks for the background to a situation?", "どうして遅れたんですか。", new[]{"んですか","てもいいですか","たことがありますか","ほうがいいですか"}, "んですか", "んですか invites an explanation of the situation.", "熱があったですから。", "熱があったからです。", "Put から after the plain-form reason, then finish with です.", "Build: The train was late, so I took a bus.", new[]{"電車が遅れましたから、","バスに","乗りました。"}, "電車が遅れましたから、バスに乗りました。"),
            ["A2: Express ability with potential forms"] = new("Which statement expresses an inability rather than a habit?", "漢字があまり読めません。", new[]{"漢字があまり読めません","漢字を毎日読みます","漢字を読んでいます","漢字を読みました"}, "漢字があまり読めません", "The potential negative 読めません expresses inability.", "日本語を話すできます。", "日本語が話せます。", "Use the potential form 話せます; the ability target commonly takes が.", "Build: I cannot read difficult kanji.", new[]{"難しい漢字は","読めません。"}, "難しい漢字は読めません。"),
            ["A2: Use casual speech with friends"] = new("Which reply is suitable in a casual conversation with a close friend?", "うん、行く。", new[]{"うん、行く。","はい、参ります。","行くでございます。","いいえ、行きますか。"}, "うん、行く。", "うん and the plain form 行く suit an informal relationship.", "きのう映画に行くた？", "きのう映画に行った？", "The casual past of 行く is the irregular form 行った.", "Build: What did you do yesterday?", new[]{"きのう","何した？"}, "きのう何した？"),
            ["A2: Explain a problem and ask for help"] = new("Which ending makes the request especially considerate?", "調べていただけますか。", new[]{"いただけますか","なければなりません","たことがあります","つもりです"}, "いただけますか", "ていただけますか makes a polite request for help.", "かばんを電車に忘れてしまうました。", "かばんを電車に忘れてしまいました。", "The completed regretful form is てしまいました.", "Build: I unfortunately lost my ticket.", new[]{"切符を","なくして","しまいました。"}, "切符をなくしてしまいました。"),
            ["A2: Give and receive help"] = new("Who benefits in 友だちが手伝ってくれました?", "友だちが手伝ってくれました。", new[]{"The speaker or speaker's group","The friend only","The teacher","Nobody"}, "The speaker or speaker's group", "くれる presents a favour directed toward the speaker's side.", "友だちに本を貸してくれました。", "友だちが本を貸してくれました。", "The person performing the favour is marked with が.", "Build: The teacher explained it to me.", new[]{"先生が","説明して","くれました。"}, "先生が説明してくれました。"),
            ["A2: Talk about conditions"] = new("What does the result clause describe?", "時間があったら、図書館へ行きます。", new[]{"What happens if there is time","A past habit","An obligation","A completed experience"}, "What happens if there is time", "The clause after ～たら gives the result under that condition.", "雨が降るたら、家にいます。", "雨が降ったら、家にいます。", "～たら attaches to the past plain form 降った.", "Build: If it is inexpensive, I will buy it.", new[]{"安かったら、","買います。"}, "安かったら、買います。"),
            ["A2: Describe nouns with clauses"] = new("Which noun is modified by the preceding clause?", "きのう買った本を読みました。", new[]{"本","きのう","買った","読みました"}, "本", "The plain-form clause きのう買った directly modifies 本.", "窓のそばで本を読みている人です。", "窓のそばで本を読んでいる人です。", "読む changes to 読んで before いる.", "Build: That is the person wearing a red shirt.", new[]{"あの人は","赤いシャツを着ている","人です。"}, "あの人は赤いシャツを着ている人です。"),
            ["A2: Express obligation and advice"] = new("Which expression gives advice rather than stating a rule?", "今日は休んだほうがいいです。", new[]{"休んだほうがいいです","連絡しなければなりません","休む予定です","休んだことがあります"}, "休んだほうがいいです", "～たほうがいい gives advice; ～なければなりません states necessity.", "今日は休んでほうがいいです。", "今日は休んだほうがいいです。", "Advice uses the past plain form before ほうがいい.", "Build: You must contact the teacher.", new[]{"先生に","連絡しなければ","なりません。"}, "先生に連絡しなければなりません。"),
            ["A2: Describe change and continuing states"] = new("What change does the sentence report?", "漢字が読めるようになりました。", new[]{"The speaker became able to read kanji","The speaker stopped reading","The speaker bought a book","The speaker must study"}, "The speaker became able to read kanji", "Potential form plus ようになりました reports a developed ability.", "日本語が話せるようになりしました。", "日本語が話せるようになりました。", "The correct polite past of なる is なりました.", "Build: I decided to study every day.", new[]{"毎日勉強する","ことに","しました。"}, "毎日勉強することにしました。"),
            ["A2: Report information and opinions"] = new("Whose statement is reported?", "山田さんは雨が降ると言っていました。", new[]{"Yamada's","The speaker's only","The listener's","Nobody's"}, "Yamada's", "The topic before と言っていました identifies the source of the report.", "この町は便利と思います。", "この町は便利だと思います。", "A な-adjective or noun uses だ before と思います.", "Build: Yamada said that it would rain.", new[]{"山田さんは","雨が降ると","言っていました。"}, "山田さんは雨が降ると言っていました。"),
            ["A2: Complete an independent travel mission"] = new("Which phrase repairs communication politely?", "漢字が読めませんから、ゆっくり説明してください。", new[]{"ゆっくり説明してください","電車にします","来月行くつもりです","京都に行ったことがありません"}, "ゆっくり説明してください", "The request states the difficulty, gives a reason and asks for slower explanation.", "電車のほうが速いより、電車にします。", "バスより電車のほうが速いですから、電車にします。", "Put the baseline before より and give the reason before the choice.", "Build: I cannot read this, so please write it.", new[]{"これは読めませんから、","書いて","ください。"}, "これは読めませんから、書いてください。")
        };

        private static A2Module[] GetA2ExpansionModules() => new[]
        {
            new A2Module(8,"A2: Explain a problem and ask for help","問題を説明して助けを求める","I can explain a simple problem, say what has happened and ask someone for practical help.","Use ～てしまいました to report an unfortunate completed event. Use ～んですが to introduce a problem softly before making a request.","旅行者：すみません、かばんを電車に忘れてしまったんですが。\n駅員：どんなかばんですか。\n旅行者：黒くて、大きいかばんです。調べていただけますか。\n駅員：はい、少々お待ちください。","Traveller: Excuse me, I unfortunately left my bag on the train. Station staff: What kind of bag is it? Traveller: It is a large black bag. Could you check for me? Staff: Yes, please wait a moment.",new[]{"忘れてしまいました|wasurete shimaimashita|unfortunately left / forgot|Pattern","困っています|komatte imasu|be in trouble|State","調べます|shirabemasu|check / investigate|Verb","見つかります|mitsukarimasu|be found|Verb","少々|shoushou|a moment|Adverb"},"What did the traveller leave on the train?",new[]{"A large black bag","A wallet","A mobile phone","A ticket"},"A large black bag","Build: I unfortunately lost my wallet.",new[]{"財布を","なくして","しまいました。"},"財布をなくしてしまいました。"),
            new A2Module(9,"A2: Give and receive help","授受表現","I can describe giving, receiving and helpful actions.","Use あげる for giving outward, もらう for receiving, and くれる when someone gives to me or my group.","A：この漢字を教えてくれますか。\nB：いいですよ。先生にも説明してもらいました。","A: Will you teach me this kanji? B: Sure. I also had the teacher explain it to me.",new[]{"あげます|agemasu|give|Verb","もらいます|moraimasu|receive|Verb","くれます|kuremasu|give to me / us|Verb","教えます|oshiemasu|teach|Verb","手伝います|tetsudaimasu|help|Verb"},"Who also explained it to B?",new[]{"The teacher","A friend","Their mother","Nobody"},"The teacher","Build: My friend helped me.",new[]{"友だちが","手伝って","くれました。"},"友だちが手伝ってくれました。"),
            new A2Module(10,"A2: Talk about conditions","条件を使う","I can explain what I will do under a likely condition.","Use ～たら for a specific if/when condition; the result may express intention, request or discovery.","A：雨が降ったら、どうしますか。\nB：家で映画を見ます。晴れたら、公園へ行きます。","A: What will you do if it rains? B: I'll watch a movie at home. If it clears, I'll go to the park.",new[]{"降ったら|futtara|if it rains|Conditional","晴れたら|haretara|if it clears|Conditional","場合|baai|case / situation|Noun","もし|moshi|if|Adverb","予定を変えます|yotei o kaemasu|change plans|Expression"},"What will B do if it rains?",new[]{"Watch a movie at home","Go to the park","Go shopping","Take a train"},"Watch a movie at home","Build: If I have time, I will study.",new[]{"時間があったら、","勉強します。"},"時間があったら、勉強します。"),
            new A2Module(11,"A2: Describe nouns with clauses","名詞を詳しく説明する","I can identify people, places and things using a clause before a noun.","A plain-form clause comes directly before the noun; Japanese needs no relative pronoun such as who or that.","A：田中さんはどの人ですか。\nB：窓のそばで本を読んでいる人です。","A: Which person is Tanaka? B: The person reading a book by the window.",new[]{"読んでいる人|yonde iru hito|person who is reading|Phrase","働いている店|hataraite iru mise|shop where one works|Phrase","買った本|katta hon|book that was bought|Phrase","窓|mado|window|Noun","そば|soba|near / beside|Position"},"How can Tanaka be identified?",new[]{"Reading by the window","Standing at the station","Buying a book","Working in a shop"},"Reading by the window","Build: This is the book I bought yesterday.",new[]{"これは","きのう買った","本です。"},"これはきのう買った本です。"),
            new A2Module(12,"A2: Express obligation and advice","義務とアドバイス","I can state an obligation and give considerate advice.","～なければなりません expresses obligation or a necessary rule; ～たほうがいいです gives considerate advice.","A：熱があります。\nB：今日は休んだほうがいいですよ。学校を休むなら、先生に連絡しなければなりません。","A: I have a fever. B: You should rest today. If you miss school, you need to contact your teacher.",new[]{"なければなりません|nakereba narimasen|must|Pattern","たほうがいい|ta hou ga ii|should|Pattern","連絡します|renraku shimasu|contact|Verb","無理|muri|overdoing / impossible|Noun","心配|shinpai|worry|Noun"},"What advice does B give?",new[]{"Rest today","Go to work","Exercise","Travel"},"Rest today","Build: You should not overdo it.",new[]{"無理を","しないほうが","いいです。"},"無理をしないほうがいいです。"),
            new A2Module(13,"A2: Describe change and continuing states","変化と状態","I can describe becoming, deciding and a continuing state.","～ようになります describes a developed ability; ～ことにします states a decision; ～ています may describe a continuing state.","A：日本の生活に慣れましたか。\nB：はい。電車も使えるようになりました。来年も日本に住むことにしました。","A: Used to life in Japan? B: Yes. I've become able to use trains. I've decided to live in Japan next year too.",new[]{"なります|narimasu|become|Verb","ようになります|you ni narimasu|come to be able to|Pattern","ことにします|koto ni shimasu|decide to|Pattern","慣れます|naremasu|get used to|Verb","住んでいます|sunde imasu|live / reside|State"},"What has B decided?",new[]{"To live in Japan next year","To stop using trains","To return today","To change jobs"},"To live in Japan next year","Build: I have become able to read kanji.",new[]{"漢字が","読めるように","なりました。"},"漢字が読めるようになりました。"),
            new A2Module(14,"A2: Report information and opinions","情報と意見を伝える","I can report what someone said and express an opinion.","Use plain form + と思います for an opinion and + と言っていました to report speech.","A：田中さんは何と言っていましたか。\nB：電車が遅れると言っていました。私はバスのほうがいいと思います。","A: What did Tanaka say? B: He said the train would be delayed. I think the bus is better.",new[]{"と思います|to omoimasu|I think|Pattern","と言っていました|to itte imashita|said that|Pattern","遅れます|okuremasu|be delayed|Verb","意見|iken|opinion|Noun","ニュース|nyuusu|news|Noun"},"What did Tanaka say?",new[]{"The train would be delayed","The bus was full","The train was fast","He missed the bus"},"The train would be delayed","Build: I think the bus is convenient.",new[]{"バスは","便利だと","思います。"},"バスは便利だと思います。")
        };

        private static A2Module[] GetA2CoreModules() => new[]
        {
            new A2Module(1,"A2: Connect actions and describe a sequence","動作をつなぐ","I can describe a short sequence of everyday actions.","Use the て-form to connect actions in chronological order; only the final verb carries tense.","朝起きて、顔を洗って、朝ごはんを食べます。","I get up, wash my face, and eat breakfast.",new[]{"起きて|okite|get up and|Verb","洗って|aratte|wash and|Verb","食べます|tabemasu|eat|Verb","顔|kao|face|Noun","それから|sorekara|after that|Connector"},"What happens after the speaker washes their face?",new[]{"They eat breakfast","They go to bed","They take a train","They study"},"They eat breakfast","Build: I get up and eat breakfast.",new[]{"起きて、","朝ごはんを","食べます。"},"起きて、朝ごはんを食べます。"),
            new A2Module(2,"A2: Talk about experiences","経験について話す","I can ask and answer whether someone has had an experience.","Past plain form + ことがあります expresses experience; it does not identify a specific occasion.","A：京都に行ったことがありますか。\nB：はい、二回行ったことがあります。お寺を見ました。","A: Have you been to Kyoto? B: Yes, twice. I saw temples.",new[]{"行ったことがあります|itta koto ga arimasu|have been|Pattern","二回|nikai|twice|Counter","お寺|otera|temple|Noun","経験|keiken|experience|Noun","まだ|mada|not yet / still|Adverb"},"Has speaker B visited Kyoto?",new[]{"Yes, twice","No, never","Yes, once","Not yet"},"Yes, twice","Build: Have you eaten sushi?",new[]{"すしを","食べたことが","ありますか。"},"すしを食べたことがありますか。"),
            new A2Module(3,"A2: Explain plans and intentions","予定とつもり","I can state a plan and distinguish it from an intention.","Dictionary form + つもりです states an intention; ～予定です describes a scheduled plan.","A：夏休みに何をするつもりですか。\nB：日本へ行くつもりです。八月十日に出発する予定です。","A: What do you intend to do in summer vacation? B: I intend to go to Japan. I am scheduled to depart August 10.",new[]{"つもり|tsumori|intention|Noun","予定|yotei|schedule / plan|Noun","出発します|shuppatsu shimasu|depart|Verb","夏休み|natsuyasumi|summer vacation|Noun","予約|yoyaku|reservation|Noun"},"When is the scheduled departure?",new[]{"August 10","July 10","Tomorrow","It is undecided"},"August 10","Build: I intend to study Japanese.",new[]{"日本語を","勉強する","つもりです。"},"日本語を勉強するつもりです。"),
            new A2Module(4,"A2: Compare choices","比べて選ぶ","I can compare two things and state a preference.","Use AよりBのほうが… for comparison, and どちらのほうが… to ask between two choices.","A：電車とバスと、どちらのほうが速いですか。\nB：電車のほうがバスより速いです。","A: Which is faster, the train or bus? B: The train is faster than the bus.",new[]{"より|yori|than|Particle","ほう|hou|the option / side|Noun","どちら|dochira|which of two|Question","速い|hayai|fast|I-adjective","便利|benri|convenient|Na-adjective"},"Which transport is faster?",new[]{"The train","The bus","They are equal","The dialogue does not say"},"The train","Build: Trains are more convenient than buses.",new[]{"バスより","電車のほうが","便利です。"},"バスより電車のほうが便利です。"),
            new A2Module(5,"A2: Give reasons and explanations","理由を説明する","I can give a simple reason for a decision or situation.","Put から after the reason. In conversation, んです can present background or invite an explanation.","A：どうして来なかったんですか。\nB：熱があったからです。","A: Why didn't you come? B: Because I had a fever.",new[]{"どうして|doushite|why|Question","から|kara|because|Particle","熱|netsu|fever|Noun","具合|guai|condition|Noun","休みます|yasumimasu|rest / take off|Verb"},"Why did speaker B not come?",new[]{"They had a fever","They were working","They missed the train","They forgot"},"They had a fever","Build: I will rest because I am tired.",new[]{"疲れていますから、","休みます。"},"疲れていますから、休みます。"),
            new A2Module(6,"A2: Express ability with potential forms","できることを伝える","I can say what I am able or unable to do.","Potential verbs express ability: 話す→話せる, 食べる→食べられる, する→できる. The ability target often takes が.","A：日本語が話せますか。\nB：少し話せますが、漢字はあまり読めません。","A: Can you speak Japanese? B: A little, but I cannot read kanji very well.",new[]{"話せます|hanasemasu|can speak|Potential verb","読めます|yomemasu|can read|Potential verb","できません|dekimasen|cannot do|Potential verb","あまり|amari|not very|Adverb","能力|nouryoku|ability|Noun"},"What is difficult for speaker B?",new[]{"Reading kanji","Speaking a little Japanese","Listening","Writing their name"},"Reading kanji","Build: I can speak a little Japanese.",new[]{"日本語が","少し","話せます。"},"日本語が少し話せます。"),
            new A2Module(7,"A2: Use casual speech with friends","友だちとの普通体","I can understand and use basic plain forms in an informal conversation.","Plain forms are common with friends: 行く／行かない／行った／行かなかった. Relationship and setting determine politeness.","A：明日映画に行く？\nB：うん、行く。何時に会う？\nA：六時はどう？","A: Going to a movie tomorrow? B: Yeah. What time shall we meet? A: How about six?",new[]{"行く|iku|go (plain)|Verb","行かない|ikanai|do not go (plain)|Verb","行った|itta|went (plain)|Verb","うん|un|yeah|Expression","会う|au|meet|Verb"},"What time is suggested?",new[]{"Six o'clock","Four o'clock","Tomorrow morning","The time is fixed at seven"},"Six o'clock","Build: Are you going tomorrow?",new[]{"明日","行く？"},"明日行く？"),
            new A2Module(8,"A2: Complete an independent travel mission","A2総合旅行タスク","I can plan a trip, compare options, explain a problem and repair communication.","Combine A2 patterns naturally: sequence, comparison, intention, reason, experience and potential forms.","旅行者：京都に行ったことがありません。来月行くつもりです。\n係員：電車とバスと、どちらがいいですか。\n旅行者：電車のほうが速いですから、電車にします。漢字が読めませんから、ゆっくり説明してください。","Traveller: I have never been to Kyoto. I intend to go next month. Staff: Which is better, train or bus? Traveller: The train is faster, so I'll choose it. I cannot read kanji, so please explain slowly.",new[]{"来月|raigetsu|next month|Noun","説明|setsumei|explanation|Noun","選びます|erabimasu|choose|Verb","切符|kippu|ticket|Noun","乗り換え|norikae|transfer|Noun"},"Why does the traveller choose the train?",new[]{"It is faster","It is cheaper","They have used it before","The bus is full"},"It is faster","Build: I cannot read kanji, so please explain slowly.",new[]{"漢字が読めませんから、","ゆっくり","説明してください。"},"漢字が読めませんから、ゆっくり説明してください。")
        };
    }
}
