namespace NihongoAcademy.Models
{
    public class LessonVocabulary
    {
        public int LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }

        public int VocabularyId { get; set; }
        public virtual Vocabulary? Vocabulary { get; set; }

        public int DisplayOrder { get; set; }
    }
}
