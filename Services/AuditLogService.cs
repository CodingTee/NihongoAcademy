using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NihongoAcademy.Data;
using NihongoAcademy.Models;

namespace NihongoAcademy.Services
{
    /// <summary>
    /// Writes login and operation audit rows. Kept deliberately thin so controllers and
    /// the login page can call it without ceremony. Time is sourced from IAppClock so all
    /// audit entries share the application's UTC+8 timeline.
    /// </summary>
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppClock _clock;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IAppClock clock, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? ClientIp() => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        private string? UserAgent() => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

        public async Task RecordLoginAsync(string? userId, string? email, bool succeeded, string? failureReason = null)
        {
            _context.LoginLogs.Add(new LoginLog
            {
                UserId = userId,
                Email = email,
                LoginTime = _clock.Now,
                IpAddress = ClientIp(),
                UserAgent = UserAgent(),
                Succeeded = succeeded,
                FailureReason = failureReason
            });
            await _context.SaveChangesAsync();
        }

        public async Task RecordOperationAsync(string action, string? entityType = null, string? entityId = null, string? description = null)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var actorUserId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var actorEmail = user?.Identity?.Name;

            _context.OperationLogs.Add(new OperationLog
            {
                UserId = actorUserId,
                UserEmail = actorEmail,
                Timestamp = _clock.Now,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                IpAddress = ClientIp()
            });
            await _context.SaveChangesAsync();
        }
    }
}
