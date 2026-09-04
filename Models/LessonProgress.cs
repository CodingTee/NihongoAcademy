using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NihongoAcademy.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual IdentityUser? User { get; set; }

        public int LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }

        [Display(Name = "Is Completed")]
        public bool IsCompleted { get; set; }

        [Display(Name = "Completion Percentage")]
        [Range(0, 100)]
        public int CompletionPercentage { get; set; }

        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        [Display(Name = "First Mastery Pass")]
        public DateTime? FirstMasteryPassedDate { get; set; }

        [Display(Name = "Mastery Review Due")]
        public DateTime? MasteryReviewDueDate { get; set; }

        [Display(Name = "Stable Mastery Confirmed")]
        public DateTime? StableMasteryDate { get; set; }

        [Display(Name = "Time Spent (minutes)")]
        public int TimeSpentMinutes { get; set; }
    }
}
