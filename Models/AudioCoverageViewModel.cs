namespace NihongoAcademy.Models
{
    public class AudioCoverageViewModel
    {
        public List<AudioCoverageItemViewModel> Items { get; set; } = new();
        public int TotalRequiredFileCount { get; set; }
        public int TotalAvailableFileCount { get; set; }
        public int RequiredFileCount => TotalRequiredFileCount;
        public int AvailableFileCount => TotalAvailableFileCount;
        public int CoveragePercentage => RequiredFileCount == 0 ? 100 :
            (int)Math.Round(AvailableFileCount * 100d / RequiredFileCount);
    }

    public class AudioCoverageItemViewModel
    {
        public int LessonId { get; set; }
        public int ActivityId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public string JapanesePreview { get; set; } = string.Empty;
        public string SlowUrl { get; set; } = string.Empty;
        public string NaturalUrl { get; set; } = string.Empty;
        public bool SlowExists { get; set; }
        public bool NaturalExists { get; set; }
        public bool SlowExternal { get; set; }
        public bool NaturalExternal { get; set; }
    }
}
