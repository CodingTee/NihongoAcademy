/* ===== NihongoAcademy Highlight Translation Helper — Multi-stage ===== */
(function () {
    var popup = null;
    var hideTimer = null;
    var translationCache = {}; // session cache

    // ========== DICTIONARY: English ↔ Japanese ==========
    var dict = {
        "hello": { jp: "こんにちは", reading: "konnichiwa", romaji: "konnichiwa" },
        "good morning": { jp: "おはようございます", reading: "ohayou gozaimasu", romaji: "ohayou gozaimasu" },
        "good evening": { jp: "こんばんは", reading: "konbanwa", romaji: "konbanwa" },
        "goodbye": { jp: "さようなら", reading: "sayounara", romaji: "sayounara" },
        "thank you": { jp: "ありがとう", reading: "arigatou", romaji: "arigatou" },
        "thanks": { jp: "ありがとう", reading: "arigatou", romaji: "arigatou" },
        "sorry": { jp: "すみません", reading: "sumimasen", romaji: "sumimasen" },
        "please": { jp: "お願いします", reading: "onegaishimasu", romaji: "onegaishimasu" },
        "yes": { jp: "はい", reading: "hai", romaji: "hai" },
        "no": { jp: "いいえ", reading: "iie", romaji: "iie" },
        "water": { jp: "水", reading: "みず", romaji: "mizu" },
        "fire": { jp: "火", reading: "ひ", romaji: "hi" },
        "food": { jp: "食べ物", reading: "たべもの", romaji: "tabemono" },
        "rice": { jp: "ご飯", reading: "ごはん", romaji: "gohan" },
        "tea": { jp: "お茶", reading: "おちゃ", romaji: "ocha" },
        "school": { jp: "学校", reading: "がっこう", romaji: "gakkou" },
        "teacher": { jp: "先生", reading: "せんせい", romaji: "sensei" },
        "student": { jp: "学生", reading: "がくせい", romaji: "gakusei" },
        "book": { jp: "本", reading: "ほん", romaji: "hon" },
        "books": { jp: "本", reading: "ほん", romaji: "hon" },
        "lesson": { jp: "レッスン", reading: "れっすん", romaji: "ressun" },
        "study": { jp: "勉強", reading: "べんきょう", romaji: "benkyou" },
        "studying": { jp: "勉強", reading: "べんきょう", romaji: "benkyou" },
        "studied": { jp: "勉強した", reading: "べんきょうした", romaji: "benkyou shita" },
        "studies": { jp: "勉強", reading: "べんきょう", romaji: "benkyou" },
        "learn": { jp: "学ぶ", reading: "まなぶ", romaji: "manabu" },
        "learning": { jp: "学ぶ", reading: "まなぶ", romaji: "manabu" },
        "read": { jp: "読む", reading: "よむ", romaji: "yomu" },
        "reading": { jp: "読む", reading: "よむ", romaji: "yomu" },
        "write": { jp: "書く", reading: "かく", romaji: "kaku" },
        "writing": { jp: "書く", reading: "かく", romaji: "kaku" },
        "speak": { jp: "話す", reading: "はなす", romaji: "hanasu" },
        "speaking": { jp: "話す", reading: "はなす", romaji: "hanasu" },
        "listen": { jp: "聞く", reading: "きく", romaji: "kiku" },
        "listening": { jp: "聞く", reading: "きく", romaji: "kiku" },
        "name": { jp: "名前", reading: "なまえ", romaji: "namae" },
        "friend": { jp: "友達", reading: "ともだち", romaji: "tomodachi" },
        "friends": { jp: "友達", reading: "ともだち", romaji: "tomodachi" },
        "family": { jp: "家族", reading: "かぞく", romaji: "kazoku" },
        "house": { jp: "家", reading: "いえ", romaji: "ie" },
        "home": { jp: "家", reading: "いえ", romaji: "ie" },
        "room": { jp: "部屋", reading: "へや", romaji: "heya" },
        "door": { jp: "ドア", reading: "どあ", romaji: "doa" },
        "window": { jp: "窓", reading: "まど", romaji: "mado" },
        "car": { jp: "車", reading: "くるま", romaji: "kuruma" },
        "train": { jp: "電車", reading: "でんしゃ", romaji: "densha" },
        "station": { jp: "駅", reading: "えき", romaji: "eki" },
        "shop": { jp: "店", reading: "みせ", romaji: "mise" },
        "restaurant": { jp: "レストラン", reading: "れすとらん", romaji: "resutoran" },
        "hospital": { jp: "病院", reading: "びょういん", romaji: "byouin" },
        "bank": { jp: "銀行", reading: "ぎんこう", romaji: "ginkou" },
        "library": { jp: "図書館", reading: "としょかん", romaji: "toshokan" },
        "weather": { jp: "天気", reading: "てんき", romaji: "tenki" },
        "rain": { jp: "雨", reading: "あめ", romaji: "ame" },
        "snow": { jp: "雪", reading: "ゆき", romaji: "yuki" },
        "hot": { jp: "暑い", reading: "あつい", romaji: "atsui" },
        "cold": { jp: "寒い", reading: "さむい", romaji: "samui" },
        "big": { jp: "大きい", reading: "おおきい", romaji: "ookii" },
        "small": { jp: "小さい", reading: "ちいさい", romaji: "chiisai" },
        "new": { jp: "新しい", reading: "あたらしい", romaji: "atarashii" },
        "old": { jp: "古い", reading: "ふるい", romaji: "furui" },
        "good": { jp: "良い", reading: "よい", romaji: "yoi" },
        "bad": { jp: "悪い", reading: "わるい", romaji: "warui" },
        "beautiful": { jp: "美しい", reading: "うつくしい", romaji: "utsukushii" },
        "cute": { jp: "可愛い", reading: "かわいい", romaji: "kawaii" },
        "fun": { jp: "楽しい", reading: "たのしい", romaji: "tanoshii" },
        "interesting": { jp: "面白い", reading: "おもしろい", romaji: "omoshiroi" },
        "difficult": { jp: "難しい", reading: "むずかしい", romaji: "muzukashii" },
        "easy": { jp: "易しい", reading: "やさしい", romaji: "yasashii" },
        "fast": { jp: "速い", reading: "はやい", romaji: "hayai" },
        "slow": { jp: "遅い", reading: "おそい", romaji: "osoi" },
        "much": { jp: "多い", reading: "おおい", romaji: "ooi" },
        "many": { jp: "多い", reading: "おおい", romaji: "ooi" },
        "little": { jp: "少ない", reading: "すくない", romaji: "sukunai" },
        "now": { jp: "今", reading: "いま", romaji: "ima" },
        "today": { jp: "今日", reading: "きょう", romaji: "kyou" },
        "tomorrow": { jp: "明日", reading: "あした", romaji: "ashita" },
        "yesterday": { jp: "昨日", reading: "きのう", romaji: "kinou" },
        "morning": { jp: "朝", reading: "あさ", romaji: "asa" },
        "evening": { jp: "夕方", reading: "ゆうがた", romaji: "yuugata" },
        "night": { jp: "夜", reading: "よる", romaji: "yoru" },
        "week": { jp: "週", reading: "しゅう", romaji: "shuu" },
        "month": { jp: "月", reading: "つき", romaji: "tsuki" },
        "year": { jp: "年", reading: "とし", romaji: "toshi" },
        "time": { jp: "時間", reading: "じかん", romaji: "jikan" },
        "day": { jp: "日", reading: "ひ", romaji: "hi" },
        "love": { jp: "愛", reading: "あい", romaji: "ai" },
        "cat": { jp: "猫", reading: "ねこ", romaji: "neko" },
        "dog": { jp: "犬", reading: "いぬ", romaji: "inu" },
        "bird": { jp: "鳥", reading: "とり", romaji: "tori" },
        "fish": { jp: "魚", reading: "さかな", romaji: "sakana" },
        "flower": { jp: "花", reading: "はな", romaji: "hana" },
        "tree": { jp: "木", reading: "き", romaji: "ki" },
        "mountain": { jp: "山", reading: "やま", romaji: "yama" },
        "river": { jp: "川", reading: "かわ", romaji: "kawa" },
        "sea": { jp: "海", reading: "うみ", romaji: "umi" },
        "ocean": { jp: "海", reading: "うみ", romaji: "umi" },
        "sky": { jp: "空", reading: "そら", romaji: "sora" },
        "sun": { jp: "太陽", reading: "たいよう", romaji: "taiyou" },
        "moon": { jp: "月", reading: "つき", romaji: "tsuki" },
        "star": { jp: "星", reading: "ほし", romaji: "hoshi" },
        "wind": { jp: "風", reading: "かぜ", romaji: "kaze" },
        "spring": { jp: "春", reading: "はる", romaji: "haru" },
        "summer": { jp: "夏", reading: "なつ", romaji: "natsu" },
        "autumn": { jp: "秋", reading: "あき", romaji: "aki" },
        "fall": { jp: "秋", reading: "あき", romaji: "aki" },
        "winter": { jp: "冬", reading: "ふゆ", romaji: "fuyu" },
        "cherry blossom": { jp: "桜", reading: "さくら", romaji: "sakura" },
        "sakura": { jp: "桜", reading: "さくら", romaji: "sakura" },
        "temple": { jp: "お寺", reading: "おてら", romaji: "otera" },
        "shrine": { jp: "神社", reading: "じんじゃ", romaji: "jinja" },
        "japan": { jp: "日本", reading: "にほん", romaji: "nihon" },
        "japanese": { jp: "日本語", reading: "にほんご", romaji: "nihongo" },
        "english": { jp: "英語", reading: "えいご", romaji: "eigo" },
        "language": { jp: "言葉", reading: "ことば", romaji: "kotoba" },
        "word": { jp: "単語", reading: "たんご", romaji: "tango" },
        "sentence": { jp: "文", reading: "ぶん", romaji: "bun" },
        "question": { jp: "質問", reading: "しつもん", romaji: "shitsumon" },
        "answer": { jp: "答え", reading: "こたえ", romaji: "kotae" },
        "number": { jp: "数字", reading: "すうじ", romaji: "suuji" },
        "one": { jp: "一", reading: "いち", romaji: "ichi" },
        "two": { jp: "二", reading: "に", romaji: "ni" },
        "three": { jp: "三", reading: "さん", romaji: "san" },
        "four": { jp: "四", reading: "よん", romaji: "yon" },
        "five": { jp: "五", reading: "ご", romaji: "go" },
        "six": { jp: "六", reading: "ろく", romaji: "roku" },
        "seven": { jp: "七", reading: "なな", romaji: "nana" },
        "eight": { jp: "八", reading: "はち", romaji: "hachi" },
        "nine": { jp: "九", reading: "きゅう", romaji: "kyuu" },
        "ten": { jp: "十", reading: "じゅう", romaji: "juu" },
        "hundred": { jp: "百", reading: "ひゃく", romaji: "hyaku" },
        "thousand": { jp: "千", reading: "せん", romaji: "sen" },
        "i": { jp: "私", reading: "わたし", romaji: "watashi" },
        "you": { jp: "あなた", reading: "あなた", romaji: "anata" },
        "he": { jp: "彼", reading: "かれ", romaji: "kare" },
        "she": { jp: "彼女", reading: "かのじょ", romaji: "kanojo" },
        "we": { jp: "私たち", reading: "わたしたち", romaji: "watashitachi" },
        "they": { jp: "彼ら", reading: "かれら", romaji: "karera" },
        "my": { jp: "私の", reading: "わたしの", romaji: "watashi no" },
        "your": { jp: "あなたの", reading: "あなたの", romaji: "anata no" },
        "what": { jp: "何", reading: "なに", romaji: "nani" },
        "who": { jp: "誰", reading: "だれ", romaji: "dare" },
        "where": { jp: "どこ", reading: "どこ", romaji: "doko" },
        "when": { jp: "いつ", reading: "いつ", romaji: "itsu" },
        "why": { jp: "なぜ", reading: "なぜ", romaji: "naze" },
        "how": { jp: "どう", reading: "どう", romaji: "dou" },
        "this": { jp: "これ", reading: "これ", romaji: "kore" },
        "that": { jp: "それ", reading: "それ", romaji: "sore" },
        "here": { jp: "ここ", reading: "ここ", romaji: "koko" },
        "there": { jp: "そこ", reading: "そこ", romaji: "soko" },
        "very": { jp: "とても", reading: "とても", romaji: "totemo" },
        "also": { jp: "も", reading: "も", romaji: "mo" },
        "but": { jp: "でも", reading: "でも", romaji: "demo" },
        "and": { jp: "と", reading: "と", romaji: "to" },
        "or": { jp: "又は", reading: "またわ", romaji: "matawa" },
        "because": { jp: "から", reading: "から", romaji: "kara" },
        "if": { jp: "もし", reading: "もし", romaji: "moshi" },
        "then": { jp: "それから", reading: "それから", romaji: "sorekara" },
        "so": { jp: "だから", reading: "だから", romaji: "dakara" },
        "think": { jp: "思う", reading: "おもう", romaji: "omou" },
        "know": { jp: "知る", reading: "しる", romaji: "shiru" },
        "see": { jp: "見る", reading: "みる", romaji: "miru" },
        "hear": { jp: "聞く", reading: "きく", romaji: "kiku" },
        "eat": { jp: "食べる", reading: "たべる", romaji: "taberu" },
        "eating": { jp: "食べている", reading: "たべている", romaji: "tabete iru" },
        "ate": { jp: "食べた", reading: "たべた", romaji: "tabeta" },
        "drink": { jp: "飲む", reading: "のむ", romaji: "nomu" },
        "walk": { jp: "歩く", reading: "あるく", romaji: "aruku" },
        "walked": { jp: "歩いた", reading: "あるいた", romaji: "aruita" },
        "run": { jp: "走る", reading: "はしる", romaji: "hashiru" },
        "running": { jp: "走っている", reading: "はしっている", romaji: "hashitte iru" },
        "ran": { jp: "走った", reading: "はしった", romaji: "hashitta" },
        "sleep": { jp: "寝る", reading: "ねる", romaji: "neru" },
        "wake up": { jp: "起きる", reading: "おきる", romaji: "okiru" },
        "work": { jp: "働く", reading: "はたらく", romaji: "hataraku" },
        "working": { jp: "働いている", reading: "はたらいている", romaji: "hataraite iru" },
        "worked": { jp: "働いた", reading: "はたらいた", romaji: "hataraita" },
        "play": { jp: "遊ぶ", reading: "あそぶ", romaji: "asobu" },
        "playing": { jp: "遊んでいる", reading: "あそんでいる", romaji: "asonde iru" },
        "buy": { jp: "買う", reading: "かう", romaji: "kau" },
        "bought": { jp: "買った", reading: "かった", romaji: "katta" },
        "come": { jp: "来る", reading: "くる", romaji: "kuru" },
        "came": { jp: "来た", reading: "きた", romaji: "kita" },
        "go": { jp: "行く", reading: "いく", romaji: "iku" },
        "going": { jp: "行っている", reading: "いっている", romaji: "itte iru" },
        "went": { jp: "行った", reading: "いった", romaji: "itta" },
        "return": { jp: "帰る", reading: "かえる", romaji: "kaeru" },
        "wait": { jp: "待つ", reading: "まつ", romaji: "matsu" },
        "open": { jp: "開ける", reading: "あける", romaji: "akeru" },
        "close": { jp: "閉める", reading: "しめる", romaji: "shimeru" },
        "give": { jp: "あげる", reading: "あげる", romaji: "ageru" },
        "take": { jp: "取る", reading: "とる", romaji: "toru" },
        "took": { jp: "取った", reading: "とった", romaji: "totta" },
        "make": { jp: "作る", reading: "つくる", romaji: "tsukuru" },
        "made": { jp: "作った", reading: "つくった", romaji: "tsukutta" },
        "use": { jp: "使う", reading: "つかう", romaji: "tsukau" },
        "used": { jp: "使った", reading: "つかった", romaji: "tsukatta" },
        "help": { jp: "助ける", reading: "たすける", romaji: "tasukeru" },
        "like": { jp: "好き", reading: "すき", romaji: "suki" },
        "want": { jp: "欲しい", reading: "ほしい", romaji: "hoshii" },
        "need": { jp: "必要", reading: "ひつよう", romaji: "hitsuyou" },
        "can": { jp: "出来る", reading: "できる", romaji: "dekiru" },
        "must": { jp: "なければならない", reading: "なかればならない", romaji: "nakereba naranai" },
        "try": { jp: "試す", reading: "ためす", romaji: "tamesu" },
        "begin": { jp: "始める", reading: "はじめる", romaji: "hajimeru" },
        "end": { jp: "終わる", reading: "おわる", romaji: "owaru" },
        "practice": { jp: "練習", reading: "れんしゅう", romaji: "renshuu" },
        "grammar": { jp: "文法", reading: "ぶんぽう", romaji: "bunpou" },
        "vocabulary": { jp: "語彙", reading: "ごい", romaji: "goi" },
        "pronunciation": { jp: "発音", reading: "はつおん", romaji: "hatsuon" },
        "character": { jp: "文字", reading: "もじ", romaji: "moji" },
        "hiragana": { jp: "ひらがな", reading: "ひらがな", romaji: "hiragana" },
        "katakana": { jp: "カタカナ", reading: "かたかな", romaji: "katakana" },
        "kanji": { jp: "漢字", reading: "かんじ", romaji: "kanji" },
        "alphabet": { jp: "アルファベット", reading: "あるふぁべっと", romaji: "arufabetto" },
        "vowel": { jp: "母音", reading: "ぼいん", romaji: "boin" },
        "consonant": { jp: "子音", reading: "しいん", romaji: "shiin" },
        "particle": { jp: "助詞", reading: "じょし", romaji: "joshi" },
        "verb": { jp: "動詞", reading: "どうし", romaji: "doushi" },
        "noun": { jp: "名詞", reading: "めいし", romaji: "meishi" },
        "adjective": { jp: "形容詞", reading: "けいようし", romaji: "keiyoushi" },
        "adverb": { jp: "副詞", reading: "ふくし", romaji: "fukushi" },
        "example": { jp: "例", reading: "れい", romaji: "rei" },
        "meaning": { jp: "意味", reading: "いみ", romaji: "imi" },
        "translation": { jp: "翻訳", reading: "ほんやく", romaji: "honyaku" },
        "progress": { jp: "進歩", reading: "しんぽ", romaji: "shinpo" },
        "beginner": { jp: "初心者", reading: "しょしんしゃ", romaji: "shoshinsha" },
        "intermediate": { jp: "中級", reading: "ちゅうきゅう", romaji: "chuukyuu" },
        "advanced": { jp: "上級", reading: "じょうきゅう", romaji: "joukyuu" },
        "welcome": { jp: "ようこそ", reading: "ようこそ", romaji: "youkoso" },
        "enjoy": { jp: "楽しむ", reading: "たのしむ", romaji: "tanoshimu" },
        "together": { jp: "一緒に", reading: "いっしょに", romaji: "isshoni" },
        "every day": { jp: "毎日", reading: "まいにち", romaji: "mainichi" },
        "always": { jp: "いつも", reading: "いつも", romaji: "itsumo" },
        "sometimes": { jp: "時々", reading: "ときどき", romaji: "tokidoki" },
        "never": { jp: "決して", reading: "けっして", romaji: "kesshite" },
        "right": { jp: "右", reading: "みぎ", romaji: "migi" },
        "left": { jp: "左", reading: "ひだり", romaji: "hidari" },
        "up": { jp: "上", reading: "うえ", romaji: "ue" },
        "down": { jp: "下", reading: "した", romaji: "shita" },
        "inside": { jp: "中", reading: "なか", romaji: "naka" },
        "outside": { jp: "外", reading: "そと", romaji: "soto" },
        "front": { jp: "前", reading: "まえ", romaji: "mae" },
        "back": { jp: "後ろ", reading: "うしろ", romaji: "ushiro" },
        "next": { jp: "次", reading: "つぎ", romaji: "tsugi" },
        "last": { jp: "最後", reading: "さいご", romaji: "saigo" },
        "first": { jp: "最初", reading: "さいしょ", romaji: "saisho" },
        "long": { jp: "長い", reading: "ながい", romaji: "nagai" },
        "short": { jp: "短い", reading: "みじかい", romaji: "mijikai" },
        "wide": { jp: "広い", reading: "ひろい", romaji: "hiroi" },
        "narrow": { jp: "狭い", reading: "せまい", romaji: "semai" },
        "heavy": { jp: "重い", reading: "おもい", romaji: "omoi" },
        "light": { jp: "軽い", reading: "かるい", romaji: "karui" },
        "white": { jp: "白い", reading: "しろい", romaji: "shiroi" },
        "black": { jp: "黒い", reading: "くろい", romaji: "kuroi" },
        "red": { jp: "赤い", reading: "あかい", romaji: "akai" },
        "blue": { jp: "青い", reading: "あおい", romaji: "aoi" },
        "green": { jp: "緑", reading: "みどり", romaji: "midori" },
        "yellow": { jp: "黄色", reading: "きいろ", romaji: "kiiro" },
        "pink": { jp: "ピンク", reading: "ぴんく", romaji: "pinku" },
        "purple": { jp: "紫", reading: "むらさき", romaji: "murasaki" },
        "orange": { jp: "オレンジ", reading: "おれんじ", romaji: "orenji" },
        "brown": { jp: "茶色", reading: "ちゃいろ", romaji: "chairo" },
        "money": { jp: "お金", reading: "おかね", romaji: "okane" },
        "price": { jp: "値段", reading: "ねだん", romaji: "nedan" },
        "cheap": { jp: "安い", reading: "やすい", romaji: "yasui" },
        "expensive": { jp: "高い", reading: "たかい", romaji: "takai" },
        "free": { jp: "無料", reading: "むりょう", romaji: "muryou" },
        "happy": { jp: "嬉しい", reading: "うれしい", romaji: "ureshii" },
        "sad": { jp: "悲しい", reading: "かなしい", romaji: "kanashii" },
        "angry": { jp: "怒る", reading: "おこる", romaji: "okoru" },
        "afraid": { jp: "怖い", reading: "こわい", romaji: "kowai" },
        "tired": { jp: "疲れた", reading: "つかれた", romaji: "tsukareta" },
        "hungry": { jp: "お腹が空いた", reading: "おなかがすいた", romaji: "onaka ga suita" },
        "thirsty": { jp: "喉が渇いた", reading: "のどがかわいた", romaji: "nodo ga kawaita" },
        "apple": { jp: "りんご", reading: "りんご", romaji: "ringo" },
        "love": { jp: "愛", reading: "あい", romaji: "ai" },
        "hate": { jp: "嫌い", reading: "きらい", romaji: "kirai" },
        "believe": { jp: "信じる", reading: "しんじる", romaji: "shinjiru" },
        "remember": { jp: "覚える", reading: "おぼえる", romaji: "oboeru" },
        "forget": { jp: "忘れる", reading: "わすれる", romaji: "wasureru" },
        "understand": { jp: "分かる", reading: "わかる", romaji: "wakaru" },
        "understood": { jp: "分かった", reading: "わかった", romaji: "wakatta" },
        "teach": { jp: "教える", reading: "おしえる", romaji: "oshieru" },
        "live": { jp: "住む", reading: "すむ", romaji: "sumu" },
        "die": { jp: "死ぬ", reading: "しぬ", romaji: "shinu" },
        "sing": { jp: "歌う", reading: "うたう", romaji: "utau" },
        "dance": { jp: "踊る", reading: "おどる", romaji: "odoru" },
        "cook": { jp: "料理する", reading: "りょうりする", romaji: "ryouri suru" },
        "clean": { jp: "掃除する", reading: "そうじする", romaji: "souji suru" },
        "wash": { jp: "洗う", reading: "あらう", romaji: "arau" },
        "travel": { jp: "旅行", reading: "りょこう", romaji: "ryokou" },
        "movie": { jp: "映画", reading: "えいが", romaji: "eiga" },
        "music": { jp: "音楽", reading: "おんがく", romaji: "ongaku" },
        "sport": { jp: "スポーツ", reading: "すぽーつ", romaji: "supootsu" },
        "game": { jp: "ゲーム", reading: "げーむ", romaji: "geemu" },
        "coffee": { jp: "コーヒー", reading: "こーひー", romaji: "koohii" },
        "beer": { jp: "ビール", reading: "びーる", romaji: "biiru" },
        "bread": { jp: "パン", reading: "ぱん", romaji: "pan" },
        "meat": { jp: "肉", reading: "にく", romaji: "niku" },
        "vegetable": { jp: "野菜", reading: "やさい", romaji: "yasai" },
        "fruit": { jp: "果物", reading: "くだもの", romaji: "kudamono" },
        "egg": { jp: "卵", reading: "たまご", romaji: "tamago" },
        "milk": { jp: "牛乳", reading: "ぎゅうにゅう", romaji: "gyuunyuu" },
        "breakfast": { jp: "朝ご飯", reading: "あさごはん", romaji: "asagohan" },
        "lunch": { jp: "昼ご飯", reading: "ひるごはん", romaji: "hirugohan" },
        "dinner": { jp: "晩ご飯", reading: "ばんごはん", romaji: "bangohan" },
        "park": { jp: "公園", reading: "こうえん", romaji: "kouen" },
        "city": { jp: "都市", reading: "とし", romaji: "toshi" },
        "country": { jp: "国", reading: "くに", romaji: "kuni" },
        "world": { jp: "世界", reading: "せかい", romaji: "sekai" },
        "earth": { jp: "地球", reading: "ちきゅう", romaji: "chikyuu" },
        "color": { jp: "色", reading: "いろ", romaji: "iro" },
        "picture": { jp: "写真", reading: "しゃしん", romaji: "shashin" },
        "photo": { jp: "写真", reading: "しゃしん", romaji: "shashin" },
        "letter": { jp: "手紙", reading: "てがみ", romaji: "tegami" },
        "phone": { jp: "電話", reading: "でんわ", romaji: "denwa" },
        "computer": { jp: "コンピュータ", reading: "こんぴゅーた", romaji: "konpyuuta" },
        "internet": { jp: "インターネット", reading: "いんたーねっと", romaji: "intaanetto" },
        "email": { jp: "メール", reading: "めーる", romaji: "meeru" },
        "pen": { jp: "ペン", reading: "ぺん", romaji: "pen" },
        "paper": { jp: "紙", reading: "かみ", romaji: "kami" },
        "bag": { jp: "鞄", reading: "かばん", romaji: "kaban" },
        "shoes": { jp: "靴", reading: "くつ", romaji: "kutsu" },
        "shirt": { jp: "シャツ", reading: "しゃつ", romaji: "shatsu" },
        "desk": { jp: "机", reading: "つくえ", romaji: "tsukue" },
        "chair": { jp: "椅子", reading: "いす", romaji: "isu" },
        "man": { jp: "男", reading: "おとこ", romaji: "otoko" },
        "woman": { jp: "女", reading: "おんな", romaji: "onna" },
        "child": { jp: "子供", reading: "こども", romaji: "kodomo" },
        "children": { jp: "子供たち", reading: "こどもたち", romaji: "kodomo tachi" },
        "person": { jp: "人", reading: "ひと", romaji: "hito" },
        "people": { jp: "人々", reading: "ひとびと", romaji: "hitobito" },
        "head": { jp: "頭", reading: "あたま", romaji: "atama" },
        "eye": { jp: "目", reading: "め", romaji: "me" },
        "ear": { jp: "耳", reading: "みみ", romaji: "mimi" },
        "mouth": { jp: "口", reading: "くち", romaji: "kuchi" },
        "hand": { jp: "手", reading: "て", romaji: "te" },
        "foot": { jp: "足", reading: "あし", romaji: "ashi" },
        "heart": { jp: "心", reading: "こころ", romaji: "kokoro" },
        "body": { jp: "体", reading: "からだ", romaji: "karada" },
        "life": { jp: "人生", reading: "じんせい", romaji: "jinsei" },
        "death": { jp: "死", reading: "し", romaji: "shi" },
        "dream": { jp: "夢", reading: "ゆめ", romaji: "yume" },
        "hope": { jp: "希望", reading: "きぼう", romaji: "kibou" },
        "peace": { jp: "平和", reading: "へいわ", romaji: "heiwa" },
        "war": { jp: "戦争", reading: "せんそう", romaji: "sensou" },
        "story": { jp: "物語", reading: "ものがたり", romaji: "monogatari" },
        "history": { jp: "歴史", reading: "れきし", romaji: "rekishi" },
        "culture": { jp: "文化", reading: "ぶんか", romaji: "bunka" },
        "art": { jp: "芸術", reading: "げいじゅつ", romaji: "geijutsu" },
        "science": { jp: "科学", reading: "かがく", romaji: "kagaku" },
        "math": { jp: "数学", reading: "すうがく", romaji: "suugaku" },
        "medicine": { jp: "医学", reading: "いがく", romaji: "igaku" },
        "law": { jp: "法律", reading: "ほうりつ", romaji: "houritsu" },
        "politics": { jp: "政治", reading: "せいじ", romaji: "seiji" },
        "economy": { jp: "経済", reading: "けいざい", romaji: "keizai" },
        "business": { jp: "ビジネス", reading: "びじねす", romaji: "bijinesu" },
        "company": { jp: "会社", reading: "かいしゃ", romaji: "kaisha" },
        "job": { jp: "仕事", reading: "しごと", romaji: "shigoto" },
        "salary": { jp: "給料", reading: "きゅうりょう", romaji: "kyuuryou" },
        "meeting": { jp: "会議", reading: "かいぎ", romaji: "kaigi" },
        "birthday": { jp: "誕生日", reading: "たんじょうび", romaji: "tanjoubi" },
        "party": { jp: "パーティー", reading: "ぱーてぃー", romaji: "paatii" },
        "gift": { jp: "プレゼント", reading: "ぷれぜんと", romaji: "purezento" },
        "health": { jp: "健康", reading: "けんこう", romaji: "kenkou" },
        "sick": { jp: "病気", reading: "びょうき", romaji: "byouki" },
        "pain": { jp: "痛い", reading: "いたい", romaji: "itai" },
        "medicine": { jp: "薬", reading: "くすり", romaji: "kusuri" },
        "danger": { jp: "危険", reading: "きけん", romaji: "kiken" },
        "safe": { jp: "安全", reading: "あんぜん", romaji: "anzen" },
        "strong": { jp: "強い", reading: "つよい", romaji: "tsuyoi" },
        "weak": { jp: "弱い", reading: "よわい", romaji: "yowai" },
        "kind": { jp: "親切", reading: "しんせつ", romaji: "shinsetsu" },
        "polite": { jp: "丁寧", reading: "ていねい", romaji: "teinei" },
        "important": { jp: "大切", reading: "たいせつ", romaji: "taisetsu" },
        "wrong": { jp: "間違い", reading: "まちがい", romaji: "machigai" },
        "correct": { jp: "正しい", reading: "ただしい", romaji: "tadashii" },
        "true": { jp: "本当", reading: "ほんとう", romaji: "hontou" },
        "false": { jp: "嘘", reading: "うそ", romaji: "uso" },
        "real": { jp: "本当の", reading: "ほんとうの", romaji: "hontou no" },
        "different": { jp: "違う", reading: "ちがう", romaji: "chigau" },
        "same": { jp: "同じ", reading: "おなじ", romaji: "onaji" },
        "other": { jp: "他の", reading: "ほかの", romaji: "hoka no" },
        "all": { jp: "全て", reading: "すべて", romaji: "subete" },
        "some": { jp: "いくつか", reading: "いくつか", romaji: "ikutsuka" },
        "every": { jp: "毎", reading: "まい", romaji: "mai" },
        "each": { jp: "各", reading: "かく", romaji: "kaku" },
        "more": { jp: "もっと", reading: "もっと", romaji: "motto" },
        "most": { jp: "一番", reading: "いちばん", romaji: "ichiban" },
        "less": { jp: "もっと少ない", reading: "もっとすくない", romaji: "motto sukunai" },
        "enough": { jp: "十分", reading: "じゅうぶん", romaji: "juubun" },
        "only": { jp: "だけ", reading: "だけ", romaji: "dake" },
        "just": { jp: "ただ", reading: "ただ", romaji: "tada" },
        "still": { jp: "まだ", reading: "まだ", romaji: "mada" },
        "already": { jp: "もう", reading: "もう", romaji: "mou" },
        "again": { jp: "また", reading: "また", romaji: "mata" },
        "together": { jp: "一緒に", reading: "いっしょに", romaji: "isshoni" },
        "alone": { jp: "一人で", reading: "ひとりで", romaji: "hitori de" },
        "perhaps": { jp: "たぶん", reading: "たぶん", romaji: "tabun" },
        "really": { jp: "本当に", reading: "ほんとうに", romaji: "hontou ni" },
        "quite": { jp: "かなり", reading: "かなり", romaji: "kanari" },
        "soon": { jp: "すぐに", reading: "すぐに", romaji: "sugu ni" },
        "recently": { jp: "最近", reading: "さいきん", romaji: "saikin" },
        "usually": { jp: "普通", reading: "ふつう", romaji: "futsuu" },
        "often": { jp: "よく", reading: "よく", romaji: "yoku" },
        "especially": { jp: "特に", reading: "とくに", romaji: "tokuni" }
    };

    // ========== REVERSE DICT: Japanese → English ==========
    var jpDict = {};
    Object.keys(dict).forEach(function (en) {
        var item = dict[en];
        if (item.jp) {
            if (!jpDict[item.jp]) {
                jpDict[item.jp] = { en: en, reading: item.reading, romaji: item.romaji };
            }
        }
    });

    // ========== JAPANESE DEINFLECTION TABLE ==========
    // Maps inflected form suffix → [base suffix, original form type]
    var jpDeinflect = [
        { suffix: "ました", base: "る", type: "v" },
        { suffix: "ました", base: "す", type: "v" },
        { suffix: "ました", base: "く", type: "v" },
        { suffix: "ました", base: "ぐ", type: "v" },
        { suffix: "ました", base: "む", type: "v" },
        { suffix: "ました", base: "ぶ", type: "v" },
        { suffix: "ました", base: "ぬ", type: "v" },
        { suffix: "ました", base: "う", type: "v" },
        { suffix: "ました", base: "つ", type: "v" },
        { suffix: "ました", base: "る", type: "v" },
        { suffix: "ました", base: "", type: "v-irr" },
        { suffix: "ている", base: "る", type: "v" },
        { suffix: "ている", base: "す", type: "v" },
        { suffix: "ている", base: "る", type: "v" },
        { suffix: "ていた", base: "る", type: "v" },
        { suffix: "て", base: "る", type: "v" },
        { suffix: "て", base: "く", type: "v" },
        { suffix: "て", base: "む", type: "v" },
        { suffix: "て", base: "ぶ", type: "v" },
        { suffix: "て", base: "ぬ", type: "v" },
        { suffix: "て", base: "う", type: "v" },
        { suffix: "て", base: "つ", type: "v" },
        { suffix: "て", base: "ぐ", type: "v" },
        { suffix: "た", base: "る", type: "v" },
        { suffix: "た", base: "く", type: "v" },
        { suffix: "た", base: "む", type: "v" },
        { suffix: "た", base: "ぶ", type: "v" },
        { suffix: "た", base: "ぬ", type: "v" },
        { suffix: "た", base: "う", type: "v" },
        { suffix: "た", base: "つ", type: "v" },
        { suffix: "た", base: "ぐ", type: "v" },
        { suffix: "た", base: "す", type: "v" },
        { suffix: "ない", base: "る", type: "v-neg" },
        { suffix: "ない", base: "う", type: "v-neg" },
        { suffix: "ない", base: "く", type: "adj" },
        { suffix: "かった", base: "い", type: "adj-past" },
        { suffix: "くない", base: "い", type: "adj-neg" },
        { suffix: "くて", base: "い", type: "adj-te" },
        { suffix: "くなかった", base: "い", type: "adj-neg-past" },
        { suffix: "です", base: "", type: "copula" },
        { suffix: "だ", base: "", type: "copula" },
        { suffix: "でしょう", base: "", type: "copula" },
        { suffix: "だった", base: "", type: "copula-past" }
    ];

    // ========== ENGLISH STEMMING (basic) ==========
    var enStemRules = [
        { suffix: "ied", base: "y" },     // studied → study
        { suffix: "ies", base: "y" },     // studies → study
        { suffix: "ied", base: "y" },     // carried → carry
        { suffix: "ing", base: "" },      // studying → study (need to try removing + adding 'e' too)
        { suffix: "ed", base: "" },       // studied → study (need to try removing 'd' + base)
        { suffix: "es", base: "" },       // goes → go
        { suffix: "s", base: "" },        // books → book
        { suffix: "er", base: "" },       // teacher → teach
        { suffix: "est", base: "" }       // fastest → fast
    ];

    // ========== NORMALIZE TEXT ==========
    function normalize(text) {
        text = text.trim();
        // Remove surrounding quotes, punctuation
        text = text.replace(/^[\s"''「」『』。、！？,.:;!?]+|[\s"''「」『』。、！？,.:;!?]+$/g, '');
        // Remove line breaks
        text = text.replace(/[\r\n]+/g, ' ');
        // Collapse spaces
        text = text.replace(/\s+/g, ' ');
        // Normalize Japanese Unicode (full-width → half-width for ASCII range)
        text = text.replace(/[\uff01-\uff5e]/g, function (c) {
            return String.fromCharCode(c.charCodeAt(0) - 0xfee0);
        });
        // Full-width space → normal space
        text = text.replace(/\u3000/g, ' ');
        return text.trim();
    }

    // ========== DETECT LANGUAGE ==========
    function isJapanese(text) {
        return /[\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FFF\uFF65-\uFF9F]/.test(text);
    }

    function detectLanguage(text) {
        return isJapanese(text) ? 'ja' : 'en';
    }

    // ========== IS SINGLE WORD? ==========
    function isSingleWord(text, lang) {
        if (lang === 'ja') {
            // Japanese doesn't use spaces — consider short selections as single words
            return text.length <= 8;
        }
        return text.indexOf(' ') === -1;
    }

    // ========== JAPANESE DEINFLECTION ==========
    function tryDeinflectJapanese(text) {
        var results = [text]; // always include original
        for (var i = 0; i < jpDeinflect.length; i++) {
            var rule = jpDeinflect[i];
            if (text.endsWith(rule.suffix)) {
                var stem = text.slice(0, text.length - rule.suffix.length);
                var candidate = stem + rule.base;
                if (candidate && candidate.length > 0 && results.indexOf(candidate) === -1) {
                    results.push(candidate);
                }
            }
        }
        // Special te-form handling: 食べて → 食べる (remove て add る)
        if (text.endsWith("て") || text.endsWith("で")) {
            var teStem = text.slice(0, -1);
            if (teStem.length > 0) {
                results.push(teStem + "る");
                results.push(teStem + "む");
                results.push(teStem + "ぶ");
                results.push(teStem + "ぬ");
                results.push(teStem + "く");
                results.push(teStem + "ぐ");
                results.push(teStem + "す");
                results.push(teStem + "う");
                results.push(teStem + "つ");
            }
        }
        // ta-form: 食べた → 食べる
        if (text.endsWith("た") || text.endsWith("だ")) {
            var taStem = text.slice(0, -1);
            if (taStem.length > 0) {
                results.push(taStem + "る");
                results.push(taStem + "む");
                results.push(taStem + "ぶ");
                results.push(taStem + "ぬ");
                results.push(taStem + "く");
                results.push(taStem + "ぐ");
                results.push(taStem + "す");
                results.push(taStem + "う");
                results.push(taStem + "つ");
            }
        }
        return results;
    }

    // ========== ENGLISH STEMMING ==========
    function tryStemEnglish(text) {
        var lower = text.toLowerCase();
        var results = [lower];
        for (var i = 0; i < enStemRules.length; i++) {
            var rule = enStemRules[i];
            if (lower.endsWith(rule.suffix) && lower.length > rule.suffix.length + 1) {
                var stem = lower.slice(0, lower.length - rule.suffix.length) + rule.base;
                if (results.indexOf(stem) === -1) results.push(stem);
                // For -ing and -ed, also try adding 'e' back (e.g. studying → study, not stud)
                if (rule.suffix === "ing" || rule.suffix === "ed") {
                    var stemE = lower.slice(0, lower.length - rule.suffix.length) + "e";
                    if (results.indexOf(stemE) === -1) results.push(stemE);
                    // Double consonant: running → run
                    var s = lower.slice(0, lower.length - rule.suffix.length);
                    if (s.length >= 2 && s[s.length - 1] === s[s.length - 2]) {
                        var stemDouble = s.slice(0, -1);
                        if (results.indexOf(stemDouble) === -1) results.push(stemDouble);
                    }
                }
                // -ied/-ies → -y
                if (rule.suffix === "ied" || rule.suffix === "ies") {
                    var stemY = lower.slice(0, lower.length - rule.suffix.length) + "y";
                    if (results.indexOf(stemY) === -1) results.push(stemY);
                }
            }
        }
        return results;
    }

    // ========== DICTIONARY LOOKUP ==========
    function lookupEnglish(text) {
        var candidates = tryStemEnglish(text);
        for (var i = 0; i < candidates.length; i++) {
            if (dict[candidates[i]]) {
                return {
                    type: 'en',
                    original: text,
                    matched: candidates[i],
                    translation: dict[candidates[i]].jp,
                    reading: dict[candidates[i]].reading,
                    romaji: dict[candidates[i]].romaji,
                    source: 'dictionary'
                };
            }
        }
        return null;
    }

    function lookupJapanese(text) {
        var candidates = tryDeinflectJapanese(text);
        for (var i = 0; i < candidates.length; i++) {
            if (jpDict[candidates[i]]) {
                return {
                    type: 'ja',
                    original: text,
                    matched: candidates[i],
                    translation: jpDict[candidates[i]].en,
                    reading: jpDict[candidates[i]].reading,
                    romaji: jpDict[candidates[i]].romaji,
                    source: 'dictionary'
                };
            }
        }
        // Substring match for longer Japanese text
        var jpKeys = Object.keys(jpDict);
        // Sort by length descending (longer matches first)
        jpKeys.sort(function (a, b) { return b.length - a.length; });
        for (var j = 0; j < jpKeys.length; j++) {
            if (text.indexOf(jpKeys[j]) >= 0) {
                return {
                    type: 'ja',
                    original: text,
                    matched: jpKeys[j],
                    translation: jpDict[jpKeys[j]].en,
                    reading: jpDict[jpKeys[j]].reading,
                    romaji: jpDict[jpKeys[j]].romaji,
                    source: 'dictionary'
                };
            }
        }
        return null;
    }

    // ========== PHRASE / SENTENCE LOOKUP ==========
    // For multi-word English, try looking up each significant word
    function lookupEnglishPhrase(text) {
        var words = text.toLowerCase().split(/\s+/);
        var results = [];
        for (var i = 0; i < words.length; i++) {
            var w = words[i];
            // Skip common particles
            if (['a', 'an', 'the', 'is', 'are', 'was', 'were', 'am', 'in', 'on', 'at', 'to', 'of', 'for', 'with', 'it', 'i'].indexOf(w) >= 0) continue;
            var r = lookupEnglish(w);
            if (r) results.push(r);
        }
        if (results.length > 0) {
            return {
                type: 'en',
                original: text,
                translation: results.map(function (r) { return r.translation; }).join(' · '),
                reading: results.map(function (r) { return r.reading; }).join(' · '),
                romaji: results.map(function (r) { return r.romaji; }).join(' · '),
                source: 'phrase-lookup',
                isPhrase: true
            };
        }
        return null;
    }

    // ========== AI TUTOR FALLBACK ==========
    async function callAiTutorForTranslation(text, lang) {
        var direction = lang === 'ja' ? 'Japanese to English' : 'English to Japanese';
        var prompt = 'Translate the following text from ' + direction + '. Return ONLY a JSON object with these fields:\n'
            + '- "translation": the translated text\n'
            + '- "reading": Japanese reading in hiragana (if applicable)\n'
            + '- "romaji": romanized reading (if applicable)\n'
            + '- "explanation": one short sentence explaining the meaning\n\n'
            + 'Text: "' + text + '"';

        try {
            var token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
            var res = await fetch('/api/tutor', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({
                    message: prompt,
                    lessonId: null,
                    history: []
                })
            });

            if (!res.ok) return null;
            var data = await res.json();
            var reply = data.reply || '';
            // Try to extract JSON from the reply
            var jsonMatch = reply.match(/\{[\s\S]*?\}/);
            if (jsonMatch) {
                try {
                    var parsed = JSON.parse(jsonMatch[0]);
                    return {
                        translation: parsed.translation || '',
                        reading: parsed.reading || '',
                        romaji: parsed.romaji || '',
                        explanation: parsed.explanation || ''
                    };
                } catch (e) { /* fall through */ }
            }
            // If no JSON, try to use the raw reply as translation
            if (reply.length > 0 && reply.length < 200) {
                return { translation: reply, reading: '', romaji: '', explanation: '' };
            }
        } catch (e) { /* silent fail */ }
        return null;
    }

    // ========== SPEAK ==========
    function speak(text, lang) {
        if (!window.speechSynthesis) return;
        window.speechSynthesis.cancel();
        var utterance = new SpeechSynthesisUtterance(text);
        // Always pronounce Japanese on this learning site
        utterance.lang = 'ja-JP';
        utterance.rate = 0.85;
        window.speechSynthesis.speak(utterance);
    }

    // ========== SHOW LOADING POPUP ==========
    function showLoadingPopup(text, rect) {
        if (!popup) {
            popup = document.createElement('div');
            popup.className = 'na-translate-popup';
            document.body.appendChild(popup);
        }
        popup.innerHTML =
            '<div class="na-translate-header">'
            + '<span class="na-translate-original">' + escapeHtml(text) + '</span>'
            + '</div>'
            + '<div class="na-translate-loading">'
            + '<span class="na-translate-loading-icon">🌸</span> Looking it up...'
            + '</div>';

        positionPopup(rect);
        popup.style.display = 'block';
        popup.classList.add('na-translate-popup--visible');
    }

    // ========== POSITION POPUP ==========
    function positionPopup(rect) {
        var popupW = 240;
        var popupH = popup.offsetHeight || 140;
        var left = rect.left + rect.width / 2 - popupW / 2 + window.scrollX;
        var top = rect.top - popupH - 10 + window.scrollY;
        if (left < 8) left = 8;
        if (left + popupW > window.innerWidth - 8) left = window.innerWidth - popupW - 8;
        if (top < window.scrollY + 8) {
            top = rect.bottom + 10 + window.scrollY;
        }
        popup.style.left = left + 'px';
        popup.style.top = top + 'px';
    }

    // ========== SHOW RESULT POPUP ==========
    function showResultPopup(result, rect) {
        if (!popup) {
            popup = document.createElement('div');
            popup.className = 'na-translate-popup';
            document.body.appendChild(popup);
        }

        var isJa = result.type === 'ja';
        var label = isJa ? 'English' : 'Japanese';
        // Always speak Japanese
        var speakText = isJa ? result.original : result.translation;

        var html =
            '<div class="na-translate-header">'
            + '<span class="na-translate-original">' + escapeHtml(result.original) + '</span>'
            + (result.matched && result.matched !== result.original ? '<small class="na-translate-matched">← ' + escapeHtml(result.matched) + '</small>' : '')
            + '</div>'
            + '<div class="na-translate-row">'
            + '<span class="na-translate-label">' + label + '</span>'
            + '<span class="na-translate-value">' + escapeHtml(result.translation) + '</span>'
            + '</div>';

        if (result.reading) {
            html += '<div class="na-translate-row">'
                + '<span class="na-translate-label">Reading</span>'
                + '<span class="na-translate-value">' + escapeHtml(result.reading) + '</span>'
                + '</div>';
        }

        if (result.romaji) {
            html += '<div class="na-translate-row">'
                + '<span class="na-translate-label">Romaji</span>'
                + '<span class="na-translate-value">' + escapeHtml(result.romaji) + '</span>'
                + '</div>';
        }

        if (result.explanation) {
            html += '<div class="na-translate-row na-translate-explain-row">'
                + '<span class="na-translate-label">Note</span>'
                + '<span class="na-translate-value na-translate-explain">' + escapeHtml(result.explanation) + '</span>'
                + '</div>';
        }

        html += '<button class="na-translate-speak" title="Hear Japanese pronunciation">'
            + '🔊'
            + '</button>';

        popup.innerHTML = html;

        popup.querySelector('.na-translate-speak').addEventListener('click', function (e) {
            e.stopPropagation();
            speak(speakText);
        });

        positionPopup(rect);
        popup.style.display = 'block';
        popup.classList.add('na-translate-popup--visible');
    }

    // ========== SHOW UNAVAILABLE POPUP ==========
    function showUnavailablePopup(text, rect) {
        if (!popup) {
            popup = document.createElement('div');
            popup.className = 'na-translate-popup';
            document.body.appendChild(popup);
        }
        popup.innerHTML =
            '<div class="na-translate-header">'
            + '<span class="na-translate-original">' + escapeHtml(text) + '</span>'
            + '</div>'
            + '<div class="na-translate-row">'
            + '<span class="na-translate-label" style="color:var(--na-text-muted);">Translation unavailable</span>'
            + '</div>';

        positionPopup(rect);
        popup.style.display = 'block';
        popup.classList.add('na-translate-popup--visible');
        hideTimer = setTimeout(hidePopup, 2500);
    }

    // ========== HIDE POPUP ==========
    function hidePopup() {
        clearTimeout(hideTimer);
        if (popup) {
            popup.classList.remove('na-translate-popup--visible');
            setTimeout(function () {
                if (popup) popup.style.display = 'none';
            }, 150);
        }
    }

    function escapeHtml(s) {
        var d = document.createElement('div');
        d.textContent = s;
        return d.innerHTML;
    }

    // ========== SHOULD IGNORE SELECTION? ==========
    function shouldIgnore(e) {
        var el = e.target;
        // Ignore interactive form controls only
        var ignoreTags = ['INPUT', 'TEXTAREA', 'SELECT', 'OPTION'];
        var tag = el.tagName;
        if (ignoreTags.indexOf(tag) >= 0) return true;
        // Ignore nav bar, mobile nav, AI tutor, and translation popup itself
        if (el.closest) {
            if (el.closest('.navbar, .na-mobile-nav, .na-tutor-panel, .na-tutor-fab, .na-translate-popup')) return true;
        }
        return false;
    }

    // ========== MAIN SELECTION HANDLER ==========
    // ALWAYS show popup first (loading), THEN look up translation.
    // Translation failure must NEVER prevent popup from appearing.
    document.addEventListener('mouseup', function (e) {
        if (popup && popup.contains(e.target)) return;
        if (shouldIgnore(e)) {
            hidePopup();
            return;
        }

        clearTimeout(hideTimer);

        var sel = window.getSelection();
        if (!sel || sel.isCollapsed || !sel.toString().trim()) {
            hidePopup();
            return;
        }

        var rawText = sel.toString().trim();
        if (rawText.length > 80) {
            hidePopup();
            return;
        }

        var text = normalize(rawText);
        if (!text) {
            hidePopup();
            return;
        }

        // Only process meaningful content (must contain Japanese or Latin letters)
        if (!isJapanese(text) && !/[a-zA-Z]/.test(text)) {
            hidePopup();
            return;
        }

        var range = sel.getRangeAt(0);
        var rect = range.getBoundingClientRect();

        // Check cache first
        var lang = detectLanguage(text);
        var cacheKey = text + '|' + lang;
        if (translationCache[cacheKey]) {
            var cached = translationCache[cacheKey];
            cached.original = rawText;
            showResultPopup(cached, rect);
            return;
        }

        // *** ALWAYS show popup immediately (loading state) ***
        showLoadingPopup(rawText, rect);

        // Then look up translation (async)
        handleTranslation(text, rawText, lang, rect);
    });

    async function handleTranslation(text, rawText, lang, rect) {
        // STAGE 1: Dictionary lookup for single words
        if (isSingleWord(text, lang)) {
            var dictResult = lang === 'ja' ? lookupJapanese(text) : lookupEnglish(text);
            if (dictResult) {
                dictResult.original = rawText;
                translationCache[text + '|' + lang] = dictResult;
                showResultPopup(dictResult, rect);
                return;
            }
        }

        // STAGE 2: Phrase lookup for multi-word
        if (lang === 'en' && !isSingleWord(text, lang)) {
            var fullPhrase = lookupEnglish(text);
            if (fullPhrase) {
                fullPhrase.original = rawText;
                translationCache[text + '|' + lang] = fullPhrase;
                showResultPopup(fullPhrase, rect);
                return;
            }
            var phraseResult = lookupEnglishPhrase(text);
            if (phraseResult) {
                phraseResult.original = rawText;
                translationCache[text + '|' + lang] = phraseResult;
                showResultPopup(phraseResult, rect);
                return;
            }
        }

        if (lang === 'ja' && !isSingleWord(text, lang)) {
            var jpResult = lookupJapanese(text);
            if (jpResult) {
                jpResult.original = rawText;
                translationCache[text + '|' + lang] = jpResult;
                showResultPopup(jpResult, rect);
                return;
            }
        }

        // STAGE 3: AI Tutor fallback
        try {
            var aiResult = await callAiTutorForTranslation(text, lang);
            if (aiResult && aiResult.translation) {
                var finalResult = {
                    type: lang,
                    original: rawText,
                    matched: null,
                    translation: aiResult.translation,
                    reading: aiResult.reading || '',
                    romaji: aiResult.romaji || '',
                    explanation: aiResult.explanation || '',
                    source: 'ai-tutor'
                };
                translationCache[text + '|' + lang] = finalResult;
                showResultPopup(finalResult, rect);
                return;
            }
        } catch (err) {
            // AI failed, fall through to unavailable
        }

        // All stages failed — still show popup with unavailable message
        showUnavailablePopup(rawText, rect);
    }

    // ========== HIDE ON CLICK OUTSIDE / ESC ==========
    document.addEventListener('mousedown', function (e) {
        if (popup && !popup.contains(e.target)) {
            hidePopup();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') hidePopup();
    });
})();
