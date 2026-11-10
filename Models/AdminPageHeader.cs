namespace NihongoAcademy.Models
{
    /// <summary>One breadcrumb node in the admin page header.</summary>
    public class AdminCrumbItem
    {
        public string Text { get; set; } = "";
        public string? Url { get; set; }
    }

    /// <summary>A link-style action button rendered on the right of the admin page header.</summary>
    public class AdminPageHeaderAction
    {
        public string Text { get; set; } = "";
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public bool Outline { get; set; }
        public string? CssClass { get; set; }
    }

    /// <summary>
    /// Shared header model for all admin pages: breadcrumb trail (image-#2 style,
    /// "Admin / Section / Current") + a compact title bar with optional actions.
    /// </summary>
    public class AdminPageHeaderModel
    {
        public List<AdminCrumbItem> Trail { get; set; } = new();
        public string Title { get; set; } = "";
        public string? TitleJp { get; set; }
        public string? Lead { get; set; }
        public string? Icon { get; set; }
        public List<AdminPageHeaderAction> Actions { get; set; } = new();

        /// <summary>Raw HTML for special action areas (badges, forms). Rendered after <see cref="Actions"/>.</summary>
        public string? ActionsHtml { get; set; }
    }
}
