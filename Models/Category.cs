using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(50)]
        [Display(Name = "Icon Class")]
        public string? IconClass { get; set; }

        [Display(Name = "Display Order")]
        [Range(0, 999)]
        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public virtual ICollection<Vocabulary> Vocabulary { get; set; } = new List<Vocabulary>();
        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
