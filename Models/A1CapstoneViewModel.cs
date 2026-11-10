namespace NihongoAcademy.Models
{
    public class A1CapstoneViewModel
    {
        public int StableLessonCount { get; set; }
        public int RequiredLessonCount { get; set; } = 18;
        public int BestFinalQuizScore { get; set; }
        public bool FoundationComplete { get; set; }
        public string LearnerName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string LevelLabel { get; set; } = "A1";
        public List<CapstoneSkillViewModel> Skills { get; set; } = new();
        public bool IsComplete => FoundationComplete && Skills.Count == 4 && Skills.All(skill => skill.IsComplete);
        public int CompletedRequirementCount => Skills.Count(skill => skill.IsComplete) + (FoundationComplete ? 1 : 0);
    }

    public class CapstoneSkillViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string JapaneseTitle { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Requirement { get; set; } = string.Empty;
        public string ProgressText { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string ActionLabel { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public int Score { get; set; }
    }

    public class A2CapstoneViewModel : A1CapstoneViewModel
    {
        public A2CapstoneViewModel() { RequiredLessonCount = 15; LevelLabel = "A2"; }
    }
}
