using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class ListeningTask
    {
        public int Id { get; set; }
        [Required, StringLength(80)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(150)] public string Title { get; set; } = string.Empty;
        [StringLength(100)] public string? JapaneseTitle { get; set; }
        [Required, StringLength(10)] public string Level { get; set; } = "A1";
        [Required, StringLength(30)] public string TaskType { get; set; } = "dialogue";
        [Required, StringLength(500)] public string Situation { get; set; } = string.Empty;
        [Required, StringLength(3000)] public string ScriptJapanese { get; set; } = string.Empty;
        [Required, StringLength(3000)] public string ScriptEnglish { get; set; } = string.Empty;
        public string? ListeningTipsJson { get; set; }
        public int EstimatedMinutes { get; set; } = 5;
        public int DisplayOrder { get; set; }
        public int? LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }
        public virtual ICollection<ListeningQuestion> Questions { get; set; } = new List<ListeningQuestion>();
    }

    public class ListeningQuestion
    {
        public int Id { get; set; }
        public int ListeningTaskId { get; set; }
        public virtual ListeningTask? ListeningTask { get; set; }
        [Required, StringLength(30)] public string QuestionType { get; set; } = "choice";
        [Required, StringLength(500)] public string Prompt { get; set; } = string.Empty;
        [StringLength(2000)] public string? OptionsJson { get; set; }
        [Required, StringLength(500)] public string CorrectAnswer { get; set; } = string.Empty;
        [StringLength(1000)] public string? Explanation { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ListeningAttempt
    {
        public int Id { get; set; }
        [Required, StringLength(450)] public string UserId { get; set; } = string.Empty;
        public int ListeningTaskId { get; set; }
        public virtual ListeningTask? ListeningTask { get; set; }
        public int ListeningQuestionId { get; set; }
        public virtual ListeningQuestion? ListeningQuestion { get; set; }
        [StringLength(1000)] public string? Answer { get; set; }
        [Required, StringLength(20)] public string PlaybackMode { get; set; } = "natural";
        public bool IsCorrect { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.Now;
    }
}
