using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Controllers
{
    /// <summary>
    /// Read-only audit viewer (login + operation logs). Both admin levels can review it.
    /// </summary>
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? scope = "all", string? from = null, string? to = null, string? q = null)
        {
            DateTime? fromDate = ParseDate(from);
            DateTime? toDate = ParseDate(to)?.AddDays(1).AddTicks(-1); // inclusive end of day
            var term = q?.Trim();

            var loginQuery = _context.LoginLogs.AsQueryable();
            if (fromDate.HasValue) loginQuery = loginQuery.Where(l => l.LoginTime >= fromDate.Value);
            if (toDate.HasValue) loginQuery = loginQuery.Where(l => l.LoginTime <= toDate.Value);
            if (!string.IsNullOrEmpty(term)) loginQuery = loginQuery.Where(l => (l.Email != null && l.Email.Contains(term)) || (l.UserId != null && l.UserId.Contains(term)));

            var opQuery = _context.OperationLogs.AsQueryable();
            if (fromDate.HasValue) opQuery = opQuery.Where(o => o.Timestamp >= fromDate.Value);
            if (toDate.HasValue) opQuery = opQuery.Where(o => o.Timestamp <= toDate.Value);
            if (!string.IsNullOrEmpty(term))
                opQuery = opQuery.Where(o => (o.UserEmail != null && o.UserEmail.Contains(term))
                    || (o.Action != null && o.Action.Contains(term))
                    || (o.UserId != null && o.UserId.Contains(term)));

            var loginLogs = scope == "operation"
                ? new List<LoginLog>()
                : await loginQuery.OrderByDescending(l => l.LoginTime).Take(300).ToListAsync();

            var operationLogs = scope == "login"
                ? new List<OperationLog>()
                : await opQuery.OrderByDescending(o => o.Timestamp).Take(300).ToListAsync();

            ViewBag.Scope = scope;
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Q = q;
            ViewBag.LoginLogs = loginLogs;
            ViewBag.OperationLogs = operationLogs;
            return View();
        }

        private static DateTime? ParseDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return DateTime.TryParse(s, out var d) ? d : null;
        }
    }
}
