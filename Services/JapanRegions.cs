using NihongoAcademy.Models.ExploreJapan;

namespace NihongoAcademy.Services
{
    /// <summary>
    /// Static data for the 9 traditional regions of Japan (9 地方).
    /// Ordered geographically from north (Hokkaido) to south (Okinawa).
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
                Tagline = "Snow country, volcanic hot springs, and the freshest seafood in Japan.",
                Description = "Japan's northernmost main island — a vast wilderness of volcanic peaks, "
                            + "caldera lakes, powder-snow ski resorts, and dairy farmland. Sapporo, the capital, "
                            + "is famous for its annual Snow Festival and miso ramen. Beyond the city lie "
                            + "Daisetsuzan National Park, the Shiretoko Peninsula, and onsen towns like Noboribetsu.",
                Highlights = new() {
                    new() { Title = "Sapporo Snow Festival", TitleJa = "さっぽろ雪まつり", Note = "Massive ice & snow sculptures each February" },
                    new() { Title = "Daisetsuzan National Park", TitleJa = "大雪山国立公園", Note = "Volcanic peaks, alpine flowers, sulfur vents" },
                    new() { Title = "Noboribetsu Onsen", TitleJa = "登別温泉", Note = "Volcanic-valley hot-spring town" },
                    new() { Title = "Shiretoko Peninsula", TitleJa = "知床半島", Note = "UNESCO wilderness, brown bears, drift ice" },
                    new() { Title = "Otaru Canal", TitleJa = "小樽運河", Note = "Historic herring port with glass shops" },
                },
                Foods = new() {
                    new() { Title = "Sapporo Miso Ramen", TitleJa = "札幌味噌ラーメン" },
                    new() { Title = "Soup Curry", TitleJa = "スープカレー" },
                    new() { Title = "Hokkaido Crab", TitleJa = "北海道産カニ" },
                    new() { Title = "Yubari Melon", TitleJa = "夕張メロン" },
                },
                Festivals = new() {
                    new() { Title = "Sapporo Snow Festival", TitleJa = "さっぽろ雪まつり", Note = "Early February" },
                    new() { Title = "Yosakoi Soran", TitleJa = "YOSAKOIソーラン", Note = "Late June, Sapporo" },
                },
                Access = "New Chitose Airport (CTS) is the main gateway. "
                       + "Hokkaido Shinkansen connects Hakodate to Aomori (extendable to Sapporo in future).",
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
                Tagline = "Mountain heartland of samurai castles, summer Nebuta festivals, and quiet onsen towns.",
                Description = "Tōhoku spans the six northern prefectures on Honshu's Pacific and Sea-of-Japan sides. "
                            + "Known for rugged mountains, apple orchards, sake breweries, and some of Japan's most "
                            + "vibrant summer festivals. The Yamagata, Akita, and Aomori regions hide remote "
                            + "onsen villages such as Ginzan Onsen and Nyūtō Onsen.",
                Highlights = new() {
                    new() { Title = "Nebuta Matsuri", TitleJa = "ねぶた祭", Note = "Giant glowing floats parade through Aomori in August" },
                    new() { Title = "Ginzan Onsen", TitleJa = "銀山温泉", Note = "Taisho-era silver-mining hot-spring town" },
                    new() { Title = "Matsushima Bay", TitleJa = "松島湾", Note = "One of Japan's Three Views, ~260 pine-clad islands" },
                    new() { Title = "Kakunodate", TitleJa = "角館", Note = "Historic samurai district with weeping cherry trees" },
                    new() { Title = "Hirosaki Castle", TitleJa = "弘前城", Note = "Famous for cherry-blossom petal moat" },
                },
                Foods = new() {
                    new() { Title = "Wanko Soba", TitleJa = "わんこそば", Note = "All-you-can-eat buckwheat noodles in Morioka" },
                    new() { Title = "Beef Tongue", TitleJa = "牛たん", Note = "Sendai's grilled gyutan" },
                    new() { Title = "Aomori Apple", TitleJa = "青森りんご" },
                    new() { Title = "Kiritanpo", TitleJa = "きりたんぽ", Note = "Mashed-rice skewers, Akita hot pot" },
                },
                Festivals = new() {
                    new() { Title = "Nebuta Matsuri", TitleJa = "ねぶた祭", Note = "2–7 August, Aomori" },
                    new() { Title = "Kanto Matsuri", TitleJa = "竿灯まつり", Note = "Mid August, Akita" },
                    new() { Title = "Sendai Tanabata", TitleJa = "仙台七夕", Note = "6–8 August, Sendai" },
                },
                Access = "Tōhoku Shinkansen connects Tokyo to Shin-Aomori in ~3 hours. "
                       + "Sendai Airport, Akita Airport and Aomori Airport serve the region.",
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
                Tagline = "Tokyo, Yokohama, ancient Edo — Japan's most populous and most modern region.",
                Description = "Kantō is the political and economic heart of modern Japan. Greater Tokyo alone "
                            + "houses over 37 million people. Beyond the capital, the region contains the ancient "
                            + "shogun capital Kamakura, the hot-spring resort of Hakone, the temple towns of "
                            + "Nikko, and Mount Fuji's eastern approaches. Edo-period culture and Shōwa-era "
                            + "neon coexist within a short train ride of each other.",
                Highlights = new() {
                    new() { Title = "Sensō-ji", TitleJa = "浅草寺", Note = "Tokyo's oldest Buddhist temple in Asakusa" },
                    new() { Title = "Meiji Shrine", TitleJa = "明治神宮", Note = "Shinto shrine in Yoyogi forest" },
                    new() { Title = "Kamakura Great Buddha", TitleJa = "鎌倉大仏", Note = "13m bronze statue, ~1252" },
                    new() { Title = "Hakone Onsen", TitleJa = "箱根温泉", Note = "Hot-spring resort with Mt. Fuji views" },
                    new() { Title = "Nikko Tōshō-gū", TitleJa = "日光東照宮", Note = "Opulently decorated Edo-era mausoleum" },
                },
                Foods = new() {
                    new() { Title = "Edo-mae Sushi", TitleJa = "江戸前寿司" },
                    new() { Title = "Monjayaki", TitleJa = "もんじゃ焼き", Note = "Tokyo street pancake" },
                    new() { Title = "Ramen", TitleJa = "ラーメン", Note = "Tokyo shoyu & tonkotsu" },
                    new() { Title = "Yokohama Iekei Ramen", TitleJa = "横浜家系" },
                },
                Festivals = new() {
                    new() { Title = "Sanja Matsuri", TitleJa = "三社祭", Note = "May, Asakusa" },
                    new() { Title = "Sumida River Fireworks", TitleJa = "隅田川花火大会", Note = "Last Saturday of July" },
                    new() { Title = "Kanda Matsuri", TitleJa = "神田祭", Note = "May, odd-numbered years" },
                },
                Access = "Two international airports — Narita (NRT) and Haneda (HND). "
                       + "Tōhoku, Jōetsu, and Hokuriku Shinkansen lines fan out from Tokyo Station.",
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
                Tagline = "Mt. Fuji, the Japanese Alps, Nagoya, and the country's geographic center.",
                Description = "Chūbu stretches across central Honshu, split into Kōshinetsu (inland), Tōkai "
                            + "(Pacific coast), and Hokuriku (Sea-of-Japan coast). It contains Japan's three "
                            + "best-known mountains — Mt. Fuji, Mt. Tate, Mt. Haku — along with the Northern, "
                            + "Central, and Southern Alps. Historic post-towns like Takayama and Shirakawa-gō "
                            + "preserve a gasshō-zukuri farmstead heritage that is now UNESCO-listed.",
                Highlights = new() {
                    new() { Title = "Mount Fuji", TitleJa = "富士山", Note = "3,776m, climbed Jul–Aug from Yoshida trail" },
                    new() { Title = "Shirakawa-gō", TitleJa = "白川郷", Note = "UNESCO gasshō-zukuri thatched farmhouses" },
                    new() { Title = "Kamikōchi", TitleJa = "上高地", Note = "Alpine valley in the Northern Alps" },
                    new() { Title = "Takayama Old Town", TitleJa = "高山古い町並み", Note = "Edo-era merchant district, morning markets" },
                    new() { Title = "Kenroku-en", TitleJa = "兼六園", Note = "One of Japan's three great gardens, Kanazawa" },
                },
                Foods = new() {
                    new() { Title = "Hōtō Noodles", TitleJa = "ほうとう", Note = "Flat-wheat noodle stew, Yamanashi" },
                    new() { Title = "Miso Katsu", TitleJa = "味噌カツ", Note = "Nagoya-style breaded pork cutlet" },
                    new() { Title = "Kishimen", TitleJa = "きしめん", Note = "Flat noodle in Nagoya" },
                    new() { Title = "Toshō-ji-style Soba", TitleJa = "五平餅", Note = "Walnut-miso rice cakes" },
                },
                Festivals = new() {
                    new() { Title = "Takayama Matsuri", TitleJa = "高山祭", Note = "Spring & autumn float festival" },
                    new() { Title = "Fuji Shibazakura", TitleJa = "富士芝桜", Note = "April–May moss-pink carpet" },
                },
                Access = "Centrair (NGO) near Nagoya, Komatsu (KMQ) and Toyama (TOY). "
                       + "Hokuriku Shinkansen links Tokyo with Kanazawa in ~2.5h.",
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
                Tagline = "Kyoto, Osaka, Nara — classical Japan, samurai history, and the street-food capital.",
                Description = "Kansai has been Japan's cultural cradle for over a millennium. Kyoto served as "
                            + "the imperial capital from 794 to 1868 and remains the spiritual home of "
                            + "traditional arts; Osaka is Japan's merchant capital and kitchen; Nara predates "
                            + "both and shelters the earliest Buddhist temples. Kansai-ben, the local dialect, "
                            + "is famously melodic and is the language of manzai comedy.",
                Highlights = new() {
                    new() { Title = "Fushimi Inari", TitleJa = "伏見稲荷", Note = "10,000+ red torii gates on Mt. Inari" },
                    new() { Title = "Kinkaku-ji", TitleJa = "金閣寺", Note = "Golden Pavilion reflected on its pond" },
                    new() { Title = "Arashiyama Bamboo", TitleJa = "嵐山竹林の小径", Note = "Iconic bamboo grove in western Kyoto" },
                    new() { Title = "Nara Park & Tōdai-ji", TitleJa = "奈良公園・東大寺", Note = "Friendly deer, Great Buddha Hall" },
                    new() { Title = "Dōtonbori", TitleJa = "道頓堀", Note = "Neon-lit Osaka street-food heart" },
                },
                Foods = new() {
                    new() { Title = "Okonomiyaki", TitleJa = "お好み焼き", Note = "Osaka-style savory cabbage pancake" },
                    new() { Title = "Takoyaki", TitleJa = "たこ焼き", Note = "Octopus batter balls, Osaka street food" },
                    new() { Title = "Kaiseki Ryōri", TitleJa = "懐石料理", Note = "Multi-course Kyoto haute cuisine" },
                    new() { Title = "Matcha Sweets", TitleJa = "抹茶スイーツ", Note = "Uji green-tea confections" },
                },
                Festivals = new() {
                    new() { Title = "Gion Matsuri", TitleJa = "祇園祭", Note = "Kyoto, entire July" },
                    new() { Title = "Tenjin Matsuri", TitleJa = "天神祭", Note = "Late July, Osaka — one of Japan's three great festivals" },
                    new() { Title = "Aoi Matsuri", TitleJa = "葵祭", Note = "15 May, Kyoto" },
                },
                Access = "Kansai International Airport (KIX), Itami (ITM), Kobe (UKY). "
                       + "Tōkaidō & San'yō Shinkansen connect Osaka/Kyoto to Tokyo in ~2h 15m.",
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
                Tagline = "Hiroshima, Miyajima, and the Izumo mythology road — Japan's western flank.",
                Description = "Chūgoku occupies western Honshu and is split into the Sanyō (Pacific / Inland-Sea) "
                            + "side with its industrial cities, and the San'in (Sea-of-Japan) side with its "
                            + "fishing ports, sand dunes, and onsen. The region contains Izumo Taisha — "
                            + "Shinto's second-most-important shrine — as well as Hiroshima's Peace Memorial, "
                            + "the UNESCO island of Miyajima, and the historic castle town of Matsue.",
                Highlights = new() {
                    new() { Title = "Miyajima & Itsukushima", TitleJa = "宮島・厳島", Note = "Floating torii gate, UNESCO" },
                    new() { Title = "Hiroshima Peace Memorial", TitleJa = "原爆ドーム", Note = "UNESCO, museum & Genbaku Dome" },
                    new() { Title = "Izumo Taisha", TitleJa = "出雲大社", Note = "Shinto shrine of marriage, October kami gathering" },
                    new() { Title = "Matsue Castle", TitleJa = "松江城", Note = "One of Japan's 12 surviving original castles" },
                    new() { Title = "Tottori Sand Dunes", TitleJa = "鳥取砂丘", Note = "Japan's largest sand dunes, Sea-of-Japan coast" },
                },
                Foods = new() {
                    new() { Title = "Hiroshima-style Okonomiyaki", TitleJa = "広島風お好み焼き", Note = "Layered pancake with noodles" },
                    new() { Title = "Oysters", TitleJa = "牡蠣", Note = "Hiroshima Bay oysters" },
                    new() { Title = "Izumo Sōmen", TitleJa = "出雲そば" },
                },
                Festivals = new() {
                    new() { Title = "Miyajima Kangensai", TitleJa = "宮島管弦祭", Note = "Late July, Itsukushima" },
                    new() { Title = "On-Matsuri", TitleJa = "おん祭", Note = "Early December, Kasuga Shrine" },
                },
                Access = "Hiroshima Airport (HIJ), Okayama (OKJ), Yonago (YGJ). "
                       + "San'yō Shinkansen connects Shin-Osaka to Hakata via Hiroshima.",
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
                Tagline = "The smallest main island — 88-temple pilgrimage, Naruto whirlpools, and Iya Valley.",
                Description = "Shikoku is Japan's fourth-largest island and is the spiritual home of the "
                            + "88-temple Shikoku Henro pilgrimage. It is divided into four prefectures: "
                            + "Tokushima (famous for Awa Odori dance), Kagawa (udon capital), Ehime (Dōgo "
                            + "Onsen, Matsuyama Castle), and Kōchi (the Pacific coast and Shimanto). The "
                            + "Iya Valley in the interior remains one of Japan's three hidden mountain regions.",
                Highlights = new() {
                    new() { Title = "Dōgo Onsen", TitleJa = "道後温泉", Note = "3,000-year-old hot spring in Matsuyama" },
                    new() { Title = "Naruto Whirlpools", TitleJa = "鳴門海峡の渦潮", Note = "Tidal eddies visible from bridges & boats" },
                    new() { Title = "Shikoku 88 Pilgrimage", TitleJa = "四国八十八ヶ所", Note = "1,200km circular Buddhist pilgrimage" },
                    new() { Title = "Iya Valley", TitleJa = "祖谷渓", Note = "Mountain gorges with vine bridges" },
                    new() { Title = "Kōchi Castle", TitleJa = "高知城", Note = "Twelve original castles still standing" },
                },
                Foods = new() {
                    new() { Title = "Sanuki Udon", TitleJa = "讃岐うどん", Note = "Kagawa's thick, chewy wheat noodles" },
                    new() { Title = "Katsuo Tataki", TitleJa = "鰹のタタキ", Note = "Seared bonito with citrus" },
                    new() { Title = "Awa Odori Sake", TitleJa = "阿波踊り" },
                },
                Festivals = new() {
                    new() { Title = "Awa Odori", TitleJa = "阿波踊り", Note = "12–15 August, Tokushima — Japan's largest dance festival" },
                    new() { Title = "Niihama Taiko", TitleJa = "新居浜太鼓祭り", Note = "Mid October, giant drum floats" },
                },
                Access = "Three bridge systems connect Shikoku to Honshu (Seto-Ōhashi, "
                       + "Kojima–Sakaide) and to Kyūshū (Kanmon Bridge). Tokushima Airport (TKS), "
                       + "Matsuyama (MYJ), Kōchi (KCZ), Takamatsu (TAK).",
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
                Tagline = "Volcanic island of hot springs, tonkotsu ramen, and samurai heritage.",
                Description = "Kyūshū is Japan's southern gateway, sitting closer to the Asian mainland than any "
                            + "other part of the country. It contains the active Mount Aso volcano, the "
                            + "sulfur-bath hot springs of Beppu, the samurai district of Kumamoto, the "
                            + "peaceful seaside of Nagasaki, and the ramen capital of Fukuoka. Historically, "
                            + "Kyūshū received foreign influences from Korea, China, and the Netherlands "
                            + "long before they reached Tokyo.",
                Highlights = new() {
                    new() { Title = "Mount Aso", TitleJa = "阿蘇山", Note = "World's largest caldera, active volcano" },
                    new() { Title = "Beppu Onsen", TitleJa = "別府温泉", Note = "Japan's top hot-spring town, eight famous baths" },
                    new() { Title = "Kumamoto Castle", TitleJa = "熊本城", Note = "One of Japan's three premier castles" },
                    new() { Title = "Nagasaki Glover Garden", TitleJa = "グラバー園", Note = "Meiji-era foreign-trader hillside" },
                    new() { Title = "Yakushima", TitleJa = "屋久島", Note = "UNESCO ancient cedar forest, 1,000+ year trees" },
                },
                Foods = new() {
                    new() { Title = "Hakata Tonkotsu Ramen", TitleJa = "博多とんこつラーメン" },
                    new() { Title = "Mentaiko", TitleJa = "明太子", Note = "Spicy pollock roe, Fukuoka" },
                    new() { Title = "Basashi", TitleJa = "馬刺し", Note = "Raw horse sashimi, Kumamoto" },
                },
                Festivals = new() {
                    new() { Title = "Hakata Gion Yamakasa", TitleJa = "博多祇園山笠", Note = "1–15 July, Fukuoka" },
                    new() { Title = "Kumamoto Castle Festival", TitleJa = "熊本城まつり", Note = "Mid October" },
                },
                Access = "Fukuoka Airport (FUK), Kagoshima (KOJ), Kumamoto (KMJ), Nagasaki (NGS). "
                       + "Kyūshū Shinkansen connects Hakata to Kagoshima-Chūō in ~1h 20m.",
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
                Tagline = "Japan's subtropical south — Ryukyu kingdom heritage, coral seas, and Uchinaaguchi.",
                Description = "Okinawa is an island chain stretching 1,000 km southwest of Kyūshū, with a "
                            + "history, language, and culture distinct from mainland Japan. The Ryukyu "
                            + "Kingdom traded extensively with China and South-East Asia from the 14th "
                            + "century, and even paid tribute to both China and the Satsuma domain of Japan "
                            + "for centuries. Today the islands are famous for their emerald seas, "
                            + "white-sand beaches, World War II memorials, and a unique cuisine with "
                            + "bitter melon, sea grapes, and agu pork.",
                Highlights = new() {
                    new() { Title = "Shuri Castle", TitleJa = "首里城", Note = "Ryukyu kingdom palace, reconstructed 2019" },
                    new() { Title = "Kerama Islands", TitleJa = "慶良間諸島", Note = "Crystal-clear waters, manta rays" },
                    new() { Title = "Peace Memorial Park", TitleJa = "平和祈念公園", Note = "Itama Battle Memorial, Mabama memorial" },
                    new() { Title = "Churaumi Aquarium", TitleJa = "美ら海水族館", Note = "World-class whale-shark tank" },
                    new() { Title = "Kumejima", TitleJa = "久米島", Note = "Crystal-clear subtropical island with traditional villages" },
                },
                Foods = new() {
                    new() { Title = "Gōyā Chanpurū", TitleJa = "ゴーヤチャンプルー", Note = "Bitter-melon stir-fry with tofu" },
                    new() { Title = "Umibudō", TitleJa = "海ぶどう", Note = "Sea grapes, 'green caviar'" },
                    new() { Title = "Agu Pork", TitleJa = "あぐー豚", Note = "Heritage Ryukyu pig breed" },
                    new() { Title = "Sōki Soba", TitleJa = "ソーキそば", Note = "Pork-rib okinawa soba" },
                },
                Festivals = new() {
                    new() { Title = "Naha Dragon Boat Race", TitleJa = "那覇ハーリー", Note = "May, fishing-port celebrations" },
                    new() { Title = "Eisa Festival", TitleJa = "エイサーまつり", Note = "Mid August, traditional kachāshī drumming" },
                },
                Access = "Naha Airport (OKA) is the main gateway. "
                       + "High-speed ferry from Kagoshima / Osaka / Tokyo; "
                       + "JTA and ANA shuttle flights from major cities.",
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