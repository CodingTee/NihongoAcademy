using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private sealed record ReadingQuestionSeed(string Type, string Prompt, string[] Options, string Answer, string Explanation);
        private sealed record ReadingSeed(string Code, string Title, string JapaneseTitle, string Level, string Type,
            string LessonTitle, string? GrammarCode, string Text, string[] Notes, string[] Glossary,
            string[] Vocabulary, ReadingQuestionSeed[] Questions);

        private static readonly Dictionary<string, string> ReadingPassageKana = new()
        {
            ["a1-profile"] = "はじめまして。わたしは アミナです。マレーシアから きました。だいがくせいです。にほんごと おんがくが すきです。よろしく おねがいします。",
            ["a1-family-message"] = "これは わたしの かぞくの しゃしんです。ひだりの ひとは ちちです。ちちは かいしゃいんです。みぎの ひとは ははです。ははは せんせいです。",
            ["a1-cafe-menu"] = "さくらカフェ\nコーヒー　さんびゃくごじゅうえん\nおちゃ　さんびゃくえん\nケーキ　よんひゃくごじゅうえん\nセット（コーヒーと ケーキ）ななひゃくえん\nえいぎょうじかん　ごぜん くじから ごご ろくじまで",
            ["a1-school-schedule"] = "げつようびの よてい\nくじ　にほんごの クラス\nじゅうにじ　ひるごはん\nにじ　としょかんで べんきょう\nごじ　ともだちと えいが",
            ["a1-shop-notice"] = "おしらせ\nきょうは シャツが にじゅっパーセント やすいです。あかと しろが あります。あおは ありません。みせは ごご はちじまでです。",
            ["a1-station-chat"] = "ミナ：どようび、いっしょに えいがを みませんか。\nケン：いいですね。なんじですか。\nミナ：にじです。いちじはんに えきで あいましょう。\nケン：はい、わかりました。",
            ["a2-sequence-email"] = "たなかさんへ\nけさ、しちじに おきて、あさごはんを たべて、えきへ いきました。でも、でんしゃが おくれたので、くじはんに かいしゃに つきました。かいぎには まにあいました。\nリー",
            ["a2-experience-blog"] = "せんしゅう、はじめて ふじさんに のぼりました。ほかの やまに のぼったことは ありましたが、ふじさんは はじめてでした。あさは はれていましたが、ごごから あめが ふりました。たいへんでしたが、ちょうじょうの けしきは すばらしかったです。",
            ["a2-travel-guide"] = "きょうと バス いちにちパス\nおとな ななひゃくえん、こども さんびゃくごじゅうえん。いちにちに さんかい いじょう バスに のる ひとには、いちにちパスの ほうが ふつうの きっぷより やすいです。えきの あんないじょで かうことが できます。ちかてつでは つかえません。",
            ["a2-help-message"] = "すみません。へやの エアコンを つけましたが、つめたい かぜが でません。まども あきません。こんや この へやで ねるのは すこし むずかしいです。みに きて いただけませんか。",
            ["a2-community-notice"] = "こうえん そうじの おしらせ\nにちようびの ごぜん くじに みなみこうえんの いりぐちに あつまってください。ごみぶくろは こちらで よういしますが、てぶくろは じぶんで もってこなければ なりません。あめの ばあいは らいしゅうの にちようびに おこないます。",
            ["a2-opinion-review"] = "えきまえの『うみ』は さかなりょうりが おいしくて、みせも しずかです。ともだちは すこし たかいと いっていました。あたらしい『やま』は『うみ』より やすいですが、しゅうまつは とても こんでいます。わたしは おちついて はなしを したいので、こんどは『うみ』に いこうと おもいます。"
        };

        private static async Task SeedReadingCurriculumAsync(ApplicationDbContext context)
        {
            static ReadingQuestionSeed Choice(string prompt, string answer, string explanation, params string[] options) => new("choice", prompt, options, answer, explanation);
            static ReadingQuestionSeed Input(string prompt, string answer, string explanation) => new("input", prompt, Array.Empty<string>(), answer, explanation);
            var seeds = new[]
            {
                new ReadingSeed("a1-profile", "A New Classmate", "新しいクラスメート", "A1", "profile", "A1: Introduce yourself", "a1-topic-wa",
                    "はじめまして。わたしはアミナです。マレーシアから来ました。大学生です。日本語と音楽が好きです。よろしくお願いします。",
                    new[]{"わたしはアミナです。|は marks Amina as the topic.","日本語と音楽が好きです。|と joins the two liked things."},
                    new[]{"大学生（だいがくせい）|university student","音楽（おんがく）|music","来ました（きました）|came"}, new[]{"わたし","マレーシア","がくせい"},
                    new[]{Choice("Where is Amina from?","Malaysia","から marks the place she came from.","Japan","Malaysia","China","Korea"),Input("What two things does Amina like?","Japanese and music|music and Japanese|Japanese language and music|日本語と音楽","日本語と音楽 are joined by と.")}),
                new ReadingSeed("a1-family-message", "A Family Photo Message", "家族の写真", "A1", "chat", "A1: Talk about your family", "a1-possession-no",
                    "これはわたしの家族の写真です。左の人は父です。父は会社員です。右の人は母です。母は先生です。",
                    new[]{"家族の写真|の connects family and photo.","左の人は父です。|The person on the left is the father."},
                    new[]{"左（ひだり）|left","右（みぎ）|right","写真（しゃしん）|photo","会社員（かいしゃいん）|company employee"}, new[]{"かぞく","ちち","はは","せんせい","かいしゃいん"},
                    new[]{Choice("Who is a teacher?","The mother","母は先生です gives the answer.","The father","The mother","Both parents","The writer"),Input("What is the father's job?","company employee|office worker|会社員","父は会社員です.")}),
                new ReadingSeed("a1-cafe-menu", "Cafe Menu", "カフェのメニュー", "A1", "menu", "A1: Order food and drinks", "a1-order-onegaishimasu",
                    "さくらカフェ\nコーヒー　350円\nお茶　300円\nケーキ　450円\nセット（コーヒーとケーキ）700円\n営業時間　午前九時から午後六時まで",
                    new[]{"セット|The set contains coffee and cake.","九時から六時まで|から and まで show opening and closing times."},
                    new[]{"営業時間（えいぎょうじかん）|business hours","午前（ごぜん）|a.m.","午後（ごご）|p.m.","セット|set meal"}, new[]{"コーヒー","ケーキ","お茶"},
                    new[]{Choice("Which choice is coffee and cake together?","The 700-yen set","The menu names both items inside the set.","Coffee","Tea","Cake","The 700-yen set"),Input("How much is the cake?","450 yen|450円","ケーキ is listed at 450円.")}),
                new ReadingSeed("a1-school-schedule", "Monday Schedule", "月曜日の予定", "A1", "schedule", "A1: Tell time and make a schedule", "a1-time-ni",
                    "月曜日の予定\n九時　日本語のクラス\n十二時　ひるごはん\n二時　図書館で勉強\n五時　友だちと映画",
                    new[]{"図書館で勉強|で marks where the studying happens.","友だちと映画|と marks the companion."},
                    new[]{"予定（よてい）|schedule","図書館（としょかん）|library","勉強（べんきょう）|study","映画（えいが）|movie"}, new[]{"ともだち"},
                    new[]{Choice("Where does the person study?","At the library","The two o'clock entry says 図書館で勉強.","At home","At the library","At school","At a cafe"),Input("What happens at five o'clock?","a movie with a friend|watch a movie with a friend|see a movie with a friend|友だちと映画","五時 is paired with 友だちと映画.")}),
                new ReadingSeed("a1-shop-notice", "Shop Notice", "店のお知らせ", "A1", "notice", "A1: Shop for an item", "a1-availability-wa",
                    "お知らせ\n今日はシャツが二十パーセント安いです。赤と白があります。青はありません。店は午後八時までです。",
                    new[]{"赤と白があります。|Red and white are available.","青はありません。|Blue is not available."},
                    new[]{"お知らせ（おしらせ）|notice","安い（やすい）|inexpensive / discounted","白（しろ）|white","ありません|not available"}, new[]{"シャツ","赤","青"},
                    new[]{Choice("Which colour is unavailable?","Blue","青はありません explicitly says blue is unavailable.","Red","White","Blue","Black"),Input("Until what time is the shop open?","8 p.m.|8 pm|8:00 p.m.|20:00|午後八時","午後八時まで gives the closing time.")}),
                new ReadingSeed("a1-station-chat", "Meeting at the Station", "駅で会う", "A1", "chat", "A1: Invite someone and make a plan", "a1-invitation-masenka",
                    "ミナ：土曜日、いっしょに映画を見ませんか。\nケン：いいですね。何時ですか。\nミナ：二時です。一時半に駅で会いましょう。\nケン：はい、わかりました。",
                    new[]{"見ませんか|A soft invitation.","会いましょう|Let's meet."},
                    new[]{"土曜日（どようび）|Saturday","一時半（いちじはん）|1:30","駅（えき）|station","会いましょう（あいましょう）|let's meet"}, new[]{"ともだち"},
                    new[]{Choice("When will Mina and Ken meet?","1:30","They meet at 1:30 before the two o'clock movie.","1:00","1:30","2:00","Saturday morning"),Input("Where will they meet?","at the station|station","駅で marks the meeting place.")}),
                new ReadingSeed("a2-sequence-email", "Morning Email", "朝のメール", "A2", "email", "A2: Connect actions and describe a sequence", null,
                    "田中さんへ\n今朝、七時に起きて、朝ごはんを食べて、駅へ行きました。でも、電車が遅れたので、九時半に会社に着きました。会議には間に合いました。\nリー",
                    new[]{"起きて、食べて、行きました|て-forms connect actions in sequence.","遅れたので|ので introduces the reason."},
                    new[]{"今朝（けさ）|this morning","遅れる（おくれる）|to be delayed","着く（つく）|to arrive","間に合う（まにあう）|to be in time"}, new[]{"駅","電車","会社"},
                    new[]{Choice("Why did Lee arrive at 9:30?","The train was delayed","遅れたので gives the cause.","Lee woke up late","The train was delayed","Breakfast took too long","The meeting changed"),Input("Did Lee miss the meeting?","no|no, he did not|he did not|did not miss it|間に合いました","会議には間に合いました means Lee was in time.")}),
                new ReadingSeed("a2-experience-blog", "Weekend Experience", "週末の経験", "A2", "story", "A2: Talk about experiences", null,
                    "先週、はじめて富士山に登りました。ほかの山に登ったことはありましたが、富士山は初めてでした。朝は晴れていましたが、午後から雨が降りました。大変でしたが、頂上の景色はすばらしかったです。",
                    new[]{"登ったことはありました|The writer had climbed a mountain before.","が|connects contrasting information."},
                    new[]{"先週（せんしゅう）|last week","登る（のぼる）|to climb","頂上（ちょうじょう）|summit","景色（けしき）|scenery"}, new[]{"山","雨"},
                    new[]{Choice("Was this the writer's first mountain climb?","No, only the first time on Mt Fuji","The writer had climbed a mountain before.","Yes, the first climb ever","No, only the first time on Mt Fuji","The text does not say","The writer did not climb"),Input("What changed in the afternoon?","it started raining|it rained|rain started|雨が降りました","午後から雨が降りました.")}),
                new ReadingSeed("a2-travel-guide", "Kyoto Day Pass", "京都一日パス", "A2", "travel information", "A2: Compare choices", null,
                    "京都バス一日パス\n大人700円、子ども350円。一日に三回以上バスに乗る人には、一日パスのほうが普通の切符より安いです。駅の案内所で買うことができます。地下鉄では使えません。",
                    new[]{"パスのほうが…より安い|The pass is cheaper than ordinary tickets.","使えません|It cannot be used."},
                    new[]{"大人（おとな）|adult","案内所（あんないじょ）|information desk","切符（きっぷ）|ticket","地下鉄（ちかてつ）|subway"}, new[]{"駅","安い"},
                    new[]{Choice("Who is the pass recommended for?","Someone taking at least three buses","The guide says 三回以上.","Someone taking one bus","Someone taking at least three buses","Only children","Subway passengers"),Input("Can the pass be used on the subway?","no|no, it cannot|it cannot|cannot be used|使えません","地下鉄では使えません.")}),
                new ReadingSeed("a2-help-message", "A Hotel Problem", "ホテルでの問題", "A2", "help message", "A2: Explain a problem and ask for help", null,
                    "すみません。部屋のエアコンをつけましたが、冷たい風が出ません。窓も開きません。今夜この部屋で寝るのは少し難しいです。見に来ていただけませんか。",
                    new[]{"つけましたが|が introduces the unexpected problem.","来ていただけませんか|A considerate request for help."},
                    new[]{"部屋（へや）|room","冷たい風（つめたいかぜ）|cold air","窓（まど）|window","見に来る（みにくる）|come and inspect"}, new[]{"ホテル"},
                    new[]{Choice("What are the two problems?","The air conditioner is not cold and the window will not open","Both are stated in the first three sentences.","No hot water and no key","The air conditioner is not cold and the window will not open","The room is too small","The bed is broken"),Input("What does the guest ask staff to do?","come and inspect it|come and check it|come and look at it|inspect the room|見に来ていただけませんか","見に来ていただけませんか is the request.")}),
                new ReadingSeed("a2-community-notice", "Park Cleanup Notice", "公園掃除のお知らせ", "A2", "notice", "A2: Express obligation and advice", null,
                    "公園掃除のお知らせ\n日曜日の午前九時に南公園の入口に集まってください。ごみ袋はこちらで用意しますが、手袋は自分で持ってこなければなりません。雨の場合は来週の日曜日に行います。",
                    new[]{"持ってこなければなりません|Participants must bring gloves.","雨の場合|In case of rain."},
                    new[]{"掃除（そうじ）|cleanup","入口（いりぐち）|entrance","ごみ袋（ごみぶくろ）|trash bag","手袋（てぶくろ）|gloves"}, new[]{"日曜日","雨"},
                    new[]{Choice("What must participants bring?","Gloves","Bags are supplied, but gloves are not.","Trash bags","Gloves","Food","Umbrellas"),Input("What happens if it rains?","it moves to next Sunday|next Sunday|it is held next Sunday|postponed until next Sunday|来週の日曜日","雨の場合は来週の日曜日に行います.")}),
                new ReadingSeed("a2-opinion-review", "Two Restaurants", "二つのレストラン", "A2", "review", "A2: Report information and opinions", null,
                    "駅前の「海」は魚料理がおいしくて、店も静かです。友だちは少し高いと言っていました。新しい「山」は海より安いですが、週末はとてもこんでいます。私は落ち着いて話をしたいので、今度は「海」に行こうと思います。",
                    new[]{"高いと言っていました|Reports what the friend said.","行こうと思います|Expresses the writer's intention/opinion."},
                    new[]{"魚料理（さかなりょうり）|fish dishes","静か（しずか）|quiet","こんでいる|crowded","今度（こんど）|next time"}, new[]{"魚","友だち","安い"},
                    new[]{Choice("Which restaurant will the writer choose?","Umi","The writer values a quiet place to talk.","Umi","Yama","Both","Neither"),Input("Why does the writer choose it?","to talk leisurely in a quiet place|because it is quiet|it is quiet|to talk comfortably|落ち着いて話をしたい","The final sentence gives the reason.")})
            };

            foreach (var seed in seeds)
            {
                var lesson = await context.Lessons.SingleAsync(l => l.Title == seed.LessonTitle);
                var grammarId = seed.GrammarCode == null ? null :
                    await context.GrammarPoints.Where(g => g.Code == seed.GrammarCode).Select(g => (int?)g.Id).SingleAsync();
                var passage = await context.ReadingPassages.SingleOrDefaultAsync(p => p.Code == seed.Code);
                if (passage == null) { passage = new ReadingPassage { Code = seed.Code }; context.ReadingPassages.Add(passage); }
                passage.Title = seed.Title; passage.JapaneseTitle = seed.JapaneseTitle; passage.Level = seed.Level;
                passage.TextType = seed.Type; passage.JapaneseText = seed.Text; passage.KanaText = ReadingPassageKana[seed.Code]; passage.SentenceNotesJson = JsonSerializer.Serialize(seed.Notes);
                passage.GlossaryJson = JsonSerializer.Serialize(seed.Glossary); passage.LessonId = lesson.Id;
                passage.GrammarPointId = grammarId; passage.EstimatedMinutes = seed.Level == "A1" ? 5 : 8;
                passage.DisplayOrder = Array.IndexOf(seeds, seed) + 1;
                await context.SaveChangesAsync();

                var existingQuestions = await context.ReadingQuestions.Where(q => q.ReadingPassageId == passage.Id).ToListAsync();
                for (var index = 0; index < seed.Questions.Length; index++)
                {
                    var questionSeed = seed.Questions[index];
                    var question = existingQuestions.SingleOrDefault(q => q.DisplayOrder == index + 1);
                    if (question == null) { question = new ReadingQuestion { ReadingPassageId = passage.Id, DisplayOrder = index + 1 }; context.ReadingQuestions.Add(question); }
                    question.QuestionType = questionSeed.Type; question.Prompt = questionSeed.Prompt;
                    question.OptionsJson = JsonSerializer.Serialize(questionSeed.Options); question.CorrectAnswer = questionSeed.Answer;
                    question.Explanation = questionSeed.Explanation;
                }

                var existingLinks = await context.ReadingPassageVocabulary.Where(link => link.ReadingPassageId == passage.Id).ToListAsync();
                context.ReadingPassageVocabulary.RemoveRange(existingLinks);
                var words = await context.Vocabulary.Where(v => seed.Vocabulary.Contains(v.Japanese)).Select(v => v.Id).Distinct().ToListAsync();
                foreach (var vocabularyId in words) context.ReadingPassageVocabulary.Add(new ReadingPassageVocabulary { ReadingPassageId = passage.Id, VocabularyId = vocabularyId });
                await context.SaveChangesAsync();
            }

            if (await context.ReadingPassages.CountAsync() < seeds.Length || await context.ReadingQuestions.CountAsync() < seeds.Length * 2)
                throw new InvalidOperationException("The graded reading curriculum is incomplete.");
        }
    }
}
