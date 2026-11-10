namespace NihongoAcademy.Models
{
    public class GrammarPracticeCardViewModel
    {
        public required GrammarPoint GrammarPoint { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public List<string> MeaningOptions { get; set; } = new();
        public string IncorrectExample { get; set; } = string.Empty;
        public List<string> SentenceParts { get; set; } = new();
        public int AttemptCount { get; set; }
        public int Accuracy { get; set; }
        public bool IsMastered { get; set; }
    }
}
