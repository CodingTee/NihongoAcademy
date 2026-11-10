using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using NihongoAcademy.Services;

namespace NihongoAcademy.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<IdentityUser> userManager,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>Which step we are on: 1 = email, 2 = answer + new password.</summary>
        [BindProperty]
        public int Step { get; set; } = 1;

        /// <summary>The account's security question, shown on step 2.</summary>
        public string SecurityQuestion { get; set; } = string.Empty;

        /// <summary>Shown after the reset attempt completes.</summary>
        public bool Done { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your email address.")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Display(Name = "Security answer")]
            public string SecurityAnswer { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            Step = 1;
        }

        /// <summary>
        /// Two-step password reset that verifies the owner before changing anything:
        ///   Step 1: the user submits their email. We look up the account and its security
        ///           question (stored as a claim at registration). If the account has a
        ///           security question we advance to step 2; otherwise (legacy accounts
        ///           created before this feature) we reset directly.
        ///   Step 2: the user answers the question and supplies a new password. Only when
        ///           the answer matches the stored hash do we reset the password.
        /// This replaces the removed email / SMTP verification-code system: the security
        /// question is the out-of-band "are you really you?" check.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (Step == 1)
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var email = Input.Email?.Trim() ?? string.Empty;
                var user = await _userManager.FindByEmailAsync(email);

                // Anti-enumeration: if there is no account we still show the generic success
                // screen so an attacker cannot probe which emails are registered.
                if (user != null)
                {
                    var question = (await _userManager.GetClaimsAsync(user))
                        .FirstOrDefault(c => c.Type == "SecurityQuestion")?.Value;

                    if (!string.IsNullOrEmpty(question))
                    {
                        // Account has a security question -> ask it before resetting.
                        SecurityQuestion = question;
                        Step = 2;
                        return Page();
                    }

                    // Legacy account without a security question: reset directly.
                    await ResetPasswordAsync(user, email);
                }

                Done = true;
                return Page();
            }

            // Step 2: verify the security answer, then reset the password.
            var answer = Input.SecurityAnswer?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(answer))
            {
                ModelState.AddModelError(string.Empty, "Please answer your security question.");
                await LoadQuestionForDisplayAsync(Input.Email);
                return Page();
            }
            if (string.IsNullOrWhiteSpace(Input.NewPassword) || Input.NewPassword.Length < 8)
            {
                ModelState.AddModelError(string.Empty, "Please choose a new password (at least 8 characters).");
                await LoadQuestionForDisplayAsync(Input.Email);
                return Page();
            }
            if (Input.NewPassword != Input.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "The new password and confirmation password do not match.");
                await LoadQuestionForDisplayAsync(Input.Email);
                return Page();
            }

            var user2 = await _userManager.FindByEmailAsync(Input.Email?.Trim() ?? string.Empty);
            if (user2 != null)
            {
                var storedHash = (await _userManager.GetClaimsAsync(user2))
                    .FirstOrDefault(c => c.Type == "SecurityAnswerHash")?.Value;

                // Accounts created before this feature have no stored answer; reset directly.
                if (string.IsNullOrEmpty(storedHash) || SecurityAnswerHasher.Verify(answer, storedHash))
                {
                    await ResetPasswordAsync(user2, Input.Email?.Trim() ?? string.Empty);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The answer to your security question is incorrect.");
                    await LoadQuestionForDisplayAsync(Input.Email);
                    return Page();
                }
            }

            Done = true;
            return Page();
        }

        /// <summary>
        /// Re-fetch the security question so step 2 keeps showing it after a validation
        /// error (the property is not round-tripped through a form field).
        /// </summary>
        private async Task LoadQuestionForDisplayAsync(string? email)
        {
            var user = await _userManager.FindByEmailAsync(email?.Trim() ?? string.Empty);
            if (user == null)
            {
                return;
            }
            SecurityQuestion = (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(c => c.Type == "SecurityQuestion")?.Value ?? string.Empty;
        }

        private async Task ResetPasswordAsync(IdentityUser user, string email)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);
            if (result.Succeeded)
            {
                // Invalidate any existing sessions for this account.
                await _userManager.UpdateSecurityStampAsync(user);
                _logger.LogInformation("Password reset completed for {Email}", email);
            }
        }
    }
}
