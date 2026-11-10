using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NihongoAcademy.Models
{
    /// <summary>
    /// Community post categories — simplified to 4 user-facing categories.
    /// ForYou is a special filter value (not a real category — shows all posts).
    /// </summary>
    public enum CommunityCategory
    {
        ForYou = 0,
        General = 1,    // Community  — daily chat, check-ins, study buddies, music recs, etc.
        Study = 2,      // Study      — learning methods, JLPT prep, grammar/vocab/reading, Japanese diary
        Questions = 3,  // Questions  — grammar/vocabulary/sentence/pronunciation help
        Japan = 4       // Japan      — culture, food, travel, regions, lifestyle
    }

    public class CommunityPost
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(5000, ErrorMessage = "Content cannot exceed 5000 characters")]
        public string Content { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Category must be between 1 and 4")]
        public CommunityCategory Category { get; set; } = CommunityCategory.General;

        /// <summary>
        /// For Japanese diary posts: user wants native/advanced speakers to correct their Japanese.
        /// Optional flag, only meaningful on Study category posts that are written in Japanese.
        /// </summary>
        public bool FeedbackWanted { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Set when a moderator approves a report against this post. The content is hidden
        /// from public feeds until the author rectifies it (or an admin restores it).
        /// </summary>
        public bool IsTakenDown { get; set; }

        public DateTime? TakenDownAt { get; set; }

        [StringLength(450)]
        public string? TakenDownByUserId { get; set; }

        [StringLength(500)]
        public string? TakenDownReason { get; set; }

        // Navigation
        public IdentityUser? User { get; set; }
        public List<CommunityComment> Comments { get; set; } = new();
        public List<CommunityLike> Likes { get; set; } = new();
    }
}
