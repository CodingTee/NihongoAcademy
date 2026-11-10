using NihongoAcademy.Services;

namespace NihongoAcademy.Models;

public class MasteryProgressViewModel
{
    public MasteryEvaluation Evaluation { get; set; } = null!;
    public int ItemCount { get; set; }
    public bool RequiresProduction { get; set; }
}
