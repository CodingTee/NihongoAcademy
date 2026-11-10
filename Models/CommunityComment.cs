using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NihongoAcademy.Models
{
    public class CommunityComment
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Null = top-level comment. Set = reply to another comment (1 level max).
        /// </summary>
        public int? ParentCommentId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Set when a moderator approves a report against this comment. Hidden from the
        /// thread for regular users until rectified/restored.
        /// </summary>
        public bool IsTakenDown { get; set; }

        public DateTime? TakenDownAt { get; set; }

        [StringLength(450)]
        public string? TakenDownByUserId { get; set; }

        [StringLength(500)]
        public string? TakenDownReason { get; set; }

        // Navigation
        public CommunityPost? Post { get; set; }
        public IdentityUser? User { get; set; }
        public CommunityComment? ParentComment { get; set; }
        public List<CommunityComment> Replies { get; set; } = new();
    }
}
