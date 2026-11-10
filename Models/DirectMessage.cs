using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class DirectMessage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string SenderUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string ReceiverUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
