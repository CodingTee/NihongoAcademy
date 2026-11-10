namespace NihongoAcademy.Models
{
    public class ReadingLibraryItemViewModel
    {
        public required ReadingPassage Passage { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsMastered { get; set; }
        public int Accuracy { get; set; }
        public int UnknownPercentage { get; set; }
    }

    public class ReadingPassageViewModel
    {
        public required ReadingPassage Passage { get; set; }
        public int Accuracy { get; set; }
        public int UnknownPercentage { get; set; }
        public bool IsMastered { get; set; }
        public List<string> Notes { get; set; } = new();
        public List<string> Glossary { get; set; } = new();
    }
}
