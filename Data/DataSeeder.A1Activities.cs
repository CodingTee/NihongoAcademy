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
                        "店員：ごちゅうもんは？\n客：ラーメンを一つと、おちゃを二杯おねがいします。\n店員：いじょうですか。\n客：はい。\n店員：ぜんぶで千五百円です。",
                        "Server: Your order?\nCustomer: One ramen and two teas, please.\nServer: Is that all?\nCustomer: Yes.\nServer: The total is 1,500 yen.", null, null,
                        "一つ counts the ramen serving; 二杯 is the drink counter. と joins the ordered items."),
                    new ActivitySeed("comprehension", "How many teas does the customer order?", "おちゃを二杯おねがいします。", "",
                        new[] { "Two", "One", "Three", "Five" }, "Two", "二杯 means two cups or servings of a drink."),
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
                        "リー：はじめまして。リーです。マレーシアからきました。\n田中：田中です。よろしくおねがいします。土曜日、いっしょにラーメンを食べませんか。\nリー：いいですね。レストランはどこですか。\n田中：駅のとなりです。四時に駅で会いましょう。\n店員：ごちゅうもんは？\nリー：ラーメンを一つと、おちゃを一杯おねがいします。\n店員：ぜんぶで千二百円です。\nリー：すみません、もういちどおねがいします。\n店員：千二百円です。",
                        "Lee: Nice to meet you. I'm Lee, from Malaysia.\nTanaka: I'm Tanaka. Nice to meet you. Would you like ramen together on Saturday?\nLee: Sounds good. Where is the restaurant?\nTanaka: Next to the station. Let's meet at the station at four.\nServer: Your order?\nLee: One ramen and one tea, please.\nServer: 1,200 yen in total.\nLee: Sorry, one more time please.\nServer: 1,200 yen.", null, null,
                        "This mission combines repair language with the major A1 situations in one continuous interaction."),
                    new ActivitySeed("comprehension", "Where and when will Lee and Tanaka meet?", "四時に駅で会いましょう。", "",
                        new[] { "At the station at four", "At the restaurant at noon", "At school at four", "At the station at three" },
                        "At the station at four", "四時に gives the time and 駅で gives the meeting place."),
                    new ActivitySeed("comprehension", "What does Lee order?", "ラーメンを一つと、おちゃを一杯おねがいします。", "",
                        new[] { "One ramen and one tea", "Two ramen and one tea", "One ramen and water", "Tea only" },
                        "One ramen and one tea", "一つ counts the ramen serving, while 一杯 counts the tea."),
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

            // The opening unit receives additional contrast, error-analysis and
            // controlled-production work so learners cannot pass by recognising
            // one model dialogue alone.
            var openingUnitExtensions = new Dictionary<string, ActivitySeed[]>
            {
                ["A1: Greetings for the situation"] = new[]
                {
                    new ActivitySeed("comprehension", "A class ends in the afternoon. What is the most suitable closing?", "授業がおわりました。", "Class has ended.",
                        new[] { "しつれいします", "おはようございます", "いただきます", "はじめまして" }, "しつれいします", "しつれいします is a polite way to take leave; the other choices belong to different situations."),
                    new ActivitySeed("comprehension", "Find the situation mismatch.", "夜、先生に会って『おはようございます』と言いました。", "At night, I met my teacher and said good morning.",
                        new[] { "Use こんばんは instead", "Use いただきます instead", "Use はじめまして every time", "The sentence is already appropriate" }, "Use こんばんは instead", "こんばんは matches an evening encounter; greetings change with time and relationship."),
                    new ActivitySeed("sentence-building", "Build a polite evening greeting to your teacher.", "", "",
                        new[] { "先生、", "こんばんは。" }, "先生、こんばんは。", "A title can be used to address the listener before the time-appropriate greeting.")
                },
                ["A1: Introduce yourself"] = new[]
                {
                    new ActivitySeed("comprehension", "Which introduction sounds natural and appropriately polite?", "", "",
                        new[] { "はじめまして。リーです。よろしくおねがいします。", "リー。よろしく。", "はじめましてをリーです。", "リーがマレーシアです。" }, "はじめまして。リーです。よろしくおねがいします。", "A first introduction normally opens with はじめまして, identifies the speaker with です, and closes politely."),
                    new ActivitySeed("comprehension", "What does から express in this introduction?", "マレーシアからきました。", "I came from Malaysia.",
                        new[] { "Point of origin", "Destination", "Possession", "Direct object" }, "Point of origin", "Place + から marks where someone or something comes from."),
                    new ActivitySeed("sentence-building", "Build: I am Mei. I came from China.", "", "",
                        new[] { "メイです。", "中国から", "きました。" }, "メイです。中国からきました。", "Use two short sentences: name + です, then origin + からきました.")
                },
                ["A1: Talk about your family"] = new[]
                {
                    new ActivitySeed("comprehension", "Which word refers to someone else's mother?", "田中さんのお母さんです。", "It is Tanaka's mother.",
                        new[] { "お母さん", "母", "姉", "父" }, "お母さん", "Use お母さん for another person's mother; 母 normally refers to your own mother when speaking outside the family."),
                    new ActivitySeed("comprehension", "Choose the correction: わたし母です。", "わたし母です。", "", new[] { "わたしの母です。", "わたしを母です。", "わたしに母です。", "わたしがの母です。" }, "わたしの母です。", "の links the possessor わたし to the family member 母."),
                    new ActivitySeed("sentence-building", "Build: Tanaka's younger sister is a student.", "", "", new[] { "田中さんの", "妹は", "学生です。" }, "田中さんの妹は学生です。", "の expresses the relationship; は then marks the younger sister as the topic.")
                },
                ["A1: Describe people simply"] = new[]
                {
                    new ActivitySeed("comprehension", "Which phrase correctly comes before 人?", "しずかな人", "a quiet person", new[] { "しずかな", "しずかい", "しずかの", "しずかでの" }, "しずかな", "A な-adjective uses な directly before a noun."),
                    new ActivitySeed("comprehension", "Choose the natural negative description.", "兄は学生ではありません。", "My older brother is not a student.", new[] { "兄は学生ではありません。", "兄は学生ないです。", "兄を学生ではありません。", "兄はでは学生ありません。" }, "兄は学生ではありません。", "Noun + ではありません is the polite negative pattern."),
                    new ActivitySeed("sentence-building", "Build: My friend is quiet and kind.", "", "", new[] { "友だちは", "しずかで", "やさしいです。" }, "友だちはしずかでやさしいです。", "The て-form-like connector で joins a な-adjective to another description.")
                },
                ["A1: Identify everyday things"] = new[]
                {
                    new ActivitySeed("comprehension", "The object is beside the listener. Which demonstrative fits?", "___は何ですか。", "What is that?", new[] { "それ", "これ", "あれ", "どれ" }, "それ", "それ points to something near the listener."),
                    new ActivitySeed("comprehension", "Why can の stand alone here?", "これは田中さんのです。", "This is Tanaka's.", new[] { "The known object is omitted", "の means a location", "の makes the sentence negative", "の marks a question" }, "The known object is omitted", "When the object is already clear, noun + のです can mean 'the one belonging to ...'."),
                    new ActivitySeed("sentence-building", "Build: Which bag is yours?", "", "", new[] { "あなたのかばんは", "どれ", "ですか。" }, "あなたのかばんはどれですか。", "どれ stands alone to ask which one; は marks the known category, your bag.")
                },
                ["A1: Use numbers, ages and prices"] = new[]
                {
                    new ActivitySeed("comprehension", "Which reading is used for a person's age of twenty?", "二十歳", "twenty years old", new[] { "はたち", "にじゅうさい", "ふたつ", "にじゅうねん" }, "はたち", "Twenty years old has the special reading はたち."),
                    new ActivitySeed("comprehension", "The price is 3,480 yen. Which is correct?", "三千四百八十円", "3,480 yen", new[] { "さんぜんよんひゃくはちじゅうえん", "さんせんよんびゃくはちじゅうえん", "さんぜんよんひゃくはってんえん", "さんひゃくよんじゅうはちえん" }, "さんぜんよんひゃくはちじゅうえん", "三千 becomes さんぜん; 四百 is よんひゃく; 八十 is はちじゅう."),
                    new ActivitySeed("sentence-building", "Build: I am twenty-two years old.", "", "", new[] { "わたしは", "二十二歳", "です。" }, "わたしは二十二歳です。", "Put the age before です; 歳 is the age counter.")
                },
                ["A1: Tell time and make a schedule"] = new[]
                {
                    new ActivitySeed("comprehension", "Choose the correct time for 8:30.", "八時半", "8:30", new[] { "はちじはん", "はちじさんじゅっぷん", "はんはちじ", "はちふんはん" }, "はちじはん", "半 follows the hour and means half past."),
                    new ActivitySeed("comprehension", "Which sentence needs the time particle に?", "", "", new[] { "七時におきます。", "きょうにべんきょうします。", "毎日に行きます。", "いつに帰りますか。" }, "七時におきます。", "A specific clock time normally takes に; relative and interrogative time words such as きょう and いつ normally do not."),
                    new ActivitySeed("sentence-building", "Build: The class is on Tuesday from nine until eleven.", "", "", new[] { "授業は", "火曜日の九時から", "十一時までです。" }, "授業は火曜日の九時から十一時までです。", "から marks the starting time and まで marks the ending time.")
                },
                ["A1: Describe your daily routine"] = new[]
                {
                    new ActivitySeed("comprehension", "Which sentence correctly describes a repeated routine?", "", "", new[] { "毎朝コーヒーを飲みます。", "毎朝コーヒーが飲みです。", "毎朝にコーヒーをです。", "コーヒー毎朝飲むます。" }, "毎朝コーヒーを飲みます。", "Frequency comes before the object; を marks the object and the polite verb closes the sentence."),
                    new ActivitySeed("comprehension", "Find the correction: 八時に学校を行きます。", "八時に学校を行きます。", "", new[] { "八時に学校に行きます。", "八時を学校に行きます。", "八時で学校を行きます。", "八時に学校で行きます。" }, "八時に学校に行きます。", "A destination takes に (or へ), not を, with 行きます."),
                    new ActivitySeed("sentence-building", "Build: I do not study on Sunday.", "", "", new[] { "日曜日は", "べんきょう", "しません。" }, "日曜日はべんきょうしません。", "しません is the polite negative form of します.")
                },
                ["A1: Find people and things"] = new[]
                {
                    new ActivitySeed("comprehension", "Which verb completes the sentence about a teacher?", "教室に先生が___。", "There is a teacher in the classroom.", new[] { "います", "あります", "ですあります", "いますかです" }, "います", "Use います for people and animals."),
                    new ActivitySeed("comprehension", "Choose the natural correction: つくえの上で本があります。", "つくえの上で本があります。", "", new[] { "つくえの上に本があります。", "つくえの上を本があります。", "つくえの上が本にあります。", "つくえに上で本です。" }, "つくえの上に本があります。", "The location of existence is marked with に, not で."),
                    new ActivitySeed("sentence-building", "Build: There is a convenience store next to the station.", "", "", new[] { "駅のとなりに", "コンビニが", "あります。" }, "駅のとなりにコンビニがあります。", "Location + に, subject + が, and あります form an existence sentence for a place or object.")
                },
                ["A1: Describe places and objects"] = new[]
                {
                    new ActivitySeed("comprehension", "Choose the polite negative of 高いです.", "この店は高くないです。", "This shop is not expensive.", new[] { "高くないです", "高いではありません", "高くありませんです", "高いないです" }, "高くないです", "For an い-adjective, replace final い with くないです to make the polite negative."),
                    new ActivitySeed("comprehension", "Which form correctly joins two descriptions?", "この町はしずかで、きれいです。", "This town is quiet and beautiful.", new[] { "しずかで", "しずかなで", "しずかくて", "しずかの" }, "しずかで", "A な-adjective uses で when connecting it to another description."),
                    new ActivitySeed("sentence-building", "Build: This restaurant is inexpensive and delicious.", "", "", new[] { "このレストランは", "安くて", "おいしいです。" }, "このレストランは安くておいしいです。", "An い-adjective changes final い to くて when joining another adjective.")
                },
                ["A1: Talk about likes and abilities"] = new[]
                {
                    new ActivitySeed("comprehension", "Choose the correct particle: 日本語___少しわかります。", "日本語が少しわかります。", "I understand a little Japanese.", new[] { "が", "を", "に", "で" }, "が", "The thing understood commonly takes が with わかります."),
                    new ActivitySeed("comprehension", "Which sentence distinguishes liking from ability?", "音楽が好きですが、歌は上手ではありません。", "I like music, but I am not good at singing.", new[] { "Liking does not automatically mean skill", "The speaker dislikes music", "The speaker sings professionally", "The sentence describes possession" }, "Liking does not automatically mean skill", "好き expresses preference, while 上手 describes skill; they are different claims."),
                    new ActivitySeed("sentence-building", "Build: I can cook, but I cannot swim.", "", "", new[] { "料理ができますが、", "泳ぐことは", "できません。" }, "料理ができますが、泳ぐことはできません。", "できます states ability; は can contrast the activity that the speaker cannot do.")
                },
                ["A1: Make requests and ask permission"] = new[]
                {
                    new ActivitySeed("comprehension", "Which expression is a request rather than a permission question?", "", "", new[] { "ここに書いてください。", "ここに書いてもいいですか。", "ここに書いてはいけません。", "ここに書きますか。" }, "ここに書いてください。", "てください asks someone to do an action; てもいいですか asks whether the speaker may do it."),
                    new ActivitySeed("comprehension", "What does this sign prohibit?", "ここで写真をとってはいけません。", "", new[] { "Taking photographs here", "Entering here", "Speaking here", "Waiting here" }, "Taking photographs here", "てはいけません expresses that an action is not permitted."),
                    new ActivitySeed("sentence-building", "Build: May I use this dictionary?", "", "", new[] { "この辞書を", "使っても", "いいですか。" }, "この辞書を使ってもいいですか。", "Verb て-form + もいいですか asks permission politely.")
                },
                ["A1: Order food and drinks"] = new[]
                {
                    new ActivitySeed("comprehension", "Which order is both complete and polite?", "", "", new[] { "カレーを一つお願いします。", "カレーが一ついます。", "カレーに一つくださいか。", "一つをカレーです。" }, "カレーを一つお願いします。", "Item + を + quantity + お願いします is a dependable polite ordering pattern."),
                    new ActivitySeed("comprehension", "What does 以上です mean after an order?", "以上です。", "", new[] { "That is all", "It is expensive", "One more, please", "Please repeat" }, "That is all", "以上です signals that the list or order is complete."),
                    new ActivitySeed("sentence-building", "Build: Two coffees and one cake, please.", "", "", new[] { "コーヒーを二杯と、", "ケーキを一つ", "お願いします。" }, "コーヒーを二杯と、ケーキを一つお願いします。", "杯 counts servings of drinks, while 一つ counts the cake; と joins the ordered items.")
                },
                ["A1: Shop for an item"] = new[]
                {
                    new ActivitySeed("comprehension", "Which phrase asks whether a blue version exists?", "", "", new[] { "青はありますか。", "青をいますか。", "青でいくらですか。", "青にしますかです。" }, "青はありますか。", "Colour + はありますか asks whether that option is available."),
                    new ActivitySeed("comprehension", "What decision does the customer make?", "じゃあ、黒にします。", "", new[] { "The customer chooses black", "The customer rejects black", "The customer asks the price", "The customer asks for a larger size" }, "The customer chooses black", "Noun + にします states a choice among available options."),
                    new ActivitySeed("sentence-building", "Build: Do you have this in a larger size?", "", "", new[] { "これより大きいサイズは", "ありますか。" }, "これより大きいサイズはありますか。", "これより means larger than this one; は marks the requested size as the option under discussion.")
                },
                ["A1: Travel and ask directions"] = new[]
                {
                    new ActivitySeed("comprehension", "Which particle marks the means of transport?", "電車で行きます。", "I go by train.", new[] { "で", "に", "を", "が" }, "で", "Transport + で marks the means used to travel."),
                    new ActivitySeed("comprehension", "Follow the direction: まっすぐ行って、右に曲がってください。", "", "", new[] { "Go straight, then turn right", "Turn left immediately", "Take the train", "Return to the station" }, "Go straight, then turn right", "まっすぐ行って means go straight; 右に曲がる means turn right."),
                    new ActivitySeed("sentence-building", "Build: How do I get from the station to the museum?", "", "", new[] { "駅から美術館まで", "どうやって", "行きますか。" }, "駅から美術館までどうやって行きますか。", "から and まで frame the route; どうやって asks by what means or route.")
                },
                ["A1: Invite someone and make a plan"] = new[]
                {
                    new ActivitySeed("comprehension", "Which expression makes a gentle invitation?", "", "", new[] { "いっしょに映画を見ませんか。", "いっしょに映画を見ません。", "映画を見てはいけません。", "映画がありませんか。" }, "いっしょに映画を見ませんか。", "The negative question form ませんか is commonly used as a considerate invitation."),
                    new ActivitySeed("comprehension", "Which response declines politely while giving a reason?", "", "", new[] { "すみません、土曜日はちょっと…。", "いいえ、見ません。", "だめです。", "映画ではありません。" }, "すみません、土曜日はちょっと…。", "ちょっと with a trailing pause is a conventional soft refusal; すみません makes it more considerate."),
                    new ActivitySeed("sentence-building", "Build: Sunday is fine. Shall we meet at two?", "", "", new[] { "日曜日は大丈夫です。", "二時に", "会いましょうか。" }, "日曜日は大丈夫です。二時に会いましょうか。", "大丈夫です accepts the day, and ましょうか proposes the meeting arrangement.")
                },
                ["A1: Talk about a past day"] = new[]
                {
                    new ActivitySeed("comprehension", "Choose the polite negative past of 行きます.", "", "", new[] { "行きませんでした", "行きませんです", "行かなかったます", "行きではありません" }, "行きませんでした", "Replace ます with ませんでした to form the polite negative past."),
                    new ActivitySeed("comprehension", "Which form correctly describes something that was not enjoyable?", "楽しくなかったです。", "It was not enjoyable.", new[] { "楽しくなかったです", "楽しいではありませんでした", "楽しくないでした", "楽しかったくないです" }, "楽しくなかったです", "For an い-adjective negative past, replace final い with くなかったです."),
                    new ActivitySeed("sentence-building", "Build: Yesterday I studied, but I did not watch television.", "", "", new[] { "きのう勉強しましたが、", "テレビは", "見ませんでした。" }, "きのう勉強しましたが、テレビは見ませんでした。", "ました reports the completed action; ませんでした gives the contrasting action that did not happen.")
                },
                ["A1: Complete a real-life mission"] = new[]
                {
                    new ActivitySeed("comprehension", "Which response best repairs communication when a price is too fast to catch?", "", "", new[] { "すみません、もう一度ゆっくりお願いします。", "これはいくらでした。", "千二百円にします。", "わかりませんでしたか。" }, "すみません、もう一度ゆっくりお願いします。", "A useful real-life repair combines an apology, a repetition request and a request for slower speech.")
                }
            };

            foreach (var (title, additions) in openingUnitExtensions)
                curriculum[title] = curriculum[title].Concat(additions).ToArray();

            var corrections = new Dictionary<string, (string Wrong, string Correct, string Explanation)>
            {
                ["A1: Greetings for the situation"] = ("夜、先生に『おはようございます』と言います。", "夜、先生に『こんばんは』と言います。", "Use こんばんは for an evening encounter."),
                ["A1: Introduce yourself"] = ("わたしをリーです。", "わたしはリーです。", "は marks わたし as the topic; を cannot identify the speaker."),
                ["A1: Talk about your family"] = ("これはわたし母です。", "これはわたしの母です。", "の is required between the possessor and the family member."),
                ["A1: Describe people simply"] = ("マリアさんはしずかい人です。", "マリアさんはしずかな人です。", "しずか is a な-adjective and takes な before a noun."),
                ["A1: Identify everyday things"] = ("このは何ですか。", "これは何ですか。", "これ stands alone; この must modify a following noun."),
                ["A1: Use numbers, ages and prices"] = ("わたしは二十歳があります。", "わたしは二十歳です。", "Age is identified with です, not the existence verb あります."),
                ["A1: Tell time and make a schedule"] = ("七時をおきます。", "七時におきます。", "A specific action time is marked with に."),
                ["A1: Describe your daily routine"] = ("学校を行きます。", "学校に行きます。", "The destination of 行きます takes に or へ."),
                ["A1: Find people and things"] = ("教室に先生があります。", "教室に先生がいます。", "Use います for a person."),
                ["A1: Describe places and objects"] = ("この町はしずかくてきれいです。", "この町はしずかできれいです。", "A な-adjective connects with で; くて is used with い-adjectives."),
                ["A1: Talk about likes and abilities"] = ("音楽を好きです。", "音楽が好きです。", "The liked item commonly takes が with 好きです."),
                ["A1: Make requests and ask permission"] = ("写真をとるてもいいですか。", "写真をとってもいいですか。", "Attach もいいですか to the verb's て-form."),
                ["A1: Order food and drinks"] = ("ラーメンが一つお願いします。", "ラーメンを一つお願いします。", "The ordered item is marked with を."),
                ["A1: Shop for an item"] = ("赤をありますか。", "赤はありますか。", "Use は to present the colour option whose availability is being checked."),
                ["A1: Travel and ask directions"] = ("電車に行きます。", "電車で行きます。", "The means of transport is marked with で."),
                ["A1: Invite someone and make a plan"] = ("いっしょに映画を見ますか。", "いっしょに映画を見ませんか。", "ませんか makes the suggestion softer and functions as an invitation."),
                ["A1: Talk about a past day"] = ("きのう公園に行きます。", "きのう公園に行きました。", "A completed action yesterday requires the polite past form ました."),
                ["A1: Complete a real-life mission"] = ("すみません、もう一つゆっくりお願いします。", "すみません、もう一度ゆっくりお願いします。", "もう一度 means once more; もう一つ means one more item.")
            };

            var readingSupport = new Dictionary<(string Lesson, int Order), string>
            {
                [("A1: Greetings for the situation", 0)] = "せんせい：おはようございます。\nりー：おはようございます、せんせい。",
                [("A1: Greetings for the situation", 1)] = "せんせい：おはようございます。\nりー：おはようございます。",
                [("A1: Greetings for the situation", 4)] = "じゅぎょうが おわりました。",
                [("A1: Greetings for the situation", 5)] = "よる、せんせいに あって『おはようございます』と いいました。",
                [("A1: Greetings for the situation", 7)] = "よる、せんせいに『おはようございます』と いいます。→ よる、せんせいに『こんばんは』と いいます。",
                [("A1: Introduce yourself", 0)] = "りー：はじめまして。りーです。まれーしあから きました。がくせいです。\nたなか：はじめまして。たなかです。よろしくおねがいします。",
                [("A1: Introduce yourself", 1)] = "りーです。まれーしあから きました。",
                [("A1: Introduce yourself", 2)] = "りーです。がくせいです。",
                [("A1: Introduce yourself", 5)] = "まれーしあから きました。",
                [("A1: Introduce yourself", 7)] = "わたしを りーです。→ わたしは りーです。",
                [("A1: Talk about your family", 0)] = "A：このひとは だれですか。\nB：わたしの ははです。こちらは あにです。",
                [("A1: Talk about your family", 1)] = "わたしの ははです。",
                [("A1: Talk about your family", 4)] = "たなかさんの おかあさんです。",
                [("A1: Talk about your family", 5)] = "わたし ははです。→ わたしの ははです。",
                [("A1: Talk about your family", 6)] = "たなかさんの いもうとは がくせいです。",
                [("A1: Talk about your family", 7)] = "これは わたし ははです。→ これは わたしの ははです。",
                [("A1: Describe people simply", 0)] = "これは まりあさんです。がくせいです。やさしいです。",
                [("A1: Describe people simply", 1)] = "まりあさんは がくせいです。やさしいです。",
                [("A1: Describe people simply", 4)] = "しずかな ひと",
                [("A1: Describe people simply", 5)] = "あには がくせいでは ありません。",
                [("A1: Describe people simply", 6)] = "ともだちは しずかで やさしいです。",
                [("A1: Describe people simply", 7)] = "まりあさんは しずかい ひとです。→ まりあさんは しずかな ひとです。"
                ,[("A1: Identify everyday things", 0)] = "A：これは なんですか。\nB：それは かさです。\nA：だれの かさですか。\nB：たなかさんのです。"
                ,[("A1: Identify everyday things", 2)] = "たなかさんのです。"
                ,[("A1: Identify everyday things", 4)] = "それは なんですか。"
                ,[("A1: Identify everyday things", 5)] = "これは たなかさんのです。"
                ,[("A1: Identify everyday things", 7)] = "このは なんですか。→ これは なんですか。"
                ,[("A1: Use numbers, ages and prices", 0)] = "きゃく：これは いくらですか。\nてんいん：せんにひゃくえんです。\nきゃく：ふたつ ください。"
                ,[("A1: Use numbers, ages and prices", 1)] = "せんにひゃくえんです。"
                ,[("A1: Use numbers, ages and prices", 2)] = "ふたつ ください。"
                ,[("A1: Use numbers, ages and prices", 4)] = "はたち"
                ,[("A1: Use numbers, ages and prices", 5)] = "さんぜんよんひゃくはちじゅうえん"
                ,[("A1: Use numbers, ages and prices", 6)] = "わたしは にじゅうにさいです。"
                ,[("A1: Use numbers, ages and prices", 7)] = "わたしは はたちが あります。→ わたしは はたちです。"
                ,[("A1: Tell time and make a schedule", 0)] = "A：べんきょうは いつですか。\nB：げつようびの しちじです。\nA：しちじはんですか。\nB：いいえ、しちじです。"
                ,[("A1: Tell time and make a schedule", 1)] = "げつようびの しちじです。"
                ,[("A1: Tell time and make a schedule", 2)] = "しちじです。"
                ,[("A1: Tell time and make a schedule", 4)] = "はちじはん"
                ,[("A1: Tell time and make a schedule", 5)] = "しちじに おきます。"
                ,[("A1: Tell time and make a schedule", 6)] = "じゅぎょうは かようびの くじから じゅういちじまでです。"
                ,[("A1: Tell time and make a schedule", 7)] = "しちじを おきます。→ しちじに おきます。"
                ,[("A1: Describe your daily routine", 0)] = "まいあさ しちじに おきます。しちじはんに あさごはんを たべます。はちじに がっこうに いきます。"
                ,[("A1: Describe your daily routine", 1)] = "しちじはんに あさごはんを たべます。"
                ,[("A1: Describe your daily routine", 2)] = "はちじに がっこうに いきます。"
                ,[("A1: Describe your daily routine", 3)] = "まいにち にほんごを べんきょうします。"
                ,[("A1: Describe your daily routine", 4)] = "まいあさ こーひーを のみます。"
                ,[("A1: Describe your daily routine", 5)] = "はちじに がっこうに いきます。"
                ,[("A1: Describe your daily routine", 6)] = "にちようびは べんきょうしません。"
                ,[("A1: Describe your daily routine", 7)] = "がっこうを いきます。→ がっこうに いきます。"
                ,[("A1: Find people and things", 0)] = "A：ほんは どこですか。\nB：つくえの うえに あります。\nA：ねこは どこですか。\nB：いすの したに います。"
                ,[("A1: Find people and things", 1)] = "つくえの うえに あります。"
                ,[("A1: Find people and things", 2)] = "ねこは したに います。"
                ,[("A1: Find people and things", 4)] = "きょうしつに せんせいが います。"
                ,[("A1: Find people and things", 5)] = "つくえの うえに ほんが あります。"
                ,[("A1: Find people and things", 6)] = "えきの となりに こんびにが あります。"
                ,[("A1: Find people and things", 7)] = "きょうしつに せんせいが あります。→ きょうしつに せんせいが います。"
                ,[("A1: Describe places and objects", 0)] = "A：この まちは どうですか。\nB：にぎやかです。でも、あの こうえんは しずかです。"
                ,[("A1: Describe places and objects", 1)] = "この まちは にぎやかです。"
                ,[("A1: Describe places and objects", 2)] = "しずかな まち"
                ,[("A1: Describe places and objects", 4)] = "この みせは たかくないです。"
                ,[("A1: Describe places and objects", 5)] = "この まちは しずかで、きれいです。"
                ,[("A1: Describe places and objects", 6)] = "この れすとらんは やすくて おいしいです。"
                ,[("A1: Describe places and objects", 7)] = "この まちは しずかくて きれいです。→ この まちは しずかで きれいです。"
                ,[("A1: Talk about likes and abilities", 0)] = "A：すぽーつが すきですか。\nB：はい、すきです。さっかーが できます。Aさんは？\nA：わたしは おんがくが すきです。"
                ,[("A1: Talk about likes and abilities", 2)] = "わたしは おんがくが すきです。"
                ,[("A1: Talk about likes and abilities", 3)] = "にほんごが すこし わかります。"
                ,[("A1: Talk about likes and abilities", 4)] = "にほんごが すこし わかります。"
                ,[("A1: Talk about likes and abilities", 5)] = "おんがくが すきですが、うたは じょうずでは ありません。"
                ,[("A1: Talk about likes and abilities", 6)] = "りょうりが できますが、およぐことは できません。"
                ,[("A1: Talk about likes and abilities", 7)] = "おんがくを すきです。→ おんがくが すきです。"
                ,[("A1: Make requests and ask permission", 0)] = "A：すみません、しゃしんを とっても いいですか。\nB：ここでは だめです。そとで とってください。\nA：わかりました。"
                ,[("A1: Make requests and ask permission", 1)] = "しゃしんを とっても いいですか。"
                ,[("A1: Make requests and ask permission", 2)] = "そとで とってください。"
                ,[("A1: Make requests and ask permission", 4)] = "ここに かいてください。"
                ,[("A1: Make requests and ask permission", 5)] = "ここで しゃしんを とっては いけません。"
                ,[("A1: Make requests and ask permission", 6)] = "この じしょを つかっても いいですか。"
                ,[("A1: Make requests and ask permission", 7)] = "しゃしんを とるても いいですか。→ しゃしんを とっても いいですか。"
                ,[("A1: Order food and drinks", 0)] = "てんいん：ごちゅうもんは？\nきゃく：らーめんを ひとつと、おちゃを にはい おねがいします。\nてんいん：いじょうですか。\nきゃく：はい。\nてんいん：ぜんぶで せんごひゃくえんです。"
                ,[("A1: Order food and drinks", 1)] = "おちゃを にはい おねがいします。"
                ,[("A1: Order food and drinks", 2)] = "ぜんぶで せんごひゃくえんです。"
                ,[("A1: Order food and drinks", 3)] = "らーめんを ひとつ おねがいします。"
                ,[("A1: Order food and drinks", 4)] = "かれーを ひとつ おねがいします。"
                ,[("A1: Order food and drinks", 5)] = "いじょうです。"
                ,[("A1: Order food and drinks", 6)] = "こーひーを にはいと、けーきを ひとつ おねがいします。"
                ,[("A1: Order food and drinks", 7)] = "らーめんが ひとつ おねがいします。→ らーめんを ひとつ おねがいします。"
                ,[("A1: Shop for an item", 0)] = "きゃく：この かばん、あかは ありますか。\nてんいん：はい、あります。さんぜんえんです。\nきゃく：じゃあ、あかに します。かーどで おねがいします。"
                ,[("A1: Shop for an item", 4)] = "あおは ありますか。"
                ,[("A1: Shop for an item", 5)] = "じゃあ、くろに します。"
                ,[("A1: Shop for an item", 6)] = "これは もっと おおきい さいずが ありますか。"
                ,[("A1: Shop for an item", 7)] = "あかを ありますか。→ あかは ありますか。"
                ,[("A1: Travel and ask directions", 0)] = "りょこうしゃ：すみません、えきは どこですか。\nまちの ひと：まっすぐ いって、ふたつめの かどを みぎに まがってください。\nりょこうしゃ：ここから ばすで いきますか。\nまちの ひと：いいえ、あるいて ごふんです。"
                ,[("A1: Travel and ask directions", 1)] = "ふたつめの かどを みぎに まがってください。"
                ,[("A1: Travel and ask directions", 2)] = "あるいて ごふんです。"
                ,[("A1: Travel and ask directions", 3)] = "まっすぐ いって、ひだりに まがってください。"
                ,[("A1: Travel and ask directions", 4)] = "でんしゃで いきます。"
                ,[("A1: Travel and ask directions", 5)] = "まっすぐ いって、みぎに まがってください。"
                ,[("A1: Travel and ask directions", 6)] = "えきから びじゅつかんまで どうやって いきますか。"
                ,[("A1: Travel and ask directions", 7)] = "でんしゃに いきます。→ でんしゃで いきます。"
                ,[("A1: Invite someone and make a plan", 0)] = "A：どようび、いっしょに えいがを みませんか。\nB：いいですね。なんじですか。\nA：さんじは どうですか。\nB：すみません、さんじは ちょっと…。よじなら だいじょうぶです。\nA：じゃあ、よじに えきで あいましょう。"
                ,[("A1: Invite someone and make a plan", 1)] = "さんじは ちょっと…。よじなら だいじょうぶです。"
                ,[("A1: Invite someone and make a plan", 2)] = "よじに えきで あいましょう。"
                ,[("A1: Invite someone and make a plan", 4)] = "いっしょに えいがを みませんか。"
                ,[("A1: Invite someone and make a plan", 5)] = "すみません、どようびは ちょっと…。"
                ,[("A1: Invite someone and make a plan", 6)] = "にちようびは だいじょうぶです。にじに あいましょうか。"
                ,[("A1: Invite someone and make a plan", 7)] = "いっしょに えいがを みますか。→ いっしょに えいがを みませんか。"
                ,[("A1: Talk about a past day", 0)] = "A：きのう なにを しましたか。\nB：ともだちと こうえんに いきました。しゃしんを とりました。\nA：どうでしたか。\nB：とても たのしかったです。でも、れすとらんには いきませんでした。"
                ,[("A1: Talk about a past day", 1)] = "ともだちと こうえんに いきました。"
                ,[("A1: Talk about a past day", 2)] = "れすとらんには いきませんでした。"
                ,[("A1: Talk about a past day", 3)] = "きのうは たのしかったです。"
                ,[("A1: Talk about a past day", 4)] = "いきませんでした"
                ,[("A1: Talk about a past day", 5)] = "たのしく なかったです。"
                ,[("A1: Talk about a past day", 6)] = "きのう べんきょうしましたが、てれびは みませんでした。"
                ,[("A1: Talk about a past day", 7)] = "きのう こうえんに いきます。→ きのう こうえんに いきました。"
                ,[("A1: Complete a real-life mission", 0)] = "りー：はじめまして。りーです。まれーしあから きました。\nたなか：たなかです。よろしく おねがいします。どようび、いっしょに らーめんを たべませんか。\nりー：いいですね。れすとらんは どこですか。\nたなか：えきの となりです。よじに えきで あいましょう。\nてんいん：ごちゅうもんは？\nりー：らーめんを ひとつと、おちゃを いっぱい おねがいします。\nてんいん：ぜんぶで せんにひゃくえんです。\nりー：すみません、もう いちど おねがいします。\nてんいん：せんにひゃくえんです。"
                ,[("A1: Complete a real-life mission", 1)] = "よじに えきで あいましょう。"
                ,[("A1: Complete a real-life mission", 2)] = "らーめんを ひとつと、おちゃを いっぱい おねがいします。"
                ,[("A1: Complete a real-life mission", 4)] = "すみません、もう いちど ゆっくり おねがいします。"
                ,[("A1: Complete a real-life mission", 5)] = "よじに えきで あいましょう。"
                ,[("A1: Complete a real-life mission", 6)] = "すみません、もう いちど ゆっくり おねがいします。"
                ,[("A1: Complete a real-life mission", 7)] = "すみません、もう ひとつ ゆっくり おねがいします。→ すみません、もう いちど ゆっくり おねがいします。"
            };

            foreach (var (title, correction) in corrections)
            {
                curriculum[title] = curriculum[title].Append(new ActivitySeed(
                    "correction", "Rewrite the sentence correctly.", correction.Wrong, "",
                    null, correction.Correct, correction.Explanation)).ToArray();
            }

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
                    activity.ReadingSupportText = readingSupport.GetValueOrDefault((lessonTitle, i));
                    activity.EnglishText = seed.English;
                    activity.OptionsJson = seed.Options == null ? null : JsonSerializer.Serialize(seed.Options);
                    activity.CorrectAnswer = seed.Answer;
                    activity.Explanation = seed.Explanation;
                }

                var validOrders = Enumerable.Range(0, seeds.Length).ToHashSet();
                var staleActivities = await context.LessonActivities
                    .Where(a => a.LessonId == lesson.Id && !validOrders.Contains(a.DisplayOrder))
                    .ToListAsync();
                context.LessonActivities.RemoveRange(staleActivities);
            }
            await context.SaveChangesAsync();
        }
    }
}
