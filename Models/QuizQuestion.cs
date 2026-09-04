using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }

        public int QuizId { get; set; }
        public virtual Quiz? Quiz { get; set; }

        [Required(ErrorMessage = "Question text is required")]
        [StringLength(500, ErrorMessage = "Question cannot exceed 500 characters")]
        [Display(Name = "Question")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Option A is required")]
        [StringLength(200)]
        [Display(Name = "Option A")]
        public string OptionA { get; set; } = string.Empty;

        [Required(ErrorMessage = "Option B is required")]
        [StringLength(200)]
        [Display(Name = "Option B")]
        public string OptionB { get; set; } = string.Empty;

        [Required(ErrorMessage = "Option C is required")]
        [StringLength(200)]
        [Display(Name = "Option C")]
        public string OptionC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Option D is required")]
        [StringLength(200)]
        [Display(Name = "Option D")]
        public string OptionD { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select the correct answer")]
        [Range(1, 4, ErrorMessage = "Correct answer must be between 1 and 4")]
        [Display(Name = "Correct Answer")]
        public int CorrectOption { get; set; }

        [StringLength(500)]
        [Display(Name = "Explanation")]
        public string? Explanation { get; set; }

        [Display(Name = "Display Order")]
        [Range(0, 999)]
        public int DisplayOrder { get; set; }

        [StringLength(100)]
        [Display(Name = "Audio Path")]
        public string? AudioPath { get; set; }
    }
}
