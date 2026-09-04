using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Models;
using System.Text.Json;

namespace NihongoAcademy.Data
{
    public static partial class DataSeeder
    {
        private record ActivitySeed(string Type, string Prompt, string Japanese, string English,
            string[]? Options, string? Answer, string Explanation);

        private static async Task SeedA1FoundationActivitiesAsync(ApplicationDbContext context)
        {
            var curriculum = new Dictionary<string, ActivitySeed[]>
            {
                ["A1: Greetings for the situation"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen first, then reveal the meaning.",
                        "先生：おはようございます。\nリー：おはようございます、先生。",
                        "Teacher: Good morning.\nLee: Good morning, teacher.", null, null,
                        "A polite morning greeting is appropriate when meeting a teacher."),
                    new ActivitySeed("comprehension", "When does this exchange most likely happen?",
                        "先生：おはようございます。\nリー：おはようございます。", "",
                        new[] { "In the morning", "In the evening", "At a first meeting", "When saying goodbye" }, "In the morning",
                        "おはようございます is the polite morning greeting."),
                    new ActivitySeed("comprehension", "Which expression fits meeting someone for the first time?",
                        "", "", new[] { "はじめまして", "こんばんは", "さようなら", "いただきます" }, "はじめまして",
                        "はじめまして is reserved for a first meeting."),
                    new ActivitySeed("sentence-building", "Build: Nice to meet you. I look forward to knowing you.",
                        "", "", new[] { "はじめまして。", "よろしく", "おねがいします。" },
                        "はじめまして。よろしくおねがいします。", "This is a natural opening and closing pair for a first greeting.")
                },
                ["A1: Introduce yourself"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the speaker's name, country and role.",
                        "リー：はじめまして。リーです。マレーシアからきました。がくせいです。\n田中：はじめまして。田中です。よろしくおねがいします。",
                        "Lee: Nice to meet you. I'm Lee. I came from Malaysia. I'm a student.\nTanaka: Nice to meet you. I'm Tanaka. It's a pleasure to meet you.", null, null,
                        "Introductions use short, clear sentences rather than one long sentence."),
                    new ActivitySeed("comprehension", "Where is Lee from?",
                        "リーです。マレーシアからきました。", "", new[] { "Malaysia", "Japan", "China", "Korea" }, "Malaysia",
                        "マレーシアからきました means 'I came from Malaysia'."),
                    new ActivitySeed("comprehension", "What is Lee's role?",
                        "リーです。がくせいです。", "", new[] { "Student", "Teacher", "Employee", "Doctor" }, "Student",
                        "がくせい means student."),
                    new ActivitySeed("sentence-building", "Build: I am a student.", "", "",
                        new[] { "わたしは", "がくせい", "です。" }, "わたしはがくせいです。",
                        "The topic comes first, and です completes the polite identification sentence.")
                },
                ["A1: Talk about your family"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen and identify the people in the photograph.",
                        "A：この人はだれですか。\nB：わたしの母です。こちらは兄です。",
                        "A: Who is this person?\nB: This is my mother. This person is my older brother.", null, null,
                        "When speaking outside the family, 母 and 兄 refer to your own family members."),
                    new ActivitySeed("comprehension", "Who is the first person identified?",
                        "わたしの母です。", "", new[] { "The speaker's mother", "The listener's mother", "The speaker's sister", "A friend" }, "The speaker's mother",
                        "わたしの母 means my mother."),
                    new ActivitySeed("comprehension", "Which term normally refers to your own father when speaking to an outsider?",
                        "", "", new[] { "ちち", "おとうさん", "おにいさん", "はは" }, "ちち",
                        "ちち is the usual humble in-group term for one's own father."),
                    new ActivitySeed("sentence-building", "Build: This is my older sister.", "", "",
                        new[] { "これは", "わたしの", "あねです。" }, "これはわたしのあねです。",
                        "の connects the person to the speaker: わたしのあね." )
                },
                ["A1: Describe people simply"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for two facts about Maria.",
                        "これはマリアさんです。がくせいです。やさしいです。",
                        "This is Maria. She is a student. She is kind.", null, null,
                        "Japanese can connect simple facts as separate short sentences."),
                    new ActivitySeed("comprehension", "Which two facts describe Maria?",
                        "マリアさんはがくせいです。やさしいです。", "",
                        new[] { "She is a student and kind", "She is a teacher and busy", "She is a student and quiet", "She is family" },
                        "She is a student and kind", "がくせい is student and やさしい is kind."),
                    new ActivitySeed("comprehension", "What does やさしい describe here?",
                        "マリアさんはやさしいです。", "", new[] { "Maria's quality", "Maria's country", "Maria's age", "Maria's job" },
                        "Maria's quality", "An adjective after the topic describes that person's quality."),
                    new ActivitySeed("sentence-building", "Build: My older brother is energetic.", "", "",
                        new[] { "あには", "げんき", "です。" }, "あにはげんきです。",
                        "は marks the older brother as topic; げんきです describes him politely.")
                },
                ["A1: Identify everyday things"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the object and its owner.",
                        "A：これはなんですか。\nB：それはかさです。\nA：だれのかさですか。\nB：田中さんのです。",
                        "A: What is this?\nB: That is an umbrella.\nA: Whose umbrella is it?\nB: It is Tanaka's.", null, null,
                        "これ refers to something near the speaker; それ refers to something near the listener."),
                    new ActivitySeed("comprehension", "What object are they discussing?", "それはかさです。", "",
                        new[] { "An umbrella", "A book", "A bag", "A telephone" }, "An umbrella", "かさ means umbrella."),
                    new ActivitySeed("comprehension", "Who owns the object?", "田中さんのです。", "",
                        new[] { "Tanaka", "The speaker", "The listener", "Nobody" }, "Tanaka", "田中さんのです means it is Tanaka's."),
                    new ActivitySeed("sentence-building", "Build: What is this?", "", "",
                        new[] { "これは", "なん", "ですか。" }, "これはなんですか。", "これ stands alone; この would require a following noun.")
                },
                ["A1: Use numbers, ages and prices"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the price and the quantity.",
                        "客：これはいくらですか。\n店員：千二百円です。\n客：二つください。",
                        "Customer: How much is this?\nClerk: It is 1,200 yen.\nCustomer: Two, please.", null, null,
                        "Prices combine number units before 円; 二つ counts general items."),
                                       new ActivitySeed("comprehension", "How much is one item?", "千二百円です。", "",
                        new[] { "1,200 yen", "2,100 yen", "200 yen", "12,000 yen" }, "1,200 yen", "千二百 is one thousand two hundred."),
                    new ActivitySeed("comprehension", "How many items does the customer request?", "二つください。", "",
                        new[] { "Two", "One", "Three", "Ten" }, "Two", "二つ is the general counter form for two items."),
                    new ActivitySeed("sentence-building", "Build: How much is this?", "", "",
                        new[] { "これは", "いくら", "ですか。" }, "これはいくらですか。", "いくら asks a price.")
                },
                ["A1: Tell time and make a schedule"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the meeting day and time.",
                        "A：べんきょうはいつですか。\nB：月曜日の七時です。\nA：七時半ですか。\nB：いいえ、七時です。",
                        "A: When is study time?\nB: Monday at seven.\nA: Is it seven thirty?\nB: No, seven.", null, null,
                        "A specific clock time commonly takes に when used with an action."),
                    new ActivitySeed("comprehension", "On which day is the study session?", "月曜日の七時です。", "",
                        new[] { "Monday", "Tuesday", "Today", "Tomorrow" }, "Monday", "月曜日 means Monday."),
                    new ActivitySeed("comprehension", "What time does it begin?", "七時です。", "",
                        new[] { "7:00", "7:30", "9:00", "9:30" }, "7:00", "七時 is seven o'clock; 七時半 would be 7:30."),
                    new ActivitySeed("sentence-building", "Build: I get up at seven.", "", "",
                        new[] { "七時に", "おきます。" }, "七時におきます。", "に marks the specific time of the action.")
                },
                ["A1: Describe your daily routine"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the order of the morning routine.",
                        "毎朝七時におきます。七時半に朝ごはんを食べます。八時に学校に行きます。",
                        "Every morning I get up at seven. I eat breakfast at seven thirty. I go to school at eight.", null, null,
                        "Time usually comes before the object or destination, and the verb closes the sentence."),
                    new ActivitySeed("comprehension", "What happens at 7:30?", "七時半に朝ごはんを食べます。", "",
                        new[] { "The speaker eats breakfast", "The speaker gets up", "The speaker goes to school", "The speaker goes to bed" },
                        "The speaker eats breakfast", "朝ごはんを食べます means eat breakfast."),
                    new ActivitySeed("comprehension", "Where does the speaker go at eight?", "八時に学校に行きます。", "",
                        new[] { "School", "Home", "Work", "A restaurant" }, "School", "学校に行きます means go to school."),
                    new ActivitySeed("sentence-building", "Build: I study Japanese every day.", "", "",
                        new[] { "毎日", "日本語を", "べんきょうします。" }, "毎日日本語をべんきょうします。", "を marks Japanese as what is studied.")
                },
                ["A1: Find people and things"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the locations of the book and the cat.",
                        "A：本はどこですか。\nB：つくえの上にあります。\nA：ねこはどこですか。\nB：いすの下にいます。",
                        "A: Where is the book?\nB: It is on the desk.\nA: Where is the cat?\nB: It is under the chair.", null, null,
                        "あります is used for the book; います is used for the cat."),
                    new ActivitySeed("comprehension", "Where is the book?", "つくえの上にあります。", "",
                        new[] { "On the desk", "Under the desk", "Inside the bag", "Next to the chair" }, "On the desk", "つくえの上 means on or above the desk."),
                    new ActivitySeed("comprehension", "Why does the speaker use います for the cat?", "ねこは下にいます。", "",
                        new[] { "The cat is animate", "The cat is an object", "The cat is above something", "The sentence is negative" },
                        "The cat is animate", "います is the existence verb for people and animals."),
                    new ActivitySeed("sentence-building", "Build: The cat is under the chair.", "", "",
                        new[] { "ねこは", "いすの下に", "います。" }, "ねこはいすの下にいます。", "Location + に precedes the animate existence verb います.")
                },
                ["A1: Describe places and objects"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the contrast between two places.",
                        "A：この町はどうですか。\nB：にぎやかです。でも、あの公園はしずかです。",
                        "A: How is this town?\nB: It is lively. But that park is quiet.", null, null,
                        "な-adjectives take な before a noun, but no な before です."),
                    new ActivitySeed("comprehension", "How is the town described?", "この町はにぎやかです。", "",
                        new[] { "Lively", "Quiet", "Small", "Old" }, "Lively", "にぎやか means lively."),
                    new ActivitySeed("comprehension", "Which phrase correctly means 'a quiet town'?", "", "",
                        new[] { "しずかな町", "しずか町", "しずかい町", "しずかの町" }, "しずかな町", "A な-adjective uses な directly before a noun."),
                    new ActivitySeed("sentence-building", "Build: This bag is inexpensive.", "", "",
                        new[] { "このかばんは", "やすい", "です。" }, "このかばんはやすいです。", "An い-adjective keeps い before です.")
                },
                ["A1: Talk about likes and abilities"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for what each person likes and can do.",
                        "A：スポーツがすきですか。\nB：はい、すきです。サッカーができます。Aさんは？\nA：わたしは音楽がすきです。",
                        "A: Do you like sports?\nB: Yes. I can play soccer. How about you?\nA: I like music.", null, null,
                        "すき and できます commonly mark their focus with が."),
                    new ActivitySeed("comprehension", "What can speaker B do?", "サッカーができます。", "",
                        new[] { "Play soccer", "Cook", "Speak Japanese", "Play music" }, "Play soccer", "サッカーができます states an ability."),
                    new ActivitySeed("comprehension", "What does A like?", "わたしは音楽がすきです。", "",
                        new[] { "Music", "Sports", "Cooking", "English" }, "Music", "音楽がすきです means liking music."),
                    new ActivitySeed("sentence-building", "Build: I understand a little Japanese.", "", "",
                        new[] { "日本語が", "すこし", "わかります。" }, "日本語がすこしわかります。", "The understood thing is marked with が.")
                },
                ["A1: Make requests and ask permission"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the request, permission question and refusal.",
                        "A：すみません、写真をとってもいいですか。\nB：ここではだめです。外でとってください。\nA：わかりました。",
                        "A: Excuse me, may I take a photo?\nB: Not here. Please take it outside.\nA: Understood.", null, null,
                        "て-form + もいいですか asks permission; てください makes a request."),
                    new ActivitySeed("comprehension", "What does the visitor ask permission to do?", "写真をとってもいいですか。", "",
                        new[] { "Take a photo", "Enter", "Use a telephone", "Sit down" }, "Take a photo", "写真をとる means take a photograph."),
                    new ActivitySeed("comprehension", "Where may the visitor take the photo?", "外でとってください。", "",
                        new[] { "Outside", "Here", "Inside", "Nowhere" }, "Outside", "外で marks outside as the action location."),
                    new ActivitySeed("sentence-building", "Build: Please wait a moment.", "", "",
                        new[] { "ちょっと", "まって", "ください。" }, "ちょっとまってください。", "Verb て-form + ください makes a polite request.")
                },
                ["A1: Order food and drinks"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the order, quantities and total.",
                        "店員：ごちゅうもんは？\n客：ラーメンを一つと、おちゃを二つおねがいします。\n店員：いじょうですか。\n客：はい。\n店員：ぜんぶで千五百円です。",
                        "Server: Your order?\nCustomer: One ramen and two teas, please.\nServer: Is that all?\nCustomer: Yes.\nServer: The total is 1,500 yen.", null, null,
                        "General counters follow the item; と joins items in a complete list."),
                    new ActivitySeed("comprehension", "How many teas does the customer order?", "おちゃを二つおねがいします。", "",
                        new[] { "Two", "One", "Three", "Five" }, "Two", "二つ means two items."),
                    new ActivitySeed("comprehension", "What is the total?", "ぜんぶで千五百円です。", "",
                        new[] { "1,500 yen", "1,050 yen", "500 yen", "2,500 yen" }, "1,500 yen", "千五百 is one thousand five hundred."),
                    new ActivitySeed("sentence-building", "Build: One ramen, please.", "", "",
                        new[] { "ラーメンを", "一つ", "おねがいします。" }, "ラーメンを一つおねがいします。", "を marks the ordered item and 一つ gives its quantity.")
                },
                ["A1: Shop for an item"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the requested colour, price and final choice.",
                        "客：このかばん、あかはありますか。\n店員：はい、あります。三千円です。\n客：じゃあ、あかにします。カードでおねがいします。",
                        "Customer: Does this bag come in red?\nClerk: Yes. It is 3,000 yen.\nCustomer: Then I'll take red. By card, please.", null, null,
                        "色 + にします selects an option; カードで marks the payment method."),
                    new ActivitySeed("comprehension", "Which colour does the customer choose?", "あかにします。", "",
                        new[] { "Red", "Blue", "White", "Black" }, "Red", "あか means red and にします marks the choice."),
                    new ActivitySeed("comprehension", "How will the customer pay?", "カードでおねがいします。", "",
                        new[] { "By card", "With cash", "Later", "The dialogue does not say" }, "By card", "カードで identifies the method of payment."),
                    new ActivitySeed("sentence-building", "Build: Please show me that bag.", "", "",
                        new[] { "そのかばんを", "みせて", "ください。" }, "そのかばんをみせてください。", "その modifies a noun near the listener; みせてください is a polite request.")
                },
                ["A1: Travel and ask directions"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the destination, route and transport method.",
                        "旅行者：すみません、駅はどこですか。\n町の人：まっすぐ行って、二つ目の角を右に曲がってください。\n旅行者：ここからバスで行きますか。\n町の人：いいえ、歩いて五分です。",
                        "Traveller: Excuse me, where is the station?\nLocal: Go straight and turn right at the second corner.\nTraveller: Do I go by bus from here?\nLocal: No, it is five minutes on foot.", null, null,
                        "Action sequence uses て-forms; で marks a transport method."),
                    new ActivitySeed("comprehension", "Where should the traveller turn?", "二つ目の角を右に曲がってください。", "",
                        new[] { "Right at the second corner", "Left at the first corner", "Right at the station", "Do not turn" },
                        "Right at the second corner", "二つ目 is second, 角 is corner, and 右 is right."),
                    new ActivitySeed("comprehension", "How far is the station?", "歩いて五分です。", "",
                        new[] { "Five minutes on foot", "Five minutes by bus", "Two minutes on foot", "Fifteen minutes by train" },
                        "Five minutes on foot", "歩いて means on foot and 五分 means five minutes."),
                    new ActivitySeed("sentence-building", "Build: Go straight and turn left.", "", "",
                        new[] { "まっすぐ行って、", "左に", "曲がってください。" }, "まっすぐ行って、左に曲がってください。",
                        "The first て-form connects the actions; に marks the direction of the turn.")
                },
                ["A1: Invite someone and make a plan"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for the invitation, response, day and time.",
                        "A：土曜日、いっしょに映画を見ませんか。\nB：いいですね。何時ですか。\nA：三時はどうですか。\nB：すみません、三時はちょっと…。四時なら大丈夫です。\nA：じゃあ、四時に駅で会いましょう。",
                        "A: Would you like to see a movie together on Saturday?\nB: Sounds good. What time?\nA: How about three?\nB: Sorry, three is a little difficult. Four is okay.\nA: Then let's meet at the station at four.", null, null,
                        "～ませんか invites; ちょっと softens a refusal; ～ましょう proposes a shared action."),
                    new ActivitySeed("comprehension", "Why do they not meet at three?", "三時はちょっと…。四時なら大丈夫です。", "",
                        new[] { "Three is inconvenient for B", "The movie starts at three", "The station closes", "A refuses the invitation" },
                        "Three is inconvenient for B", "ちょっと leaves the refusal tactfully unstated."),
                    new ActivitySeed("comprehension", "Where and when will they meet?", "四時に駅で会いましょう。", "",
                        new[] { "At the station at four", "At the cinema at three", "At school at four", "At the station on Sunday" },
                        "At the station at four", "四時に is the time and 駅で is the meeting place."),
                    new ActivitySeed("sentence-building", "Build: Would you like to eat together?", "", "",
                        new[] { "いっしょに", "食べませんか。" }, "いっしょに食べませんか。", "A polite negative question with ませんか functions as an invitation.")
                },
                ["A1: Talk about a past day"] = new[]
                {
                    new ActivitySeed("dialogue", "Listen for where the speaker went and what happened.",
                        "A：きのう何をしましたか。\nB：友だちと公園に行きました。写真をとりました。\nA：どうでしたか。\nB：とても楽しかったです。でも、レストランには行きませんでした。",
                        "A: What did you do yesterday?\nB: I went to the park with a friend and took photos.\nA: How was it?\nB: It was very enjoyable. But we did not go to a restaurant.", null, null,
                        "ました is polite past; ませんでした is polite negative past; い-adjectives change to ～かったです."),
                    new ActivitySeed("comprehension", "Where did the speaker go?", "友だちと公園に行きました。", "",
                        new[] { "To a park", "To a restaurant", "To school", "To a station" }, "To a park", "公園に行きました means went to a park."),
                    new ActivitySeed("comprehension", "Which action did not happen?", "レストランには行きませんでした。", "",
                        new[] { "Going to a restaurant", "Taking photographs", "Meeting a friend", "Going to a park" },
                        "Going to a restaurant", "行きませんでした is the negative past form."),
                    new ActivitySeed("sentence-building", "Build: Yesterday was enjoyable.", "", "",
                        new[] { "きのうは", "楽しかった", "です。" }, "きのうは楽しかったです。", "楽しい becomes 楽しかった in the polite past description.")
                },
                ["A1: Complete a real-life mission"] = new[]
                {
                    new ActivitySeed("dialogue", "Final mission: listen for identity, plan, route, order and total.",
                        "リー：はじめまして。リーです。マレーシアからきました。\n田中：田中です。よろしくおねがいします。土曜日、いっしょにラーメンを食べませんか。\nリー：いいですね。レストランはどこですか。\n田中：駅のとなりです。四時に駅で会いましょう。\n店員：ごちゅうもんは？\nリー：ラーメンを一つと、おちゃを一つおねがいします。\n店員：ぜんぶで千二百円です。\nリー：すみません、もういちどおねがいします。\n店員：千二百円です。",
                        "Lee: Nice to meet you. I'm Lee, from Malaysia.\nTanaka: I'm Tanaka. Nice to meet you. Would you like ramen together on Saturday?\nLee: Sounds good. Where is the restaurant?\nTanaka: Next to the station. Let's meet at the station at four.\nServer: Your order?\nLee: One ramen and one tea, please.\nServer: 1,200 yen in total.\nLee: Sorry, one more time please.\nServer: 1,200 yen.", null, null,
                        "This mission combines repair language with the major A1 situations in one continuous interaction."),
                    new ActivitySeed("comprehension", "Where and when will Lee and Tanaka meet?", "四時に駅で会いましょう。", "",
                        new[] { "At the station at four", "At the restaurant at noon", "At school at four", "At the station at three" },
                        "At the station at four", "四時に gives the time and 駅で gives the meeting place."),
                    new ActivitySeed("comprehension", "What does Lee order?", "ラーメンを一つと、おちゃを一つおねがいします。", "",
                        new[] { "One ramen and one tea", "Two ramen and one tea", "One ramen and water", "Tea only" },
                        "One ramen and one tea", "Each item is followed by 一つ."),
                    new ActivitySeed("comprehension", "Why does Lee say もういちどおねがいします?", "すみません、もういちどおねがいします。", "",
                        new[] { "To ask for repetition", "To order another ramen", "To refuse the price", "To ask directions" },
                        "To ask for repetition", "もういちどおねがいします repairs communication by asking the speaker to repeat."),
                    new ActivitySeed("sentence-building", "Build: Excuse me, please say it slowly one more time.", "", "",
                        new[] { "すみません、", "もういちど", "ゆっくり", "おねがいします。" },
                        "すみません、もういちどゆっくりおねがいします。", "This combines an attention-getter, repetition request and speed request."),
                    new ActivitySeed("sentence-building", "Build: Let's meet at the station at four.", "", "",
                        new[] { "四時に", "駅で", "会いましょう。" }, "四時に駅で会いましょう。", "に marks time, で marks the meeting place, and ましょう proposes the plan.")
                }
            };

            foreach (var (lessonTitle, seeds) in curriculum)
            {
                var lesson = await context.Lessons.FirstAsync(l => l.Title == lessonTitle);
                for (var i = 0; i < seeds.Length; i++)
                {
                    var seed = seeds[i];
                    var activity = await context.LessonActivities.FirstOrDefaultAsync(a =>
                        a.LessonId == lesson.Id && a.ActivityType == seed.Type && a.DisplayOrder == i);
                    if (activity == null)
                    {
                        activity = new LessonActivity { LessonId = lesson.Id, ActivityType = seed.Type, DisplayOrder = i };
                        context.LessonActivities.Add(activity);
                    }
                    activity.Prompt = seed.Prompt;
                    activity.JapaneseText = seed.Japanese;
                    activity.EnglishText = seed.English;
                    activity.OptionsJson = seed.Options == null ? null : JsonSerializer.Serialize(seed.Options);
                    activity.CorrectAnswer = seed.Answer;
                    activity.Explanation = seed.Explanation;
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
