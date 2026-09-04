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
                    vocabulary.ExampleEnglish = $"{seed.ExampleReading} — {seed.ExampleMeaning}";
                    await context.SaveChangesAsync();

                    if (!await context.LessonVocabulary.AnyAsync(x => x.LessonId == lesson.Id && x.VocabularyId == vocabulary.Id))
                        context.LessonVocabulary.Add(new LessonVocabulary { LessonId = lesson.Id, VocabularyId = vocabulary.Id, DisplayOrder = i });
                }

                await SeedKanjiActivities(context, lesson, module);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedKanjiActivities(ApplicationDbContext context, Lesson lesson, KanjiModule module)
        {
            var first = module.Items[0];
            var last = module.Items[^1];
            var distractors = module.Items.Select(x => x.Meaning).Distinct().Take(4).ToArray();
            if (distractors.Length < 4) distractors = distractors.Concat(new[] { "person", "place", "time", "thing" }).Distinct().Take(4).ToArray();

            var seeds = new[]
            {
                new LessonActivity
                {
                    ActivityType = "dialogue", DisplayOrder = 0,
                    Prompt = "Listen to the example words, then compare character, reading and meaning.",
                    JapaneseText = string.Join("。", module.Items.Select(x => x.Example)) + "。",
                    EnglishText = string.Join("\n", module.Items.Select(x => $"{x.Example}（{x.ExampleReading}）— {x.ExampleMeaning}")),
                    Explanation = "Kanji readings are learned inside useful words, not as an isolated list."
                },
                new LessonActivity
                {
                    ActivityType = "comprehension", DisplayOrder = 1,
                    Prompt = $"What is the core meaning of {first.Character}?", JapaneseText = first.Example,
                    OptionsJson = JsonSerializer.Serialize(distractors.Contains(first.Meaning) ? distractors : new[] { first.Meaning }.Concat(distractors).Take(4)),
                    CorrectAnswer = first.Meaning, Explanation = $"{first.Character} appears in {first.Example}（{first.ExampleReading}）."
                },
                new LessonActivity
                {
                    ActivityType = "comprehension", DisplayOrder = 2,
                    Prompt = $"Which reading is used in {last.Example}?", JapaneseText = last.Example,
                    OptionsJson = JsonSerializer.Serialize(new[] { last.Reading }
                        .Concat(module.Items.Select(x => x.Reading).Where(x => x != last.Reading)).Distinct().Take(4)),
                    CorrectAnswer = last.Reading, Explanation = $"Read it as {last.ExampleReading} in this word."
                },
                new LessonActivity
                {
                    ActivityType = "sentence-building", DisplayOrder = 3,
                    Prompt = $"Type the kanji shown by this meaning: {first.Meaning}",
                    OptionsJson = JsonSerializer.Serialize(module.Items.Select(x => x.Character)),
                    CorrectAnswer = first.Character, Explanation = first.Hint
                }
            };

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
        }

        private static string BuildKanjiContent(KanjiModule module)
        {
            var rows = string.Join("", module.Items.Select(x =>
                $"<tr><td class='fs-2'>{x.Character}</td><td>{x.Reading}</td><td>{x.Meaning}</td><td><strong>{x.Example}</strong><br/><small>{x.ExampleReading} — {x.ExampleMeaning}</small></td><td>{x.Hint}</td></tr>"));
            return $"<h2>Can-do goal</h2><p>{module.Goal}</p>" +
                "<h2>Learn kanji through words</h2><p>Notice the shape and core meaning, then learn the reading used in the example word. A kanji may have other readings later.</p>" +
                $"<div class='table-responsive'><table class='table'><thead><tr><th>Kanji</th><th>Target reading</th><th>Core meaning</th><th>Example word</th><th>Shape cue</th></tr></thead><tbody>{rows}</tbody></table></div>" +
                "<h2>Practice sequence</h2><ol><li>Say the example word.</li><li>Identify the kanji inside it.</li><li>Recall its core meaning.</li><li>Type the character from memory.</li><li>Review again on a later day.</li></ol>";
        }

        private static KanjiModule[] GetKanjiModules() => new[]
        {
            new KanjiModule(1, "Kanji Foundations: Numbers 1–5", "漢字基礎：一〜五", "I can recognise and use the kanji for one through five.", new[]
            {
                new KanjiSeed("一","いち","one","一つ","ひとつ","one item","One horizontal line represents one."),
                new KanjiSeed("二","に","two","二人","ふたり","two people","Two horizontal lines represent two."),
                new KanjiSeed("三","さん","three","三時","さんじ","three o'clock","Three horizontal lines represent three."),
                new KanjiSeed("四","よん","four","四つ","よっつ","four items","The inner strokes sit inside an enclosure."),
                new KanjiSeed("五","ご","five","五月","ごがつ","May","Keep the crossing structure balanced.")
            }),
            new KanjiModule(2, "Kanji Foundations: Numbers 6–10", "漢字基礎：六〜十", "I can recognise six through ten and build basic numbers.", new[]
            {
                new KanjiSeed("六","ろく","six","六月","ろくがつ","June","A lid-like top sits over spreading lower strokes."),
                new KanjiSeed("七","なな","seven","七時","しちじ","seven o'clock","The bent stroke cuts through the horizontal."),
                new KanjiSeed("八","はち","eight","八日","ようか","eighth day","Two strokes spread apart."),
                new KanjiSeed("九","きゅう","nine","九時","くじ","nine o'clock","A sweeping hook follows the first stroke."),
                new KanjiSeed("十","じゅう","ten","十人","じゅうにん","ten people","A simple cross represents ten.")
            }),
            new KanjiModule(3, "Kanji Foundations: Time and calendar", "時間と曜日の漢字", "I can recognise common kanji used in dates, days and time.", new[]
            {
                new KanjiSeed("日","にち","day / sun","日曜日","にちようび","Sunday","A sun simplified into a rectangle."),
                new KanjiSeed("月","げつ","month / moon","月曜日","げつようび","Monday","The crescent moon became a vertical form."),
                new KanjiSeed("年","ねん","year","今年","ことし","this year","Think of the yearly harvest."),
                new KanjiSeed("時","じ","time / hour","七時","しちじ","seven o'clock","The sun radical combines with temple 寺."),
                new KanjiSeed("分","ふん","minute / part","五分","ごふん","five minutes","A knife divides something into parts.")
            }),
            new KanjiModule(4, "Kanji Foundations: People and family", "人と家族の漢字", "I can recognise high-frequency kanji for people and family.", new[]
            {
                new KanjiSeed("人","ひと","person","日本人","にほんじん","Japanese person","A person walking on two legs."),
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
                new KanjiSeed("山","やま","mountain","富士山","ふじさん","Mount Fuji","Three peaks form a mountain."),
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
            })
        };
    }
}
