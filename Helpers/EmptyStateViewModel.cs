namespace NihongoAcademy.Helpers
{
    public class EmptyStateViewModel
    {
        public string Icon { get; set; } = "bi-inbox";
        public string Title { get; set; } = "No data found";
        public string Message { get; set; } = "";
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
    }
}
