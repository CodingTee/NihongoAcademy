using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private sealed record GrammarSeed(string Lesson, string Code, string Pattern, string Meaning,
            string Formation, string Notes, string Mistake, string Japanese, string English);

        private static readonly Dictionary<string, string> GrammarExampleReadings = new()
        {
            ["a1-greetings"]="こんばんは、たなかせんせい。", ["a1-topic-wa"]="わたしは リーです。",
            ["a1-possession-no"]="これは わたしの ははです。", ["a1-na-adjective"]="マリアさんは しずかな ひとです。",
            ["a1-kore-kono"]="これは なんですか。", ["a1-number-desu"]="はたちです。",
            ["a1-time-ni"]="しちじに おきます。", ["a1-destination-ni"]="がっこうに いきます。",
            ["a1-arimasu-imasu"]="きょうしつに せんせいが います。", ["a1-adjective-link"]="この まちは しずかで きれいです。",
            ["a1-suki-ga"]="おんがくが すきです。", ["a1-permission"]="しゃしんを とっても いいですか。",
            ["a1-order-onegaishimasu"]="ラーメンを ひとつ おねがいします。", ["a1-availability-wa"]="あかは ありますか。",
            ["a1-means-de"]="でんしゃで いきます。", ["a1-invitation-masenka"]="いっしょに えいがを みませんか。",
            ["a1-polite-past"]="きのう こうえんに いきました。", ["a1-clarification"]="もう いちど ゆっくり おねがいします。",
            ["a2-te-sequence"]="あさ おきて、かおを あらって、あさごはんを たべます。", ["a2-experience-koto"]="きょうとに いったことが あります。",
            ["a2-plan-intention"]="らいげつ にほんへ いく よていです。", ["a2-comparison"]="バスより でんしゃの ほうが はやいです。",
            ["a2-reason-ndesu"]="ねつが あったからです。", ["a2-potential"]="にほんごが すこし はなせます。",
            ["a2-plain-forms"]="あした えいがに いく？", ["a2-shimau-request"]="かばんを でんしゃに わすれて しまいました。",
            ["a2-giving-receiving"]="ともだちが てつだって くれました。", ["a2-tara"]="じかんが あったら、べんきょうします。",
            ["a2-relative-clause"]="これは きのう かった ほんです。", ["a2-obligation-advice"]="きょうは やすんだ ほうが いいです。",
            ["a2-change-decision"]="かんじが よめるように なりました。", ["a2-report-opinion"]="たなかさんは でんしゃが おくれると いっていました。",
            ["a2-integrated-repair"]="かんじが よめませんから、ゆっくり せつめいしてください。"
        };

        private static async Task SeedA1GrammarPointsAsync(ApplicationDbContext context)
        {
            var seeds = new[]
            {
                new GrammarSeed("A1: Greetings for the situation", "a1-greetings", "場面に合う定型表現", "Choose a greeting that matches the time and situation.", "greeting expression", "Japanese greetings are usually complete fixed expressions.", "Do not use おはようございます at night.", "こんばんは、田中先生。", "Good evening, Tanaka-sensei."),
                new GrammarSeed("A1: Introduce yourself", "a1-topic-wa", "N は N です", "Identify or describe the topic.", "topic + は + information + です", "は is written ha but pronounced wa as a particle.", "Use は, not を, after わたし.", "わたしはリーです。", "I am Lee."),
                new GrammarSeed("A1: Talk about your family", "a1-possession-no", "N の N", "Show possession or a relationship between nouns.", "owner/group + の + person/thing", "The first noun explains the second noun.", "Do not omit の between わたし and a family member.", "これはわたしの母です。", "This is my mother."),
                new GrammarSeed("A1: Describe people simply", "a1-na-adjective", "な-adjective + な + N", "Describe a noun with a na-adjective.", "な-adjective stem + な + noun", "Use な only when the adjective directly modifies a noun.", "Do not add い to しずか.", "マリアさんはしずかな人です。", "Maria is a quiet person."),
                new GrammarSeed("A1: Identify everyday things", "a1-kore-kono", "これ／この N", "Point to a thing near the speaker.", "これ + particle / この + noun", "これ stands alone; この must be followed by a noun.", "このは何ですか is incomplete.", "これは何ですか。", "What is this?"),
                new GrammarSeed("A1: Use numbers, ages and prices", "a1-number-desu", "number + counter + です", "State an age, price or quantity politely.", "number + appropriate counter + です", "Choose the counter that matches what is counted.", "Use です, not あります, to state your age.", "二十歳です。", "I am twenty years old."),
                new GrammarSeed("A1: Tell time and make a schedule", "a1-time-ni", "time に Vます", "Mark the specific time of an action.", "specific time + に + polite verb", "Relative times such as 今日 often omit に.", "Do not mark an action time with を.", "七時におきます。", "I get up at seven."),
                new GrammarSeed("A1: Describe your daily routine", "a1-destination-ni", "place に／へ 行きます", "Express the destination of movement.", "destination + に/へ + movement verb", "へ is written he but pronounced e as a particle.", "Do not use を for the destination in this pattern.", "学校に行きます。", "I go to school."),
                new GrammarSeed("A1: Find people and things", "a1-arimasu-imasu", "place に N が あります／います", "Say that something or someone exists somewhere.", "place + に + noun + が + あります/います", "Use います for people and animals; あります for other things.", "Do not use あります for a person.", "教室に先生がいます。", "There is a teacher in the classroom."),
                new GrammarSeed("A1: Describe places and objects", "a1-adjective-link", "い-adj くて／な-adj で", "Connect two descriptions.", "い-adjective: ～くて; な-adjective: ～で", "The connector depends on the adjective type.", "しずか is a な-adjective, so use しずかで.", "この町はしずかできれいです。", "This town is quiet and beautiful."),
                new GrammarSeed("A1: Talk about likes and abilities", "a1-suki-ga", "N が 好きです", "Say what you like.", "liked item + が + 好きです", "好き behaves like a na-adjective, not an action verb.", "The liked item normally takes が, not を.", "音楽が好きです。", "I like music."),
                new GrammarSeed("A1: Make requests and ask permission", "a1-permission", "Vて もいいですか", "Ask whether an action is permitted.", "verb て-form + もいいですか", "Use a polite rising intonation when asking permission.", "Attach the pattern to the て-form, not the dictionary form.", "写真をとってもいいですか。", "May I take a photo?"),
                new GrammarSeed("A1: Order food and drinks", "a1-order-onegaishimasu", "N を quantity お願いします", "Order an item politely.", "item + を + quantity + お願いします", "The quantity can be omitted when context is clear.", "Mark the ordered item with を, not が.", "ラーメンを一つお願いします。", "One ramen, please."),
                new GrammarSeed("A1: Shop for an item", "a1-availability-wa", "N は ありますか", "Ask whether a particular option is available.", "desired option + は + ありますか", "は presents the item or option being checked.", "Avoid を before あります.", "赤はありますか。", "Do you have it in red?"),
                new GrammarSeed("A1: Travel and ask directions", "a1-means-de", "transport で 行きます", "State the means of transportation.", "vehicle/method + で + movement verb", "で marks the means used to perform the action.", "Use で for the vehicle, not に in this meaning.", "電車で行きます。", "I will go by train."),
                new GrammarSeed("A1: Invite someone and make a plan", "a1-invitation-masenka", "Vませんか", "Invite someone to do something together.", "verb ます-stem + ませんか", "Although negative in form, it is a soft invitation.", "Vますか merely asks; Vませんか clearly invites.", "いっしょに映画を見ませんか。", "Would you like to see a movie together?"),
                new GrammarSeed("A1: Talk about a past day", "a1-polite-past", "Vました", "Describe a completed past action politely.", "polite verb stem + ました", "Time words such as きのう make the past context clear.", "Do not use present Vます for a completed action yesterday.", "きのう公園に行きました。", "I went to the park yesterday."),
                new GrammarSeed("A1: Complete a real-life mission", "a1-clarification", "もう一度 + お願いします", "Ask someone to repeat something.", "もう一度 + optional manner + お願いします", "Add ゆっくり to request slower speech.", "もう一つ means one more item, not once more.", "もう一度ゆっくりお願いします。", "Once more slowly, please.")
            };

            var lessonTitles = seeds.Select(s => s.Lesson).ToArray();
            var lessons = await context.Lessons.Where(l => lessonTitles.Contains(l.Title)).ToDictionaryAsync(l => l.Title);
            if (lessons.Count != seeds.Length)
                throw new InvalidOperationException($"A1 grammar seeding expected {seeds.Length} lessons but found {lessons.Count}.");

            foreach (var seed in seeds)
            {
                var grammar = await context.GrammarPoints.SingleOrDefaultAsync(g => g.Code == seed.Code);
                if (grammar == null)
                {
                    grammar = new GrammarPoint { Code = seed.Code };
                    context.GrammarPoints.Add(grammar);
                }
                grammar.Pattern = seed.Pattern;
                grammar.Meaning = seed.Meaning;
                grammar.Formation = seed.Formation;
                grammar.UsageNotes = seed.Notes;
                grammar.CommonMistake = seed.Mistake;
                grammar.ExampleJapanese = seed.Japanese;
                grammar.ExampleReading = GrammarExampleReadings[seed.Code];
                grammar.ExampleEnglish = seed.English;
                grammar.Level = "A1";
                await context.SaveChangesAsync();

                var lesson = lessons[seed.Lesson];
                if (!await context.LessonGrammarPoints.AnyAsync(x => x.LessonId == lesson.Id && x.GrammarPointId == grammar.Id))
                    context.LessonGrammarPoints.Add(new LessonGrammarPoint { LessonId = lesson.Id, GrammarPointId = grammar.Id, DisplayOrder = 1, IsPrimary = true });
            }
            await context.SaveChangesAsync();

            var mappedCount = await context.LessonGrammarPoints.CountAsync(x => lessonTitles.Contains(x.Lesson!.Title));
            if (mappedCount < seeds.Length)
                throw new InvalidOperationException("Every A1 lesson must have a structured grammar focus.");
        }

        private static async Task SeedA2GrammarPointsAsync(ApplicationDbContext context)
        {
            var seeds = new[]
            {
                new GrammarSeed("A2: Connect actions and describe a sequence","a2-te-sequence","Vて、Vて、Vます","Connect actions in chronological order.","verb て-form + verb て-form + final verb","Only the final verb carries the tense for the sequence.","Do not connect verbs with their dictionary form plus て.","朝起きて、顔を洗って、朝ごはんを食べます。","I get up, wash my face and eat breakfast."),
                new GrammarSeed("A2: Talk about experiences","a2-experience-koto","Vたことがあります","Describe whether an experience has occurred.","past plain verb + ことがあります","Use this for general experience, not a dated event report.","Do not use the dictionary form before ことがあります.","京都に行ったことがあります。","I have been to Kyoto."),
                new GrammarSeed("A2: Explain plans and intentions","a2-plan-intention","Vるつもりです／Vる予定です","Distinguish an intention from a scheduled plan.","dictionary verb + つもりです / 予定です","つもり presents intention; 予定 presents an arrangement or schedule.","Do not say 予定します after a dictionary-form verb.","来月日本へ行く予定です。","I am scheduled to go to Japan next month."),
                new GrammarSeed("A2: Compare choices","a2-comparison","AよりBのほうが～","Compare two choices using the same quality.","baseline + より + preferred side + のほうが + quality","The noun before より is the comparison baseline.","Do not put より after the result adjective.","バスより電車のほうが速いです。","The train is faster than the bus."),
                new GrammarSeed("A2: Give reasons and explanations","a2-reason-ndesu","plain form + から／んです","Give a reason or invite background information.","reason + からです / explanatory plain form + んです","から marks a reason; んです frames explanatory background.","With nouns and na-adjectives, use な before んです.","熱があったからです。","It is because I had a fever."),
                new GrammarSeed("A2: Express ability with potential forms","a2-potential","potential verb","State what someone can or cannot do.","話す→話せる; 読む→読める; 食べる→食べられる; する→できる","The ability target commonly takes が, though を also occurs in modern usage.","Do not attach できます directly to a dictionary-form verb.","日本語が少し話せます。","I can speak a little Japanese."),
                new GrammarSeed("A2: Use casual speech with friends","a2-plain-forms","普通体","Use plain forms in an appropriately informal relationship.","行く／行かない／行った／行かなかった","Relationship and setting determine whether casual speech is suitable.","Do not attach ですか directly to a plain verb.","明日映画に行く？","Are you going to a movie tomorrow?"),
                new GrammarSeed("A2: Explain a problem and ask for help","a2-shimau-request","Vてしまいました／Vていただけますか","Report an unfortunate result and request help politely.","verb て-form + しまいました; verb て-form + いただけますか","～んですが can introduce the problem softly before the request.","Use てしまいました, not dictionary form + しまいました.","かばんを電車に忘れてしまいました。","I unfortunately left my bag on the train."),
                new GrammarSeed("A2: Give and receive help","a2-giving-receiving","～てあげる／もらう／くれる","Describe helpful actions from a chosen viewpoint.","giver が + Vてくれる; receiver は + person に + Vてもらう","くれる directs the favour toward the speaker's side.","The person doing a くれる favour is marked with が.","友だちが手伝ってくれました。","My friend helped me."),
                new GrammarSeed("A2: Talk about conditions","a2-tara","Vたら／い-adjかったら／Nだったら","State what happens under a likely condition.","past plain form + ら, followed by the result","The result can express an intention, request or discovery.","Do not attach たら directly to the dictionary form.","時間があったら、勉強します。","If I have time, I will study."),
                new GrammarSeed("A2: Describe nouns with clauses","a2-relative-clause","plain clause + N","Identify a noun through a preceding description.","plain-form clause directly before the noun","Japanese does not add a relative pronoun such as who or that.","Do not use ます inside a noun-modifying clause.","これはきのう買った本です。","This is the book I bought yesterday."),
                new GrammarSeed("A2: Express obligation and advice","a2-obligation-advice","Vなければなりません／Vたほうがいい","Distinguish necessary action from considerate advice.","negative stem + なければなりません; past plain + ほうがいい","Use strong obligation only when the situation genuinely requires it.","Advice uses the past plain form before ほうがいい.","今日は休んだほうがいいです。","You should rest today."),
                new GrammarSeed("A2: Describe change and continuing states","a2-change-decision","Vるようになります／Vることにします","Describe a developed ability or a personal decision.","potential/plain verb + ようになります; dictionary verb + ことにします","ようになる describes change; ことにする presents a decision.","Do not confuse a developed change with a deliberate decision.","漢字が読めるようになりました。","I became able to read kanji."),
                new GrammarSeed("A2: Report information and opinions","a2-report-opinion","plain form + と言っていました／と思います","Report another person's words and add an opinion.","plain quotation + と言っていました; plain opinion + と思います","Nouns and na-adjectives normally use だ before と.","Do not omit だ after a na-adjective before と思います.","田中さんは電車が遅れると言っていました。","Tanaka said the train would be delayed."),
                new GrammarSeed("A2: Complete an independent travel mission","a2-integrated-repair","reason + request／comparison + choice","Combine known patterns to decide and repair communication.","difficulty + から + request; comparison + から + choice","State the problem before requesting a practical response.","Do not give a reason after the choice without a connector.","漢字が読めませんから、ゆっくり説明してください。","I cannot read kanji, so please explain slowly.")
            };

            var lessonTitles = seeds.Select(seed => seed.Lesson).ToArray();
            var lessons = await context.Lessons.Where(lesson => lessonTitles.Contains(lesson.Title)).ToDictionaryAsync(lesson => lesson.Title);
            if (lessons.Count != seeds.Length) throw new InvalidOperationException("A2 grammar seeding requires all 15 A2 lessons.");
            foreach (var seed in seeds)
            {
                var grammar = await context.GrammarPoints.SingleOrDefaultAsync(point => point.Code == seed.Code);
                if (grammar == null) { grammar = new GrammarPoint { Code = seed.Code }; context.GrammarPoints.Add(grammar); }
                grammar.Pattern=seed.Pattern; grammar.Meaning=seed.Meaning; grammar.Formation=seed.Formation;
                grammar.UsageNotes=seed.Notes; grammar.CommonMistake=seed.Mistake; grammar.ExampleJapanese=seed.Japanese;
                grammar.ExampleReading=GrammarExampleReadings[seed.Code]; grammar.ExampleEnglish=seed.English; grammar.Level="A2";
                await context.SaveChangesAsync();
                var lesson = lessons[seed.Lesson];
                if (!await context.LessonGrammarPoints.AnyAsync(link => link.LessonId == lesson.Id && link.GrammarPointId == grammar.Id))
                    context.LessonGrammarPoints.Add(new LessonGrammarPoint { LessonId=lesson.Id, GrammarPointId=grammar.Id, DisplayOrder=1, IsPrimary=true });
            }
            await context.SaveChangesAsync();
        }
    }
}
