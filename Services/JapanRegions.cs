using NihongoAcademy.Models.ExploreJapan;

namespace NihongoAcademy.Services
{
    /// <summary>
    /// Static data for the 9 traditional regions of Japan (9 地方).
    /// Ordered geographically from north (Hokkaido) to south (Okinawa).
    /// Text sourced from the group's "NihongoAcademy Region Guide" document.
    /// </summary>
    public static class JapanRegions
    {
        private static readonly Dictionary<string, Region> _all = new()
        {
            ["hokkaido"] = new Region {
                Slug = "hokkaido",
                Name = "Hokkaido", NameJa = "北海道",
                Subtitle = "北の楽園",
                Capital = "Sapporo", CapitalJa = "札幌",
                Area = "83,424 km²",
                Climate = "Cool summers, heavy snowfall in winter",
                BestSeason = "Jul–Aug (lavender) · Feb (snow festival) · Sep–Oct (autumn foliage)",
                WinterMonths = "11月–3月", WinterSub = "November to March",
                WinterNote = "Winter is the whole point here: deep snow, hot springs and snow festivals. "
                            + "Mornings sit well below freezing, so layers and proper boots matter more than souvenirs.",
                SummerMonths = "7月–8月", SummerSub = "July and August",
                SummerNote = "Lavender, corn and melons, with daylight that lasts until nine at night. "
                            + "The interior stays a few degrees cooler than Tokyo even at the height of summer.",
                Tagline = "Where Japan turns wild: caldera lakes, brown bears, and snow deep enough to swallow a city.",
                Description = "Japan's northernmost main island feels like a different country — vast, sparsely "
                            + "populated, and shaped by volcanoes and snow rather than temples and shrines. "
                            + "Sapporo, the capital, blends grid-planned streets with a beer-garden and ramen "
                            + "culture, while just beyond the city limits lie caldera lakes, drift ice, and some "
                            + "of the best skiing in the world. Hokkaido was only settled by mainland Japanese in "
                            + "the 19th century, and the indigenous Ainu people's history and culture still shape "
                            + "the island's place names and identity today.",
                OverviewHighlights = new() {
                    "northernmost main island", "vast, sparsely populated", "volcanoes and snow", "Sapporo",
                    "grid-planned streets", "beer-garden and ramen culture", "caldera lakes", "drift ice",
                    "best skiing in the world", "19th century", "Ainu people's history and culture", "place names and identity"
                },
                // Photos live in wwwroot/Japance Image — filenames are mirrored here
                // verbatim (spaces, "&" and macrons included) and encoded at render time.
                Highlights = new() {
                    new() { Title = "Sapporo Snow Festival", TitleJa = "さっぽろ雪まつり", Note = "Even outside festival season, a long green corridor through the city center worth a walk.", Image = "Hokkaido-Must See Highlights-Sapporo Snow Festival.png" },
                    new() { Title = "Daisetsuzan National Park", TitleJa = "大雪山国立公園", Note = "Hokkaido's largest national park, nicknamed the \"roof of Hokkaido\"; volcanic peaks, alpine flowers, and Japan's earliest autumn foliage.", Image = "Hokkaido-Must See Highlights-Daisetsuzan National Park.png" },
                    new() { Title = "Noboribetsu Onsen", TitleJa = "登別温泉", Note = "One of Japan's most famous hot spring towns, centered on the sulfurous, steaming \"Hell Valley\" (Jigokudani).", Image = "Hokkaido-Must see Highlights-Noboribetsu Onsen.png" },
                    new() { Title = "Shiretoko Peninsula", TitleJa = "知床半島", Note = "UNESCO World Heritage wilderness where brown bears roam freely and drift ice covers the sea each winter.", Image = "Hokkaido-Must see Highlights-Shiretoko Peninsula.png" },
                    new() { Title = "Otaru Canal", TitleJa = "小樽運河", Note = "A gas-lit historic canal in a former herring-trading port, lined with glassblowing shops and old warehouses.", Image = "Hokkaido-Must see Highlights-Otaru canal.png" },
                },
                Foods = new() {
                    new() { Title = "Sapporo Miso Ramen", TitleJa = "札幌味噌ラーメン", Note = "The birthplace of miso-broth ramen; rich, buttery, often topped with corn and a knob of butter to fight the cold.", Image = "Hokkaido-Regional cuisine-Sapporo Miso Ramen-small.png" },
                    new() { Title = "Soup Curry", TitleJa = "スープカレー", Note = "A Sapporo invention: thin, spiced broth over rice, closer to a spiced consommé than Indian or mainland Japanese curry.", Image = "Hokkaido-Regional cuisine-Soup Curry-small.png" },
                    new() { Title = "Hokkaido Crab", TitleJa = "北海道産カニ", Note = "King, snow, and hairy crab, often eaten simply grilled or in a hot pot (kani-nabe).", Image = "Hokkaido-Regional cuisine-Hokkaido Crab-small.png" },
                    new() { Title = "Yubari Melon", TitleJa = "夕張メロン", Note = "A cantaloupe so prized it's sold as a luxury gift; pairs of melons have auctioned for over ¥1 million.", Image = "Hokkaido-Regional cuisine-Yubari Melon-small.png" },
                    new() { Title = "Genghis Khan (Jingisukan)", TitleJa = "ジンギスカン", Note = "Grilled lamb/mutton cooked at the table on a dome-shaped skillet, tied to the island's sheep-farming history.", Image = "Hokkaido-Regional cuisine-Genghis Khan (Jingisukan)-small.png" },
                    new() { Title = "Sapporo Beer", TitleJa = "サッポロビール", Note = "Japan's oldest beer brand, brewed in Hokkaido since 1876; the old brewery is now a museum and beer hall.", Image = "Hokkaido-Regional cuisine-Sapporo Beer-small.png" },
                },
                Festivals = new() {
                    new() { Title = "Sapporo Snow Festival", TitleJa = "さっぽろ雪まつり", When = "Early February", Note = "Massive snow and ice sculptures fill Odori Park; one of Japan's most-visited winter events", Image = "Hokkaido-Festivals & events-Sapporo Snow Festival.png" },
                    new() { Title = "Yosakoi Sōran", TitleJa = "YOSAKOIソーラン", When = "Early–mid June", Note = "High-energy dance festival blending Yosakoi choreography with Hokkaido's Sōran Bushi fishing song", Image = "Hokkaido-Festivals & events-Yosakoi Sōran.png" },
                    new() { Title = "Otaru Snow Light Path", TitleJa = "小樽雪あかりの路", When = "Mid-February", Note = "Canal-side candles and snow lanterns in the historic port town of Otaru", Image = "Hokkaido-Festivals & events-Otaru Snow Light Path.png" },
                    new() { Title = "Sapporo Autumn Fest", TitleJa = "さっぽろオータムフェスト", When = "September", Note = "Month-long food festival showcasing Hokkaido produce, seafood, and dairy in Odori Park", Image = "Hokkaido-Festivals & events-Sapporo Autumn Fest.png" },
                },
                // Text-only trivia — the one part of the page that is not a photo.
                // The two "Words" cards double as a little Hokkaido-ben lesson.
                FunFacts = new() {
                    new() {
                        Icon = "bi-apple", Tag = "Food", Stat = "¥1M+",
                        Label = "夕張メロン", Reading = "Yūbari meron",
                        Title = "Hokkaido's most expensive fruit",
                        Note = "A cantaloupe from a small town south of Sapporo, graded by size and sugar like jewellery and sold as a luxury gift. Pairs of melons have auctioned for over ¥1 million.",
                        Say = "夕張メロン"
                    },
                    new() {
                        Icon = "bi-snow", Tag = "Winter", Stat = "Feb–Mar",
                        Label = "流氷", Reading = "Ryūhyō (drift ice)",
                        Title = "You can walk on the sea",
                        Note = "Every winter the Okhotsk Sea pushes floes south to Shiretoko and Abashiri. For a few weeks the ice drifts up the shore and local guides take people out onto the floes.",
                        Say = "流氷"
                    },
                    new() {
                        Icon = "bi-translate", Tag = "Words",
                        Label = "ばり", Reading = "bari",
                        Title = "Hokkaido-ben's very own \"very\"",
                        Note = "Hokkaido speaks its own flavour of Japanese. ばり means very — ばりおいしい is \"really delicious\". In Sapporo you will also hear なまら, and out in the countryside an old うんまい for おいしい.",
                        Say = "ばりおいしい"
                    },
                    new() {
                        Icon = "bi-tree", Tag = "Animals", Stat = "1,000 kg+",
                        Label = "ヒグマ", Reading = "Higuma (brown bear)",
                        Title = "Bears large enough to be an attraction",
                        Note = "Hokkaido's brown bears are the biggest in Japan — over a metre at the shoulder. Bear parks outside Sapporo and Noboribetsu let you watch them bathe and rear up on two legs; in the wild they roam freely around Shiretoko.",
                        Say = "ヒグマ"
                    },
                    new() {
                        Icon = "bi-cup-straw", Tag = "Food", Stat = "1st in Japan",
                        Label = "ソフトクリーム", Reading = "Sofuto kurīmu",
                        Title = "The milk road is everywhere",
                        Note = "Hokkaido leads Japan in milk, butter and cheese, so roadside creameries sell a cone for almost nothing. The flavour changes with the season — melon in summer, pumpkin in autumn, cocoa in winter.",
                        Say = "ソフトクリーム"
                    },
                    new() {
                        Icon = "bi-droplet", Tag = "Seasons", Stat = "May",
                        Label = "雪解け", Reading = "Yukidoke (the thaw)",
                        Title = "The snow takes its time leaving",
                        Note = "Inland towns can hold four metres of snow until well into May. The meltwater feeds the rivers and lakes that fill Hokkaido's calderas — the same snow that starts as a blizzard ends up as the island's clear spring water.",
                        Say = "雪解け"
                    },
                },
                Access = "New Chitose Airport (CTS), about 45 minutes south of Sapporo, is Hokkaido's main gateway, "
                       + "with direct flights from Tokyo, Osaka, and several international hubs. From Tokyo, flying is "
                       + "faster and often cheaper than the train — the Hokkaido Shinkansen currently only reaches "
                       + "Hakodate (at the island's southern tip, ~4 hours from Tokyo); an extension to Sapporo "
                       + "is under construction but not "
                       + "expected before the early 2030s. Once in Hokkaido, distances are large and public transit "
                       + "thins out fast outside Sapporo, so renting a car is common for exploring places like "
                       + "Shiretoko or Daisetsuzan National Park.",
                AccessHighlights = new() {
                    "New Chitose Airport (CTS)", "45 minutes south of Sapporo", "main gateway", "Tokyo, Osaka, and several international hubs",
                    "flying is faster and often cheaper than the train", "Hokkaido Shinkansen currently only reaches Hakodate", "under construction but not expected before the early 2030s", "distances are large and public transit thins out fast outside Sapporo",
                    "renting a car is common", "Shiretoko", "Daisetsuzan National Park"
                },
                Accent = "na-primary",
                Center = (43.06, 141.35),
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Hokkaido",
                HeroImage = "/images/Japan%20Region/Hokkaido.png",
                MapX = 75, MapY = 17,
            },

            ["tohoku"] = new Region {
                Slug = "tohoku",
                Name = "Tōhoku", NameJa = "東北",
                Subtitle = "山と祭りの里",
                Capital = "Sendai", CapitalJa = "仙台",
                Area = "66,952 km²",
                Climate = "Cool winters with heavy snow · mild summers · cherry blossoms late April",
                BestSeason = "Apr–May (cherry blossoms) · Aug (Nebuta festival)",
                WinterMonths = "12月–3月", WinterSub = "December to March",
                WinterNote = "Some of the heaviest snowfall on earth buries the mountains here — Zao's juhyō "
                            + "\"snow monsters\" and steaming onsen towns are worth the cold. Pack for it.",
                SummerMonths = "7月–8月", SummerSub = "July and August",
                SummerNote = "Festival season: Nebuta, Tanabata and Kantō light up the short northern summer. "
                            + "Nights cool off fast in the mountains, so bring a layer for the evening events.",
                Tagline = "Japan's rugged north — samurai strongholds, blazing summer festivals, and a coastline still rebuilding after tragedy.",
                Description = "Tōhoku is Japan's less-traveled north — six prefectures of rice paddies, apple "
                            + "orchards, and mountain ranges that isolated the region for centuries and gave it a "
                            + "strong, independent identity. Sendai, the largest city, is known as the \"City of "
                            + "Trees\" for its tree-lined boulevards. Summers bring some of Japan's most "
                            + "spectacular festivals, from Aomori's Nebuta floats to Akita's Kantō festival, while "
                            + "winters blanket the region in snow that feeds its many onsen towns. The 2011 "
                            + "earthquake and tsunami devastated much of the Pacific coast, and the region's "
                            + "ongoing recovery is part of its story today — coastal towns like Ishinomaki and "
                            + "Rikuzentakata welcome visitors interested in both memorial sites and renewal.",
                OverviewHighlights = new() {
                    "six prefectures", "Sendai", "\"City of Trees\"", "Aomori's Nebuta floats",
                    "Akita's Kantō festival", "2011 earthquake and tsunami", "Ishinomaki", "Rikuzentakata"
                },
                Highlights = new() {
                    new() { Title = "Matsushima Bay", TitleJa = "松島湾", Note = "One of Japan's \"Three Great Views,\" a bay scattered with over 200 pine-covered islets.",
                            Image = "Tōhoku-Must-see highlights-Matsushima Bay.png" },
                    new() { Title = "Chūson-ji Temple", TitleJa = "中尊寺", Note = "UNESCO World Heritage temple complex, home to the gold-leafed Konjikidō hall.",
                            Image = "Tōhoku-Must-see highlights-Chūson-ji Temple.png" },
                    new() { Title = "Ginzan Onsen", TitleJa = "銀山温泉", Note = "Snowbound hot spring town of gas-lamp-lit wooden ryokan.",
                            Image = "Tōhoku-Must-see highlights-Ginzan Onsen.png" },
                    new() { Title = "Zaō Onsen & Snow Monsters", TitleJa = "蔵王樹氷", Note = "Frost-covered \"snow monster\" trees, best seen by ropeway in winter.",
                            Image = "Tōhoku-Must-see highlights-Zaō Onsen & Snow Monsters.png" },
                    new() { Title = "Towada-Hachimantai National Park", TitleJa = "十和田八幡平国立公園", Note = "Volcanic caldera lakes and hot springs straddling Aomori, Akita, and Iwate.",
                            Image = "Tōhoku-Must-see highlights-Towada-Hachimantai National Park.png" },
                },
                Foods = new() {
                    new() { Title = "Gyūtan (Sendai Beef Tongue)", TitleJa = "牛タン", Note = "Grilled, thinly sliced ox tongue; Sendai's signature dish, usually served with barley rice and oxtail soup.",
                            Image = "Tōhoku-Regional cuisine-Gyūtan (Sendai Beef Tongue).png" },
                    new() { Title = "Wanko Soba", TitleJa = "わんこそば", Note = "Iwate's bottomless soba tradition, where servers keep refilling your bowl until you cover it to signal you're done.",
                            Image = "Tōhoku-Regional cuisine-Wanko Soba.png" },
                    new() { Title = "Kiritanpo", TitleJa = "きりたんぽ", Note = "Akita specialty of mashed rice formed around a skewer and grilled, then simmered in a hot pot with chicken and vegetables.",
                            Image = "Tōhoku-Regional cuisine-Kiritanpo.png" },
                    new() { Title = "Sasa Kamaboko", TitleJa = "笹かまぼこ", Note = "Leaf-shaped grilled fish cake, a Sendai souvenir staple.",
                            Image = "Tōhoku-Regional cuisine-Sasa Kamaboko.png" },
                    new() { Title = "Aomori Apples", TitleJa = "青森りんご", Note = "Japan's top apple-producing prefecture, prized for their size and sweetness.",
                            Image = "Tōhoku-Regional cuisine-Aomori Apples.png" },
                    new() { Title = "Zunda Mochi", TitleJa = "ずんだ餅", Note = "Mochi covered in a sweet mashed edamame paste, another Sendai specialty.",
                            Image = "Tōhoku-Regional cuisine-Zunda Mochi.png" },
                },
                Festivals = new() {
                    new() { Title = "Nebuta Matsuri", TitleJa = "ねぶた祭", Note = "Early August — Aomori's iconic festival: giant illuminated warrior floats paraded through the streets",
                            Image = "Tōhoku-Festivals & events-Nebuta Matsuri.png" },
                    new() { Title = "Akita Kantō Matsuri", TitleJa = "秋田竿燈まつり", Note = "Early August — performers balance long poles hung with dozens of lit lanterns on their palms, shoulders, and foreheads",
                            Image = "Tōhoku-Festivals & events-Akita Kantō Matsuri.png" },
                    new() { Title = "Sendai Tanabata", TitleJa = "仙台七夕まつり", Note = "Early August — star festival famous for enormous, elaborate paper streamers hung throughout the city",
                            Image = "Tōhoku-Festivals & events-Sendai Tanabata.png" },
                    new() { Title = "Hanagasa Matsuri", TitleJa = "山形花笠まつり", Note = "Early August — Yamagata dancers in flower-decorated straw hats parade through the streets",
                            Image = "Tōhoku-Festivals & events-Hanagasa Matsuri.png" },
                },
                Access = "Sendai is Tōhoku's main hub, roughly 90 minutes from Tokyo on the Tōhoku Shinkansen — "
                       + "making it an easy day trip or gateway before heading further north. The Shinkansen "
                       + "continues on to Morioka, Akita, and Aomori, so most of the region is reachable without "
                       + "a car. That said, coastal areas and smaller onsen towns often have limited train "
                       + "service, and renting a car opens up far more of the region, especially in Iwate and "
                       + "Yamagata. Sendai Airport also has direct flights from other parts of Japan and a few "
                       + "international routes.",
                AccessHighlights = new() {
                    "Sendai", "90 minutes from Tokyo", "Tōhoku Shinkansen", "Morioka, Akita, and Aomori",
                    "Iwate", "Yamagata", "Sendai Airport"
                },
                Accent = "na-secondary",
                Center = (39.72, 140.40),
                LearnMoreUrl = "https://en.wikipedia.org/wiki/T%C5%8Dhoku_region",
                HeroImage = "/images/Japan%20Region/T%C3%B6hoku.png",
                MapX = 60, MapY = 35,
            },

            ["kanto"] = new Region {
                Slug = "kanto",
                Name = "Kantō", NameJa = "関東",
                Subtitle = "百万の都",
                Capital = "Tokyo", CapitalJa = "東京",
                Area = "32,423 km²",
                Climate = "Mild Pacific climate · hot humid summers · cool dry winters",
                BestSeason = "Mar–Apr (sakura) · Oct–Nov (autumn colours)",
                WinterMonths = "12月–2月", WinterSub = "December to February",
                WinterNote = "Crisp, dry and mostly sunny — Tokyo rarely drops below freezing. "
                            + "Cold clear air means the sharpest Mt. Fuji views of the year and citywide illuminations.",
                SummerMonths = "7月–8月", SummerSub = "July and August",
                SummerNote = "Hot and seriously humid — 35°C days are normal, so plan museums and cafés around the afternoon. "
                            + "Fireworks over the Sumida and cooler hills in Nikkō and Chichibu are the payoff.",
                Tagline = "Japan's beating heart: 40 million people, one shogun's old capital, and Mt. Fuji watching over it all.",
                Description = "Kantō is Japan's most populous region — home to Tokyo, Yokohama, and the old "
                            + "capital of Edo, now the beating heart of modern Japan. It's a mix of dense "
                            + "metropolis and quiet contrast: Kamakura's temples, Hakone's hot springs, and views "
                            + "of Mt. Fuji are all within day-trip distance of the world's largest urban area. "
                            + "Kantō is where centuries of history collide with neon skylines — the Tokugawa "
                            + "shogunate ruled from Edo Castle for over 250 years, and traces of that era survive "
                            + "in Nikkō's shrines and old castle towns even as the region drives Japan's economy, "
                            + "fashion, and pop culture today.",
                OverviewHighlights = new() {
                    "Kantō", "Tokyo, Yokohama", "Edo", "Kamakura's",
                    "Hakone's", "Mt. Fuji", "Tokugawa shogunate", "250 years",
                    "Nikkō's shrines"
                },
                Highlights = new() {
                    new() { Title = "Shibuya Crossing", TitleJa = "渋谷スクランブル交差点", Note = "The world's busiest pedestrian crossing, with up to 3,000 people crossing at once.",
                            Image = "Kantō-Must-see highlights-Shibuya Crossing.png" },
                    new() { Title = "Senso-ji Temple", TitleJa = "浅草寺", Note = "Tokyo's oldest temple, in the historic Asakusa district, fronted by the iconic Kaminarimon gate.",
                            Image = "Kantō-Must-see highlights-Senso-ji Temple.png" },
                    new() { Title = "Nikkō Tōshō-gū", TitleJa = "日光東照宮", Note = "UNESCO World Heritage shrine complex honoring shogun Tokugawa Ieyasu, deep in the mountains of Tochigi.",
                            Image = "Kantō-Must-see highlights-Nikkō Tōshō-gū.png" },
                    new() { Title = "Kamakura's Great Buddha", TitleJa = "鎌倉大仏", Note = "A 13-meter bronze Buddha statue dating to 1252, in the old seaside capital of Kamakura.",
                            Image = "Kantō-Must-see highlights-Kamakura's Great Buddha.png" },
                    new() { Title = "Hakone", TitleJa = "箱根", Note = "Hot spring town with views of Mt. Fuji across Lake Ashi and an open-air art museum.",
                            Image = "Kantō-Must-see highlights-Hakone.png" },
                },
                Foods = new() {
                    new() { Title = "Edomae Sushi", TitleJa = "江戸前寿司", Note = "The original style of sushi, born in Tokyo Bay; fish cured or marinated rather than served raw, atop hand-pressed rice.",
                            Image = "Kantō-Regional cuisine-Edomae Sushi.png" },
                    new() { Title = "Monjayaki", TitleJa = "もんじゃ焼き", Note = "A runny, savory batter dish grilled tableside, a Tokyo cousin of okonomiyaki centered in Tsukishima.",
                            Image = "Kantō-Regional cuisine-Monjayaki.png" },
                    new() { Title = "Fukagawa-meshi", TitleJa = "深川めし", Note = "A Tokyo fisherman's dish of rice topped or simmered with clams and miso broth.",
                            Image = "Kantō-Regional cuisine-Fukagawa-meshi.png" },
                    new() { Title = "Chanko Nabe", TitleJa = "ちゃんこ鍋", Note = "The protein-heavy hot pot eaten by sumo wrestlers, widely available near Tokyo's sumo stables in Ryōgoku.",
                            Image = "Kantō-Regional cuisine-Chanko Nabe.png" },
                    new() { Title = "Yokohama Chūka (Chinatown cuisine)", TitleJa = "横浜中華街", Note = "Japan's largest Chinatown, home to a distinct Japanese-Chinese fusion food culture.",
                            Image = "Kantō-Regional cuisine-Yokohama Chūka (Chinatown cuisine).png" },
                    new() { Title = "Ningyō-yaki", TitleJa = "人形焼き", Note = "Small cake-like sweets shaped like lanterns or figures, a classic Asakusa souvenir.",
                            Image = "Kantō-Regional cuisine-Ningyō-yaki.png" },
                },
                Festivals = new() {
                    new() { Title = "Sanja Matsuri", TitleJa = "三社祭", Note = "Mid-May — one of Tokyo's largest and wildest festivals, centered on Asakusa's Sensō-ji, with over 100 mikoshi (portable shrines)",
                            Image = "Kantō-Festivals & events-Sanja Matsuri.png" },
                    new() { Title = "Kanda Matsuri", TitleJa = "神田祭", Note = "Mid-May (odd years) — one of Tokyo's three great Shinto festivals, with a massive procession through central Tokyo",
                            Image = "Kantō-Festivals & events-Kanda Matsuri.png" },
                    new() { Title = "Sumidagawa Fireworks", TitleJa = "隅田川花火大会", Note = "Late July — one of Japan's oldest fireworks festivals, launched over the Sumida River since the Edo period",
                            Image = "Kantō-Festivals & events-Sumidagawa Fireworks.png" },
                    new() { Title = "Kamakura Fireworks Festival", TitleJa = "鎌倉花火大会", Note = "Late July / August — fireworks launched from boats off the coast, framed by Kamakura's beaches and temples",
                            Image = "Kantō-Festivals & events-Kamakura Fireworks Festival.png" },
                },
                Access = "Tokyo is Japan's main international gateway, served by Narita (NRT) and Haneda (HND) "
                       + "airports, with Haneda being closer to the city center at about 30 minutes from central "
                       + "Tokyo by train. The Shinkansen network radiates outward from Tokyo Station, making day "
                       + "trips to Hakone, Nikkō, Kamakura, and beyond extremely easy — most of Kantō is "
                       + "reachable by train without ever needing a car. Tokyo's own transit system, including "
                       + "the JR Yamanote Line and an extensive subway network, is dense enough that a car is "
                       + "more of a hindrance than a help within the city itself.",
                AccessHighlights = new() {
                    "Tokyo", "Narita (NRT)", "Haneda (HND)", "30 minutes",
                    "Shinkansen network", "Tokyo Station", "Hakone, Nikkō, Kamakura", "JR Yamanote Line"
                },
                Accent = "na-primary",
                Center = (35.69, 139.69),
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Kant%C5%8D_region",
                HeroImage = "/images/Japan%20Region/Kant%C5%8D.png",
                MapX = 63, MapY = 50,
            },

            ["chubu"] = new Region {
                Slug = "chubu",
                Name = "Chūbu", NameJa = "中部",
                Subtitle = "日本の屋根",
                Capital = "Nagoya", CapitalJa = "名古屋",
                Area = "72,572 km²",
                Climate = "Highly varied — alpine snow country in the north, mild Pacific climate in the south",
                BestSeason = "Mar–May · Jul–Aug for climbing Mt. Fuji · Oct–Nov (foliage)",
                WinterMonths = "12月–3月", WinterSub = "December to March",
                WinterNote = "The Sea of Japan side turns into real snow country — Shirakawa-gō under snow and the "
                            + "snow monkeys of Nagano are winter-only sights. The Pacific side around Nagoya stays mild.",
                SummerMonths = "7月–8月", SummerSub = "July and August",
                SummerNote = "Mt. Fuji's climbing season opens, and Kamikōchi's alpine valleys are at their greenest. "
                            + "Nagoya itself is muggy — head for the mountains, where 2,000 m of altitude does the cooling.",
                Tagline = "From snow monkeys to samurai castles: the mountains, coastlines, and forgotten valleys of central Japan.",
                Description = "Chūbu is Japan's largest and most geographically diverse region, stretching from "
                            + "the Sea of Japan coast to the Pacific, and anchored by Mt. Fuji and the Japanese "
                            + "Alps. It's nicknamed \"the roof of Japan\" for its mountainous spine, home to some "
                            + "of the country's best hiking, skiing, and hot springs. Nagoya, the region's largest "
                            + "city, is a manufacturing powerhouse — Toyota is headquartered nearby — while "
                            + "historic towns like Takayama and Kanazawa preserve Edo-period streets and "
                            + "samurai-era craft traditions largely untouched by wartime bombing. Chūbu is often "
                            + "overlooked by first-time visitors racing between Tokyo and Kyoto, but it holds some "
                            + "of Japan's most dramatic scenery and best-preserved traditional towns.",
                OverviewHighlights = new() {
                    "Chūbu", "Sea of Japan", "Pacific", "Mt. Fuji",
                    "Japanese Alps", "Nagoya", "Takayama", "Kanazawa"
                },
                Highlights = new() {
                    new() { Title = "Mt. Fuji", TitleJa = "富士山", Note = "Japan's tallest peak and most iconic symbol, straddling Shizuoka and Yamanashi prefectures.",
                            Image = "Chūbu-Must see Highlights-Mt. Fuji.png" },
                    new() { Title = "Shirakawa-go", TitleJa = "白川郷", Note = "UNESCO World Heritage village of steep thatched-roof farmhouses, especially striking under snow.",
                            Image = "Chūbu-Must see Highlights-Shirakawa-go.png" },
                    new() { Title = "Kanazawa's Kenroku-en", TitleJa = "兼六園", Note = "One of Japan's \"Three Great Gardens,\" alongside a beautifully preserved samurai and geisha district.",
                            Image = "Chūbu-Must see Highlights-Kanazawa's Kenroku-en.png" },
                    new() { Title = "Takayama Old Town", TitleJa = "高山の古い町並み", Note = "Edo-period merchant streets, sake breweries, and a morning market in the Japanese Alps.",
                            Image = "Chūbu-Must see Highlights-Takayama Old Town.png" },
                    new() { Title = "Jigokudani Monkey Park", TitleJa = "地獄谷野猿公苑", Note = "Wild snow monkeys bathing in natural hot springs, a globally famous winter sight.",
                            Image = "Chūbu-Must see Highlights-Jigokudani Monkey Park.png" },
                },
                Foods = new() {
                    new() { Title = "Hida Beef", TitleJa = "飛騨牛", Note = "A premium wagyu from the Takayama area, prized for its marbling and tenderness.",
                            Image = "Chūbu-Regional cuisine-Hida Beef.png" },
                    new() { Title = "Miso Katsu", TitleJa = "みそかつ", Note = "Nagoya's breaded pork cutlet served with a rich, sweet-savory red miso sauce.",
                            Image = "Chūbu-Regional cuisine-Miso Katsu.png" },
                    new() { Title = "Hōtō", TitleJa = "ほうとう", Note = "Yamanashi's thick, flat udon noodles simmered in a miso-based vegetable stew.",
                            Image = "Chūbu-Regional cuisine-Hōtō.png" },
                    new() { Title = "Toyama's Shiroebi (White Shrimp)", TitleJa = "白えび", Note = "Translucent, sweet shrimp caught almost exclusively in Toyama Bay, often served as sashimi or tempura.",
                            Image = "Chūbu-Regional cuisine-Toyama's Shiroebi (White Shrimp).png" },
                    new() { Title = "Sasa-zushi", TitleJa = "笹寿司", Note = "Nagano's sushi wrapped in bamboo leaves, a mountain-region preservation tradition.",
                            Image = "Chūbu-Regional cuisine-Sasa-zushi.png" },
                    new() { Title = "Gohei-mochi", TitleJa = "五平餅", Note = "Grilled, skewered rice cakes coated in a sweet nutty miso glaze, common in the Chūbu mountains.",
                            Image = "Chūbu-Regional cuisine-Gohei-mochi.png" },
                },
                Festivals = new() {
                    new() { Title = "Takayama Matsuri", TitleJa = "高山祭", Note = "Mid-April & mid-October — one of Japan's most beautiful float festivals, with elaborately carved and gilded yatai floats",
                            Image = "Chūbu-Festivals & events-Takayama Matsuri.png" },
                    new() { Title = "Nagoya Festival", TitleJa = "名古屋まつり", Note = "Mid-October — a parade celebrating three of Japan's great unifiers: Oda Nobunaga, Toyotomi Hideyoshi, and Tokugawa Ieyasu",
                            Image = "Chūbu-Festivals & events-Nagoya Festival.png" },
                    new() { Title = "Owara Kaze no Bon", TitleJa = "おわら風の盆", Note = "Early September — a haunting, lantern-lit dance festival in Toyama praying for protection from typhoon winds",
                            Image = "Chūbu-Festivals & events-Owara Kaze no Bon.png" },
                    new() { Title = "Shirakawa-go Light-Up", TitleJa = "白川郷ライトアップ", Note = "Late January–February (select nights) — snow-covered thatched-roof village illuminated at night, requires advance reservation",
                            Image = "Chūbu-Festivals & events-Shirakawa-go Light-Up.png" },
                },
                Access = "Nagoya is Chūbu's main hub, about 100 minutes from Tokyo and 35 minutes from Osaka on "
                       + "the Tōkaidō Shinkansen, making it a convenient midpoint stop between the two. Chubu "
                       + "Centrair International Airport (NGO) offers direct international flights. Reaching "
                       + "mountain towns like Takayama and Shirakawa-go usually requires a limited express train "
                       + "plus a bus, and having a car opens much more of the region's rural scenery — though "
                       + "winter driving in the Alps can be demanding. Matsumoto and Nagano are also "
                       + "well-connected by rail for those heading toward the Japanese Alps.",
                AccessHighlights = new() {
                    "Nagoya", "100 minutes from Tokyo", "35 minutes from Osaka", "Tōkaidō Shinkansen",
                    "Chubu Centrair International Airport (NGO)", "Takayama", "Shirakawa-go", "Matsumoto",
                    "Nagano"
                },
                Accent = "na-secondary",
                Center = (36.07, 137.85),
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Ch%C5%ABbu_region",
                HeroImage = "/images/Japan%20Region/Ch%C5%ABbu.png",
                MapX = 52, MapY = 53,
            },

            ["kansai"] = new Region {
                Slug = "kansai",
                Name = "Kansai", NameJa = "関西",
                Subtitle = "千年の都",
                Capital = "Osaka", CapitalJa = "大阪",
                Area = "27,361 km²",
                Climate = "Mild Pacific climate · humid summers · mild winters",
                BestSeason = "Mar–Apr (sakura) · Nov (autumn temples)",
                WinterMonths = "12月–2月", WinterSub = "December to February",
                WinterNote = "Short, dry and cold but rarely harsh — and the quietest season at Kyoto's temples. "
                            + "A rare dusting of snow on Kinkaku-ji draws queues before the gates even open.",
                SummerMonths = "6月–8月", SummerSub = "June to August",
                SummerNote = "Steamy, but this is festival heartland: Gion Matsuri fills Kyoto through July, and five "
                            + "bonfires stamp out the Gozan no Okuribi on August 16. Shaded river kawadoko dining is the local escape.",
                Tagline = "A thousand years as Japan's capital — temples, geisha alleys, and the country's best street food.",
                Description = "Kansai (also called Kinki) was the political and cultural center of Japan for over "
                            + "a millennium, and it still feels like the country's spiritual heart. Kyoto, the "
                            + "imperial capital for over 1,000 years until 1868, holds thousands of temples and "
                            + "shrines and one of the world's best-preserved geisha districts, Gion. Osaka, by "
                            + "contrast, is brash, food-obsessed, and famously the birthplace of takoyaki and "
                            + "okonomiyaki — its unofficial motto, kuidaore (\"eat until you drop\"), says it all. "
                            + "Nara, Japan's capital before Kyoto, is home to free-roaming sacred deer and some of "
                            + "the country's oldest wooden buildings. Together the region packs classical Japan — "
                            + "temples, castles, and centuries of court culture — into a compact, easy-to-travel area.",
                OverviewHighlights = new() {
                    "Kansai", "Kinki", "Kyoto", "over 1,000 years",
                    "Gion", "Osaka", "Nara"
                },
                Highlights = new() {
                    new() { Title = "Fushimi Inari Taisha", TitleJa = "伏見稲荷大社", Note = "Kyoto's iconic shrine, famous for its thousands of vermilion torii gates climbing the mountainside.",
                            Image = "Kansai-Must-see highlights-Fushimi Inari Taisha.png" },
                    new() { Title = "Kinkaku-ji (Golden Pavilion)", TitleJa = "金閣寺", Note = "A Zen temple covered in gold leaf, reflected in its surrounding pond.",
                            Image = "Kansai-Must-see highlights-Kinkaku-ji (Golden Pavilion).png" },
                    new() { Title = "Nara Park", TitleJa = "奈良公園", Note = "Over 1,000 wild, sacred deer roam freely among the park's temples and shrines.",
                            Image = "Kansai-Must-see highlights-Nara Park.png" },
                    new() { Title = "Osaka Castle", TitleJa = "大阪城", Note = "A reconstructed 16th-century castle central to Toyotomi Hideyoshi's unification of Japan.",
                            Image = "Kansai-Must-see highlights-Osaka Castle.png" },
                    new() { Title = "Dōtonbori", TitleJa = "道頓堀", Note = "Osaka's neon-lit canal-side food and entertainment district, home to the giant Glico running man sign.",
                            Image = "Kansai-Must-see highlights-Dōtonbori.png" },
                },
                Foods = new() {
                    new() { Title = "Takoyaki", TitleJa = "たこ焼き", Note = "Osaka's octopus-filled batter balls, grilled crisp outside and molten inside, topped with sauce and bonito flakes.",
                            Image = "Kansai-Regional cuisine-Takoyaki.png" },
                    new() { Title = "Okonomiyaki", TitleJa = "お好み焼き", Note = "A savory grilled pancake of batter, cabbage, and toppings, associated with both Osaka and Hiroshima styles.",
                            Image = "Kansai-Regional cuisine-Okonomiyaki.png" },
                    new() { Title = "Kyoto Kaiseki", TitleJa = "京懐石", Note = "Kyoto's refined multi-course haute cuisine, rooted in tea-ceremony traditions and seasonal ingredients.",
                            Image = "Kansai-Regional cuisine-Kyoto Kaiseki.png" },
                    new() { Title = "Yudofu", TitleJa = "湯豆腐", Note = "Simmered tofu hot pot, a Kyoto specialty tied to the city's temple and Buddhist vegetarian cooking traditions.",
                            Image = "Kansai-Regional cuisine-Yudofu.png" },
                    new() { Title = "Nara-zuke", TitleJa = "奈良漬け", Note = "Vegetables pickled in sake lees, a Nara specialty dating back over a thousand years.",
                            Image = "Kansai-Regional cuisine-Nara-zuke.png" },
                    new() { Title = "Kushikatsu", TitleJa = "串カツ", Note = "Osaka's breaded, deep-fried skewers, traditionally eaten with a strict \"no double-dipping\" sauce rule.",
                            Image = "Kansai-Regional cuisine-Kushikatsu.png" },
                },
                Festivals = new() {
                    new() { Title = "Gion Matsuri", TitleJa = "祇園祭", Note = "Throughout July — one of Japan's most famous festivals, centered on Kyoto's Yasaka Shrine, with massive wooden float processions",
                            Image = "Kansai-Festivals & events-Gion Matsuri.png" },
                    new() { Title = "Tenjin Matsuri", TitleJa = "天神祭", Note = "Late July — one of Japan's three great festivals, held in Osaka with a river procession and huge fireworks display",
                            Image = "Kansai-Festivals & events-Tenjin Matsuri.png" },
                    new() { Title = "Jidai Matsuri", TitleJa = "時代祭", Note = "October 22 — Kyoto's \"Festival of Ages,\" a costumed procession spanning 1,000+ years of Japanese history",
                            Image = "Kansai-Festivals & events-Jidai Matsuri.png" },
                    new() { Title = "Nara Lantern Festival", TitleJa = "なら燈花会", Note = "Early–mid August — roughly 20,000 candles lit throughout Nara Park and its temples in the evening",
                            Image = "Kansai-Festivals & events-Nara Lantern Festival.png" },
                },
                Access = "Osaka and Kyoto are Kansai's main hubs, connected to Tokyo by the Tōkaidō Shinkansen in "
                       + "about 2.5 hours, and to each other in under 15 minutes. Kansai International Airport "
                       + "(KIX) is the region's main international gateway, located on an artificial island in "
                       + "Osaka Bay. Kyoto, Osaka, and Nara are close enough together (all within roughly an hour "
                       + "of each other by train) that most visitors base themselves in one city and day-trip to "
                       + "the others without needing a car — Kansai has one of the most convenient rail networks "
                       + "in Japan for this kind of regional hopping.",
                AccessHighlights = new() {
                    "Osaka", "Kyoto", "Tōkaidō Shinkansen", "2.5 hours",
                    "15 minutes", "Kansai International Airport (KIX)"
                },
                Accent = "na-primary",
                Center = (34.69, 135.50),
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Kansai_region",
                HeroImage = "/images/Japan%20Region/Kansai.png",
                MapX = 43, MapY = 60,
            },

            ["chugoku"] = new Region {
                Slug = "chugoku",
                Name = "Chūgoku", NameJa = "中国",
                Subtitle = "山陰山陽",
                Capital = "Hiroshima", CapitalJa = "広島",
                Area = "31,921 km²",
                Climate = "Mild Seto-Inland climate · Sea-of-Japan side snowy in winter · Pacific side dry",
                BestSeason = "Apr (sakura at Miyajima) · Oct–Nov (foliage)",
                WinterMonths = "12月–2月", WinterSub = "December to February",
                WinterNote = "Split in two: the San'in coast gets real snow and Daisen turns into a ski mountain, while "
                            + "the Seto side stays mild and sunny. Miyajima is blissfully quiet outside New Year.",
                SummerMonths = "7月–8月", SummerSub = "July and August",
                SummerNote = "The calm Inland Sea is made for island-hopping and evening fireworks over Hiroshima's river deltas. "
                            + "Days are hot and sticky, but a sea breeze off the Seto softens the coast.",
                Tagline = "Peace memorials, sacred torii gates, and a mythological coastline where Japan's gods first walked.",
                Description = "Chūgoku occupies Japan's western tip, split between a rugged San'in coast facing "
                            + "the Sea of Japan and a milder San'yō coast facing the Inland Sea — the region's "
                            + "alternate name, \"Sanin-Sanyo,\" reflects this divide. Hiroshima, the region's "
                            + "largest city, was devastated by the atomic bomb in 1945 and rebuilt as a global "
                            + "center for peace education, anchored by the Peace Memorial Park. Just offshore, "
                            + "Miyajima Island holds the \"floating\" torii gate of Itsukushima Shrine, one of "
                            + "Japan's most photographed sights. Inland and along the San'in coast, Chūgoku is "
                            + "steeped in mythology — Izumo Taisha, one of Japan's oldest and most important "
                            + "shrines, is said to be where Japan's gods gather each autumn.",
                OverviewHighlights = new() {
                    "Chūgoku", "San'in coast", "San'yō coast", "Hiroshima",
                    "1945", "Peace Memorial Park", "Miyajima Island", "Izumo Taisha"
                },
                Highlights = new() {
                    new() { Title = "Hiroshima Peace Memorial Park", TitleJa = "広島平和記念公園", Note = "Home to the Atomic Bomb Dome (UNESCO World Heritage) and museum, dedicated to the 1945 bombing and world peace.",
                            Image = "Chūgoku-Must-see highlights-Hiroshima Peace Memorial Park.png" },
                    new() { Title = "Itsukushima Shrine (Miyajima)", TitleJa = "厳島神社", Note = "A shrine with a torii gate that appears to float at high tide, one of Japan's most iconic views.",
                            Image = "Chūgoku-Must-see highlights-Itsukushima Shrine (Miyajima).png" },
                    new() { Title = "Izumo Taisha", TitleJa = "出雲大社", Note = "One of Japan's oldest and most sacred Shinto shrines, associated with the god of marriage and relationships.",
                            Image = "Chūgoku-Must-see highlights-Izumo Taisha.png" },
                    new() { Title = "Kurashiki Bikan Historical Quarter", TitleJa = "倉敷美観地区", Note = "A canal-lined Edo-period merchant district with white-walled warehouses turned shops and cafes.",
                            Image = "Chūgoku-Must-see highlights-Kurashiki Bikan Historical Quarter.png" },
                    new() { Title = "Tottori Sand Dunes", TitleJa = "鳥取砂丘", Note = "Japan's only large-scale desert-like dune system, stretching along the San'in coast.",
                            Image = "Chūgoku-Must-see highlights-Tottori Sand Dunes.png" },
                },
                Foods = new() {
                    new() { Title = "Hiroshima-style Okonomiyaki", TitleJa = "広島風お好み焼き", Note = "Layered rather than mixed, with noodles and cabbage stacked between batter and egg, distinct from the Osaka style.",
                            Image = "Chūgoku-Regional cuisine-Hiroshima-style Okonomiyaki.png" },
                    new() { Title = "Izumo Soba", TitleJa = "出雲そば", Note = "Dark, nutty buckwheat noodles served in stacked lacquered trays, a San'in specialty.",
                            Image = "Chūgoku-Regional cuisine-Izumo Soba.png" },
                    new() { Title = "Kaki (Oysters)", TitleJa = "牡蠣", Note = "Hiroshima Prefecture produces roughly half of Japan's oysters, grilled, fried, or raw.",
                            Image = "Chūgoku-Regional cuisine-Kaki (Oysters).png" },
                    new() { Title = "Momiji Manju", TitleJa = "もみじ饅頭", Note = "Maple-leaf-shaped cakes filled with sweet bean paste, a Miyajima souvenir staple.",
                            Image = "Chūgoku-Regional cuisine-Momiji Manju.png" },
                    new() { Title = "Fugu (Pufferfish)", TitleJa = "ふぐ", Note = "Shimonoseki, at Chūgoku's western tip, is Japan's top fugu market and a center for the delicacy.",
                            Image = "Chūgoku-Regional cuisine-Fugu (Pufferfish).png" },
                    new() { Title = "Okayama's Kibi Dango", TitleJa = "きび団子", Note = "Millet dumplings tied to the local legend of Momotarō (Peach Boy).",
                            Image = "Chūgoku-Regional cuisine-Okayama's Kibi Dango.png" },
                },
                Festivals = new() {
                    new() { Title = "Peace Memorial Ceremony", TitleJa = "平和記念式典", Note = "August 6 — Hiroshima's annual ceremony commemorating the atomic bombing, with lanterns floated down the river at dusk",
                            Image = "Chūgoku-Festivals & events-Peace Memorial Ceremony.png" },
                    new() { Title = "Kurayoshi Utsubuki Matsuri", TitleJa = "打吹まつり", Note = "Early August — a lively summer dance festival in the Tottori castle town of Kurayoshi",
                            Image = "Chūgoku-Festivals & events-Kurayoshi Utsubuki Matsuri.png" },
                    new() { Title = "Izumo Taisha Kamiari Festival", TitleJa = "神在祭", Note = "November (lunar calendar) — marks the month when Shinto gods are believed to gather at Izumo Taisha from across Japan",
                            Image = "Chūgoku-Festivals & events-Izumo Taisha Kamiari Festival.png" },
                    new() { Title = "Miyajima Water Fireworks", TitleJa = "みやじま水中花火大会", Note = "Late August — fireworks launched over the sea near the floating torii gate",
                            Image = "Chūgoku-Festivals & events-Miyajima Water Fireworks.png" },
                },
                Access = "Hiroshima is Chūgoku's main hub, about 4 hours from Tokyo and under 1.5 hours from "
                       + "Osaka on the San'yō Shinkansen. From Hiroshima, Miyajima is a short train-and-ferry "
                       + "combination, while the San'in coast (Izumo, Tottori, Matsue) is less connected by rail "
                       + "and often easier to explore by car or limited express train. Hiroshima Airport offers "
                       + "some domestic and limited international flights, though most travelers arrive via Osaka "
                       + "or Tokyo and continue by Shinkansen.",
                AccessHighlights = new() {
                    "Hiroshima", "4 hours from Tokyo", "1.5 hours from Osaka", "San'yō Shinkansen",
                    "Miyajima", "Hiroshima Airport"
                },
                Accent = "na-gold",
                Center = (34.85, 133.55),
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Ch%C5%ABgoku_region",
                HeroImage = "/images/Japan%20Region/Ch%C5%ABgoku.png",
                MapX = 30, MapY = 65,
            },

            ["shikoku"] = new Region {
                Slug = "shikoku",
                Name = "Shikoku", NameJa = "四国",
                Subtitle = "巡礼の道",
                Capital = "Matsuyama", CapitalJa = "松山",
                Area = "18,800 km²",
                Climate = "Warm Pacific & Seto-Inland climate · mild winters · typhoon-prone autumns",
                BestSeason = "Apr · Oct–Nov · Aug (Awa Odori)",
                WinterMonths = "12月–2月", WinterSub = "December to February",
                WinterNote = "The mildest winters of the four main islands — bright citrus groves and an empty 88-temple "
                            + "pilgrimage trail. Winter sun makes the Shimanto gorges glass-clear.",
                SummerMonths = "7月–8月", SummerSub = "July and August",
                SummerNote = "Awa Odori takes over Tokushima in mid-August, and the Shimanto and Yoshino rivers are whitewater season. "
                            + "Keep an eye on typhoon forecasts before booking island ferries.",
                Tagline = "Japan's smallest main island, and its quietest — sacred trails, citrus groves, and turquoise valleys.",
                Description = "Shikoku is the smallest of Japan's four main islands and one of its least-visited, "
                            + "best known for the 88 Temple Pilgrimage (Ohenro), a roughly 1,200-kilometer loop "
                            + "of sacred sites tracing the footsteps of the Buddhist monk Kūkai. The island is "
                            + "defined by dramatic natural scenery — the swirling tidal whirlpools of the Naruto "
                            + "Strait, the impossibly clear turquoise waters of the Iya Valley, and vine bridges "
                            + "built by fleeing samurai centuries ago. Shikoku's four prefectures — Tokushima, "
                            + "Kagawa, Ehime, and Kōchi — each have a distinct identity, from Kagawa's udon "
                            + "obsession to Kōchi's rowdy, sake-heavy festival culture. With no Shinkansen access, "
                            + "Shikoku remains one of the more untouched corners of Japan.",
                OverviewHighlights = new() {
                    "Shikoku", "88 Temple Pilgrimage", "1,200-kilometer", "Kūkai",
                    "Naruto Strait", "Iya Valley", "Tokushima, Kagawa, Ehime, and Kōchi"
                },
                Highlights = new() {
                    new() { Title = "Iya Valley Vine Bridges", TitleJa = "祖谷のかずら橋", Note = "Hand-woven vine suspension bridges deep in a remote mountain gorge, once used by escaping samurai.",
                            Image = "Shikoku-Must-see highlights-Iya Valley Vine Bridges.png" },
                    new() { Title = "Naruto Whirlpools", TitleJa = "鳴門の渦潮", Note = "Powerful tidal whirlpools in the Naruto Strait, best viewed from a boat or the Onaruto Bridge.",
                            Image = "Shikoku-Must-see highlights-Naruto Whirlpools.png" },
                    new() { Title = "88 Temple Pilgrimage", TitleJa = "四国八十八箇所", Note = "A centuries-old Buddhist pilgrimage circling the entire island, still walked by thousands each year.",
                            Image = "Shikoku-Must-see highlights-88 Temple Pilgrimage.png" },
                    new() { Title = "Dōgo Onsen", TitleJa = "道後温泉", Note = "One of Japan's oldest hot springs, said to have inspired the bathhouse in Spirited Away.",
                            Image = "Shikoku-Must-see highlights-Dōgo Onsen.png" },
                    new() { Title = "Ritsurin Garden", TitleJa = "栗林公園", Note = "A sprawling Edo-period landscape garden in Takamatsu, considered one of Japan's finest.",
                            Image = "Shikoku-Must-see highlights-Ritsurin Garden.png" },
                },
                Foods = new() {
                    new() { Title = "Sanuki Udon", TitleJa = "讃岐うどん", Note = "Kagawa's famously firm, chewy udon noodles; the prefecture has more udon shops per capita than anywhere else in Japan.",
                            Image = "Shikoku-Regional cuisine-Sanuki Udon.png" },
                    new() { Title = "Katsuo no Tataki", TitleJa = "かつおのたたき", Note = "Kōchi's seared bonito, lightly torched outside and raw inside, served with garlic and citrus.",
                            Image = "Shikoku-Regional cuisine-Katsuo no Tataki.png" },
                    new() { Title = "Imabari Yakitori", TitleJa = "今治焼き鳥", Note = "Ehime's grilled skewers, cooked on a hot iron plate rather than an open grill.",
                            Image = "Shikoku-Regional cuisine-Imabari Yakitori.png" },
                    new() { Title = "Sudachi Citrus", TitleJa = "すだち", Note = "Tokushima's small, tart green citrus fruit used to flavor everything from soba to sake.",
                            Image = "Shikoku-Regional cuisine-Sudachi Citrus.png" },
                    new() { Title = "Tai Meshi", TitleJa = "鯛めし", Note = "Ehime's sea bream rice, either simmered whole or served as a raw-fish rice bowl depending on the region.",
                            Image = "Shikoku-Regional cuisine-Tai Meshi.png" },
                    new() { Title = "Goshiki Sōmen", TitleJa = "五色そうめん", Note = "Tokushima's thin wheat noodles dyed in five colors, often given as gifts.",
                            Image = "Shikoku-Regional cuisine-Goshiki Sōmen.png" },
                },
                Festivals = new() {
                    new() { Title = "Awa Odori", TitleJa = "阿波踊り", Note = "Mid-August — Japan's most famous dance festival, held in Tokushima during Obon, with hundreds of dance troupes parading through the streets",
                            Image = "Shikoku-Festivals & events-Awa Odori.png" },
                    new() { Title = "Kōchi Yosakoi Festival", TitleJa = "よさこい祭り", Note = "Early August — a high-energy dance festival born in Kōchi, now replicated across Japan (including Hokkaido's Yosakoi Sōran)",
                            Image = "Shikoku-Festivals & events-Kōchi Yosakoi Festival.png" },
                    new() { Title = "Niihama Taiko Matsuri", TitleJa = "新居浜太鼓祭り", Note = "Mid-October — Ehime's festival of massive, elaborately decorated taiko drum floats carried through the streets",
                            Image = "Shikoku-Festivals & events-Niihama Taiko Matsuri.png" },
                    new() { Title = "Marugame Uchiwa Festival", TitleJa = "丸亀うちわ祭", Note = "Summer — celebrates Marugame's centuries-old tradition as Japan's top producer of uchiwa fans",
                            Image = "Shikoku-Festivals & events-Marugame Uchiwa Festival.png" },
                },
                Access = "Shikoku has no Shinkansen line, so most visitors arrive via Okayama on the mainland, "
                       + "crossing the Seto Great Bridge by limited express train into Takamatsu, Shikoku's main "
                       + "gateway (about 1 hour from Okayama). Matsuyama and Kōchi also have small airports with "
                       + "domestic flights from Tokyo and Osaka. Once on the island, a car is highly recommended "
                       + "— rural areas like the Iya Valley have very limited public transit, and much of "
                       + "Shikoku's appeal lies in its slower-paced countryside.",
                AccessHighlights = new() {
                    "Okayama", "Seto Great Bridge", "Takamatsu", "1 hour",
                    "Matsuyama", "Kōchi", "Iya Valley"
                },
                Accent = "na-sakura",
                Center = (33.74, 133.50),
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Shikoku",
                HeroImage = "/images/Japan%20Region/Shikoku.png",
                MapX = 35, MapY = 73,
            },

            ["kyushu"] = new Region {
                Slug = "kyushu",
                Name = "Kyūshū", NameJa = "九州",
                Subtitle = "火山と温泉の島",
                Capital = "Fukuoka", CapitalJa = "福岡",
                Area = "42,232 km²",
                Climate = "Warm Pacific & East China Sea climate · hot humid summers · mild winters",
                BestSeason = "Mar–May · Oct–Nov · winter for hot springs",
                WinterMonths = "12月–2月", WinterSub = "December to February",
                WinterNote = "Mild along the coast, snow-capped on Aso — and this is the region's trump card: steaming "
                            + "hotspring towns like Beppu and Yufuin are at their best when the air is cold.",
                SummerMonths = "7月–8月", SummerSub = "July and August",
                SummerNote = "Blazing sun and the Hakata Gion Yamakasa races in July. Beaches in Miyazaki fill up, "
                            + "and late summer means typhoon watch — check forecasts before crossing to the islands.",
                Tagline = "Volcanoes, tonkotsu ramen, and hot springs — the island where Japan first made contact with the world.",
                Description = "Kyūshū is Japan's third-largest island and its southern gateway, historically the "
                            + "first point of contact with Chinese, Korean, and later Portuguese and Dutch traders "
                            + "— Nagasaki in particular was one of Japan's only windows to the outside world "
                            + "during the country's 200+ years of isolation (sakoku). The island remains "
                            + "geologically restless: Mount Aso, one of the world's largest volcanic calderas, "
                            + "still smokes actively, and Beppu and Yufuin channel that volcanic energy into some "
                            + "of Japan's most famous hot spring towns. Fukuoka, Kyūshū's largest city, is a "
                            + "modern, food-driven hub known for its yatai (street food stalls) and tonkotsu "
                            + "ramen, while Kagoshima in the south sits in the shadow of the still-active volcano "
                            + "Sakurajima.",
                OverviewHighlights = new() {
                    "Kyūshū", "Chinese, Korean, and later Portuguese and Dutch", "Nagasaki", "200+ years of isolation",
                    "Mount Aso", "Beppu", "Yufuin", "Fukuoka",
                    "Kagoshima", "Sakurajima"
                },
                Highlights = new() {
                    new() { Title = "Mount Aso", TitleJa = "阿蘇山", Note = "One of the world's largest active volcanic calderas, with vast grasslands and a smoking crater visitors can approach.",
                            Image = "Kyūshū-Must-see highlights-Mount Aso.png" },
                    new() { Title = "Beppu Hot Springs", TitleJa = "別府温泉", Note = "Japan's top hot spring producer, famous for its \"hells\" (jigoku) — vividly colored boiling ponds not meant for bathing.",
                            Image = "Kyūshū-Must-see highlights-Beppu Hot Springs.png" },
                    new() { Title = "Nagasaki Peace Park & Glover Garden", TitleJa = "長崎平和公園・グラバー園", Note = "A peace memorial to the 1945 atomic bombing, alongside a hillside of preserved 19th-century foreign trader residences.",
                            Image = "Kyūshū-Must-see highlights-Nagasaki Peace Park & Glover Garden.png" },
                    new() { Title = "Sakurajima", TitleJa = "桜島", Note = "An active volcano looming over Kagoshima Bay, still erupting small ash clouds regularly.",
                            Image = "Kyūshū-Must-see highlights-Sakurajima.png" },
                    new() { Title = "Yufuin", TitleJa = "湯布院", Note = "A boutique hot spring town known for its art museums, craft shops, and mountain backdrop.",
                            Image = "Kyūshū-Must-see highlights-Yufuin.png" },
                },
                Foods = new() {
                    new() { Title = "Hakata Tonkotsu Ramen", TitleJa = "博多とんこつラーメン", Note = "Fukuoka's famous pork-bone broth ramen, rich and cloudy, with thin, firm noodles.",
                            Image = "Kyūshū-Regional cuisine-Hakata Tonkotsu Ramen.png" },
                    new() { Title = "Motsunabe", TitleJa = "もつ鍋", Note = "A hot pot of beef or pork offal simmered with cabbage and chives in a garlic-chili or miso broth, another Fukuoka specialty.",
                            Image = "Kyūshū-Festivals & events-Motsunabe.png" },
                    new() { Title = "Nagasaki Champon", TitleJa = "長崎ちゃんぽん", Note = "A noodle dish blending Chinese and Japanese influences, reflecting Nagasaki's history as a trading port.",
                            Image = "Kyūshū-Regional cuisine-Nagasaki Champon.png" },
                    new() { Title = "Kurobuta Tonkatsu", TitleJa = "黒豚とんかつ", Note = "Kagoshima's prized black pork, breaded and fried, known for its tenderness.",
                            Image = "Kyūshū-Regional cuisine-Kurobuta Tonkatsu.png" },
                    new() { Title = "Karashi Renkon", TitleJa = "からし蓮根", Note = "Kumamoto's deep-fried lotus root stuffed with spicy mustard-miso paste.",
                            Image = "Kyūshū-Regional cuisine-Karashi Renkon.png" },
                    new() { Title = "Shōchū", TitleJa = "焼酎", Note = "Kyūshū is Japan's heartland for shōchū, a distilled spirit made from sweet potato, barley, or rice, especially in Kagoshima and Miyazaki.",
                            Image = "Kyūshū-Regional cuisine-Shōchū.png" },
                },
                Festivals = new() {
                    new() { Title = "Hakata Gion Yamakasa", TitleJa = "博多祇園山笠", Note = "Early July — Fukuoka's centuries-old festival where teams race through the streets carrying massive decorated floats",
                            Image = "Kyūshū-Festivals & events-Hakata Gion Yamakasa.png" },
                    new() { Title = "Nagasaki Kunchi", TitleJa = "長崎くんち", Note = "Early October — a festival blending Japanese, Chinese, and Dutch influences, featuring a dragon dance reflecting Nagasaki's trading history",
                            Image = "Kyūshū-Festivals & events-Nagasaki Kunchi.png" },
                    new() { Title = "Karatsu Kunchi", TitleJa = "唐津くんち", Note = "Early November — Saga's festival of enormous lacquered floats shaped like lions, dragons, and helmets, paraded through the streets",
                            Image = "Kyūshū-Festivals & events-Karatsu Kunchi.png" },
                    new() { Title = "Ohara Matsuri", TitleJa = "おはら祭", Note = "Mid-November — Kagoshima's largest festival, with thousands dancing through the streets to the folk song \"Ohara Bushi\"",
                            Image = "Kyūshū-Festivals & events-Ohara Matsuri.png" },
                },
                Access = "Fukuoka is Kyūshū's main hub, reachable from Osaka in about 2.5 hours and Tokyo in "
                       + "about 5 hours on the San'yō/Kyūshū Shinkansen, or via direct flights into Fukuoka "
                       + "Airport, which sits unusually close to the city center. The Kyūshū Shinkansen connects "
                       + "Fukuoka south to Kumamoto and Kagoshima, making the island's west side easy to travel "
                       + "by rail, while eastern areas like Beppu and Miyazaki are better reached by limited "
                       + "express train or car. Nagasaki is also directly Shinkansen-connected as of 2022.",
                AccessHighlights = new() {
                    "Fukuoka", "Osaka", "2.5 hours", "Tokyo",
                    "5 hours", "San'yō/Kyūshū Shinkansen", "Fukuoka Airport", "Kyūshū Shinkansen",
                    "Kumamoto", "Kagoshima", "Beppu", "Miyazaki"
                },
                Accent = "na-primary",
                Center = (33.25, 130.69),
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Ky%C5%ABsh%C5%AB",
                HeroImage = "/images/Japan%20Region/Ky%C5%ABsh%C5%AB.png",
                MapX = 22, MapY = 75,
            },

            ["okinawa"] = new Region {
                Slug = "okinawa",
                Name = "Okinawa", NameJa = "沖縄",
                Subtitle = "琉球の島々",
                Capital = "Naha", CapitalJa = "那覇",
                Area = "2,281 km²",
                Climate = "Subtropical — warm year-round, mild typhoon season Jul–Sep",
                BestSeason = "Apr–Jun · Oct–Nov",
                WinterMonths = "12月–2月", WinterSub = "December to February",
                WinterNote = "No snow here — 15–20°C days, humpback whales off Kerama from January, and Okinawa's own "
                            + "pink kanhizakura cherry blossoms bloom as early as January. Too cool for swimming, perfect for exploring.",
                SummerMonths = "5月–9月", SummerSub = "May to September",
                SummerNote = "The long season the rest of Japan never gets: beaches open in April and Eisa drumming fills "
                            + "the streets in late August. The trade-off is typhoon season peaking July–September.",
                Tagline = "Where Japan meets the Pacific — coral seas, castle ruins, and the islands that were once their own kingdom.",
                Description = "Okinawa is Japan's southernmost prefecture, a chain of subtropical islands closer "
                            + "to Taipei than to Tokyo, and culturally distinct from the rest of Japan in ways "
                            + "that go beyond geography. For roughly 450 years, the islands were an independent "
                            + "Ryukyu Kingdom, trading with China, Korea, and Southeast Asia, before being "
                            + "formally annexed by Japan in 1879. That history shows up everywhere — in the "
                            + "red-tiled roofs and shisa lion-dog statues of Okinawan architecture, in a distinct "
                            + "culinary tradition built around pork and bitter melon, and in Uchinaaguchi, the "
                            + "indigenous Ryukyuan language still spoken by some older residents (and classified "
                            + "by UNESCO as endangered). Okinawa was also the site of one of the Pacific War's "
                            + "bloodiest battles in 1945, a history preserved at peace memorials across the main "
                            + "island. Beyond history, the islands are known for some of Japan's best diving and "
                            + "beaches, with coral reefs ringing Ishigaki, Miyako, and the Kerama Islands.",
                OverviewHighlights = new() {
                    "Okinawa", "450 years", "Ryukyu Kingdom", "1879",
                    "Uchinaaguchi", "Pacific War's"
                },
                Highlights = new() {
                    new() { Title = "Shuri Castle", TitleJa = "首里城", Note = "The former royal palace of the Ryukyu Kingdom; still being rebuilt after a 2019 fire, with restoration ongoing.",
                            Image = "Okinawa-Must-see highlights-Shuri Castle.png" },
                    new() { Title = "Churaumi Aquarium", TitleJa = "美ら海水族館", Note = "One of the world's largest aquariums, famous for its massive whale shark tank.",
                            Image = "Okinawa-Must-see highlights-Churaumi Aquarium.png" },
                    new() { Title = "Kerama Islands", TitleJa = "慶良間諸島", Note = "A national park known for some of the clearest water in the world, popular for diving and snorkeling.",
                            Image = "Okinawa-Must-see highlights-Kerama Islands.png" },
                    new() { Title = "Peace Memorial Park (Itoman)", TitleJa = "平和祈念公園", Note = "A memorial to the Battle of Okinawa, inscribed with the names of all who died in the 1945 fighting.",
                            Image = "Okinawa-Must-see highlights-Peace Memorial Park (Itoman).png" },
                    new() { Title = "Ishigaki & Miyako Islands", TitleJa = "石垣島・宮古島", Note = "Remote outer islands with some of Okinawa's best beaches and coral reefs, a short flight from the main island.",
                            Image = "Okinawa-Must-see highlights-Ishigaki & Miyako Islands.png" },
                },
                Foods = new() {
                    new() { Title = "Goya Chanpurū", TitleJa = "ゴーヤーチャンプルー", Note = "A stir-fry of bitter melon, tofu, egg, and pork, Okinawa's most iconic everyday dish.",
                            Image = "Okinawa-Regional cuisine-Goya Chanpurū.png" },
                    new() { Title = "Okinawa Soba", TitleJa = "沖縄そば", Note = "Wheat noodles (despite the name) in a pork-and-bonito broth, topped with soki (stewed pork ribs).",
                            Image = "Okinawa-Regional cuisine-Okinawa Soba.png" },
                    new() { Title = "Rafute", TitleJa = "ラフテー", Note = "Pork belly slow-simmered in awamori, soy sauce, and sugar until meltingly tender.",
                            Image = "Okinawa-Regional cuisine-Rafute.png" },
                    new() { Title = "Taco Rice", TitleJa = "タコライス", Note = "A fusion dish born near U.S. military bases, taco fillings served over rice instead of a tortilla.",
                            Image = "Okinawa-Regional cuisine-Taco Rice.png" },
                    new() { Title = "Awamori", TitleJa = "泡盛", Note = "Okinawa's distilled rice liquor, distinct from mainland sake or shōchū, some varieties aged for decades.",
                            Image = "Okinawa-Regional cuisine-Awamori.png" },
                    new() { Title = "Sata Andagi", TitleJa = "サーターアンダーギー", Note = "Okinawan deep-fried doughnuts, denser and less sweet than mainland versions.",
                            Image = "Okinawa-Regional cuisine-Sata Andagi.png" },
                },
                Festivals = new() {
                    new() { Title = "Naha Great Tug-of-War", TitleJa = "那覇大綱挽", Note = "Early October — one of the world's largest tug-of-war events, using a rope over 200 meters long and weighing tons",
                            Image = "Okinawa-Festivals & events-Naha Great Tug-of-War.png" },
                    new() { Title = "Eisā Festival", TitleJa = "エイサー祭り", Note = "Mid-August (Obon) — traditional Okinawan drum dance performed by youth groups throughout the islands",
                            Image = "Okinawa-Festivals & events-Eisā Festival.png" },
                    new() { Title = "Naha Hārī", TitleJa = "那覇ハーリー", Note = "Early May — dragon boat races in Naha harbor, tracing back to Ryukyu Kingdom maritime traditions",
                            Image = "Okinawa-Festivals & events-Naha Hārī.png" },
                    new() { Title = "Peace Memorial Day", TitleJa = "慰霊の日", Note = "June 23 — a solemn day of remembrance marking the end of the Battle of Okinawa",
                            Image = "Okinawa-Festivals & events-Peace Memorial Day.png" },
                },
                Access = "Naha Airport, on the main island, is Okinawa's primary gateway, with direct flights "
                       + "from Tokyo (about 2.5 hours), Osaka, and several other Japanese cities, as well as some "
                       + "international routes. There is no rail or road connection to mainland Japan — Okinawa "
                       + "is reachable only by air or ferry. Getting around the main island is easiest by rental "
                       + "car, as public transit is limited outside Naha (the monorail, Yui Rail, covers only the "
                       + "city center). Reaching the outer islands like Ishigaki or Miyako requires a separate "
                       + "short domestic flight from Naha.",
                AccessHighlights = new() {
                    "Naha Airport", "Tokyo (about 2.5 hours)", "Ishigaki", "Miyako"
                },
                Accent = "na-gold",
                Center = (26.33, 127.80),
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Okinawa_Prefecture",
                HeroImage = "/images/Japan%20Region/Okinawa.png",
                MapX = 18, MapY = 87,
            },
        };

        /// <summary>All 9 regions in geographic (north → south) order.</summary>
        public static IReadOnlyList<Region> AllOrdered { get; } = new[]
        {
            _all["hokkaido"], _all["tohoku"], _all["kanto"], _all["chubu"],
            _all["kansai"], _all["chugoku"], _all["shikoku"], _all["kyushu"],
            _all["okinawa"],
        };

        /// <summary>Find a region by URL slug (case-insensitive). Returns null if not found.</summary>
        public static Region? FindBySlug(string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;
            return _all.TryGetValue(slug.ToLowerInvariant(), out var r) ? r : null;
        }

        /// <summary>
        /// HTML-encodes <paramref name="text"/> and wraps every occurrence of the given
        /// <paramref name="terms"/> in a &lt;mark&gt; element so the key facts of a region
        /// (bolded in the source Region Guide document) stand out on the detail page.
        /// The returned string is already escaped — render it with @Html.Raw().
        /// </summary>
        public static string Highlight(string? text, IEnumerable<string>? terms)
        {
            if (string.IsNullOrEmpty(text)) return "";

            static string Esc(string s) => System.Net.WebUtility.HtmlEncode(s);

            var list = terms?.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            if (list is null || list.Count == 0) return Esc(text);

            // 1. find every (case-insensitive) occurrence of every term
            var ranges = new List<(int Start, int End)>();
            foreach (var term in list)
            {
                int from = 0, hit;
                while ((hit = text.IndexOf(term, from, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    ranges.Add((hit, hit + term.Length));
                    from = hit + term.Length;
                }
            }
            if (ranges.Count == 0) return Esc(text);

            // 2. sort and merge overlaps ("San'yō/Kyūshū Shinkansen" contains "Kyūshū Shinkansen")
            ranges.Sort((a, b) => a.Start != b.Start ? a.Start.CompareTo(b.Start) : b.End.CompareTo(a.End));
            var merged = new List<(int Start, int End)>();
            foreach (var r in ranges)
            {
                if (merged.Count > 0 && r.Start <= merged[^1].End)
                    merged[^1] = (merged[^1].Start, Math.Max(merged[^1].End, r.End));
                else
                    merged.Add(r);
            }

            // 3. rebuild the paragraph, escaping everything and marking the hits
            var sb = new System.Text.StringBuilder();
            int pos = 0;
            foreach (var r in merged)
            {
                if (r.Start < pos) continue;          // fully consumed by a previous range
                sb.Append(Esc(text[pos..r.Start]));
                sb.Append("<mark class=\"na-region-hl\">").Append(Esc(text[r.Start..r.End])).Append("</mark>");
                pos = r.End;
            }
            sb.Append(Esc(text[pos..]));
            return sb.ToString();
        }

        /// <summary>Get the region just before / after the current one in geographic order, or null.</summary>
        public static (Region? Prev, Region? Next) Neighbors(string? slug)
        {
            var list = AllOrdered;
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i].Slug, slug, StringComparison.OrdinalIgnoreCase))
                {
                    return (i > 0 ? list[i - 1] : null, i < list.Count - 1 ? list[i + 1] : null);
                }
            }
            return (null, null);
        }
    }
}
