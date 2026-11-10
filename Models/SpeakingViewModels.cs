namespace NihongoAcademy.Models
{
    public class SpeakingLibraryItemViewModel
    {
        public required SpeakingTask Task { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsMastered { get; set; }
        public int BestScore { get; set; }
        public int AttemptCount { get; set; }
    }

    public class SpeakingWorkspaceViewModel
    {
        public required SpeakingTask Task { get; set; }
        public List<string> Steps { get; set; } = new();
        public List<string> Requirements { get; set; } = new();
        public List<string> Checklist { get; set; } = new();
        public List<SpeakingAttempt> History { get; set; } = new();
        public bool IsMastered { get; set; }
    }
}
