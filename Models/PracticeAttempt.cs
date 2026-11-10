using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class PracticeAttempt
    {
        public int Id { get; set; }
        [Required, StringLength(450)] public string UserId { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }
        public int? VocabularyId { get; set; }
        public virtual Vocabulary? Vocabulary { get; set; }
        public int? LessonActivityId { get; set; }
        public virtual LessonActivity? LessonActivity { get; set; }
        public int? GrammarPointId { get; set; }
        public virtual GrammarPoint? GrammarPoint { get; set; }
        [Required, StringLength(30)] public string ExerciseType { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

        /// <summary>Points earned (0-100) when the attempt was scored by an activity or the AI tutor.</summary>
        public int? Score { get; set; }

        /// <summary>Short description of what was answered. Set by the AI tutor tool.</summary>
        [StringLength(200)] public string? Details { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.Now;
    }
}
