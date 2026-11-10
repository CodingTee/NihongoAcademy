using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string? AvatarJson { get; set; }

        /// <summary>Community avatar mode: "generated" or "uploaded".</summary>
        [StringLength(20)]
        public string AvatarMode { get; set; } = "generated";

        /// <summary>Path to an uploaded community avatar image under /uploads/avatars.</summary>
        [StringLength(256)]
        public string? AvatarImagePath { get; set; }

        [StringLength(50)]
        public string? DisplayName { get; set; }

        [StringLength(240)]
        public string? Bio { get; set; }

        [StringLength(30)]
        public string? NativeLanguage { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        /// <summary>UI theme preference: "light", "dark" or "system".</summary>
        [StringLength(10)]
        public string? ThemePreference { get; set; }

        public bool DailyReminder { get; set; }

        [StringLength(200)]
        public string? LearningGoal { get; set; }

        [StringLength(50)]
        public string? CurrentLevel { get; set; }

        [StringLength(30)]
        public string? AgeGroup { get; set; }

        public int TotalXp { get; set; } = 0;

        /// <summary>Collectible sparks (community/engagement reward currency). Admin-adjustable.</summary>
        public int Sparks { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }
}
