namespace NihongoAcademy.Models
{
    public class LessonGrammarPoint
    {
        public int LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }
        public int GrammarPointId { get; set; }
        public virtual GrammarPoint? GrammarPoint { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; } = true;
    }
}
