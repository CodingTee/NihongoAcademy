using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private record A1Module(int Order, string Title, string JapaneseTitle, int Minutes, string Goal, string Content, string[] Words);

        private static async Task SeedRemainingA1CurriculumAsync(ApplicationDbContext context)
        {
            var category = await context.Categories.FirstAsync(c => c.Name == "A1 Course");
            foreach (var module in GetA1Modules())
            {
                var lesson = await UpsertA1Lesson(context, category.Id, module.Title, module.JapaneseTitle,
                    module.Order, module.Minutes, module.Goal, module.Content);
                var items = new List<Vocabulary>();
                for (var i = 0; i < module.Words.Length; i++)
                {
                    var parts = module.Words[i].Split('|');
                    var item = await context.Vocabulary.FirstOrDefaultAsync(v => v.CategoryId == category.Id && v.Japanese == parts[0]);
                    if (item == null)
                    {
                        item = new Vocabulary { CategoryId = category.Id, Japanese = parts[0], Romaji = parts[1], English = parts[2], PartOfSpeech = parts[3], DisplayOrder = module.Order * 100 + i, JlptLevel = 5 };
                        context.Vocabulary.Add(item);
                    }
                    else { item.Romaji = parts[1]; item.English = parts[2]; item.PartOfSpeech = parts[3]; }
                    items.Add(item);
                }
                await context.SaveChangesAsync();
                await MapLessonItems(context, lesson, items);
            }
        }

        private static A1Module[] GetA1Modules() => new[]
        {
            new A1Module(5, "A1: Identify everyday things", "身の回りの物", 35,
                "I can identify everyday objects, ask what something is, and say who it belongs to.",
                "<h2>Scene</h2><p>You are in a classroom or lost-property office.</p><h2>Core language</h2><p><strong>これは なんですか。</strong> What is this?<br/><strong>それは ほんです。</strong><br/><strong>これは だれの かさですか。</strong><br/><strong>わたしのです。</strong></p><h2>System</h2><p>これ／それ／あれ stand alone. この／その／あの must precede a noun. Their choice depends on distance from speaker and listener.</p><h2>Output task</h2><p>Identify five objects, ask about one unknown item, and say who two items belong to.</p>",
                new[]{"これ|kore|this one|Demonstrative","それ|sore|that one near you|Demonstrative","あれ|are|that one over there|Demonstrative","どれ|dore|which one|Question","この|kono|this ...|Determiner","その|sono|that ...|Determiner","あの|ano|that ... over there|Determiner","なん|nan|what|Question","ほん|hon|book|Noun","かばん|kaban|bag|Noun","かさ|kasa|umbrella|Noun","でんわ|denwa|telephone|Noun"}),

            new A1Module(6, "A1: Use numbers, ages and prices", "数字・年齢・値段", 40,
                "I can understand basic numbers, state an age, and ask or understand a simple price.",
                "<h2>Number building</h2><p>十一 is ten-one, 二十 is two-ten, and 二十一 is two-ten-one.</p><h2>Sound changes</h2><p>300 さんびゃく, 600 ろっぴゃく, 800 はっぴゃく. Four, seven and nine have context-dependent readings.</p><h2>Age and price</h2><p><strong>なんさいですか。</strong> can be sensitive with adults.<br/><strong>これは いくらですか。</strong><br/><strong>せんえんです。</strong></p><h2>Output task</h2><p>Read three prices, ask two prices and complete a short payment exchange.</p>",
                new[]{"いち|ichi|one|Number","に|ni|two|Number","さん|san|three|Number","よん|yon|four|Number","ご|go|five|Number","ろく|roku|six|Number","なな|nana|seven|Number","はち|hachi|eight|Number","きゅう|kyuu|nine|Number","じゅう|juu|ten|Number","ひゃく|hyaku|hundred|Number","せん|sen|thousand|Number","えん|en|yen|Counter","いくら|ikura|how much|Question","なんさい|nansai|how old|Question"}),

            new A1Module(7, "A1: Tell time and make a schedule", "時間と予定", 40,
                "I can ask and tell the time and state when a simple activity happens.",
                "<h2>Time</h2><p><strong>いま なんじですか。</strong><br/><strong>くじはんです。</strong> Four, seven and nine o'clock are よじ, しちじ and くじ.</p><h2>Particle に</h2><p>Use に with a specific time: しちじに おきます. Relative times such as きょう and あした normally omit it.</p><h2>Output task</h2><p>Create a three-item schedule and ask when one activity begins.</p>",
                new[]{"いま|ima|now|Noun","なんじ|nanji|what time|Question","じ|ji|o'clock|Counter","はん|han|half past|Time","ふん|fun|minute|Counter","あさ|asa|morning|Noun","ひる|hiru|daytime|Noun","ばん|ban|evening|Noun","きょう|kyou|today|Noun","あした|ashita|tomorrow|Noun","げつようび|getsu youbi|Monday|Day","しちじ|shichiji|seven o'clock|Time","くじ|kuji|nine o'clock|Time"}),

            new A1Module(8, "A1: Describe your daily routine", "毎日の生活", 45,
                "I can describe a simple daily routine using basic polite verbs and time expressions.",
                "<h2>Polite verbs</h2><p>ます describes habitual or future actions; ません is negative.</p><h2>Particles</h2><p><strong>あさごはんを たべます。</strong> を marks the object.<br/><strong>がっこうに いきます。</strong> に marks the destination.</p><h2>Word order</h2><p>Context and time normally come before the final verb: まいにち しちじに おきます。</p><h2>Output task</h2><p>Describe four stages of your day and ask one routine question.</p>",
                new[]{"おきます|okimasu|get up|Verb","ねます|nemasu|go to bed|Verb","たべます|tabemasu|eat|Verb","のみます|nomimasu|drink|Verb","いきます|ikimasu|go|Verb","きます|kimasu|come|Verb","かえります|kaerimasu|return home|Verb","べんきょうします|benkyou shimasu|study|Verb","はたらきます|hatarakimasu|work|Verb","まいにち|mainichi|every day|Noun","あさごはん|asagohan|breakfast|Noun","がっこう|gakkou|school|Noun"}),

            new A1Module(9, "A1: Find people and things", "場所と存在", 40,
                "I can ask where a person or object is and describe its basic location.",
                "<h2>Existence</h2><p>Use あります for objects and plants; use います for people and animals.</p><h2>Location</h2><p><strong>ほんは つくえの うえに あります。</strong><br/><strong>ねこは いすの したに います。</strong></p><h2>Output task</h2><p>Describe three locations in a room and ask where one person or item is.</p>",
                new[]{"ここ|koko|here|Place","そこ|soko|there near you|Place","あそこ|asoko|over there|Place","どこ|doko|where|Question","うえ|ue|above / on|Position","した|shita|below / under|Position","なか|naka|inside|Position","となり|tonari|next to|Position","あります|arimasu|exist (inanimate)|Verb","います|imasu|exist (animate)|Verb","つくえ|tsukue|desk|Noun","ねこ|neko|cat|Noun"}),

            new A1Module(10, "A1: Describe places and objects", "物と場所を説明する", 40,
                "I can describe familiar things and places with basic adjectives.",
                "<h2>Two adjective types</h2><p>い-adjective: おおきいです; negative おおきくないです. な-adjective: しずかです; before a noun しずかな まち.</p><h2>Exception awareness</h2><p>きれい is a な-adjective despite ending in い.</p><h2>Output task</h2><p>Describe your town or classroom with three adjectives and contrast two objects.</p>",
                new[]{"おおきい|ookii|big|I-adjective","ちいさい|chiisai|small|I-adjective","あたらしい|atarashii|new|I-adjective","ふるい|furui|old (thing)|I-adjective","たかい|takai|expensive / high|I-adjective","やすい|yasui|inexpensive|I-adjective","きれい|kirei|beautiful / clean|Na-adjective","しずか|shizuka|quiet|Na-adjective","にぎやか|nigiyaka|lively|Na-adjective","まち|machi|town|Noun"}),

            new A1Module(11, "A1: Talk about likes and abilities", "好きなこと・できること", 40,
                "I can say what I like, understand, or can do, and ask another person.",
                "<h2>Use が</h2><p><strong>おんがくが すきです。</strong><br/><strong>にほんごが すこし わかります。</strong><br/><strong>りょうりが できます。</strong></p><h2>Nuance</h2><p>じょうず commonly praises someone else. For yourself, state what you can do instead.</p><h2>Output task</h2><p>State two likes and one ability, then ask two interest questions.</p>",
                new[]{"すき|suki|liked / favourite|Na-adjective","きらい|kirai|disliked|Na-adjective","じょうず|jouzu|skilful|Na-adjective","へた|heta|unskilful|Na-adjective","わかります|wakarimasu|understand|Verb","できます|dekimasu|can do|Verb","りょうり|ryouri|cooking|Noun","スポーツ|supootsu|sports|Noun","おんがく|ongaku|music|Noun","えいご|eigo|English language|Noun","すこし|sukoshi|a little|Adverb"}),

            new A1Module(12, "A1: Make requests and ask permission", "依頼と許可", 45,
                "I can make a polite request, ask permission, and understand a basic prohibition.",
                "<h2>Request</h2><p>Verb て-form + ください: ちょっと まってください.</p><h2>Permission</h2><p><strong>しゃしんを とっても いいですか。</strong><br/>はい、いいです。／すみません、ちょっと…</p><h2>Prohibition</h2><p><strong>ここで しゃしんを とっては いけません。</strong></p><h2>Output task</h2><p>Role-play a museum: make two requests, ask permission and respond politely to refusal.</p>",
                new[]{"みてください|mite kudasai|please look|Expression","きいてください|kiite kudasai|please listen|Expression","まってください|matte kudasai|please wait|Expression","はいってもいいですか|haitte mo ii desu ka|may I enter?|Expression","しゃしん|shashin|photograph|Noun","とります|torimasu|take|Verb","つかいます|tsukaimasu|use|Verb","だめ|dame|not allowed|Expression","ここで|koko de|here (action place)|Expression"}),

            new A1Module(13, "A1: Order food and drinks", "店で注文する", 45,
                "I can read a simple menu, order items with quantities, and understand a basic total.",
                "<h2>Order</h2><p><strong>ラーメンを ひとつ おねがいします。</strong><br/><strong>おちゃを ふたつ ください。</strong></p><h2>Restaurant language</h2><p>Respond to ごちゅうもんは？ and いじょうですか. いただきます and ごちそうさまでした express appreciation around a meal.</p><h2>Output task</h2><p>Order two items, state quantities, answer a confirmation and understand the total.</p>",
                new[]{"メニュー|menyuu|menu|Noun","みず|mizu|water|Noun","おちゃ|ocha|tea|Noun","ごはん|gohan|rice / meal|Noun","ラーメン|raamen|ramen|Noun","ひとつ|hitotsu|one item|Counter","ふたつ|futatsu|two items|Counter","ください|kudasai|please give me|Expression","ぜんぶで|zenbu de|in total|Expression","いただきます|itadakimasu|said before eating|Expression","ごちそうさまでした|gochisousama deshita|said after a meal|Expression"}),

            new A1Module(14, "A1: Shop for an item", "買い物をする", 45,
                "I can ask for an item, understand its price, and choose a size or colour.",
                "<h2>Ask</h2><p><strong>その かばんを みせてください。</strong><br/><strong>あかは ありますか。</strong><br/><strong>おおきい サイズは ありますか。</strong></p><h2>Choose</h2><p><strong>これに します。</strong> に marks the chosen option.</p><h2>Output task</h2><p>Ask for an item, specify colour or size, ask the price and choose payment.</p>",
                new[]{"あか|aka|red|Colour","あお|ao|blue|Colour","しろ|shiro|white|Colour","くろ|kuro|black|Colour","サイズ|saizu|size|Noun","みせてください|misete kudasai|please show me|Expression","これにします|kore ni shimasu|I will take this|Expression","カード|kaado|card|Noun","げんきん|genkin|cash|Noun"}),

            new A1Module(15, "A1: Travel and ask directions", "交通・道を聞く", 45,
                "I can ask where a place is, understand simple directions, and say how I travel.",
                "<h2>Ask</h2><p><strong>すみません、えきは どこですか。</strong></p><h2>Directions</h2><p><strong>まっすぐ いって、みぎに まがってください。</strong></p><h2>Transport</h2><p><strong>バスで いきます。</strong> で marks means. <strong>バスに のります。</strong> に marks what you board.</p><h2>Output task</h2><p>Ask for a station, follow three steps and explain your usual transport.</p>",
                new[]{"えき|eki|station|Noun","バス|basu|bus|Noun","でんしゃ|densha|train|Noun","あるいて|aruite|on foot|Expression","みぎ|migi|right|Direction","ひだり|hidari|left|Direction","まっすぐ|massugu|straight|Direction","まがります|magarimasu|turn|Verb","のります|norimasu|board / ride|Verb","おります|orimasu|get off|Verb","つぎ|tsugi|next|Noun","どうやって|dou yatte|how / by what means|Question"}),

            new A1Module(16, "A1: Invite someone and make a plan", "誘う・予定を決める", 40,
                "I can invite someone, accept or decline politely, and agree on a time and place.",
                "<h2>Invite</h2><p><strong>いっしょに えいがを みませんか。</strong></p><h2>Accept</h2><p><strong>いいですね。いきましょう。</strong></p><h2>Decline softly</h2><p><strong>すみません、しゅうまつは ちょっと…</strong> Here ちょっと signals a tactful refusal.</p><h2>Output task</h2><p>Invite someone, handle acceptance or refusal, and settle time and place.</p>",
                new[]{"いっしょに|issho ni|together|Adverb","いきませんか|ikimasen ka|would you like to go?|Invitation","たべませんか|tabemasen ka|would you like to eat?|Invitation","いきましょう|ikimashou|let's go|Suggestion","いいですね|ii desu ne|sounds good|Expression","ちょっと|chotto|soft refusal cue|Expression","こんど|kondo|next time|Noun","しゅうまつ|shuumatsu|weekend|Noun","えいが|eiga|movie|Noun","こうえん|kouen|park|Noun"}),

            new A1Module(17, "A1: Talk about a past day", "過去の一日を話す", 45,
                "I can say where I went and what I did using basic polite past forms.",
                "<h2>Polite past</h2><p>Change ます to ました; negative past is ませんでした.</p><h2>Past adjectives</h2><p>たのしいです → たのしかったです. Nouns and な-adjectives use でした.</p><h2>Model</h2><p>きのう こうえんに いきました。ともだちと しゃしんを とりました。たのしかったです。</p><h2>Output task</h2><p>Give a three-sentence account and answer どうでしたか.</p>",
                new[]{"きのう|kinou|yesterday|Noun","せんしゅう|senshuu|last week|Noun","いきました|ikimashita|went|Verb","みました|mimashita|saw / watched|Verb","たべました|tabemashita|ate|Verb","のみました|nomimashita|drank|Verb","かいました|kaimashita|bought|Verb","しませんでした|shimasen deshita|did not do|Verb","たのしかったです|tanoshikatta desu|was enjoyable|Expression","どうでしたか|dou deshita ka|how was it?|Question"}),

            new A1Module(18, "A1: Complete a real-life mission", "A1総合タスク", 60,
                "I can combine beginner Japanese to meet someone, make a plan, travel, and complete a purchase or order.",
                "<h2>Mission</h2><p>Meet a classmate, introduce yourselves, arrange a meal, ask how to reach the restaurant and order.</p><h2>Repair communication</h2><p>Use <strong>もういちど おねがいします</strong>, <strong>ゆっくり おねがいします</strong>, and <strong>わかりません</strong>.</p><h2>Checkpoints</h2><ol><li>Exchange personal information.</li><li>Invite and agree on time/place.</li><li>Ask directions.</li><li>Order with quantity and understand a price.</li><li>Close politely.</li></ol><h2>Output task</h2><p>Perform the whole interaction, apply feedback, then repeat without the model.</p>",
                new[]{"もういちど|mou ichido|one more time|Expression","ゆっくり|yukkuri|slowly|Adverb","おねがいします|onegai shimasu|please|Expression","わかりません|wakarimasen|I do not understand|Expression","だいじょうぶです|daijoubu desu|it is okay / no thank you|Expression","ありがとう|arigatou|thank you|Expression"})
        };
    }
}
