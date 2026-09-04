namespace NihongoAcademy.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string? AvatarJson { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }
}
