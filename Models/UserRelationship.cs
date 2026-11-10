using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class UserRelationship
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string TargetUserId { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
