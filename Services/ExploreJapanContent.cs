using NihongoAcademy.Models.ExploreJapan;

namespace NihongoAcademy.Services
{
    /// <summary>
    /// Static demo content for the Explore Japan module.
    /// 6 entries per category, covering the most well-known items
    /// so the page feels alive without needing a DB.
    /// </summary>
    public static class ExploreJapanContent
    {
        public static List<ExploreItem> Regions() => new()
        {
            new() {
                Icon = "bi-snow", Title = "Hokkaido", TitleJa = "北海道",
                Description = "Snow country, volcanic hot springs, fresh seafood, and Daisetsuzan National Park.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Hokkaido"
            },
            new() {
                Icon = "bi-tree", Title = "Tōhoku", TitleJa = "東北",
                Description = "Mountain heartland of samurai castles, summer Nebuta festivals, and quiet onsen towns.",
                Accent = "na-secondary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/T%C5%8Dhoku_region"
            },
            new() {
                Icon = "bi-building", Title = "Kantō", TitleJa = "関東",
                Description = "Tokyo, Yokohama, ancient Edo — Japan's most populous, most modern region.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Kant%C5%8D_region"
            },
            new() {
                Icon = "bi-triangle", Title = "Chūbu", TitleJa = "中部",
                Description = "Mt. Fuji, the Japanese Alps, Nagoya, and the country's geographic center.",
                Accent = "na-secondary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Ch%C5%ABbu_region"
            },
            new() {
                Icon = "bi-torii", Title = "Kansai", TitleJa = "関西",
                Description = "Kyoto, Osaka, Nara — classical Japan, samurai history, and street-food capital.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Kansai_region"
            },
            new() {
                Icon = "bi-volcano", Title = "Kyūshū", TitleJa = "九州",
                Description = "Volcanic island of hot springs, tonkotsu ramen, and samurai heritage.",
                Accent = "na-secondary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Ky%C5%ABsh%C5%AB"
            }
        };

        public static List<ExploreItem> Food() => new()
        {
            new() {
                Icon = "bi-circle-fill", Title = "Sushi", TitleJa = "寿司",
                Description = "Vinegared rice with the freshest seafood — born in Edo, loved worldwide.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Sushi"
            },
            new() {
                Icon = "bi-droplet-fill", Title = "Ramen", TitleJa = "ラーメン",
                Description = "Regional noodle soups — from Hokkaido miso to Hakata tonkotsu.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Ramen"
            },
            new() {
                Icon = "bi-flower2", Title = "Tempura", TitleJa = "天ぷら",
                Description = "Lightly battered seafood and vegetables in delicate crisp.",
                Accent = "na-gold",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Tempura"
            },
            new() {
                Icon = "bi-square-fill", Title = "Okonomiyaki", TitleJa = "お好み焼き",
                Description = "Savory cabbage pancake — both Osaka-style and Hiroshima-style are wonderful.",
                Accent = "na-gold",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Okonomiyaki"
            },
            new() {
                Icon = "bi-award-fill", Title = "Wagyu", TitleJa = "和牛",
                Description = "Heavily marbled Japanese beef — rich, buttery, melt-in-mouth.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Wagyu"
            },
            new() {
                Icon = "bi-circle-fill", Title = "Takoyaki", TitleJa = "たこ焼き",
                Description = "Octopus-filled batter balls, brushed with sauce — soul of Osaka street food.",
                Accent = "na-gold",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Takoyaki"
            }
        };

        public static List<ExploreItem> Festivals() => new()
        {
            new() {
                Icon = "bi-flower1", Title = "Hanami", TitleJa = "花見",
                Description = "Cherry-blossom viewing parties — picnics under sakura each spring.",
                Accent = "na-sakura",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Hanami"
            },
            new() {
                Icon = "bi-flag", Title = "Gion Matsuri", TitleJa = "祇園祭",
                Description = "Kyoto's grand summer float procession through the old capital in July.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Gion_Matsuri"
            },
            new() {
                Icon = "bi-stars", Title = "Tanabata", TitleJa = "七夕",
                Description = "Wish-writing on bamboo strips, the star-crossed lovers festival on July 7.",
                Accent = "na-sakura",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Tanabata"
            },
            new() {
                Icon = "bi-house-heart", Title = "Obon", TitleJa = "お盆",
                Description = "Honoring ancestor spirits returning home with lantern dances, mid-August.",
                Accent = "na-gold",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Obon"
            },
            new() {
                Icon = "bi-snow", Title = "Sapporo Snow Festival", TitleJa = "さっぽろ雪まつり",
                Description = "Massive ice and snow sculptures transforming Hokkaido each February.",
                Accent = "na-indigo",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Sapporo_Snow_Festival"
            },
            new() {
                Icon = "bi-fire", Title = "Nebuta Matsuri", TitleJa = "ねぶた祭",
                Description = "Glowing giant paper floats parade through Aomori streets each August.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Nebuta_Matsuri"
            }
        };

        public static List<ExploreItem> Dialects() => new()
        {
            new() {
                Icon = "bi-mic", Title = "Kansai-ben", TitleJa = "関西弁",
                Description = "Osaka and Kyoto's melodic, theatrical speech — home of manzai comedy.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Kansai_dialect"
            },
            new() {
                Icon = "bi-soundwave", Title = "Tōhoku-ben", TitleJa = "東北弁",
                Description = "Northern rural dialect with rising intonation — warm and direct.",
                Accent = "na-secondary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/T%C5%8Dhoku_dialects"
            },
            new() {
                Icon = "bi-chat-dots", Title = "Hakata-ben", TitleJa = "博多弁",
                Description = "Friendly Kyushu dialect, colloquially tied to Fukuoka food culture.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Hakata_dialect"
            },
            new() {
                Icon = "bi-sun", Title = "Okinawa 方言", TitleJa = "うちなーぐち",
                Description = "Ryukyuan language family, distinct from mainland Japanese.",
                Accent = "na-gold",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Ryukyuan_languages"
            },
            new() {
                Icon = "bi-geo-alt", Title = "Nagoya-ben", TitleJa = "名古屋弁",
                Description = "Famous for phrases like \"だぎゃ\" and unique final particles.",
                Accent = "na-secondary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Nagoya_dialect"
            },
            new() {
                Icon = "bi-broadcast", Title = "Standard", TitleJa = "標準語",
                Description = "NHK news-style Tokyo speech — the educational \"default\" Japanese.",
                Accent = "na-indigo",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Standard_Japanese"
            }
        };

        public static List<ExploreItem> Landmarks() => new()
        {
            new() {
                Icon = "bi-triangle-fill", Title = "Mt. Fuji", TitleJa = "富士山",
                Description = "Iconic 3,776m stratovolcano — UNESCO World Heritage and Japan's ultimate symbol.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Mount_Fuji"
            },
            new() {
                Icon = "bi-broadcast-tower", Title = "Tokyo Tower", TitleJa = "東京タワー",
                Description = "Vermillion Eiffel-style tower in Minato, lighting Tokyo since 1958.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Tokyo_Tower"
            },
            new() {
                Icon = "bi-torii", Title = "Fushimi Inari", TitleJa = "伏見稲荷",
                Description = "Thousands of red torii gates winding up Kyoto's sacred mountain.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Fushimi_Inari-taisha"
            },
            new() {
                Icon = "bi-bank", Title = "Kiyomizu-dera", TitleJa = "清水寺",
                Description = "Famous wooden stage overlooking Kyoto — a millennium-old Buddhist temple.",
                Accent = "na-sakura",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Kiyomizu-dera"
            },
            new() {
                Icon = "bi-people-fill", Title = "Shibuya Crossing", TitleJa = "渋谷スクランブル",
                Description = "The world's busiest pedestrian scramble intersection, made for people-watching.",
                Accent = "na-indigo",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Shibuya_Crossing"
            },
            new() {
                Icon = "bi-gem", Title = "Kinkaku-ji", TitleJa = "金閣寺",
                Description = "The Golden Pavilion mirrored on its calm reflective pond.",
                Accent = "na-gold",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Kinkaku-ji"
            }
        };

        public static List<ExploreItem> Culture() => new()
        {
            new() {
                Icon = "bi-cup-straw", Title = "Tea Ceremony", TitleJa = "茶道",
                Description = "Mindful matcha ritual guided by 和敬清寂 — harmony, respect, purity, tranquility.",
                Accent = "na-secondary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Japanese_tea_ceremony"
            },
            new() {
                Icon = "bi-pencil", Title = "Calligraphy", TitleJa = "書道",
                Description = "Brush-and-ink writing as art — kanji as living strokes on paper.",
                Accent = "na-indigo",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Calligraphy"
            },
            new() {
                Icon = "bi-person-arms-up", Title = "Kimono", TitleJa = "着物",
                Description = "Traditional silk robe worn for ceremonies, festivals, and tea gatherings.",
                Accent = "na-sakura",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Kimono"
            },
            new() {
                Icon = "bi-tree", Title = "Bonsai", TitleJa = "盆栽",
                Description = "Miniaturized trees shaped patiently over decades — time as aesthetic.",
                Accent = "na-secondary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Bonsai"
            },
            new() {
                Icon = "bi-square", Title = "Origami", TitleJa = "折り紙",
                Description = "Paper folding — pure geometry and art from a single sheet.",
                Accent = "na-gold",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Origami"
            },
            new() {
                Icon = "bi-shield-shaded", Title = "Sumo", TitleJa = "相撲",
                Description = "Shinto-rooted wrestling ritual — Japan's national sport and spiritual tradition.",
                Accent = "na-primary",
                LearnMoreUrl = "https://en.wikipedia.org/wiki/Sumo"
            }
        };

        public static List<ExploreItem> For(string category) => category?.ToLowerInvariant() switch
        {
            "regions" => Regions(),
            "food" => Food(),
            "festivals" => Festivals(),
            "dialects" => Dialects(),
            "landmarks" => Landmarks(),
            "culture" => Culture(),
            _ => new List<ExploreItem>()
        };
    }
}
