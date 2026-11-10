namespace NihongoAcademy.Models.ExploreJapan
{
    /// <summary>
    /// One trivia card in a region's "Fun &amp; curious" block.
    ///
    /// Deliberately text-only — no photo — so this section reads differently
    /// from the photo-heavy sections above it (sights, cuisine, festivals).
    /// Each card can carry a big display number, a Japanese headword with its
    /// romaji reading, and (for the language cards) the Japanese phrase that a
    /// speaker button reads aloud.
    /// </summary>
    public class RegionFact
    {
        /// <summary>Bootstrap icon name for the card, e.g. "bi-apple".</summary>
        public string Icon { get; set; } = "bi-lightbulb";

        /// <summary>Category chip, e.g. "Food", "Words", "Winter".</summary>
        public string Tag { get; set; } = "";

        /// <summary>
        /// Big display number for the card (e.g. "¥1M+", "Feb–Mar").
        /// Leave null on word cards, where the headword itself is the focus.
        /// </summary>
        public string? Stat { get; set; }

        /// <summary>Japanese headword, e.g. "夕張メロン".</summary>
        public string Label { get; set; } = "";

        /// <summary>Romaji / English reading of the headword, e.g. "Yūbari meron".</summary>
        public string Reading { get; set; } = "";

        /// <summary>One-line English headline.</summary>
        public string Title { get; set; } = "";

        /// <summary>Body copy — two or three short lines.</summary>
        public string Note { get; set; } = "";

        /// <summary>
        /// Japanese text read aloud when the card's speaker button is pressed
        /// (SpeechSynthesis). Empty means no speaker button on that card.
        /// </summary>
        public string? Say { get; set; }
    }
}
