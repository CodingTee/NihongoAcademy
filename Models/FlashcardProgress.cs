using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    /// <summary>
    /// Tracks SRS (Spaced Repetition System) progress for each vocabulary flashcard per user.
    /// Uses a simplified SM-2 algorithm.
    /// </summary>
    public class FlashcardProgress
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        public int VocabularyId { get; set; }
        public virtual Vocabulary? Vocabulary { get; set; }

        /// <summary>0=New, 1=Learning, 2=Review, 3=Relearning</summary>
        public int State { get; set; } = 0;

        /// <summary>SM-2 ease factor (minimum 1.3, default 2.5)</summary>
        public double EaseFactor { get; set; } = 2.5;

        /// <summary>Number of consecutive correct responses</summary>
        public int Repetitions { get; set; } = 0;

        /// <summary>Interval in days until next review</summary>
        public int IntervalDays { get; set; } = 0;

        /// <summary>When this card is due for review</summary>
        public DateTime NextReview { get; set; } = DateTime.Now;

        /// <summary>Last time the user reviewed this card</summary>
        public DateTime? LastReview { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
