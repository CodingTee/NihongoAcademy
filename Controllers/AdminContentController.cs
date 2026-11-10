using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers;

/// <summary>
/// Canonical two-page administration surface. The type and mode parameters
/// select the data set while the proven CRUD forms and POST handlers are reused.
/// </summary>
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminContentController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminContentController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string type = "lesson", int page = 1)
    {
        type = NormalizeType(type);
        ViewBag.ContentType = type;

        return type switch
        {
            "category" => View("~/Views/AdminCategories/Index.cshtml", await _context.Categories
                .Include(c => c.Lessons).Include(c => c.Vocabulary).Include(c => c.Quizzes)
                .OrderBy(c => c.DisplayOrder).ToListAsync()),
            "vocabulary" => View("~/Views/AdminVocabulary/Index.cshtml", await AdminVocabularyListViewModel.CreateAsync(
                _context.Vocabulary.Include(v => v.Category).OrderBy(v => v.Category!.DisplayOrder).ThenBy(v => v.DisplayOrder), page)),
            "quiz" => View("~/Views/AdminQuizzes/Index.cshtml", await _context.Quizzes
                .Include(q => q.Category).Include(q => q.Questions)
                .OrderBy(q => q.Category!.DisplayOrder).ThenBy(q => q.DisplayOrder).ToListAsync()),
            _ => View("~/Views/AdminLessons/Index.cshtml", await _context.Lessons
                .Include(l => l.Category).OrderBy(l => l.Category!.DisplayOrder).ThenBy(l => l.DisplayOrder).ToListAsync())
        };
    }

    public async Task<IActionResult> Editor(string type, string mode = "edit", int? id = null)
    {
        type = NormalizeType(type);
        mode = NormalizeMode(mode, id);
        ViewBag.ContentType = type;
        ViewBag.EditorMode = mode;
        ViewBag.Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.DisplayOrder).ToListAsync();

        if (mode != "create" && id == null) return BadRequest();

        return type switch
        {
            "category" => await CategoryEditor(mode, id),
            "vocabulary" => await VocabularyEditor(mode, id),
            "quiz" => await QuizEditor(mode, id),
            _ => await LessonEditor(mode, id)
        };
    }

    private async Task<IActionResult> CategoryEditor(string mode, int? id)
    {
        if (mode == "create") return View("~/Views/AdminCategories/Create.cshtml", new Category());
        var item = await _context.Categories.FindAsync(id);
        if (item == null) return NotFound();
        var view = mode == "delete" ? "Delete" : "Edit";
        return View($"~/Views/AdminCategories/{view}.cshtml", item);
    }

    private async Task<IActionResult> LessonEditor(string mode, int? id)
    {
        if (mode == "create") return View("~/Views/AdminLessons/Create.cshtml", new Lesson());
        var item = await _context.Lessons.AsNoTracking().Include(l => l.Category).FirstOrDefaultAsync(l => l.Id == id);
        if (item == null) return NotFound();
        if (mode == "delete") await SetLessonDeleteStatsAsync(id!.Value);
        var view = mode switch { "details" => "Details", "delete" => "Delete", _ => "Edit" };
        return View($"~/Views/AdminLessons/{view}.cshtml", item);
    }

    // Match the legacy delete page's cascade counts.
    private async Task SetLessonDeleteStatsAsync(int id)
    {
        ViewBag.LearnerRecords = await _context.LessonProgress.CountAsync(p => p.LessonId == id)
            + await _context.PracticeAttempts.CountAsync(a => a.LessonId == id);
        ViewBag.ActivityCount = await _context.LessonActivities.CountAsync(a => a.LessonId == id);
        ViewBag.VocabularyLinkCount = await _context.LessonVocabulary.CountAsync(l => l.LessonId == id);
        ViewBag.GrammarLinkCount = await _context.LessonGrammarPoints.CountAsync(g => g.LessonId == id);
        ViewBag.SkillTaskCount = await _context.ListeningTasks.CountAsync(t => t.LessonId == id)
            + await _context.ReadingPassages.CountAsync(t => t.LessonId == id)
            + await _context.SpeakingTasks.CountAsync(t => t.LessonId == id)
            + await _context.WritingTasks.CountAsync(t => t.LessonId == id);
    }

    private async Task<IActionResult> VocabularyEditor(string mode, int? id)
    {
        if (mode == "create") return View("~/Views/AdminVocabulary/Create.cshtml", new Vocabulary());
        var item = await _context.Vocabulary.AsNoTracking().Include(v => v.Category).FirstOrDefaultAsync(v => v.Id == id);
        if (item == null) return NotFound();
        var view = mode switch { "details" => "Details", "delete" => "Delete", _ => "Edit" };
        return View($"~/Views/AdminVocabulary/{view}.cshtml", item);
    }

    private async Task<IActionResult> QuizEditor(string mode, int? id)
    {
        if (mode == "create") return View("~/Views/AdminQuizzes/Create.cshtml", new Quiz());
        var item = await _context.Quizzes.AsNoTracking().Include(q => q.Category).Include(q => q.Questions.OrderBy(x => x.DisplayOrder))
            .FirstOrDefaultAsync(q => q.Id == id);
        if (item == null) return NotFound();
        var view = mode switch { "details" => "Details", "delete" => "Delete", _ => "Edit" };
        return View($"~/Views/AdminQuizzes/{view}.cshtml", item);
    }

    private static string NormalizeType(string? type) => type?.ToLowerInvariant() switch
    {
        "category" or "categories" => "category",
        "vocabulary" or "vocab" => "vocabulary",
        "quiz" or "quizzes" => "quiz",
        _ => "lesson"
    };

    private static string NormalizeMode(string? mode, int? id) => mode?.ToLowerInvariant() switch
    {
        "create" => "create",
        "details" => "details",
        "delete" => "delete",
        _ => id.HasValue ? "edit" : "create"
    };
}
