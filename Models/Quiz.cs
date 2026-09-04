using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required(ErrorMessage = "Quiz title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Quiz Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(30)]
        [Display(Name = "Japanese Title")]
        public string? JapaneseTitle { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Time Limit (minutes)")]
        [Range(1, 120, ErrorMessage = "Time limit must be between 1 and 120 minutes")]
        public int TimeLimitMinutes { get; set; } = 10;

        [Display(Name = "Passing Score (%)")]
        [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100")]
        public int PassingScore { get; set; } = 60;

        [Display(Name = "Is Published")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "Display Order")]
        [Range(0, 999)]
        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
        public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}
