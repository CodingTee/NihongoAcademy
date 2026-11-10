namespace NihongoAcademy.Models
{
    /// <summary>
    /// What kind of community content a report targets.
    /// </summary>
    public enum ReportTargetType
    {
        Post = 1,
        Comment = 2
    }

    /// <summary>
    /// Lifecycle of a user-submitted content report.
    /// </summary>
    public enum ReportStatus
    {
        Pending = 0,
        Approved = 1,   // moderator agreed -> content taken down for rectification
        Dismissed = 2   // moderator disagreed -> nothing happens
    }

    /// <summary>
    /// A user-submitted report against a community post or comment.
    /// </summary>
    public class ContentReport
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(450)]
        public string ReporterUserId { get; set; } = string.Empty;

        public ReportTargetType TargetType { get; set; }

        public int TargetId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(1000)]
        public string? Detail { get; set; }

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedDate { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(450)]
        public string? ResolvedByUserId { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? ResolutionNote { get; set; }

        // Navigation
        public Microsoft.AspNetCore.Identity.IdentityUser? ReporterUser { get; set; }
    }

    /// <summary>
    /// One row per authentication attempt (success or failure).
    /// </summary>
    public class LoginLog
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(450)]
        public string? UserId { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(256)]
        public string? Email { get; set; }

        public DateTime LoginTime { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string? IpAddress { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? UserAgent { get; set; }

        public bool Succeeded { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// One row per sensitive administrative / system operation.
    /// </summary>
    public class OperationLog
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(450)]
        public string? UserId { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(256)]
        public string? UserEmail { get; set; }

        public DateTime Timestamp { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string? EntityType { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string? EntityId { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? Description { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string? IpAddress { get; set; }
    }
}
