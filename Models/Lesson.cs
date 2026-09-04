using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required(ErrorMessage = "Lesson title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Lesson Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "Japanese title cannot exceed 30 characters")]
        [Display(Name = "Japanese Title")]
        public string? JapaneseTitle { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Video URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? VideoUrl { get; set; }

        [StringLength(200)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Difficulty Level")]
        [Range(1, 5, ErrorMessage = "Difficulty must be between 1 and 5")]
        public int DifficultyLevel { get; set; } = 1;

        [Display(Name = "Estimated Minutes")]
        [Range(1, 300, ErrorMessage = "Estimated time must be between 1 and 300 minutes")]
        public int EstimatedMinutes { get; set; } = 15;

        [Display(Name = "Is Published")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "Display Order")]
        [Range(0, 999)]
        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<LessonProgress> ProgressRecords { get; set; } = new List<LessonProgress>();
        public virtual ICollection<LessonVocabulary> LessonVocabulary { get; set; } = new List<LessonVocabulary>();
        public virtual ICollection<LessonPrerequisite> Prerequisites { get; set; } = new List<LessonPrerequisite>();
        public virtual ICollection<LessonPrerequisite> RequiredByLessons { get; set; } = new List<LessonPrerequisite>();
        public virtual ICollection<LessonActivity> Activities { get; set; } = new List<LessonActivity>();
    }
}
