namespace NihongoAcademy.Models.ExploreJapan
{
    /// <summary>
    /// One of Japan's nine traditional regions (9 地方).
    /// Richer than <see cref="ExploreItem"/> because each region has its own
    /// detail page with multi-section content.
    /// </summary>
    public class Region
    {
        /// <summary>URL-safe id (e.g. "hokkaido", "kanto").</summary>
        public string Slug { get; set; } = "";

        /// <summary>English / romaji name.</summary>
        public string Name { get; set; } = "";

        /// <summary>Japanese kanji + hiragana name.</summary>
        public string NameJa { get; set; } = "";

        /// <summary>Short Japanese subtitle (e.g. "北の楽園" = "Northern paradise").</summary>
        public string Subtitle { get; set; } = "";

        /// <summary>Capital or principal city (e.g. "Sapporo").</summary>
        public string Capital { get; set; } = "";

        /// <summary>Capital Japanese name (e.g. "札幌").</summary>
        public string CapitalJa { get; set; } = "";

        /// <summary>Area string (e.g. "83,424 km²").</summary>
        public string Area { get; set; } = "";

        /// <summary>Climate / weather one-liner.</summary>
        public string Climate { get; set; } = "";

        /// <summary>Best time to visit (e.g. "July–August for festivals").</summary>
        public string BestSeason { get; set; } = "";

        /// <summary>Headline tagline shown on the region tile.</summary>
        public string Tagline { get; set; } = "";

        /// <summary>Long paragraph (2–3 sentences) shown on the detail page.</summary>
        public string Description { get; set; } = "";

        /// <summary>3–5 must-see sights, each with optional ja-name.</summary>
        public List<RegionPoint> Highlights { get; set; } = new();

        /// <summary>3–4 signature foods/drinks of this region.</summary>
        public List<RegionPoint> Foods { get; set; } = new();

        /// <summary>2–3 famous matsuri / seasonal events.</summary>
        public List<RegionPoint> Festivals { get; set; } = new();

        /// <summary>How to get there (e.g. airports, shinkansen lines).</summary>
        public string Access { get; set; } = "";

        /// <summary>Accent CSS class fragment (e.g. "na-primary").</summary>
        public string Accent { get; set; } = "na-primary";

        /// <summary>Approx. lat/lng used to place the hotspot dot on the hero map.</summary>
        /// Stored as a (lat, lng) tuple — kept simple for the static demo.
        public (double Lat, double Lng) Center { get; set; }

        /// <summary>Wikipedia link for the region.</summary>
        public string LearnMoreUrl { get; set; } = "";

        /// <summary>URL to the region's isolated silhouette image (e.g. Hokkaido.png).</summary>
        /// Used as the main visual on the region detail page.
        public string HeroImage { get; set; } = "";

        /// <summary>
        /// Hand-tuned X position (0-100, percent) for placing the region marker dot on
        /// the full Japan map. Calibrated to match the visual location of the region
        /// on Japan.png — not derived from lat/lng because the source graphic is stylized.
        /// </summary>
        public double MapX { get; set; }

        /// <summary>
        /// Hand-tuned Y position (0-100, percent) for placing the region marker dot on
        /// the full Japan map. 0 = top of the image, 100 = bottom.
        /// </summary>
        public double MapY { get; set; }
    }

    /// <summary>One bullet point on a region detail page.</summary>
    public class RegionPoint
    {
        public string Title { get; set; } = "";
        public string TitleJa { get; set; } = "";
        public string? Note { get; set; }
        public string? Url { get; set; }
    }
}