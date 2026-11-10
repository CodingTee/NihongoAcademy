namespace NihongoAcademy.Models
{
    public class WritingLibraryItemViewModel
    {
        public required WritingTask Task { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsMastered { get; set; }
        public int BestScore { get; set; }
        public int SubmissionCount { get; set; }
    }

    public class WritingWorkspaceViewModel
    {
        public required WritingTask Task { get; set; }
        public List<string> Steps { get; set; } = new();
        public List<string> Requirements { get; set; } = new();
        public List<string> Checklist { get; set; } = new();
        public List<WritingSubmission> History { get; set; } = new();
        public bool IsMastered { get; set; }
    }
}
