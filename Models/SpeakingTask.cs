using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class SpeakingTask
    {
        public int Id { get; set; }
        [Required, StringLength(80)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(150)] public string Title { get; set; } = string.Empty;
        [StringLength(100)] public string? JapaneseTitle { get; set; }
        [Required, StringLength(10)] public string Level { get; set; } = "A1";
        [Required, StringLength(30)] public string TaskType { get; set; } = "role-play";
        [Required, StringLength(500)] public string Situation { get; set; } = string.Empty;
        [Required, StringLength(1000)] public string PromptJapanese { get; set; } = string.Empty;
        [Required, StringLength(1200)] public string PromptReading { get; set; } = string.Empty;
        [Required, StringLength(1000)] public string PromptEnglish { get; set; } = string.Empty;
        public string? ResponseStepsJson { get; set; }
        public string? RequirementsJson { get; set; }
        public string? ChecklistJson { get; set; }
        [Required, StringLength(2000)] public string ModelResponse { get; set; } = string.Empty;
        [Required, StringLength(2400)] public string ModelResponseReading { get; set; } = string.Empty;
        public int MinimumJapaneseCharacters { get; set; }
        public int DisplayOrder { get; set; }
        public int? LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }
        public virtual ICollection<SpeakingAttempt> Attempts { get; set; } = new List<SpeakingAttempt>();
    }

    public class SpeakingAttempt
    {
        public int Id { get; set; }
        [Required, StringLength(450)] public string UserId { get; set; } = string.Empty;
        public int SpeakingTaskId { get; set; }
        public virtual SpeakingTask? SpeakingTask { get; set; }
        [Required, StringLength(4000)] public string Transcript { get; set; } = string.Empty;
        [Required, StringLength(20)] public string InputMode { get; set; } = "typed";
        public int Score { get; set; }
        public bool IsPassing { get; set; }
        public string? FeedbackJson { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.Now;
    }
}
