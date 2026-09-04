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
        [Required, StringLength(30)] public string ExerciseType { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.Now;
    }
}
