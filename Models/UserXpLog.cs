using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class UserXpLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        public int Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Source { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    }
}
