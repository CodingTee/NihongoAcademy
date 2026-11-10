using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private record KanjiSeed(string Character, string Reading, string Meaning, string Example, string ExampleReading, string ExampleMeaning, string Hint);
        private record KanjiModule(int Order, string Title, string JapaneseTitle, string Goal, KanjiSeed[] Items);

        private static async Task SeedKanjiFoundationCurriculumAsync(ApplicationDbContext context)
        {
            var category = await context.Categories.FirstAsync(c => c.Name == "Kanji");
            var modules = GetKanjiModules();

            foreach (var module in modules)
            {
                var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.Title == module.Title);
                if (lesson == null)
                {
                    lesson = new Lesson { Title = module.Title, CategoryId = category.Id };
                    context.Lessons.Add(lesson);
                }
                lesson.CategoryId = category.Id;
                lesson.JapaneseTitle = module.JapaneseTitle;
                lesson.Description = module.Goal;
                lesson.Content = BuildKanjiContent(module);
                lesson.DisplayOrder = module.Order;
                lesson.DifficultyLevel = 2;
                lesson.EstimatedMinutes = 35;
                lesson.IsPublished = true;
                await context.SaveChangesAsync();

                var targetVocabularyIds = new List<int>();
                for (var i = 0; i < module.Items.Length; i++)
                {
                    var seed = module.Items[i];
                    var vocabulary = await context.Vocabulary.FirstOrDefaultAsync(v =>
                        v.CategoryId == category.Id && v.Japanese == seed.Character);
                    if (vocabulary == null)
                    {
                        vocabulary = new Vocabulary
                        {
                            CategoryId = category.Id,
                            Japanese = seed.Character,
                            DisplayOrder = module.Order * 100 + i,
                            JlptLevel = 5
                        };
                        context.Vocabulary.Add(vocabulary);
                    }
                    vocabulary.Romaji = seed.Reading;
                    vocabulary.English = seed.Meaning;
                    vocabulary.PartOfSpeech = "Kanji";
                    vocabulary.ExampleJapanese = seed.Example;
                    vocabulary.ExampleReading = seed.ExampleReading;
                    vocabulary.ExampleEnglish = seed.ExampleMeaning;
                    await context.SaveChangesAsync();
                    targetVocabularyIds.Add(vocabulary.Id);

                    if (!await context.LessonVocabulary.AnyAsync(x => x.LessonId == lesson.Id && x.VocabularyId == vocabulary.Id))
                        context.LessonVocabulary.Add(new LessonVocabulary { LessonId = lesson.Id, VocabularyId = vocabulary.Id, DisplayOrder = i });
                }

                var staleMappings = await context.LessonVocabulary
                    .Where(x => x.LessonId == lesson.Id && !targetVocabularyIds.Contains(x.VocabularyId))
                    .ToListAsync();
                context.LessonVocabulary.RemoveRange(staleMappings);

                await SeedKanjiActivities(context, lesson, module);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedKanjiActivities(ApplicationDbContext context, Lesson lesson, KanjiModule module)
        {
            var seeds = new List<LessonActivity>
            {
                new LessonActivity
                {
                    ActivityType = "dialogue", DisplayOrder = 0,
                    Prompt = "Listen to the example words, then compare character, reading and meaning.",
                    JapaneseText = string.Join("。", module.Items.Select(x => x.Example)) + "。",
                    EnglishText = string.Join("\n", module.Items.Select(x => $"{x.Example}（{x.ExampleReading}）— {x.ExampleMeaning}")),
                    Explanation = "Kanji readings are learned inside useful words, not as an isolated list."
                }
            };

            for (var index = 0; index < module.Items.Length; index++)
            {
                var item = module.Items[index];
                var meanings = new[] { item.Meaning }.Concat(module.Items.Where(x => x.Character != item.Character).Select(x => x.Meaning))
                    .Concat(new[] { "person", "place", "time", "thing" }).Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToArray();
                var readings = new[] { item.Reading }.Concat(module.Items.Where(x => x.Character != item.Character).Select(x => x.Reading))
                    .Concat(new[] { "ひと", "もの", "とき", "ところ" }).Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToArray();
                var order = index * 3 + 1;
                seeds.Add(new LessonActivity { ActivityType="comprehension", DisplayOrder=order,
                    Prompt=$"What is the core meaning of {item.Character}?", JapaneseText=item.Example,
                    OptionsJson=JsonSerializer.Serialize(meanings), CorrectAnswer=item.Meaning,
                    Explanation=$"{item.Character} appears in {item.Example}（{item.ExampleReading}）." });
                seeds.Add(new LessonActivity { ActivityType="comprehension", DisplayOrder=order + 1,
                    Prompt=$"Which target reading of {item.Character} is used in {item.Example}?", JapaneseText=item.Example,
                    OptionsJson=JsonSerializer.Serialize(readings), CorrectAnswer=item.Reading,
                    Explanation=$"The target reading is {item.Reading}, as heard inside {item.ExampleReading}." });
                seeds.Add(new LessonActivity { ActivityType="sentence-building", DisplayOrder=order + 2,
                    Prompt=$"Type the kanji shown by this meaning: {item.Meaning}",
                    OptionsJson=JsonSerializer.Serialize(module.Items.Select(x => x.Character)),
                    CorrectAnswer=item.Character, Explanation=item.Hint });
            }

            foreach (var seed in seeds)
            {
                var activity = await context.LessonActivities.FirstOrDefaultAsync(a =>
                    a.LessonId == lesson.Id && a.ActivityType == seed.ActivityType && a.DisplayOrder == seed.DisplayOrder);
                if (activity == null)
                {
                    activity = new LessonActivity { LessonId = lesson.Id, ActivityType = seed.ActivityType, DisplayOrder = seed.DisplayOrder };
                    context.LessonActivities.Add(activity);
                }
                activity.Prompt = seed.Prompt; activity.JapaneseText = seed.JapaneseText; activity.EnglishText = seed.EnglishText;
                activity.OptionsJson = seed.OptionsJson; activity.CorrectAnswer = seed.CorrectAnswer; activity.Explanation = seed.Explanation;
            }

            var validOrders = seeds.Select(seed => seed.DisplayOrder).ToHashSet();
            var stale = await context.LessonActivities.Where(activity => activity.LessonId == lesson.Id && !validOrders.Contains(activity.DisplayOrder)).ToListAsync();
            context.LessonActivities.RemoveRange(stale);
        }

        private static string BuildKanjiContent(KanjiModule module)
        {
            var rows = string.Join("", module.Items.Select(x =>
                $"<tr><td class='fs-2'>{x.Character}</td><td>{GetStrokeCount(x.Character)}</td><td>{x.Reading}</td><td>{x.Meaning}</td><td><strong>{x.Example}</strong><br/><small>{x.ExampleReading} — {x.ExampleMeaning}</small></td><td>{x.Hint}{GetContrastNote(x.Character)}</td></tr>"));
            return $"<h2>Can-do goal</h2><p>{module.Goal}</p>" +
                "<h2>Learn kanji through words</h2><p>Notice the shape and core meaning, then learn the reading used in the example word. A kanji may have other readings later.</p>" +
                $"<div class='table-responsive'><table class='table'><thead><tr><th>Kanji</th><th>Strokes</th><th>Target reading</th><th>Core meaning</th><th>Example word</th><th>Shape cue</th></tr></thead><tbody>{rows}</tbody></table></div>" +
                "<h2>Practice sequence</h2><ol><li>Say the example word.</li><li>Identify the kanji inside it.</li><li>Recall its core meaning.</li><li>Type the character from memory.</li><li>Review again on a later day.</li></ol>";
        }

        private static string GetContrastNote(string character) => character switch
        {
            "人" => " <strong>Contrast:</strong> 人 (person) opens more widely than 入 (enter).",
            "入" => " <strong>Contrast:</strong> 入 (enter) crosses differently from 人 (person).",
            "日" => " <strong>Contrast:</strong> 日 has one inner line; 目 has two.",
            "目" => " <strong>Contrast:</strong> 目 has two inner lines; 日 has one.",
            "口" => " <strong>Contrast:</strong> 口 is an empty opening; 日 contains an inner line.",
            "木" => " <strong>Contrast:</strong> 本 adds a short root mark to 木.",
            "本" => " <strong>Contrast:</strong> 本 has a root mark that 木 does not.",
            "大" => " <strong>Contrast:</strong> 天 adds a horizontal line above 大.",
            "天" => " <strong>Contrast:</strong> 天 has a top line above 大.",
            "土" => " <strong>Contrast:</strong> 土 has the longer lower line; 士 has the longer upper line.",
            "千" => " <strong>Contrast:</strong> 千 begins with a sloping stroke; 干 begins horizontally.",
            _ => string.Empty
        };

        private static int GetStrokeCount(string character) => character switch
        {
            "一" => 1, "二" => 2, "三" => 3, "四" => 5, "五" => 4,
            "六" => 4, "七" => 2, "八" => 2, "九" => 2, "十" => 2,
            "日" => 4, "月" => 4, "年" => 6, "時" => 10, "分" => 4,
            "人" => 2, "女" => 3, "男" => 7, "父" => 4, "母" => 5,
            "学" => 8, "校" => 10, "先" => 6, "生" => 5, "語" => 14,
            "上" => 3, "下" => 3, "中" => 4, "左" => 5, "右" => 5,
            "山" => 3, "川" => 3, "木" => 4, "水" => 4, "雨" => 8,
            "本" => 5, "今" => 4, "友" => 4, "気" => 6, "行" => 6,
            "火" => 4, "金" => 8, "土" => 3, "曜" => 18, "午" => 4,
            "前" => 9, "後" => 9, "毎" => 6, "週" => 11, "半" => 5,
            "国" => 8, "外" => 5, "東" => 8, "西" => 6, "北" => 5,
            "南" => 9, "口" => 3, "出" => 5, "入" => 2, "駅" => 14,
            "来" => 7, "帰" => 10, "車" => 7, "電" => 13, "道" => 12,
            "食" => 9, "飲" => 12, "見" => 7, "聞" => 14, "読" => 14,
            "書" => 10, "話" => 13, "言" => 7, "名" => 6, "何" => 7,
            "家" => 10, "室" => 9, "店" => 8, "会" => 6, "社" => 7,
            "大" => 3, "小" => 3, "高" => 10, "安" => 6, "新" => 13,
            "古" => 5, "長" => 8, "多" => 6, "少" => 4, "白" => 5,
            "目" => 5, "耳" => 6, "手" => 4, "足" => 7, "力" => 2,
            "子" => 3, "円" => 4, "天" => 4, "空" => 8, "元" => 4,
            _ => throw new InvalidOperationException($"Missing stroke count for {character}.")
        };

        private static KanjiModule[] GetKanjiModules() => new[]
        {
            new KanjiModule(1, "Kanji Foundations: Numbers 1–5", "漢字基礎：一〜五", "I can recognise and use the kanji for one through five.", new[]
            {
                new KanjiSeed("一","いち","one","一年","いちねん","one year","One horizontal line represents one."),
                new KanjiSeed("二","に","two","二時","にじ","two o'clock","Two horizontal lines represent two."),
                new KanjiSeed("三","さん","three","三時","さんじ","three o'clock","Three horizontal lines represent three."),
                new KanjiSeed("四","よん","four","四","よん","four","The inner strokes sit inside an enclosure."),
                new KanjiSeed("五","ご","five","五月","ごがつ","May","Keep the crossing structure balanced.")
            }),
            new KanjiModule(2, "Kanji Foundations: Numbers 6–10", "漢字基礎：六〜十", "I can recognise six through ten and build basic numbers.", new[]
            {
                new KanjiSeed("六","ろく","six","六月","ろくがつ","June","A lid-like top sits over spreading lower strokes."),
                new KanjiSeed("七","しち","seven","七時","しちじ","seven o'clock","The bent stroke cuts through the horizontal."),
                new KanjiSeed("八","はち","eight","八時","はちじ","eight o'clock","Two strokes spread apart."),
                new KanjiSeed("九","きゅう","nine","九人","きゅうにん","nine people","A sweeping hook follows the first stroke."),
                new KanjiSeed("十","じゅう","ten","十人","じゅうにん","ten people","A simple cross represents ten.")
            }),
            new KanjiModule(3, "Kanji Foundations: Time and calendar", "時間と曜日の漢字", "I can recognise common kanji used in dates, days and time.", new[]
            {
                new KanjiSeed("日","にち","day / sun","日曜日","にちようび","Sunday","A sun simplified into a rectangle."),
                new KanjiSeed("月","げつ","month / moon","月曜日","げつようび","Monday","The crescent moon became a vertical form."),
                new KanjiSeed("年","ねん","year","一年","いちねん","one year","Think of the yearly harvest."),
                new KanjiSeed("時","じ","time / hour","七時","しちじ","seven o'clock","The sun radical combines with temple 寺."),
                new KanjiSeed("分","ふん","minute / part","五分","ごふん","five minutes","A knife divides something into parts.")
            }),
            new KanjiModule(4, "Kanji Foundations: People and family", "人と家族の漢字", "I can recognise high-frequency kanji for people and family.", new[]
            {
                new KanjiSeed("人","ひと","person","あの人","あのひと","that person","A person walking on two legs."),
                new KanjiSeed("女","おんな","woman","女の人","おんなのひと","woman","A seated figure forms the traditional shape cue."),
                new KanjiSeed("男","おとこ","man","男の人","おとこのひと","man","Field 田 over strength 力."),
                new KanjiSeed("父","ちち","father","父です","ちちです","is my father","Crossing strokes distinguish it from 文."),
                new KanjiSeed("母","はは","mother","母です","ははです","is my mother","Two dots sit inside the enclosing shape.")
            }),
            new KanjiModule(5, "Kanji Foundations: School and language", "学校と言葉の漢字", "I can recognise kanji in basic school and Japanese-language words.", new[]
            {
                new KanjiSeed("学","がく","study","学生","がくせい","student","A child 子 learns under a roof."),
                new KanjiSeed("校","こう","school","学校","がっこう","school","Tree 木 is the left component."),
                new KanjiSeed("先","せん","ahead / previous","先生","せんせい","teacher","Someone who was born before you."),
                new KanjiSeed("生","せい","life / birth","学生","がくせい","student","A plant grows from the ground."),
                new KanjiSeed("語","ご","language / word","日本語","にほんご","Japanese language","The speech radical 言 signals language.")
            }),
            new KanjiModule(6, "Kanji Foundations: Position", "位置の漢字", "I can read basic position kanji in signs and simple descriptions.", new[]
            {
                new KanjiSeed("上","うえ","above / up","机の上","つくえのうえ","on the desk","The mark is above the baseline."),
                new KanjiSeed("下","した","below / down","いすの下","いすのした","under the chair","The mark falls below the baseline."),
                new KanjiSeed("中","なか","inside / middle","かばんの中","かばんのなか","inside the bag","A line passes through the centre."),
                new KanjiSeed("左","ひだり","left","左です","ひだりです","it is left","The lower component is 工."),
                new KanjiSeed("右","みぎ","right","右です","みぎです","it is right","The lower component is 口.")
            }),
            new KanjiModule(7, "Kanji Foundations: Nature", "自然の漢字", "I can recognise common nature kanji and their basic words.", new[]
            {
                new KanjiSeed("山","やま","mountain","あの山","あのやま","that mountain","Three peaks form a mountain."),
                new KanjiSeed("川","かわ","river","川です","かわです","it is a river","Three flowing lines form a river."),
                new KanjiSeed("木","き","tree","木の下","きのした","under a tree","Trunk, branches and roots form a tree."),
                new KanjiSeed("水","みず","water","水をください","みずをください","water, please","The side strokes splash from the centre."),
                new KanjiSeed("雨","あめ","rain","雨です","あめです","it is raining","Drops fall inside a sky-like frame.")
            }),
            new KanjiModule(8, "Kanji Foundations: Integrated reading", "漢字総合読み", "I can decode short A1 messages containing familiar foundational kanji.", new[]
            {
                new KanjiSeed("本","ほん","book / origin","日本","にほん","Japan","A mark at the root of 木 indicates origin."),
                new KanjiSeed("今","いま","now","今は七時です","いまはしちじです","it is seven now","A cover sits over the contained moment."),
                new KanjiSeed("友","とも","friend","友だち","ともだち","friend","Two hands meet and cooperate."),
                new KanjiSeed("気","き","spirit / air","元気です","げんきです","is well / energetic","Air-like strokes surround the inner mark."),
                new KanjiSeed("行","い","go","学校に行きます","がっこうにいきます","go to school","A crossroads suggests movement.")
            }),
            new KanjiModule(9, "Kanji Foundations: Week and calendar", "曜日と暦の漢字", "I can recognise the building blocks used in weekdays and calendar expressions.", new[]
            {
                new KanjiSeed("火","か","fire","火曜日","かようび","Tuesday","Flames rise from the centre."),
                new KanjiSeed("金","きん","gold / money","金曜日","きんようび","Friday","The lower dots suggest metal underground."),
                new KanjiSeed("土","ど","earth / soil","土曜日","どようび","Saturday","A plant rises from the ground."),
                new KanjiSeed("曜","よう","weekday","月曜日","げつようび","Monday","The sun radical anchors every weekday."),
                new KanjiSeed("午","ご","noon","午後","ごご","afternoon","The crossing strokes mark the middle of the day.")
            }),
            new KanjiModule(10, "Kanji Foundations: Time expressions", "時間表現の漢字", "I can read common expressions for before, after and recurring time.", new[]
            {
                new KanjiSeed("前","まえ","before / front","駅の前","えきのまえ","in front of the station","The lower knife component cuts a point before."),
                new KanjiSeed("後","あと","after / behind","学校の後","がっこうのあと","after school","The walking radical suggests following behind."),
                new KanjiSeed("毎","まい","every","毎日","まいにち","every day","Recognise the repeated shape in recurring time words."),
                new KanjiSeed("週","しゅう","week","毎週","まいしゅう","every week","The movement radical wraps around 周."),
                new KanjiSeed("半","はん","half","七時半","しちじはん","half past seven","The vertical line divides the form in half.")
            }),
            new KanjiModule(11, "Kanji Foundations: Countries and regions", "国と方角の漢字", "I can recognise basic country and regional direction words.", new[]
            {
                new KanjiSeed("国","くに","country","この国","このくに","this country","A jewel 玉 is enclosed by a national border."),
                new KanjiSeed("外","そと","outside","外にいます","そとにいます","be outside","The components point beyond the boundary."),
                new KanjiSeed("東","ひがし","east","東口","ひがしぐち","east exit","The sun rises through a tree in the east."),
                new KanjiSeed("西","にし","west","西口","にしぐち","west exit","The sun settles in the west."),
                new KanjiSeed("北","きた","north","北口","きたぐち","north exit","Two figures face away from each other.")
            }),
            new KanjiModule(12, "Kanji Foundations: Signs and stations", "標識と駅の漢字", "I can follow basic direction and station signs.", new[]
            {
                new KanjiSeed("南","みなみ","south","南口","みなみぐち","south exit","The detailed centre hangs inside the outer frame."),
                new KanjiSeed("口","くち","mouth / opening","口を開けます","くちをあけます","open one's mouth","A simple opening forms a square."),
                new KanjiSeed("出","で","exit / go out","出口","でぐち","exit","One mountain-like shape emerges from another."),
                new KanjiSeed("入","はい","enter","入ります","はいります","enter","The strokes narrow as they enter."),
                new KanjiSeed("駅","えき","station","駅前","えきまえ","in front of the station","The horse radical appears on the left.")
            }),
            new KanjiModule(13, "Kanji Foundations: Travel and movement", "移動の漢字", "I can recognise high-frequency kanji in basic travel expressions.", new[]
            {
                new KanjiSeed("来","き","come","来ます","きます","come","A person arrives through the branching centre."),
                new KanjiSeed("帰","かえ","return","帰ります","かえります","return home","The right side suggests returning to a place."),
                new KanjiSeed("車","しゃ","vehicle","電車","でんしゃ","train","The central axle sits inside a vehicle."),
                new KanjiSeed("電","でん","electricity","電車","でんしゃ","train","Rain above and a bent current below suggest electricity."),
                new KanjiSeed("道","みち","road / way","この道","このみち","this road","The movement radical carries a head along a road.")
            }),
            new KanjiModule(14, "Kanji Foundations: Everyday actions", "日常動作の漢字", "I can recognise five essential action kanji inside familiar verbs.", new[]
            {
                new KanjiSeed("食","た","eat","食べます","たべます","eat","A lid covers gathered food."),
                new KanjiSeed("飲","の","drink","水を飲みます","みずをのみます","drink water","The food radical combines with 欠."),
                new KanjiSeed("見","み","see / look","見ます","みます","see / watch","An eye 目 stands on legs."),
                new KanjiSeed("聞","き","hear / ask","聞きます","ききます","listen / ask","An ear 耳 listens inside a gate."),
                new KanjiSeed("読","よ","read","本を読みます","ほんをよみます","read a book","The speech radical introduces written language.")
            }),
            new KanjiModule(15, "Kanji Foundations: Communication", "伝える漢字", "I can recognise basic kanji used for writing, speaking, names and questions.", new[]
            {
                new KanjiSeed("書","か","write","名前を書きます","なまえをかきます","write a name","A writing brush forms the stacked shape."),
                new KanjiSeed("話","はな","speak / talk","日本語を話します","にほんごをはなします","speak Japanese","Speech 言 combines with tongue 舌."),
                new KanjiSeed("言","い","say / word","言います","いいます","say","Lines of words emerge from a mouth."),
                new KanjiSeed("名","な","name","名前","なまえ","name","A mouth identifies someone in the evening."),
                new KanjiSeed("何","なん","what","何ですか","なんですか","what is it?","The person radical stands beside 可.")
            }),
            new KanjiModule(16, "Kanji Foundations: Home and places", "家と場所の漢字", "I can recognise common kanji for home, rooms, shops and organisations.", new[]
            {
                new KanjiSeed("家","いえ","house / home","家に帰ります","いえにかえります","return home","A roof shelters the shape below."),
                new KanjiSeed("室","しつ","room","教室","きょうしつ","classroom","A roof covers the arriving component 至."),
                new KanjiSeed("店","みせ","shop","店の前","みせのまえ","in front of the shop","A roof shelters a place of business."),
                new KanjiSeed("会","あ","meet","会います","あいます","meet","Two people come together under one cover."),
                new KanjiSeed("社","しゃ","company / shrine","会社","かいしゃ","company","The altar radical appears on the left.")
            }),
            new KanjiModule(17, "Kanji Foundations: Size and value", "大きさと価値の漢字", "I can read common descriptions of size, price and age.", new[]
            {
                new KanjiSeed("大","おお","big","大きい","おおきい","big","A person stretches arms wide."),
                new KanjiSeed("小","ちい","small","小さい","ちいさい","small","Three small marks gather together."),
                new KanjiSeed("高","たか","high / expensive","高いです","たかいです","is expensive","A tall building rises in stacked levels."),
                new KanjiSeed("安","やす","inexpensive / safe","安いです","やすいです","is inexpensive","A peaceful figure rests under a roof."),
                new KanjiSeed("新","あたら","new","新しい本","あたらしいほん","new book","An axe 斤 shapes standing wood into something new.")
            }),
            new KanjiModule(18, "Kanji Foundations: Basic descriptions", "基本的な描写の漢字", "I can recognise common kanji that describe age, length, quantity and colour.", new[]
            {
                new KanjiSeed("古","ふる","old","古い本","ふるいほん","old book","Ten 十 sits over mouth 口, suggesting old stories."),
                new KanjiSeed("長","なが","long / leader","長い道","ながいみち","long road","The extended final stroke gives a long shape."),
                new KanjiSeed("多","おお","many","人が多いです","ひとがおおいです","there are many people","Two evening shapes stack into many."),
                new KanjiSeed("少","すく","few / little","人が少ないです","ひとがすくないです","there are few people","Small 小 loses a stroke, leaving fewer."),
                new KanjiSeed("白","しろ","white","白い車","しろいくるま","white car","A bright drop appears above the sun-like box.")
            }),
            new KanjiModule(19, "Kanji Foundations: Body and senses", "体と感覚の漢字", "I can recognise basic body kanji used in everyday words and instructions.", new[]
            {
                new KanjiSeed("目","め","eye","目を見ます","めをみます","look at the eyes","The rectangle contains the lines of an eye."),
                new KanjiSeed("耳","みみ","ear","耳です","みみです","it is an ear","Horizontal lines show the inner ear."),
                new KanjiSeed("手","て","hand","手を上げます","てをあげます","raise a hand","Fingers extend from the central palm."),
                new KanjiSeed("足","あし","foot / leg","足です","あしです","it is a foot","A kneeling leg extends below a mouth-like top."),
                new KanjiSeed("力","ちから","power / strength","力があります","ちからがあります","have strength","A bent arm supplies strength.")
            }),
            new KanjiModule(20, "Kanji Foundations: Final integrated reading", "N5漢字総合読み", "I can combine all one hundred target kanji in short beginner messages.", new[]
            {
                new KanjiSeed("子","こ","child","女の子","おんなのこ","girl","A child stretches both arms."),
                new KanjiSeed("円","えん","yen / circle","五百円","ごひゃくえん","500 yen","The enclosing curve recalls a round coin."),
                new KanjiSeed("天","てん","sky / heaven","天気","てんき","weather","A line above 大 indicates the sky."),
                new KanjiSeed("空","そら","sky / empty","空です","そらです","it is the sky","A hole under a roof opens into empty space."),
                new KanjiSeed("元","げん","origin / base","元気です","げんきです","is well","Two legs support the original base.")
            })
        };
    }
}
