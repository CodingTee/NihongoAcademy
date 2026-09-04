namespace NihongoAcademy.Models
{
    public class LessonPrerequisite
    {
        public int LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }

        public int PrerequisiteLessonId { get; set; }
        public virtual Lesson? PrerequisiteLesson { get; set; }
    }
}
