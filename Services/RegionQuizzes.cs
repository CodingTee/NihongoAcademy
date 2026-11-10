using System.Collections.Generic;
using NihongoAcademy.Models.ExploreJapan;

namespace NihongoAcademy.Services
{
    /// <summary>
    /// The content of the "Let's play" game on every region detail page.
    ///
    /// Each region gets its own <see cref="QuizSet"/>: eight rounds built from
    /// that region's own food, sights and festivals, plus the stamp, the four
    /// rank names and the four end-of-game tips shown once the run is over.
    /// The rounds are authored the same way as the rest of the region text —
    /// sourced from the group's Region Guide, one line of teaching per round —
    /// so a learner plays Hokkaido, then Kansai, then Okinawa without ever
    /// being handed another island's vocabulary.
    /// </summary>
    public static class RegionQuizzes
    {
        /// <summary>One region's game: the rounds plus the end-of-run card copy.</summary>
        public sealed class QuizSet
        {
            /// <summary>Single kanji stamped on the result card, e.g. 雪 for Hokkaido.</summary>
            public string Stamp { get; set; } = "";

            /// <summary>Rank names from a perfect run down to a beginner run.</summary>
            public string[] Ranks { get; set; } = new string[4];

            /// <summary>Tips shown next to each of the four ranks.</summary>
            public string[] Tips { get; set; } = new string[4];

            /// <summary>The eight rounds, in the order they are handed to the view.</summary>
            public List<RegionQuizRound> Rounds { get; set; } = new();
        }

        private static readonly Dictionary<string, QuizSet> _all = new()
        {
            /* ================================================================
               HOKKAIDO — 北海道
               ================================================================ */
            ["hokkaido"] = new QuizSet {
                Stamp = "雪",
                Ranks = new[] {
                    "雪印 — Sand plover",
                    "なまら 上手",
                    "Hokkaido 見習い",
                    "うんまい！beginner"
                },
                Tips = new[] {
                    "A perfect run. You already know a little Hokkaido-ben — try ばりおいしい on your next bowl of ramen.",
                    "Almost there. Keep an ear out for なまら in Sapporo — it is the friendliest word on the island.",
                    "Good start. The trick with Hokkaido-ben is simple: ばり just replaces とても — very.",
                    "No matter — the words stick after two rounds. Press play again."
                },
                Rounds = new() {
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "ジンギスカン", Reading = "Jingisukan",
                        Romaji = "jin-gi-su-kan", Say = "ジンギスカン",
                        Pic = "Hokkaido-Regional cuisine-Genghis Khan (Jingisukan)-small.png",
                        Note = "Literally Genghis Khan — the dome-shaped skillet is named after the conqueror, but on a Hokkaido table it means grilled mutton. The restaurant brings the pan to your table.",
                        Answer = 0,
                        Opts = new[] {
                            "Grilled mutton cooked at the table on a dome-shaped skillet",
                            "The 13th-century conqueror of Asia",
                            "A sweet red-bean paste for buns",
                            "Steamed rice dressed with soy sauce"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "スープカレー", Reading = "Sūpu karē (soup curry)",
                        Romaji = "su-pu ka-ree", Say = "スープカレー",
                        Pic = "Hokkaido-Regional cuisine-Soup Curry-small.png",
                        Note = "Sapporo invented it: the curry is a thin, spicy broth poured over rice instead of a thick roux. Order it at a counter and drink the leftovers from the bowl.",
                        Answer = 0,
                        Opts = new[] {
                            "Thin spiced broth poured over rice, invented in Sapporo",
                            "A thick Japanese curry roux with breaded pork",
                            "Cold buckwheat noodles in dashi",
                            "Sweet green tea served with a dessert bean"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "札幌ラーメン", Reading = "Sapporo rāmen",
                        Romaji = "sap-pe-ro raa-men", Say = "札幌ラーメン",
                        Pic = "Hokkaido-Regional cuisine-Sapporo Miso Ramen-small.png",
                        Note = "Sapporo's ramen is built on miso, not the soy-and-bone broth of Tokyo. The classic bowl arrives with a slab of butter and sweet corn on top — and the tall stacks of chāshu are for sharing.",
                        Answer = 0,
                        Opts = new[] {
                            "Sapporo's miso ramen, served with butter and sweet corn",
                            "A cold dipping noodle made from buckwheat soba",
                            "Udon noodles in a light dashi broth with tempura",
                            "Thin rice noodles in a hot and sour soup"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "夕張メロン", Reading = "Yūbari meron",
                        Romaji = "yuu-ba-ri me-ron", Say = "夕張メロン",
                        Pic = "Hokkaido-Regional cuisine-Yubari Melon-small.png",
                        Note = "Yūbari, a small town on Hokkaido's central coast, grows the melon that won Japan's All-Japan Fruit Grand Prix in 1967. 夕張 is written with 夕, evening — the harbour glow used to light the boats home at dusk.",
                        Answer = 0,
                        Opts = new[] {
                            "Yūbari melon — Hokkaido's prize-winning fruit",
                            "A green-tea sweet sold at temple festivals",
                            "A winter citrus the size of a fist",
                            "A sweet potato roasted on a roadside stick"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Place", Ja = "小樽", Reading = "Otaru",
                        Romaji = "o-ta-ru", Say = "おたる",
                        Pic = "Hokkaido-Must see Highlights-Otaru canal.png",
                        Note = "Otaru is a former herring port on the Sea of Japan, half an hour by train from Sapporo. The stone warehouses along its canal now hold music-box workshops, glass studios and cafés.",
                        Answer = 0,
                        Opts = new[] {
                            "Otaru — the canal town of old warehouses and music boxes",
                            "Asahikawa — the riverside city ringed by mountains",
                            "Hakodate — the southern cape town of night views",
                            "Wakkanai — the northernmost harbour of the island"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Onsen", Ja = "登別温泉", Reading = "Noboribetsu onsen",
                        Romaji = "no-bo-ri-be-tsu on-sen", Say = "登別温泉",
                        Pic = "Hokkaido-Must see Highlights-Noboribetsu Onsen.png",
                        Note = "One of Japan's three most famous hot springs. 温泉 is written with two kanji that say it outright: 熱 (hot) and 泉 (spring). The water here is milky white with sulphur, and the whole valley steams.",
                        Answer = 0,
                        Opts = new[] {
                            "Noboribetsu onsen — one of Japan's three most famous hot springs",
                            "A cold northern cape where sea ice stacks up high",
                            "A harbour frozen solid from December through March",
                            "A fishing port famous for its summer squid"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "雪まつり", Reading = "Yukimatsuri (snow festival)",
                        Romaji = "yu-ki ma-tsu-ri", Say = "雪まつり",
                        Pic = "Hokkaido-Must See Highlights-Sapporo Snow Festival.png",
                        Note = "Every February Sapporo builds huge snow sculptures and ice slides along Odori Park's 1.2 km avenue. 雪まつり is written in hiragana rather than kanji, and 米パン — steamed buns filled with sweet red bean paste — are the festival food.",
                        Answer = 0,
                        Opts = new[] {
                            "Yukimatsuri — Sapporo's February snow-sculpture festival",
                            "A summer fireworks show over the harbour",
                            "A temple festival with paper lantern floats",
                            "A harvest festival celebrating the first rice"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Nature", Ja = "大雪山", Reading = "Daisetsuzan",
                        Romaji = "dai-se-tsu-zan", Say = "大雪山",
                        Pic = "Hokkaido-Must See Highlights-Daisetsuzan National Park.png",
                        Note = "Great snow mountain — Hokkaido's largest national park and one of its oldest, nicknamed the roof of Hokkaido. The 19 peaks include Japan's northernmost volcano.",
                        Answer = 0,
                        Opts = new[] {
                            "Hokkaido's largest national park, the roof of the island",
                            "Japan's tallest mountain",
                            "A caldera lake in Shiretoko",
                            "The site of the February snow festival"
                        }
                    },
                },
            },

            /* ================================================================
               TŌHOKU — 東北
               ================================================================ */
            ["tohoku"] = new QuizSet {
                Stamp = "東",
                Ranks = new[] {
                    "ねぶた 完走",
                    "ずんだ 上手",
                    "Tōhoku 見習い",
                    "うんまい！beginner"
                },
                Tips = new[] {
                    "A perfect run. Try ordering in Sendai: 牛タン、お願いします — beef tongue, please.",
                    "Almost there. ずんだ is the sound of crushing soybeans; ずんだ餅 is that sweet paste spread over a rice cake.",
                    "Good start. In Iwate わんこ is the childish word for a bowl — cover it to say stop.",
                    "No matter — the kanji stick after two rounds. Press play again."
                },
                Rounds = new() {
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "牛タン", Reading = "Gyūtan (beef tongue)",
                        Romaji = "gyuu-tan", Say = "牛タン",
                        Pic = "Tōhoku-Regional cuisine-Gyūtan (Sendai Beef Tongue).png",
                        Note = "牛 is cattle, タン is a shortening of タン (tongue) — Sendai's signature dish is grilled, thinly sliced ox tongue, usually with barley rice and oxtail soup.",
                        Answer = 0,
                        Opts = new[] {
                            "Sendai's grilled beef tongue, served with barley rice",
                            "Akita rice pressed around a skewer and simmered in a pot",
                            "A leaf-shaped grilled fish cake from Sendai",
                            "Buckwheat noodles refilled until you cover the bowl"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "わんこそば", Reading = "Wanko soba",
                        Romaji = "wan-ko so-ba", Say = "わんこそば",
                        Pic = "Tōhoku-Regional cuisine-Wanko Soba.png",
                        Note = "わんこ is the childish word for a bowl. In Iwate the servers keep refilling it, and the only polite way to say stop is to stack the lid on top.",
                        Answer = 0,
                        Opts = new[] {
                            "Iwate's bottomless soba you stop by covering the bowl",
                            "Akita's mashed-rice skewers grilled and simmered in a hot pot",
                            "Mochi covered in sweet mashed edamame",
                            "Fresh apple cider from the orchards of Aomori"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "ずんだ餅", Reading = "Zunda mochi",
                        Romaji = "zun-da mo-chi", Say = "ずんだ餅",
                        Pic = "Tōhoku-Regional cuisine-Zunda Mochi.png",
                        Note = "ずんだ is the sound of soybeans being crushed; 餅 is the rice cake underneath. A Sendai sweet you eat in one bite, in midwinter or midsummer.",
                        Answer = 0,
                        Opts = new[] {
                            "Mochi coated in sweet mashed edamame, a Sendai sweet",
                            "Thin ox-tongue slices grilled over charcoal",
                            "Buckwheat noodles served in stacked lacquer trays",
                            "A skewer of rice shaped like a maple leaf"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Place", Ja = "松島湾", Reading = "Matsushima Bay",
                        Romaji = "mat-su-shi-ma wan", Say = "松島湾",
                        Pic = "Tōhoku-Must-see highlights-Matsushima Bay.png",
                        Note = "松 is pine, 島 is island — 松島 means pine islands. More than 200 islets, one of Japan's Three Great Views.",
                        Answer = 0,
                        Opts = new[] {
                            "Matsushima Bay — one of Japan's Three Great Views",
                            "Ginzan, the snowbound town of gas-lamp wooden ryokan",
                            "Zaō's frost-frozen snow-monster trees",
                            "Towada-Hachimantai's volcanic caldera lakes"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Onsen", Ja = "銀山温泉", Reading = "Ginzan onsen",
                        Romaji = "gin-za-n on-sen", Say = "銀山温泉",
                        Pic = "Tōhoku-Must-see highlights-Ginzan Onsen.png",
                        Note = "金 is gold, 銀 is silver — the town took its name from the silver mine it grew around, long before the gas lamps were lit for the winter.",
                        Answer = 0,
                        Opts = new[] {
                            "Ginzan Onsen — the snowbound town of gas-lamp wooden ryokan",
                            "Matsushima Bay's pine-clad islets",
                            "Kamakura's 13-metre bronze Buddha",
                            "Tokyo's world-busiest neon crossing"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Nature", Ja = "蔵王樹氷", Reading = "Zaō jumoku-hyō (snow monsters)",
                        Romaji = "za-o juu-hyou", Say = "蔵王樹氷",
                        Pic = "Tōhoku-Must-see highlights-Zaō Onsen & Snow Monsters.png",
                        Note = "樹 is tree, 氷 is ice — 樹氷 are trees wrapped in packed frost, nicknamed snow monsters because the wind shapes them into something ungainly and watchable.",
                        Answer = 0,
                        Opts = new[] {
                            "Zaō's 樹氷 — trees frozen into snow monsters",
                            "A whirlpool in the Naruto Strait",
                            "Japan's tallest peak",
                            "The gold-leafed Konjikidō hall of Chūson-ji"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "ねぶた祭", Reading = "Nebuta matsuri",
                        Romaji = "ne-bu-ta ma-tsu-ri", Say = "ねぶた祭",
                        Pic = "Tōhoku-Festivals & events-Nebuta Matsuri.png",
                        Note = "ねぶた is a dialect shortening of ねむる (nemuru, to sleep). The giant illuminated warrior floats are meant to drive sleep out of the last hot days of summer.",
                        Answer = 0,
                        Opts = new[] {
                            "Aomori's Nebuta — giant lit warrior floats in early August",
                            "Osaka's octopus batter balls",
                            "Kyoto's gilded pavilion reflected in a pond",
                            "Tokyo's oldest temple and its thunder gate"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "秋田竿燈まつり", Reading = "Akita Kantō matsuri",
                        Romaji = "a-ki-ta kan-too ma-tsu-ri", Say = "秋田竿燈まつり",
                        Pic = "Tōhoku-Festivals & events-Akita Kantō Matsuri.png",
                        Note = "竿 is a pole, 燈 is a lamp. Performers balance a pole hung with dozens of lit lanterns on their palms, their shoulders and — the hard part — their foreheads.",
                        Answer = 0,
                        Opts = new[] {
                            "Akita Kantō — long poles balanced on the palms of the hands",
                            "Sendai's paper-streamer star festival",
                            "Sapporo's February snow sculptures",
                            "Yamagata's dancers in flower-decorated straw hats"
                        }
                    },
                },
            },

            /* ================================================================
               KANTŌ — 関東
               ================================================================ */
            ["kanto"] = new QuizSet {
                Stamp = "江",
                Ranks = new[] {
                    "江戸っ子",
                    "なじみ 上手",
                    "Kantō 見習い",
                    "ようこそ！beginner"
                },
                Tips = new[] {
                    "A perfect run. Stand on Shibuya with the lights counting down — 三千人 means three thousand people at once.",
                    "Almost there. 浅草 is shallow grass: the old name of the riverside where the first temple was raised.",
                    "Good start. 江戸前寿司 — in front of Edo, the sushi cooked and sold at the fish market by the castle.",
                    "No matter — the words stick after two rounds. Press play again."
                },
                Rounds = new() {
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "江戸前寿司", Reading = "Edomae sushi",
                        Romaji = "e-do-ma-e su-shi", Say = "江戸前寿司",
                        Pic = "Kantō-Regional cuisine-Edomae Sushi.png",
                        Note = "江戸 is Tokyo's old name and 前 means in front of — Edomae sushi was the fish cured and sold in front of Edo Castle, not the raw sashimi of today.",
                        Answer = 0,
                        Opts = new[] {
                            "Edomae sushi — Edo-era sushi of cured fish over hand-pressed rice",
                            "Osaka's octopus balls",
                            "Kagawa's chewy udon",
                            "Kōchi's seared bonito with garlic"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "もんじゃ焼き", Reading = "Monjayaki",
                        Romaji = "mon-ja ya-ki", Say = "もんじゃ焼き",
                        Pic = "Kantō-Regional cuisine-Monjayaki.png",
                        Note = "もんじゃ is the sound of batter hitting a very hot griddle. A runny, lacy batter cooked tableside on Tsukishima — the Tokyo cousin of okonomiyaki.",
                        Answer = 0,
                        Opts = new[] {
                            "Monjayaki — a runny Tokyo batter dish grilled tableside",
                            "Deep-fried lotus root stuffed with mustard miso",
                            "Maple-leaf steam buns filled with sweet bean paste",
                            "Rice topped with clams and miso broth"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "ちゃんこ鍋", Reading = "Chanko nabe",
                        Romaji = "chan-ko na-be", Say = "ちゃんこ鍋",
                        Pic = "Kantō-Regional cuisine-Chanko Nabe.png",
                        Note = "ちゃんこ is the word sumo use for a meal instead of 食事. A protein-heavy hot pot, and the whole stable eats out of one pot.",
                        Answer = 0,
                        Opts = new[] {
                            "Chanko nabe — the protein hot pot of sumo wrestlers",
                            "Nagoya's miso-glazed pork cutlet",
                            "Kyoto's simmered tofu hot pot",
                            "Buckwheat noodles served in stacked lacquer trays"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Place", Ja = "渋谷スクランブル交差点",
                        Reading = "Shibuya scramble crossing",
                        Romaji = "shi-bu-ya su-ku-ran-bu-roo", Say = "渋谷スクランブル交差点",
                        Pic = "Kantō-Must-see highlights-Shibuya Crossing.png",
                        Note = "渋 was a muddy, 谷 a valley — the place where travellers once changed horses. Up to 3,000 people now cross here on one green light.",
                        Answer = 0,
                        Opts = new[] {
                            "Shibuya Crossing — the world's busiest pedestrian scramble",
                            "Kamakura's 13-metre bronze Buddha",
                            "Hakone's lake-and-mountain onsen town",
                            "Nikkō's gilded shrine deep in the mountains"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Temple", Ja = "浅草寺", Reading = "Sensō-ji",
                        Romaji = "sen-soo-ji", Say = "浅草寺",
                        Pic = "Kantō-Must-see highlights-Senso-ji Temple.png",
                        Note = "The full name is 浅草寺 (Asakusa-dera). 浅草 means shallow grass — what the floodplain by the Sumida was called when the temple went up.",
                        Answer = 0,
                        Opts = new[] {
                            "Sensō-ji — Tokyo's oldest temple and its Kaminarimon gate",
                            "Osaka Castle, rebuilt after 1945",
                            "Nikkō's UNESCO shrine complex",
                            "Nara Park's free-roaming sacred deer"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Shrine", Ja = "日光東照宮", Reading = "Nikkō Tōshō-gū",
                        Romaji = "nik-ko too-shoo-oo", Say = "日光東照宮",
                        Pic = "Kantō-Must-see highlights-Nikkō Tōshō-gū.png",
                        Note = "東照宮 holds the deified title of Tokugawa Ieyasu, the first shogun. The carvings are crowded and bright on purpose — a shrine is meant to keep the faithful awake.",
                        Answer = 0,
                        Opts = new[] {
                            "Nikkō Tōshō-gū — the shrine of the first shogun",
                            "Kamakura's bronze Buddha",
                            "Hakone's open-air art museum",
                            "Sendai's grilled ox tongue"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "三社祭", Reading = "Sanja matsuri",
                        Romaji = "san-ja ma-tsu-ri", Say = "三社祭",
                        Pic = "Kantō-Festivals & events-Sanja Matsuri.png",
                        Note = "三社 means three shrines. The festival is shared by three Shinto shrines around Asakusa, and over a hundred mikoshi go out at once.",
                        Answer = 0,
                        Opts = new[] {
                            "Sanja Matsuri — over 100 portable shrines around Asakusa",
                            "Nagasaki's dragon dance",
                            "Kyoto's float procession through July",
                            "Kōchi's dance festival in early August"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "隅田川花火大会", Reading = "Sumidagawa hanabi",
                        Romaji = "su-mi-da-ga-wa ha-na-bi", Say = "隅田川花火大会",
                        Pic = "Kantō-Festivals & events-Sumidagawa Fireworks.png",
                        Note = "花 is flower, 火 is fire — hanabi is the Japanese word for fireworks. The Sumida has been lit since the Edo period, when two men drowned and the river was closed to boats for a year.",
                        Answer = 0,
                        Opts = new[] {
                            "Sumidagawa Fireworks — one of Japan's oldest hanabi nights",
                            "Aomori's lit warrior floats",
                            "Tokyo's busiest pedestrian crossing",
                            "Kurashiki's white-walled merchant quarter"
                        }
                    },
                },
            },

            /* ================================================================
               CHŪBU — 中部
               ================================================================ */
            ["chubu"] = new QuizSet {
                Stamp = "山",
                Ranks = new[] {
                    "富士 頂上",
                    "ほうとう 上手",
                    "Chūbu 見習い",
                    "ほうとう beginner"
                },
                Tips = new[] {
                    "A perfect run. Open the trailhead in July — the 五合目 sign is the friendliest Japanese you will read all trip.",
                    "Almost there. ほうとう is said to come from 方々, everywhere: Yamanashi's thick noodles in a miso vegetable stew.",
                    "Good start. 郷 is village — 白川郷 is where the 合掌 (joined-hands) thatched roofs were built by hand.",
                    "No matter — the words stick after two rounds. Press play again."
                },
                Rounds = new() {
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "飛騨牛", Reading = "Hida beef",
                        Romaji = "hi-da gyuu", Say = "飛騨牛",
                        Pic = "Chūbu-Regional cuisine-Hida Beef.png",
                        Note = "飛騨 is the old province name, now Gifu; 牛 is cattle. Marbled wagyu from the cool highlands around Takayama.",
                        Answer = 0,
                        Opts = new[] {
                            "Hida beef — marbled wagyu from the Takayama highlands",
                            "Nagoya's miso-glazed pork cutlet",
                            "Yamanashi's thick noodles in a miso stew",
                            "Toyama's translucent white shrimp"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "みそかつ", Reading = "Miso katsu",
                        Romaji = "mi-so ka-tsu", Say = "みそかつ",
                        Pic = "Chūbu-Regional cuisine-Miso Katsu.png",
                        Note = "かつ is the cut of カツレツ — a cutlet. Nagoya's version is defined by the sauce: rich, sweet-savoury red miso.",
                        Answer = 0,
                        Opts = new[] {
                            "Miso katsu — Nagoya's pork cutlet under a red miso glaze",
                            "Hida's premium marbled beef",
                            "Nagano's sushi wrapped in bamboo leaves",
                            "Kōchi's seared bonito with garlic"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "ほうとう", Reading = "Hōtō",
                        Romaji = "hoo-too", Say = "ほうとう",
                        Pic = "Chūbu-Regional cuisine-Hōtō.png",
                        Note = "Hōtō is the noodle of Yamanashi, at the foot of Fuji: thicker than udon, simmered with vegetables in miso rather than boiled in broth.",
                        Answer = 0,
                        Opts = new[] {
                            "Hōtō — Yamanashi's flat noodles simmered in a miso stew",
                            "Kumamoto's mustard-stuffed lotus root",
                            "Osaka's grilled cabbage pancake",
                            "Kyoto's simmered tofu hot pot"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Place", Ja = "富士山", Reading = "Mt. Fuji",
                        Romaji = "fu-ji-san", Say = "富士山",
                        Pic = "Chūbu-Must see Highlights-Mt. Fuji.png",
                        Note = "Fuji is drawn with a white cap because, as the old poem has it, 富士山は雪で頭を隠す — Fuji hides its head in snow. Japan's tallest peak, 3,776 m.",
                        Answer = 0,
                        Opts = new[] {
                            "Mt. Fuji — Japan's tallest peak and national symbol",
                            "Shirakawa-go's thatched roof village",
                            "Kanazawa's Kenroku-en garden",
                            "Sakurajima's smoking cone over Kagoshima"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Village", Ja = "白川郷", Reading = "Shirakawa-gō",
                        Romaji = "shi-ra-ka-wa goo", Say = "白川郷",
                        Pic = "Chūbu-Must see Highlights-Shirakawa-go.png",
                        Note = "郷 is a village, and the roofs are 合掌造 — gasshō-zukuri, joined hands. Thatch laid by hand three times a century, and heavy enough to need the whole village to hold it down.",
                        Answer = 0,
                        Opts = new[] {
                            "Shirakawa-gō — thatched gasshō-zukuri farmhouse village",
                            "Nagoya's industrial skyline",
                            "Nagasaki's hillside of foreign residences",
                            "Akita's balancing-pole festival"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Nature", Ja = "地獄谷野猿公苑", Reading = "Jigokudani monkey park",
                        Romaji = "ji-go-ku-ta-ni ya-en", Say = "地獄谷野猿公苑",
                        Pic = "Chūbu-Must see Highlights-Jigokudani Monkey Park.png",
                        Note = "地獄谷 means Valley of Hell. The hot spring here is too hot to bathe in, so the wild macaques do — and sit in steam while snow falls around them.",
                        Answer = 0,
                        Opts = new[] {
                            "Jigokudani — wild macaques bathing in a hot-spring valley of hell",
                            "Iya's vine bridges",
                            "Dōgo Onsen's bathhouse of spirits",
                            "Beppu's boiling coloured ponds"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "高山祭", Reading = "Takayama matsuri",
                        Romaji = "ta-ka-ya-ma ma-tsu-ri", Say = "高山祭",
                        Pic = "Chūbu-Festivals & events-Takayama Matsuri.png",
                        Note = "The floats are 屋台, yatai — market stalls. Takayama's are carved and gilded to a standard that makes them moving treasures, and the festival runs twice a year.",
                        Answer = 0,
                        Opts = new[] {
                            "Takayama Matsuri — the gilded yatai floats of the Japanese Alps",
                            "Nagoya's parade of three unifiers",
                            "Toyama's lantern-lit dance against the wind",
                            "Kurashiki's white-walled merchant quarter"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "おわら風の盆", Reading = "Owara Kaze no Bon",
                        Romaji = "o-wa-ra ka-ze no bon", Say = "おわら風の盆",
                        Pic = "Chūbu-Festivals & events-Owara Kaze no Bon.png",
                        Note = "風の盆 is the night of wind. 盆 here is not the Buddhist festival but a night of dancing, held in Toyama to ask the typhoon winds to pass by.",
                        Answer = 0,
                        Opts = new[] {
                            "Owara Kaze no Bon — Toyama's lantern-lit dance against the typhoon wind",
                            "Hakata's racing floats",
                            "Nara's deer and giant lanterns",
                            "Marugame's uchiwa fan festival"
                        }
                    },
                },
            },

            /* ================================================================
               KANSAI — 関西
               ================================================================ */
            ["kansai"] = new QuizSet {
                Stamp = "都",
                Ranks = new[] {
                    "千年の都",
                    "お好み 上手",
                    "Kansai 見習い",
                    "たこ焼き beginner"
                },
                Tips = new[] {
                    "A perfect run. Order the ten-piece plate in Osaka: たこ焼き、十個、お願いします.",
                    "Almost there. お好み means what you like — that one word is the whole dish, and the whole city.",
                    "Good start. 都, miyako, is the old word for capital; this region was it for a thousand years.",
                    "No matter — またね. Press play again, and the kanji will be there next time."
                },
                Rounds = new() {
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "たこ焼き", Reading = "Takoyaki",
                        Romaji = "ta-ko ya-ki", Say = "たこ焼き",
                        Pic = "Kansai-Regional cuisine-Takoyaki.png",
                        Note = "たこ is octopus, 焼き is grilled. Osaka's batter balls, crisp outside and molten inside, with sauce and bonito that moves in the wind.",
                        Answer = 0,
                        Opts = new[] {
                            "Takoyaki — Osaka's octopus balls, crisp out and molten in",
                            "Kyoto's multi-course kaiseki",
                            "Kagawa's chewy udon",
                            "Kōchi's seared bonito with garlic"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "お好み焼き", Reading = "Okonomiyaki",
                        Romaji = "o-ko-no-mi ya-ki", Say = "お好み焼き",
                        Pic = "Kansai-Regional cuisine-Okonomiyaki.png",
                        Note = "お好み is what you like — you say what goes in. Kansai mixes batter and cabbage; Hiroshima later stacked them in layers.",
                        Answer = 0,
                        Opts = new[] {
                            "Okonomiyaki — the grilled cabbage pancake you top as you like",
                            "Nagoya's miso cutlet",
                            "Nara's vegetables pickled in sake lees",
                            "Iwate's bottomless soba"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "京懐石", Reading = "Kyō kaiseki",
                        Romaji = "kyoo kai-seki", Say = "京懐石",
                        Pic = "Kansai-Regional cuisine-Kyoto Kaiseki.png",
                        Note = "懐石 literally means stones to warm the knees — the iron pots tea masters rested on their laps in winter, and the meal that grew out of it.",
                        Answer = 0,
                        Opts = new[] {
                            "Kaiseki — Kyoto's seasonal multi-course meal born in the tea house",
                            "Osaka's breaded skewers, no double-dipping",
                            "Sendai's grilled ox tongue",
                            "Toyama's white shrimp"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Shrine", Ja = "伏見稲荷大社", Reading = "Fushimi Inari Taisha",
                        Romaji = "fu-shi-mi i-na-ri tai-sha", Say = "伏見稲荷大社",
                        Pic = "Kansai-Must-see highlights-Fushimi Inari Taisha.png",
                        Note = "稲荷 is the god of rice, and every 鳥居 along the mountain was bought by a business hoping for a harvest. Ten thousand gates, given one at a time.",
                        Answer = 0,
                        Opts = new[] {
                            "Fushimi Inari Taisha — a mountainside of ten thousand vermilion torii",
                            "Kinkaku-ji's gold-leaf pavilion",
                            "Osaka's neon canal of Dōtonbori",
                            "Hiroshima's floating torii in the sea"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Temple", Ja = "金閣寺", Reading = "Kinkaku-ji (Golden Pavilion)",
                        Romaji = "kin-ka-ku-ji", Say = "金閣寺",
                        Pic = "Kansai-Must-see highlights-Kinkaku-ji (Golden Pavilion).png",
                        Note = "金 is gold, 閣 is a tower. It began as a shogun's retirement villa — 金閣 was the retirement, and only later the temple it is today.",
                        Answer = 0,
                        Opts = new[] {
                            "Kinkaku-ji — the gold-leaf pavilion of Kyoto",
                            "Nara Park's sacred deer",
                            "Nikkō's shrine of the first shogun",
                            "Matsushima Bay's pine islands"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Place", Ja = "道頓堀", Reading = "Dōtonbori",
                        Romaji = "doo-ton-bo-ri", Say = "道頓堀",
                        Pic = "Kansai-Must-see highlights-Dōtonbori.png",
                        Note = "堀 is a canal. Dōtonbori is named for the merchant Dōton who built the quay; the GICO running man has watched the same street since 1935.",
                        Answer = 0,
                        Opts = new[] {
                            "Dōtonbori — Osaka's canal-side neon food street",
                            "Kyoto's Gion geisha lanes",
                            "Nara Park's deer",
                            "Nagoya's castle keep"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "祇園祭", Reading = "Gion matsuri",
                        Romaji = "gi-on ma-tsu-ri", Say = "祇園祭",
                        Pic = "Kansai-Festivals & events-Gion Matsuri.png",
                        Note = "祇園 is the market quarter of Kyoto named for the god Gozu Tenno. The floats go out all through July — a month chosen when the old calendar was hottest.",
                        Answer = 0,
                        Opts = new[] {
                            "Gion Matsuri — Kyoto's month-long float procession in July",
                            "Tenjin Matsuri's river procession in Osaka",
                            "Tokushima's Awa dance",
                            "Tokyo's three shrines around Asakusa"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "天神祭", Reading = "Tenjin matsuri",
                        Romaji = "ten-jin ma-tsu-ri", Say = "天神祭",
                        Pic = "Kansai-Festivals & events-Tenjin Matsuri.png",
                        Note = "天神 is Sugawara no Michizane, the god of learning. Osaka puts his portable shrine on a boat, lights the river and lets the fireworks finish the night.",
                        Answer = 0,
                        Opts = new[] {
                            "Tenjin Matsuri — Osaka's riverborne shrine boat and fireworks",
                            "Kyoto's costumed Festival of Ages",
                            "Aomori's Nebuta floats",
                            "Kōchi's yosakoi dance"
                        }
                    },
                },
            },

            /* ================================================================
               CHŪGOKU — 中国
               ================================================================ */
            ["chugoku"] = new QuizSet {
                Stamp = "和",
                Ranks = new[] {
                    "平和の達人",
                    "そば 上手",
                    "Chūgoku 見習い",
                    "もみじ beginner"
                },
                Tips = new[] {
                    "A perfect run. The Genbaku Dome in Hiroshima is a World Heritage site left exactly as it stood — nothing was cleared away.",
                    "Almost there. 出雲そば comes in 重ね, stacked trays: the buckwheat noodles of the province the gods walk in.",
                    "Good start. 厳島神社 is one of Japan's Three Great Views — the torii stands in the sea and looks like it floats.",
                    "No matter — the kanji stick after two rounds. Press play again."
                },
                Rounds = new() {
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "広島風お好み焼き", Reading = "Hiroshima-style okonomiyaki",
                        Romaji = "hi-ro-shi-ma-fuu o-ko-no-mi ya-ki", Say = "広島風お好み焼き",
                        Pic = "Chūgoku-Regional cuisine-Hiroshima-style Okonomiyaki.png",
                        Note = "Hiroshima stacks where Osaka mixes. Noodles and cabbage go in layers between the batter and the egg, and the そば — buckwheat noodles — is the giveaway.",
                        Answer = 0,
                        Opts = new[] {
                            "Hiroshima-style okonomiyaki — layered with noodles, not mixed",
                            "Osaka's batter-and-cabbage mix",
                            "Izumo's noodles in stacked lacquer trays",
                            "Kurashiki's millet dumplings"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "出雲そば", Reading = "Izumo soba",
                        Romaji = "i-zu-mo so-ba", Say = "出雲そば",
                        Pic = "Chūgoku-Regional cuisine-Izumo Soba.png",
                        Note = "そば is buckwheat. The dry San'in hills grow it well, and it is served in 重ね — stacks of lacquer trays — one on top of another.",
                        Answer = 0,
                        Opts = new[] {
                            "Izumo soba — dark buckwheat noodles in stacked lacquer trays",
                            "Hiroshima's layered pancake",
                            "Hakata's cloudy pork-bone broth",
                            "Kurashiki's canal quarter"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "もみじ饅頭", Reading = "Momiji manjū",
                        Romaji = "mo-mi-ji man-juu", Say = "もみじ饅頭",
                        Pic = "Chūgoku-Regional cuisine-Momiji Manju.png",
                        Note = "もみじ is the maple leaf, 饅頭 is the steamed bun. On Miyajima they are baked in a waffle-like iron, so the leaf is pressed into the crust.",
                        Answer = 0,
                        Opts = new[] {
                            "Momiji manjū — maple-leaf steam buns from Miyajima",
                            "Fugu, the pufferfish of Shimonoseki",
                            "Hiroshima's grilled oysters",
                            "Tottori's desert dunes"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Shrine", Ja = "厳島神社", Reading = "Itsukushima Shrine",
                        Romaji = "i-tsu-ku-shi-ma jin-ja", Say = "厳島神社",
                        Pic = "Chūgoku-Must-see highlights-Itsukushima Shrine (Miyajima).png",
                        Note = "The torii is planted in the seabed, so at high tide the whole gate appears to float — one of Japan's Three Great Views, and a sight the shrine has been protecting for centuries.",
                        Answer = 0,
                        Opts = new[] {
                            "Itsukushima Shrine — the torii gate that seems to float on the tide",
                            "Izumo Taisha, where the gods gather",
                            "Tottori's sand dunes",
                            "Hiroshima's atomic-bomb dome"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Shrine", Ja = "出雲大社", Reading = "Izumo Taisha",
                        Romaji = "i-zu-mo tai-sha", Say = "出雲大社",
                        Pic = "Chūgoku-Must-see highlights-Izumo Taisha.png",
                        Note = "出雲 is an old province and the shrine is said to hold the largest 注連縄, the sacred rope, in Japan. Every November the gods are believed to travel here.",
                        Answer = 0,
                        Opts = new[] {
                            "Izumo Taisha — where Japan's gods gather each November",
                            "Itsukushima's floating torii",
                            "Kurashiki's merchant canal",
                            "Miyajima's maple buns"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Place", Ja = "倉敷美観地区", Reading = "Kurashiki Bikan quarter",
                        Romaji = "ku-ra-shi-ki bi-kan chi-ki", Say = "倉敷美観地区",
                        Pic = "Chūgoku-Must-see highlights-Kurashiki Bikan Historical Quarter.png",
                        Note = "倉 is a storehouse. 倉敷 was a rice port, and its 白壁 — white-walled 土蔵 storehouses — were kept rather than pulled down, which is why the canal still works as a picture.",
                        Answer = 0,
                        Opts = new[] {
                            "Kurashiki Bikan — the white-walled merchant warehouses by the canal",
                            "Tottori's sand dunes",
                            "Hiroshima's peace park",
                            "Nara's laughing deer"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "平和記念式典", Reading = "Hiroshima peace ceremony",
                        Romaji = "hei-wa ki-nen shi-ke-ten", Say = "平和記念式典",
                        Pic = "Chūgoku-Festivals & events-Peace Memorial Ceremony.png",
                        Note = "平和 means peace. On the 6th of August paper 灯籠 are set afloat on the Ota River, and the names of the dead travel out with the current.",
                        Answer = 0,
                        Opts = new[] {
                            "Hiroshima's Peace Memorial — lanterns floated on the river on August 6",
                            "Itsukushima's sea fireworks",
                            "Tokushima's Awa dance",
                            "Kyoto's Gion float procession"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "神在祭", Reading = "Kamiari (the gods' gathering)",
                        Romaji = "ka-mi-a-ri", Say = "神在祭",
                        Pic = "Chūgoku-Festivals & events-Izumo Taisha Kamiari Festival.png",
                        Note = "神 is god, 在 is to be — 神在祭 marks the month when every god in Japan leaves home and gathers at Izumo. In the old calendar that month was the eleventh.",
                        Answer = 0,
                        Opts = new[] {
                            "Kamiari — the festival of the gods gathering at Izumo",
                            "Tottori's dance festival in August",
                            "Naha's 200-metre tug-of-war rope",
                            "Nagoya's parade of three unifiers"
                        }
                    },
                },
            },

            /* ================================================================
               SHIKOKU — 四国
               ================================================================ */
            ["shikoku"] = new QuizSet {
                Stamp = "四",
                Ranks = new[] {
                    "八十八 満願",
                    "かずら 上手",
                    "Shikoku 見習い",
                    "かずら beginner"
                },
                Tips = new[] {
                    "A perfect run. You know the pilgrimage now: 八十八箇所, some 1,200 kilometres, in the footsteps of the monk Kūkai.",
                    "Almost there. かずら is the wild wisteria woven into the Iya bridges — no nails and no planks, only vines and a very good rope.",
                    "Good start. 讃岐, Sanuki, is old Kagawa, and its udon is chewier and colder than anywhere else in Japan.",
                    "No matter — the words stick after two rounds. Press play again."
                },
                Rounds = new() {
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "讃岐うどん", Reading = "Sanuki udon",
                        Romaji = "sa-nu-ki u-don", Say = "讃岐うどん",
                        Pic = "Shikoku-Regional cuisine-Sanuki Udon.png",
                        Note = "讃岐 is the old province of Kagawa; うどん is the noodle. Kagawa has more udon shops per person than anywhere else in Japan — a shop every few streets.",
                        Answer = 0,
                        Opts = new[] {
                            "Sanuki udon — Kagawa's firm, chewy noodles",
                            "Kōchi's seared bonito",
                            "Ehime's skewers grilled on a hot plate",
                            "Tokushima's five-colour noodles"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "かつおのたたき", Reading = "Katsuo no tataki",
                        Romaji = "ka-tsu-o no ta-ta-ki", Say = "かつおのたたき",
                        Pic = "Shikoku-Regional cuisine-Katsuo no Tataki.png",
                        Note = "たたき means pounded. Kōchi sears the bonito skin-side on a hot plank, chops it, and dresses it with garlic and citrus — the raw inside is the point.",
                        Answer = 0,
                        Opts = new[] {
                            "Katsuo no tataki — Kōchi's seared-and-sliced bonito",
                            "Sanuki's cold noodles",
                            "Kumamoto's mustard-stuffed lotus root",
                            "Nagasaki's mixed noodles"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "すだち", Reading = "Sudachi (green citrus)",
                        Romaji = "su-da-chi", Say = "すだち",
                        Pic = "Shikoku-Regional cuisine-Sudachi Citrus.png",
                        Note = "酢 is vinegar, 橘 is a tangerine. Sudachi is a thumb-sized green citrus from Tokushima, used exactly where the rest of Japan would squeeze vinegar.",
                        Answer = 0,
                        Opts = new[] {
                            "Sudachi — Tokushima's tart green citrus, used like vinegar",
                            "Kōchi's seared bonito",
                            "Nagoya's miso cutlet",
                            "Kyoto's simmered tofu hot pot"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Place", Ja = "祖谷のかずら橋", Reading = "Iya kazura (vine) bridges",
                        Romaji = "i-ya no ka-zu-ra ha-shi", Say = "祖谷のかずら橋",
                        Pic = "Shikoku-Must-see highlights-Iya Valley Vine Bridges.png",
                        Note = "かずら is the wild wisteria the bridges are woven from, hand-bound into a rope thick enough to walk on. In the Edo period this gorge was a refuge for defeated samurai.",
                        Answer = 0,
                        Opts = new[] {
                            "Iya's kazura bridges — wisteria vines woven into suspension bridges",
                            "Dōgo Onsen's bathhouse of spirits",
                            "Naruto's tidal whirlpools",
                            "Mount Aso's smoking caldera"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Nature", Ja = "鳴門の渦潮", Reading = "Naruto no utashio (whirlpools)",
                        Romaji = "na-ru-to no u-da-shi-o", Say = "鳴門の渦潮",
                        Pic = "Shikoku-Must-see highlights-Naruto Whirlpools.png",
                        Note = "鳴門 is the gate of the strait; 渦 is whirlpool and 潮 is tide. Two seas push against each other here and pull the water into spirals that can swallow a boat.",
                        Answer = 0,
                        Opts = new[] {
                            "Naruto's whirlpools — spiralling tides in the strait",
                            "Iya's vine bridges",
                            "Ritsurin's strolling pond garden",
                            "Beppu's boiling ponds"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Pilgrimage", Ja = "四国八十八箇所", Reading = "The 88 Temple Pilgrimage",
                        Romaji = "shi-ko-ku ha-choo-juu-ka-sho", Say = "四国八十八箇所",
                        Pic = "Shikoku-Must-see highlights-88 Temple Pilgrimage.png",
                        Note = "箇所 is the counting word for places. 88 sacred sites, roughly 1,200 kilometres, walked in the footsteps of the monk Kūkai — 空海, sky sea — who was born on this island.",
                        Answer = 0,
                        Opts = new[] {
                            "The 88 Temple Pilgrimage — a 1,200 km loop of Kūkai's temples",
                            "Awa Odori's dancing streets",
                            "Kōchi's yosakoi",
                            "Marugame's paper fans"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "阿波踊り", Reading = "Awa odori",
                        Romaji = "a-wa o-do-ri", Say = "阿波踊り",
                        Pic = "Shikoku-Festivals & events-Awa Odori.png",
                        Note = "阿波 is the old province of Tokushima, 踊り is dance, and the whole city joins in: come along, we are dancing. It lands in Obon, mid-August.",
                        Answer = 0,
                        Opts = new[] {
                            "Awa Odori — Tokushima's Obon dance that takes over the whole city",
                            "Niihama's giant taiko drums",
                            "Kyoto's Gion floats",
                            "Naha's tug-of-war"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "よさこい祭り", Reading = "Yosakoi",
                        Romaji = "yo-sa-ko-i", Say = "よさこい祭り",
                        Pic = "Shikoku-Festivals & events-Kōchi Yosakoi Festival.png",
                        Note = "Kōchi borrowed a dance from Tokushima in the 1950s, added a flower hat and a chant, and sent it out again — Hokkaido later copied it back as the Sōran.",
                        Answer = 0,
                        Opts = new[] {
                            "Kōchi Yosakoi — dancers in flower hats shouting yosakoi",
                            "Awa Odori's Obon troupes",
                            "Hakata's racing floats",
                            "Izumo's gathering gods"
                        }
                    },
                },
            },

            /* ================================================================
               KYŪSHŪ — 九州
               ================================================================ */
            ["kyushu"] = new QuizSet {
                Stamp = "火",
                Ranks = new[] {
                    "阿蘇 火口",
                    "とんこつ 上手",
                    "Kyūshū 見習い",
                    "らーめん beginner"
                },
                Tips = new[] {
                    "A perfect run. Stand on the Aso crater rim in the morning and the ground hums — the island is still building itself.",
                    "Almost there. とんこつ is 豚, pig, plus 骨, bone: a broth boiled until the bones give up and the water clouds.",
                    "Good start. 別府 wells up more hot water than anywhere in Japan, and its 地獄 are eight boiling ponds you are not meant to enter.",
                    "No matter — the words stick after two rounds. Press play again."
                },
                Rounds = new() {
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "博多とんこつラーメン", Reading = "Hakata tonkotsu rāmen",
                        Romaji = "ha-ka-to ton-ko-tsu raa-men", Say = "博多とんこつラーメン",
                        Pic = "Kyūshū-Regional cuisine-Hakata Tonkotsu Ramen.png",
                        Note = "豚 is pig, 骨 is bone. Tonkotsu is pork bone boiled for hours until the broth clouds; Hakata's version thickens the noodles as thin as thread.",
                        Answer = 0,
                        Opts = new[] {
                            "Hakata tonkotsu — the cloudy pork-bone ramen of Fukuoka",
                            "Nagasaki's Chinese-influenced noodles",
                            "Kagoshima's black-pork cutlet",
                            "Sendai's grilled ox tongue"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "長崎ちゃんぽん", Reading = "Nagasaki champon",
                        Romaji = "na-ga-sa-ki chan-pon", Say = "長崎ちゃんぽん",
                        Pic = "Kyūshū-Regional cuisine-Nagasaki Champon.png",
                        Note = "ちゃんぽん is the Nagasaki word for a mixed bowl. The port's Chinese students and merchants put one noodle dish together out of every port they had traded with.",
                        Answer = 0,
                        Opts = new[] {
                            "Nagasaki champon — one bowl borrowing every port's noodles",
                            "Hakata's pork-bone broth",
                            "Kumamoto's fried lotus root",
                            "Kōchi's seared bonito"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "からし蓮根", Reading = "Karashi renkon",
                        Romaji = "ka-ra-shi ren-kon", Say = "からし蓮根",
                        Pic = "Kyūshū-Regional cuisine-Karashi Renkon.png",
                        Note = "からし is mustard, 蓮根 is lotus root. Kumamoto slices the root, packs it with mustard-miso, fries it — and the first bite still tingles.",
                        Answer = 0,
                        Opts = new[] {
                            "Karashi renkon — Kumamoto's lotus root stuffed with mustard miso",
                            "Kagoshima's shōchū",
                            "Iwate's bottomless soba",
                            "Osaka's breaded skewers"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Nature", Ja = "阿蘇山", Reading = "Mount Aso",
                        Romaji = "a-so-san", Say = "阿蘇山",
                        Pic = "Kyūshū-Must-see highlights-Mount Aso.png",
                        Note = "阿蘇 is one of the world's largest calderas: a crater rim big enough to hold five towns, with a smoking cone inside and grasslands you can walk across.",
                        Answer = 0,
                        Opts = new[] {
                            "Mount Aso — one of the world's largest volcanic calderas",
                            "Sakurajima, the erupting cone over Kagoshima",
                            "Beppu's boiling hells",
                            "Yufuin's art-museum lanes"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Onsen", Ja = "別府温泉", Reading = "Beppu hot springs",
                        Romaji = "be-p-pu on-sen", Say = "別府温泉",
                        Pic = "Kyūshū-Must-see highlights-Beppu Hot Springs.png",
                        Note = "別府 was once 別能, another province. It wells up more hot water than anywhere in Japan, and its 地獄 — hells — are vividly coloured pools kept for looking at, not bathing.",
                        Answer = 0,
                        Opts = new[] {
                            "Beppu — Japan's top hot-spring town and its boiling hells",
                            "Mount Aso's caldera rim",
                            "Nagasaki's hillside of foreign residences",
                            "Dōgo's bathhouse of spirits"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Nature", Ja = "桜島", Reading = "Sakurajima",
                        Romaji = "saku-ra-ji-ma", Say = "桜島",
                        Pic = "Kyūshū-Must-see highlights-Sakurajima.png",
                        Note = "桜 is cherry, 島 is island. It looks like a quiet cone above Kagoshima Bay, but it throws grey ash over the city most mornings and erupts without much notice.",
                        Answer = 0,
                        Opts = new[] {
                            "Sakurajima — the active volcano looming over Kagoshima Bay",
                            "Yufuin's boutique onsen town",
                            "Aso's caldera rim",
                            "Hakata's yatai stalls"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "博多祇園山笠", Reading = "Hakata Gion Yamakasa",
                        Romaji = "ha-ka-to gi-on ya-ma-ka-sa", Say = "博多祇園山笠",
                        Pic = "Kyūshū-Festivals & events-Hakata Gion Yamakasa.png",
                        Note = "山 is mountain, 笠 is a bamboo hat. The 山笠 is a float piled into the shape of hats, and the teams carry it shoulder-first through streets too narrow for wheels.",
                        Answer = 0,
                        Opts = new[] {
                            "Hakata Gion Yamakasa — teams race with massive hat-shaped floats",
                            "Ohara Matsuri's Kagoshima dancing",
                            "Nagasaki's dragon dance",
                            "Awa Odori's troupes"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "長崎くんち", Reading = "Nagasaki Kunchi",
                        Romaji = "na-ga-sa-ki kun-chi", Say = "長崎くんち",
                        Pic = "Kyūshū-Festivals & events-Nagasaki Kunchi.png",
                        Note = "くんち is the festival of the 講, the mutual-help groups that once ran the Chinese quarter. A dragon as long as a street leads the procession down to the water.",
                        Answer = 0,
                        Opts = new[] {
                            "Nagasaki Kunchi — the dragon festival of the old trading port",
                            "Karatsu's lacquered lion floats",
                            "Hakata's hat-shaped floats",
                            "Itsukushima's floating torii"
                        }
                    },
                },
            },

            /* ================================================================
               OKINAWA — 沖縄
               ================================================================ */
            ["okinawa"] = new QuizSet {
                Stamp = "琉",
                Ranks = new[] {
                    "琉球 王",
                    "チャンプルー 上手",
                    "Okinawa 見習い",
                    "泡盛 beginner"
                },
                Tips = new[] {
                    "A perfect run. ゴーヤー is the island word for bitter melon — one word, and a whole way of cooking with tofu and pork.",
                    "Almost there. チャンプルー means mixed up. Anything goes in the pan, and that is the compliment, not the complaint.",
                    "Good start. 首里城 was the Ryukyu Kingdom's palace for some 450 years, and it was never a Japanese castle.",
                    "No matter — the words stick after two rounds. Press play again."
                },
                Rounds = new() {
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "ゴーヤーチャンプルー", Reading = "Goya chanpurū",
                        Romaji = "goo-yaa chan-pu-ruu", Say = "ゴーヤーチャンプルー",
                        Pic = "Okinawa-Regional cuisine-Goya Chanpurū.png",
                        Note = "ゴーヤー is the islands' own word for bitter melon; チャンプルー is a stir-fry with no fixed recipe. Tofu and pork go in, and the pan does the rest.",
                        Answer = 0,
                        Opts = new[] {
                            "Goya chanpurū — Okinawa's bitter-melon stir-fry with tofu and pork",
                            "Wheat noodles in pork broth topped with ribs",
                            "Pork belly slow-braised in awamori",
                            "Deep-fried doughnuts, denser than mainland ones"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Local food", Ja = "ラフテー", Reading = "Rafute (braised pork belly)",
                        Romaji = "ra-fu-tee", Say = "ラフテー",
                        Pic = "Okinawa-Regional cuisine-Rafute.png",
                        Note = "Rafute is the islands' answer to the mainland's kakuni: pork belly simmered for hours in awamori, soy and sugar until a chopstick goes through it.",
                        Answer = 0,
                        Opts = new[] {
                            "Rafute — pork belly braised in awamori until it melts",
                            "Bitter-melon stir-fry with tofu",
                            "Wheat soba with stewed pork ribs",
                            "Taco fillings served over rice"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Drink", Ja = "泡盛", Reading = "Awamori",
                        Romaji = "a-wa-mo-ri", Say = "泡盛",
                        Pic = "Okinawa-Regional cuisine-Awamori.png",
                        Note = "泡 is foam, 盛 is a bowlful. Okinawa's rice spirit, distilled here and often left in clay for decades — older to these islands than sake ever was.",
                        Answer = 0,
                        Opts = new[] {
                            "Awamori — Okinawa's distilled rice spirit, sometimes aged decades",
                            "Kyūshū's sweet-potato shōchū",
                            "Sapporo's beer",
                            "Nagoya's miso sake"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Place", Ja = "首里城", Reading = "Shuri Castle",
                        Romaji = "shu-ri-jo", Say = "首里城",
                        Pic = "Okinawa-Must-see highlights-Shuri Castle.png",
                        Note = "首里 was the seat of the Ryukyu Kingdom for some 450 years. The red tiles and the シーサー lion-dogs are Ryukyuan, not mainland Japanese — that is the whole point of the building.",
                        Answer = 0,
                        Opts = new[] {
                            "Shuri Castle — the royal palace of the Ryukyu Kingdom",
                            "Churaumi's whale-shark tank",
                            "Kerama Islands' clear water",
                            "The Itoman peace memorial"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Aquarium", Ja = "美ら海水族館", Reading = "Churaumi Aquarium",
                        Romaji = "chuu-ra-u-mi", Say = "美ら海水族館",
                        Pic = "Okinawa-Must-see highlights-Churaumi Aquarium.png",
                        Note = "美ら海 is a deliberate misspelling of 美しい海 — the beautiful sea — a name invented for the aquarium when it opened in 2000.",
                        Answer = 0,
                        Opts = new[] {
                            "Churaumi Aquarium — the whale-shark tank of Okinawa's beautiful sea",
                            "Shuri Castle's tiled roofs",
                            "Naha's dragon boats",
                            "Ishigaki's coral reefs"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Nature", Ja = "慶良間諸島", Reading = "Kerama Islands",
                        Romaji = "ke-ra-ma sho-too", Say = "慶良間諸島",
                        Pic = "Okinawa-Must-see highlights-Kerama Islands.png",
                        Note = "諸島 means an island group. The Keramas are a national park of near-perfect visibility, and humpback whales come here to breed every winter.",
                        Answer = 0,
                        Opts = new[] {
                            "Kerama Islands — clear-water island park and winter whale ground",
                            "Churaumi's big tank",
                            "Shuri's royal tiles",
                            "Miyako's sand bars"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "那覇大綱挽", Reading = "Naha Great Tug-of-War",
                        Romaji = "na-ha oo-tsu-na-bi-ki", Say = "那覇大綱挽",
                        Pic = "Okinawa-Festivals & events-Naha Great Tug-of-War.png",
                        Note = "綱 is a rope, 挽く is to pull. The rope is over 200 metres of twisted straw and weighs more than a small car — one of the largest tug-of-wars in the world.",
                        Answer = 0,
                        Opts = new[] {
                            "Naha Great Tug-of-War — a rope longer than two football pitches",
                            "Eisā's drum and dance",
                            "Naha Hārī's dragon boats",
                            "Hakata's festival floats"
                        }
                    },
                    new RegionQuizRound {
                        Kind = "Festival", Ja = "那覇ハーリー", Reading = "Naha Hārī (dragon boats)",
                        Romaji = "na-haa hee-ree", Say = "那覇ハーリー",
                        Pic = "Okinawa-Festivals & events-Naha Hārī.png",
                        Note = "ハーリー is the Okinawan word for the dragon boat itself. The races in Naha harbour trace back to the Ryukyu Kingdom's trading ships, and the drum boat leads the field.",
                        Answer = 0,
                        Opts = new[] {
                            "Naha Hārī — dragon boats racing in Naha harbour, Ryukyu's sea legacy",
                            "The 200-metre tug-of-war rope",
                            "Eisā's Obon drumming",
                            "Kerama's whale season"
                        }
                    },
                },
            },
        };

        /// <summary>The game for one region, or an empty set when the slug is unknown.</summary>
        public static QuizSet Get(string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return new QuizSet();

            if (!_all.TryGetValue(slug.ToLowerInvariant(), out var set))
            {
                // Unknown slug — hand back an empty game rather than blowing up.
                return new QuizSet();
            }

            return set;
        }
    }
}
