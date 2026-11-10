using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class LessonActivity
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }

        [Required, StringLength(30)]
        public string ActivityType { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Prompt { get; set; } = string.Empty;

        [StringLength(1000)] public string? JapaneseText { get; set; }
        [StringLength(1200)] public string? ReadingSupportText { get; set; }
        [StringLength(1000)] public string? EnglishText { get; set; }
        [StringLength(300), Url] public string? SlowAudioUrl { get; set; }
        [StringLength(300), Url] public string? NaturalAudioUrl { get; set; }
        [StringLength(2000)] public string? OptionsJson { get; set; }
        [StringLength(500)] public string? CorrectAnswer { get; set; }
        [StringLength(500)] public string? Explanation { get; set; }
        public int DisplayOrder { get; set; }
    }
}
