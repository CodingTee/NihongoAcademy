namespace NihongoAcademy.Models.ExploreJapan
{
    /// <summary>
    /// One round of a region's "Let's play" Japanese game — the little photo
    /// quiz at the foot of the region detail page.
    ///
    /// Every region ships its own set, so the game asks about <i>that</i>
    /// region's food, sights and festivals rather than Hokkaido's on every
    /// page. A round shows one Japanese headword (with a reading, a romaji
    /// breakdown and a speaker button) and offers four English answers; the
    /// teaching <see cref="Note"/> is revealed after the player picks one.
    /// </summary>
    public class RegionQuizRound
    {
        /// <summary>Category chip above the headword, e.g. "Local food", "Place", "Festival".</summary>
        public string Kind { get; set; } = "";

        /// <summary>Japanese prompt shown on the quiz card, e.g. "牛タン".</summary>
        public string Ja { get; set; } = "";

        /// <summary>English / romaji gloss of the headword, e.g. "Gyūtan (beef tongue)".</summary>
        public string Reading { get; set; } = "";

        /// <summary>Syllable-by-syllable breakdown, e.g. "gyuu-tan".</summary>
        public string Romaji { get; set; } = "";

        /// <summary>Japanese text that the speaker button reads aloud (SpeechSynthesis).</summary>
        public string Say { get; set; } = "";

        /// <summary>
        /// Filename of the photo shown with the round, stored in wwwroot/Japance Image.
        /// Left empty when there is no matching picture — the card then falls back
        /// to an illustrated tile.
        /// </summary>
        public string Pic { get; set; } = "";

        /// <summary>Teaching note revealed once the round is over.</summary>
        public string Note { get; set; } = "";

        /// <summary>Index inside <see cref="Opts"/> holding the correct answer.</summary>
        public int Answer { get; set; }

        /// <summary>The four English answers; the correct one sits at <see cref="Answer"/>.</summary>
        public string[] Opts { get; set; } = new string[0];
    }
}
