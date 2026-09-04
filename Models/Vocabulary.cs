using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class Vocabulary
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required(ErrorMessage = "Japanese word is required")]
        [StringLength(50, ErrorMessage = "Japanese word cannot exceed 50 characters")]
        [Display(Name = "Japanese Word")]
        public string Japanese { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reading is required")]
        [StringLength(50, ErrorMessage = "Reading cannot exceed 50 characters")]
        [Display(Name = "Reading (Romaji)")]
        public string Romaji { get; set; } = string.Empty;

        [Required(ErrorMessage = "English meaning is required")]
        [StringLength(100, ErrorMessage = "Meaning cannot exceed 100 characters")]
        [Display(Name = "English Meaning")]
        public string English { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Part of Speech")]
        public string? PartOfSpeech { get; set; }

        [StringLength(200)]
        [Display(Name = "Example Sentence (Japanese)")]
        public string? ExampleJapanese { get; set; }

        [StringLength(200)]
        [Display(Name = "Example Sentence (English)")]
        public string? ExampleEnglish { get; set; }

        [StringLength(100)]
        [Display(Name = "Audio File Path")]
        public string? AudioPath { get; set; }

        [StringLength(100)]
        [Display(Name = "Image Path")]
        public string? ImagePath { get; set; }

        [Display(Name = "JLPT Level")]
        [Range(1, 5, ErrorMessage = "JLPT level must be between 1 and 5")]
        public int JlptLevel { get; set; } = 5;

        [Display(Name = "Display Order")]
        [Range(0, 999)]
        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public virtual ICollection<LessonVocabulary> LessonVocabulary { get; set; } = new List<LessonVocabulary>();
    }
}
