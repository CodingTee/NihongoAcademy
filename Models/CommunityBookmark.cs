using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NihongoAcademy.Models
{
    public class CommunityBookmark
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public CommunityPost? Post { get; set; }
        public IdentityUser? User { get; set; }
    }
}
