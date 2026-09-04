namespace NihongoAcademy.Models.ExploreJapan
{
    /// <summary>
    /// One entry inside an Explore Japan category page
    /// (e.g. one region, one food, one festival).
    /// Static data for the demo — no DB model needed.
    /// </summary>
    public class ExploreItem
    {
        /// <summary>Bootstrap-icons class for the tile, e.g. "bi-snow".</summary>
        public string Icon { get; set; } = "bi-circle";

        /// <summary>English / romaji name shown as the card title.</summary>
        public string Title { get; set; } = "";

        /// <summary>Japanese (kanji + hiragana) name shown below the title.</summary>
        public string TitleJa { get; set; } = "";

        /// <summary>1–2 sentence English description.</summary>
        public string Description { get; set; } = "";

        /// <summary>Optional accent CSS variable (without the leading "--"), e.g. "na-secondary".</summary>
        public string? Accent { get; set; }

        /// <summary>Optional external link for "Learn more" — Wiki / official site. Null hides the link.</summary>
        public string? LearnMoreUrl { get; set; }
    }
}
