using Microsoft.AspNetCore.Mvc.Rendering;

namespace NihongoAcademy.Models
{
    public class AdminProgressTestingViewModel
    {
        public string SelectedUserId { get; set; } = string.Empty;
        public int? TargetLessonId { get; set; }
        public List<SelectListItem> Users { get; set; } = new();
        public List<AdminProgressStageViewModel> Stages { get; set; } = new();
        public int SimulatedLessonCount { get; set; }
        public int RealLessonCount { get; set; }
    }

    public class AdminProgressStageViewModel
    {
        public int LessonId { get; set; }
        public string StageLabel { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
    }
}
