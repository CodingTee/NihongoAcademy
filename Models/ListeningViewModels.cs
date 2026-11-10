namespace NihongoAcademy.Models
{
    public class ListeningLibraryItemViewModel
    {
        public required ListeningTask Task { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsMastered { get; set; }
        public int Accuracy { get; set; }
        public int AttemptCount { get; set; }
    }

    public class ListeningWorkspaceViewModel
    {
        public required ListeningTask Task { get; set; }
        public List<string> Tips { get; set; } = new();
        public List<ListeningAttempt> History { get; set; } = new();
        public bool IsMastered { get; set; }
        public int Accuracy { get; set; }
    }
}
