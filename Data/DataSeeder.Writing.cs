using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private sealed record WritingSeed(string Code, string Title, string JapaneseTitle, string Level, string Type,
            string Lesson, string? Grammar, string Prompt, int Characters, int Sentences, string[] Steps,
            string[] Requirements, string[] Checklist, string ModelAnswer);

        private static readonly Dictionary<string, string> WritingModelReadings = new()
        {
            ["a1-self-intro"]="はじめまして。リーです。マレーシアから きました。だいがくせいです。よろしく おねがいします。",
            ["a1-family"]="これは わたしの かぞくです。ちちは かいしゃいんです。ははは せんせいです。ははは やさしいです。",
            ["a1-schedule"]="まいあさ しちじに おきます。はちじに あさごはんを たべます。くじに だいがくに いきます。よる じゅういちじに ねます。",
            ["a1-cafe-order"]="コーヒーを いっぱいと ケーキを ひとつ おねがいします。みずも おねがいします。",
            ["a1-invitation"]="どようび、いっしょに えいがを みませんか。にじの えいがです。いちじはんに えきで あいましょう。",
            ["a1-past-day"]="きのう ともだちと まちに いきました。レストランで ひるごはんを たべました。それから えいがを みました。とても たのしかったです。",
            ["a2-sequence"]="にちようび、ともだちと まちへ いきました。えきで あって、カフェで はなして、それから えいがを みました。えいがは すこし ながかったですが、おもしろかったです。いい いちにちだったと おもいます。",
            ["a2-experience"]="ふじさんに のぼったことが あります。きょねんの なつ、かぞくと のぼりました。あさは はれていましたが、ごごは あめでした。たいへんでしたが、ちょうじょうの けしきは すばらしかったです。また いきたいと おもいます。",
            ["a2-comparison"]="うみカフェは やまカフェより しずかです。やまカフェの ほうが やすくて、えきにも ちかいです。でも、わたしは おちついて はなしを したいので、うみカフェの ほうが いいと おもいます。ともだちとの ひるごはんに おすすめです。",
            ["a2-help-request"]="すみません。へやの エアコンを つけましたが、つめたい かぜが でません。まども あきません。へやを みに きて いただけませんか。よろしく おねがいします。",
            ["a2-advice"]="にほんごを はじめるなら、まず ひらがなを おぼえる ひつようが あります。まいにち すこしずつ こえに だして よむと いいです。あたらしい ことばは ぶんの なかで おぼえた ほうが いいと おもいます。つづけることが たいせつなので、がんばってください。",
            ["a2-opinion-email"]="たなかさんへ\nこんにちは。りょこうについて そうだんが あります。やまださんは きょうとが べんりだと いっていました。わたしも ぶんかを もっと しりたいので、きょうとが いいと おもいます。らいしゅう、ホテルを しらべる つもりです。たなかさんは どう おもいますか。\nリー"
        };

        private static async Task SeedWritingCurriculumAsync(ApplicationDbContext context)
        {
            var seeds = new[]
            {
                new WritingSeed("a1-self-intro","Introduce Yourself","自己紹介","A1","guided","A1: Introduce yourself","a1-topic-wa","Write a short introduction for a new class.",35,4,
                    new[]{"Greet the reader.","Give your name and home country.","State your role or occupation.","Close politely."},
                    new[]{"First-meeting greeting|はじめまして","Identity pattern|です","Home country|マレーシア/日本/中国/韓国/から","Polite closing|よろしく"},
                    new[]{"I used です consistently.","I did not repeat わたしは unnecessarily.","My message has a clear opening and closing."},"はじめまして。リーです。マレーシアから来ました。大学生です。よろしくお願いします。"),
                new WritingSeed("a1-family","Describe Your Family","家族を紹介する","A1","guided","A1: Talk about your family","a1-possession-no","Introduce at least two people in a real or imaginary family.",40,3,
                    new[]{"Identify the relationship.","Give each person one fact.","Add one description."},new[]{"Family relationship|母/父/姉/兄/妹/弟","Possession の|の","Polite statement|です","Description|やさしい/元気/しずか"},
                    new[]{"I distinguished my family terms appropriately.","I placed の between related nouns.","Each sentence communicates one clear fact."},"これはわたしの家族です。父は会社員です。母は先生です。母はやさしいです。"),
                new WritingSeed("a1-schedule","Write a Daily Schedule","一日の予定","A1","sentence","A1: Describe your daily routine","a1-time-ni","Write what you do during a normal day and when you do it.",50,4,
                    new[]{"Choose four times of day.","Write one action for each time.","Include a destination or activity place."},new[]{"Specific time に|時に/分に","Polite verb|ます","Movement destination|学校に/会社に/大学に","At least one daily action|起き/食べ/勉強/寝"},
                    new[]{"My events are in a logical order.","I used に after specific times.","My verbs end consistently."},"毎朝七時に起きます。八時に朝ごはんを食べます。九時に大学に行きます。夜十一時に寝ます。"),
                new WritingSeed("a1-cafe-order","Write a Cafe Order","カフェで注文する","A1","sentence","A1: Order food and drinks","a1-order-onegaishimasu","Write a polite order containing a drink, food and quantities.",30,2,
                    new[]{"Choose a drink and food.","Use 杯 for the drink and a suitable general counter for the food.","Finish with a polite request."},new[]{"Object particle|を","Food quantity|一つ/二つ","Drink counter|一杯/二杯","Polite request|お願いします"},
                    new[]{"Every item has a clear quantity.","I marked ordered items with を.","The request is suitable for service staff."},"コーヒーを一杯とケーキを一つお願いします。水もお願いします。"),
                new WritingSeed("a1-invitation","Invite a Friend","友だちを誘う","A1","guided","A1: Invite someone and make a plan","a1-invitation-masenka","Write a message inviting a friend and proposing a time and meeting place.",45,3,
                    new[]{"Name the activity.","Use a soft invitation.","Suggest a time and place."},new[]{"Invitation|ませんか","Time|時","Meeting proposal|会いましょう/どうですか","Place particle|で/に"},
                    new[]{"The invitation is clear and polite.","The date, time and place do not conflict.","I used ませんか rather than only asking a factual question."},"土曜日、いっしょに映画を見ませんか。二時の映画です。一時半に駅で会いましょう。"),
                new WritingSeed("a1-past-day","Describe Yesterday","きのうの一日","A1","free","A1: Talk about a past day","a1-polite-past","Write four or more sentences about where you went and what you did yesterday.",60,4,
                    new[]{"Set the time as yesterday.","Name a destination.","Describe at least two completed actions.","Add a simple opinion."},new[]{"Past context|きのう/昨日","Polite past|ました","Destination|に行きました/へ行きました","Opinion|楽しかった/おいしかった/よかった"},
                    new[]{"Completed actions use past forms.","Events appear in a sensible sequence.","I included a personal reaction."},"きのう友だちと町に行きました。レストランで昼ごはんを食べました。それから映画を見ました。とても楽しかったです。"),
                new WritingSeed("a2-sequence","Explain a Sequence","週末の出来事","A2","guided","A2: Connect actions and describe a sequence",null,"Describe a short sequence of events from your weekend.",90,5,
                    new[]{"Introduce when and where.","Connect at least three actions.","End with the result or reaction."},new[]{"Connected actions|て/で","Sequence word|それから/そのあと/まず","Past ending|ました","Reaction|と思いました/楽しかった/大変でした"},
                    new[]{"I did not end every action as a separate ました sentence.","The sequence is easy to follow.","The final sentence closes the account."},"日曜日、友だちと町へ行きました。駅で会って、カフェで話して、それから映画を見ました。映画は少し長かったですが、おもしろかったです。いい一日だったと思います。"),
                new WritingSeed("a2-experience","Share an Experience","経験を伝える","A2","free","A2: Talk about experiences",null,"Write about something you have experienced, including when it happened and how it felt.",100,5,
                    new[]{"State the experience.","Say when or where it happened.","Add two supporting details.","Explain your reaction."},new[]{"Experience pattern|たことがあります/たことがありません","Past detail|ました","Contrast or connection|が/ので/て","Feeling|と思いました/うれしかった/大変でした"},
                    new[]{"I separated general experience from one specific event.","My time references are understandable.","I explained why the experience mattered."},"富士山に登ったことがあります。去年の夏、家族と登りました。朝は晴れていましたが、午後は雨でした。大変でしたが、頂上の景色はすばらしかったです。また行きたいと思います。"),
                new WritingSeed("a2-comparison","Compare Two Choices","二つを比べる","A2","guided","A2: Compare choices",null,"Compare two restaurants, places or products and recommend one.",100,5,
                    new[]{"Introduce both choices.","Compare price or convenience.","Compare one more quality.","Give your recommendation and reason."},new[]{"Comparison より|より","Preference のほうが|ほうが","Reason|から/ので","Opinion|と思います/おすすめです"},
                    new[]{"I compared the same quality on both sides.","My recommendation follows from my reasons.","I used comparative particles correctly."},"海カフェは山カフェより静かです。山カフェのほうが安くて、駅にも近いです。でも、私は落ち着いて話をしたいので、海カフェのほうがいいと思います。友だちとの昼ごはんにおすすめです。"),
                new WritingSeed("a2-help-request","Explain a Problem","問題を説明する","A2","guided","A2: Explain a problem and ask for help",null,"Write a polite message explaining a problem and requesting practical help.",90,4,
                    new[]{"Open politely.","State what you tried.","Explain what is still wrong.","Ask for a specific action."},new[]{"Polite opening|すみません/申し訳ありません","Attempt|ましたが/てみました","Problem|ません/できません/出ません","Request|ていただけませんか/てください"},
                    new[]{"The reader can identify the exact problem.","I did not demand help too directly.","My requested action is specific."},"すみません。部屋のエアコンをつけましたが、冷たい風が出ません。窓も開きません。部屋を見に来ていただけませんか。よろしくお願いします。"),
                new WritingSeed("a2-advice","Give Advice","アドバイスを書く","A2","free","A2: Express obligation and advice",null,"Reply to a friend who wants to begin studying Japanese. Give obligations and considerate advice.",100,5,
                    new[]{"Acknowledge the goal.","State one necessary action.","Give two pieces of advice.","Add encouragement."},new[]{"Obligation|なければなりません/必要があります","Advice|ほうがいい/といい","Reason|から/ので","Encouragement|がんばって/応援"},
                    new[]{"My advice is practical rather than vague.","I did not overuse strong obligation forms.","My tone remains friendly."},"日本語を始めるなら、まずひらがなを覚える必要があります。毎日少しずつ声に出して読むといいです。新しい言葉は文の中で覚えたほうがいいと思います。続けることが大切なので、がんばってください。"),
                new WritingSeed("a2-opinion-email","Write an Opinion Email","意見のメール","A2","free","A2: Report information and opinions",null,"Write an email reporting what another person said and giving your own opinion and plan.",120,6,
                    new[]{"Greet the recipient.","Introduce the topic.","Report someone else's information.","Give your opinion and reason.","State your next action.","Close the email."},new[]{"Reported speech|と言っていました/そうです","Opinion|と思います","Reason|から/ので","Plan|つもりです/予定です/ようと思います"},
                    new[]{"It is clear which idea belongs to the other person.","My opinion includes a reason.","The email has a suitable opening and closing."},"田中さんへ\nこんにちは。旅行について相談があります。山田さんは京都が便利だと言っていました。私も文化をもっと知りたいので、京都がいいと思います。来週、ホテルを調べるつもりです。田中さんはどう思いますか。\nリー"),
            };

            for (var index = 0; index < seeds.Length; index++)
            {
                var seed = seeds[index];
                var lesson = await context.Lessons.SingleAsync(l => l.Title == seed.Lesson);
                var grammarId = seed.Grammar == null ? null : await context.GrammarPoints.Where(g => g.Code == seed.Grammar).Select(g => (int?)g.Id).SingleAsync();
                var task = await context.WritingTasks.SingleOrDefaultAsync(t => t.Code == seed.Code);
                if (task == null) { task = new WritingTask { Code = seed.Code }; context.WritingTasks.Add(task); }
                task.Title = seed.Title; task.JapaneseTitle = seed.JapaneseTitle; task.Level = seed.Level; task.TaskType = seed.Type;
                task.Prompt = seed.Prompt; task.MinimumJapaneseCharacters = seed.Characters; task.MinimumSentences = seed.Sentences;
                task.StepsJson = JsonSerializer.Serialize(seed.Steps); task.RequirementsJson = JsonSerializer.Serialize(seed.Requirements);
                task.ChecklistJson = JsonSerializer.Serialize(seed.Checklist); task.ModelAnswer = seed.ModelAnswer;
                task.ModelAnswerReading = WritingModelReadings[seed.Code];
                task.LessonId = lesson.Id; task.GrammarPointId = grammarId; task.DisplayOrder = index + 1;
            }
            await context.SaveChangesAsync();
            if (await context.WritingTasks.CountAsync() < seeds.Length) throw new InvalidOperationException("The structured writing curriculum is incomplete.");
        }
    }
}
