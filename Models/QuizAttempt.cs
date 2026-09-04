using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NihongoAcademy.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual IdentityUser? User { get; set; }

        public int QuizId { get; set; }
        public virtual Quiz? Quiz { get; set; }

        [Display(Name = "Total Questions")]
        public int TotalQuestions { get; set; }

        [Display(Name = "Correct Answers")]
        public int CorrectAnswers { get; set; }

        [Display(Name = "Score (%)")]
        [Range(0, 100)]
        public int ScorePercentage { get; set; }

        [Display(Name = "Passed")]
        public bool IsPassed { get; set; }

        [Display(Name = "Time Taken (seconds)")]
        public int TimeTakenSeconds { get; set; }

        [Display(Name = "Attempt Date")]
        public DateTime AttemptDate { get; set; } = DateTime.Now;
    }
}
