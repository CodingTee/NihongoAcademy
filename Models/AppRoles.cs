namespace NihongoAcademy.Models
{
    /// <summary>
    /// Canonical role names used across authorization attributes and the seeder.
    /// SuperAdmin  : full access, including managing role accounts.
    /// Admin       : content management + moderation only, cannot touch role accounts.
    /// Student     : regular learner.
    /// </summary>
    public static class AppRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Student = "Student";

        /// <summary>Roles allowed into any admin surface (content + accounts).</summary>
        public const string AdminAndSuperAdmin = "Admin,SuperAdmin";
    }
}
