using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class ReadingPassage
    {
        public int Id { get; set; }
        [Required, StringLength(80)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(150)] public string Title { get; set; } = string.Empty;
        [StringLength(100)] public string? JapaneseTitle { get; set; }
        [Required, StringLength(10)] public string Level { get; set; } = "A1";
        [Required, StringLength(30)] public string TextType { get; set; } = "message";
        [Required] public string JapaneseText { get; set; } = string.Empty;
        [Required] public string KanaText { get; set; } = string.Empty;
        public string? SentenceNotesJson { get; set; }
        public string? GlossaryJson { get; set; }
        public int EstimatedMinutes { get; set; } = 5;
        public int DisplayOrder { get; set; }
        public int? LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }
        public int? GrammarPointId { get; set; }
        public virtual GrammarPoint? GrammarPoint { get; set; }
        public virtual ICollection<ReadingQuestion> Questions { get; set; } = new List<ReadingQuestion>();
        public virtual ICollection<ReadingPassageVocabulary> Vocabulary { get; set; } = new List<ReadingPassageVocabulary>();
    }

    public class ReadingQuestion
    {
        public int Id { get; set; }
        public int ReadingPassageId { get; set; }
        public virtual ReadingPassage? ReadingPassage { get; set; }
        [Required, StringLength(30)] public string QuestionType { get; set; } = "choice";
        [Required, StringLength(500)] public string Prompt { get; set; } = string.Empty;
        [StringLength(2000)] public string? OptionsJson { get; set; }
        [Required, StringLength(500)] public string CorrectAnswer { get; set; } = string.Empty;
        [StringLength(1000)] public string? Explanation { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ReadingPassageVocabulary
    {
        public int ReadingPassageId { get; set; }
        public virtual ReadingPassage? ReadingPassage { get; set; }
        public int VocabularyId { get; set; }
        public virtual Vocabulary? Vocabulary { get; set; }
    }

    public class ReadingAttempt
    {
        public int Id { get; set; }
        [Required, StringLength(450)] public string UserId { get; set; } = string.Empty;
        public int ReadingPassageId { get; set; }
        public virtual ReadingPassage? ReadingPassage { get; set; }
        public int ReadingQuestionId { get; set; }
        public virtual ReadingQuestion? ReadingQuestion { get; set; }
        [StringLength(1000)] public string? Answer { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.Now;
    }
}
