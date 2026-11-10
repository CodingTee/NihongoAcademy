using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace NihongoAcademy.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Resends the email-confirmation link. Email sending was removed from this project
    /// (the SMTP / verification-code system was deleted), and new accounts are confirmed
    /// automatically at registration, so there is nothing to resend - the page simply
    /// tells the user they are already set.
    /// </summary>
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ResendEmailConfirmationModel> _logger;

        public ResendEmailConfirmationModel(
            UserManager<IdentityUser> userManager,
            ILogger<ResendEmailConfirmationModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>A fresh confirmation link was emailed.</summary>
        public bool Sent { get; set; }

        /// <summary>The account is already confirmed (or unknown) - nothing to send.</summary>
        public bool AlreadyConfirmed { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Email sending was removed; accounts are confirmed at registration. Nothing to
            // resend, so report the account as already confirmed rather than emailing a link.
            AlreadyConfirmed = true;
            return Page();
        }
    }
}
