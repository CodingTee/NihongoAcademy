using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class WritingTask
    {
        public int Id { get; set; }
        [Required, StringLength(80)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(150)] public string Title { get; set; } = string.Empty;
        [StringLength(100)] public string? JapaneseTitle { get; set; }
        [Required, StringLength(10)] public string Level { get; set; } = "A1";
        [Required, StringLength(30)] public string TaskType { get; set; } = "guided";
        [Required, StringLength(1000)] public string Prompt { get; set; } = string.Empty;
        public string? StepsJson { get; set; }
        public string? RequirementsJson { get; set; }
        public string? ChecklistJson { get; set; }
        [Required, StringLength(2000)] public string ModelAnswer { get; set; } = string.Empty;
        [Required, StringLength(2400)] public string ModelAnswerReading { get; set; } = string.Empty;
        public int MinimumJapaneseCharacters { get; set; }
        public int MinimumSentences { get; set; }
        public int DisplayOrder { get; set; }
        public int? LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }
        public int? GrammarPointId { get; set; }
        public virtual GrammarPoint? GrammarPoint { get; set; }
        public virtual ICollection<WritingSubmission> Submissions { get; set; } = new List<WritingSubmission>();
    }

    public class WritingSubmission
    {
        public int Id { get; set; }
        [Required, StringLength(450)] public string UserId { get; set; } = string.Empty;
        public int WritingTaskId { get; set; }
        public virtual WritingTask? WritingTask { get; set; }
        public int? PreviousSubmissionId { get; set; }
        public virtual WritingSubmission? PreviousSubmission { get; set; }
        [Required, StringLength(4000)] public string Content { get; set; } = string.Empty;
        public int Score { get; set; }
        public bool IsPassing { get; set; }
        public string? FeedbackJson { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
