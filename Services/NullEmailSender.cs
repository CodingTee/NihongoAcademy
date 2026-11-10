// Services/NullEmailSender.cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace NihongoAcademy.Services;

/// <summary>
/// No-op IEmailSender. This app no longer sends email (the SMTP / verification-code
/// system was removed), but the scaffolded Identity UI pages that inject IEmailSender
/// — ResendEmailConfirmation and Manage/Email — still need a registered implementation,
/// so this stub is provided: it logs the intent instead of contacting any mail server.
/// </summary>
public sealed class NullEmailSender : IEmailSender
{
    private readonly ILogger<NullEmailSender> _logger;

    public NullEmailSender(ILogger<NullEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogWarning(
            "Email not sent (email sending was removed from this project). Recipient {Email}, subject \"{Subject}\"",
            email, subject);
        return Task.CompletedTask;
    }
}
