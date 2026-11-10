using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class UserNotification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(450)]
        public string? SenderUserId { get; set; }

        /// <summary>Type: "like", "comment", "reply", "achievement"</summary>
        [Required]
        [StringLength(30)]
        public string Type { get; set; } = "comment";

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Message { get; set; } = string.Empty;

        [StringLength(256)]
        public string TargetUrl { get; set; } = "/Community";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
