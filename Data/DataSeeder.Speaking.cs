using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private sealed record SpeakingSeed(string Code, string Title, string JapaneseTitle, string Level, string Type,
            string Lesson, string Situation, string PromptJapanese, string PromptEnglish, int Characters,
            string[] Steps, string[] Requirements, string[] Checklist, string ModelResponse);

        private static readonly Dictionary<string, (string Prompt, string Model)> SpeakingReadings = new()
        {
            ["a1-greeting"]=("はじめまして。おなまえは なんですか。","はじめまして。リーです。よろしく おねがいします。"),
            ["a1-self-intro"]=("じこしょうかいを おねがいします。","リーと いいます。マレーシアから きました。だいがくせいです。にほんの おんがくが すきです。"),
            ["a1-family"]=("かぞくは なんにんですか。どんな かぞくですか。","かぞくは よにんです。ちちと ははと いもうとが います。ははは とても やさしいです。"),
            ["a1-order"]=("いらっしゃいませ。ごちゅうもんは おきまりですか。","コーヒーを いっぱいと ケーキを ひとつ おねがいします。"),
            ["a1-shopping"]=("なにを おさがしですか。","あおい シャツを さがしています。しろも ありますか。"),
            ["a1-invitation"]=("しゅうまつは なにを しますか。","どようび、いっしょに えいがを みませんか。にじに えきで あいましょう。"),
            ["a2-weekend"]=("しゅうまつは どうでしたか。なにを しましたか。","にちようびに ともだちと まちへ いきました。まず ひるごはんを たべて、それから えいがを みました。とても たのしかったです。"),
            ["a2-experience"]=("いままでに、どんな とくべつな けいけんを したことが ありますか。","ふじさんに のぼったことが あります。きょねん、かぞくと のぼりました。たいへんでしたが、けしきが きれいで、うれしかったです。"),
            ["a2-compare"]=("うみと やまでは、どちらが おすすめですか。","やまは うみより しずかです。けしきも きれいなので、わたしは やまの ほうが おすすめです。"),
            ["a2-help"]=("フロントです。どう なさいましたか。","すみません。へやの エアコンを つけましたが、つめたい かぜが でません。みに きて いただけませんか。"),
            ["a2-advice"]=("にほんごの べんきょうを はじめたいです。なにを したら いいですか。","まず、ひらがなを おぼえた ほうが いいです。まいにち こえに だして よむと いいですよ。すこしずつ がんばってください。"),
            ["a2-opinion"]=("りょこうについて、やまださんは なんと いっていましたか。あなたは どう おもいますか。","やまださんは きょうとが べんりだと いっていました。わたしも ぶんかを しりたいので、きょうとが いいと おもいます。こんや ホテルを しらべる つもりです。")
        };

        internal static async Task SeedSpeakingCurriculumAsync(ApplicationDbContext context)
        {
            var seeds = new[]
            {
                new SpeakingSeed("a1-greeting","Meet a New Classmate","はじめてのあいさつ","A1","role-play","A1: Greetings for the situation","You meet a classmate for the first time.","はじめまして。お名前は何ですか。","Nice to meet you. What is your name?",18,
                    new[]{"Return the greeting.","Say your name.","Close politely."},new[]{"First-meeting greeting|はじめまして","Name|です/といいます","Polite closing|よろしく"},new[]{"I spoke in complete phrases.","My greeting matches a first meeting.","I listened to the whole prompt before answering."},"はじめまして。リーです。よろしくお願いします。"),
                new SpeakingSeed("a1-self-intro","Give a Self-introduction","自己紹介をする","A1","topic","A1: Introduce yourself","A new class asks you to introduce yourself.","自己紹介をお願いします。","Please introduce yourself.",30,
                    new[]{"Give your name.","Say where you are from.","Add your role or one interest."},new[]{"Identity|です/といいます","Origin|から来ました","Personal detail|学生/会社員/好き"},new[]{"I did not rush the opening.","My information is easy to follow.","I used polite endings consistently."},"リーといいます。マレーシアから来ました。大学生です。日本の音楽が好きです。"),
                new SpeakingSeed("a1-family","Talk about Your Family","家族について話す","A1","topic","A1: Talk about your family","A friend asks about your family.","家族は何人ですか。どんな家族ですか。","How many people are in your family? What are they like?",28,
                    new[]{"Say how many people.","Name at least two relationships.","Describe one person."},new[]{"Family count|家族/人","Relationship|父/母/兄/姉/弟/妹","Description|です/やさしい/元気"},new[]{"My family count is clear.","I used family words accurately.","I gave more than a list of names."},"家族は四人です。父と母と妹がいます。母はとてもやさしいです。"),
                new SpeakingSeed("a1-order","Order at a Cafe","カフェで注文する","A1","role-play","A1: Order food and drinks","You are ordering at a cafe.","いらっしゃいませ。ご注文はお決まりですか。","Welcome. Are you ready to order?",18,
                    new[]{"Order a drink.","Order one food item.","Use a polite request."},new[]{"Drink|コーヒー/お茶/水/ジュース","Food|ケーキ/パン/サンドイッチ","Polite request|お願いします/ください"},new[]{"I included quantities where useful.","I used a service-appropriate ending.","The order is unambiguous."},"コーヒーを一杯とケーキを一つお願いします。"),
                new SpeakingSeed("a1-shopping","Ask About an Item","買い物で質問する","A1","role-play","A1: Shop for an item","You want a different colour in a shop.","何をお探しですか。","What are you looking for?",18,
                    new[]{"Name the item.","Ask whether another colour exists.","Respond politely."},new[]{"Item|シャツ/かばん/くつ","Colour|赤/青/白/黒","Availability|ありますか"},new[]{"I made the requested colour clear.","I used a question intonation.","I thanked the staff."},"青いシャツを探しています。白いシャツもありますか。"),
                new SpeakingSeed("a1-invitation","Invite a Friend","友だちを誘う","A1","role-play","A1: Invite someone and make a plan","Invite a friend to do something this weekend.","週末は何をしますか。","What are you doing this weekend?",24,
                    new[]{"Suggest an activity.","Use a soft invitation.","Propose a time or place."},new[]{"Invitation|ませんか","Activity|映画/食事/買い物/公園","Plan detail|時/駅/店/会いましょう"},new[]{"My invitation is not too direct.","I included enough detail to answer.","My time and place are consistent."},"土曜日、いっしょに映画を見ませんか。二時に駅で会いましょう。"),
                new SpeakingSeed("a2-weekend","Tell a Weekend Story","週末の出来事","A2","topic","A2: Connect actions and describe a sequence","Tell a classmate what happened last weekend.","週末はどうでしたか。何をしましたか。","How was your weekend? What did you do?",50,
                    new[]{"Set the time and place.","Connect at least three events.","Give your reaction."},new[]{"Sequence|まず/それから/そのあと/て","Past action|ました","Reaction|楽しかった/大変でした/と思います"},new[]{"My events follow a clear order.","I connected ideas instead of listing words.","My final reaction matches the story."},"日曜日に友だちと町へ行きました。まず昼ごはんを食べて、それから映画を見ました。とても楽しかったです。"),
                new SpeakingSeed("a2-experience","Share an Experience","経験について話す","A2","topic","A2: Talk about experiences","Talk about a memorable experience.","今までに、どんな特別な経験をしたことがありますか。","What special experience have you had?",55,
                    new[]{"State the experience.","Say when or where it happened.","Explain how you felt."},new[]{"Experience|たことがあります","Past detail|ました","Feeling|と思いました/うれしかった/大変でした"},new[]{"I separated general experience from event details.","My time reference is clear.","I explained why it was memorable."},"富士山に登ったことがあります。去年、家族と登りました。大変でしたが、景色がきれいで、うれしかったです。"),
                new SpeakingSeed("a2-compare","Recommend One Choice","二つを比べてすすめる","A2","topic","A2: Compare choices","Compare two places and recommend one.","海と山では、どちらがおすすめですか。","Which do you recommend, the sea or the mountains?",50,
                    new[]{"Compare both choices.","Choose one.","Give at least one reason."},new[]{"Comparison|より/ほうが","Recommendation|おすすめ","Reason|から/ので"},new[]{"I compared the same feature.","My recommendation is explicit.","My reason supports my choice."},"山は海より静かです。景色もきれいなので、私は山のほうがおすすめです。"),
                new SpeakingSeed("a2-help","Explain a Problem","問題を説明して助けを求める","A2","role-play","A2: Explain a problem and ask for help","Your hotel room air conditioner is not working.","フロントです。どうなさいましたか。","Front desk. How may I help you?",45,
                    new[]{"Open politely.","Describe the exact problem.","Say what you tried.","Request help."},new[]{"Polite opening|すみません","Problem|エアコン/動きません/出ません","Attempt|ましたが/てみました","Request|てください/ていただけませんか"},new[]{"The problem is specific.","My request is polite.","The staff knows what action I need."},"すみません。部屋のエアコンをつけましたが、冷たい風が出ません。見に来ていただけませんか。"),
                new SpeakingSeed("a2-advice","Give Study Advice","勉強のアドバイス","A2","topic","A2: Express obligation and advice","A friend is beginning Japanese and asks for advice.","日本語の勉強を始めたいです。何をしたらいいですか。","I want to start studying Japanese. What should I do?",55,
                    new[]{"Acknowledge the goal.","Give two practical suggestions.","Add encouragement."},new[]{"Advice|ほうがいい/といい","Reason|から/ので","Encouragement|がんばって/応援"},new[]{"My advice is actionable.","My tone is friendly rather than commanding.","I explained at least one reason."},"まず、ひらがなを覚えたほうがいいです。毎日声に出して読むといいですよ。少しずつがんばってください。"),
                new SpeakingSeed("a2-opinion","Report and Give an Opinion","話を伝えて意見を言う","A2","topic","A2: Report information and opinions","Report what someone said and give your own plan.","旅行について、山田さんは何と言っていましたか。あなたはどう思いますか。","What did Yamada say about the trip, and what do you think?",60,
                    new[]{"Report Yamada's view.","Give your own opinion and reason.","State your next action."},new[]{"Reported speech|と言っていました/そうです","Opinion|と思います","Plan|つもりです/予定です/ようと思います"},new[]{"It is clear whose opinion is whose.","My own opinion has a reason.","I finished with a concrete plan."},"山田さんは京都が便利だと言っていました。私も文化を知りたいので、京都がいいと思います。今夜ホテルを調べるつもりです。")
            };

            for (var index = 0; index < seeds.Length; index++)
            {
                var seed = seeds[index];
                // Course titles changed during curriculum revisions. Keep the speaking
                // task available on older databases even when an optional lesson link
                // cannot be resolved; a later seed run will attach it once present.
                var lesson = await context.Lessons.SingleOrDefaultAsync(l => l.Title == seed.Lesson);
                var task = await context.SpeakingTasks.SingleOrDefaultAsync(t => t.Code == seed.Code);
                if (task == null) { task = new SpeakingTask { Code = seed.Code }; context.SpeakingTasks.Add(task); }
                task.Title = seed.Title; task.JapaneseTitle = seed.JapaneseTitle; task.Level = seed.Level; task.TaskType = seed.Type;
                task.Situation = seed.Situation; task.PromptJapanese = seed.PromptJapanese; task.PromptEnglish = seed.PromptEnglish;
                task.PromptReading = SpeakingReadings[seed.Code].Prompt;
                task.MinimumJapaneseCharacters = seed.Characters; task.ResponseStepsJson = JsonSerializer.Serialize(seed.Steps);
                task.RequirementsJson = JsonSerializer.Serialize(seed.Requirements); task.ChecklistJson = JsonSerializer.Serialize(seed.Checklist);
                task.ModelResponse = seed.ModelResponse; task.ModelResponseReading = SpeakingReadings[seed.Code].Model; task.LessonId = lesson?.Id; task.DisplayOrder = index + 1;
            }
            await context.SaveChangesAsync();
            if (await context.SpeakingTasks.CountAsync() < seeds.Length) throw new InvalidOperationException("The structured speaking curriculum is incomplete.");
        }
    }
}
