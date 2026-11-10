using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Models
{
    public class GrammarPoint
    {
        public int Id { get; set; }
        [Required, StringLength(80)] public string Code { get; set; } = string.Empty;
        [Required, StringLength(120)] public string Pattern { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Meaning { get; set; } = string.Empty;
        [Required, StringLength(300)] public string Formation { get; set; } = string.Empty;
        [StringLength(1000)] public string? UsageNotes { get; set; }
        [StringLength(500)] public string? CommonMistake { get; set; }
        [Required, StringLength(300)] public string ExampleJapanese { get; set; } = string.Empty;
        [Required, StringLength(400)] public string ExampleReading { get; set; } = string.Empty;
        [Required, StringLength(300)] public string ExampleEnglish { get; set; } = string.Empty;
        [Required, StringLength(10)] public string Level { get; set; } = "A1";
        public virtual ICollection<LessonGrammarPoint> Lessons { get; set; } = new List<LessonGrammarPoint>();
    }
}
