using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NihongoAcademy.Data;
using NihongoAcademy.Services;

namespace NihongoAcademy.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly AuditLogService _audit;

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<LoginModel> logger, AuditLogService audit)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _audit = audit;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IList<AuthenticationScheme> ExternalLogins { get; private set; } = new List<AuthenticationScheme>();
        public string ReturnUrl { get; private set; } = string.Empty;

        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>True when arriving from a completed password reset (Login?reset=ok).</summary>
        public bool ResetSucceeded { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null, string? remoteError = null, string? reset = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            // External provider failures bounce back here with ?remoteError=.
            if (!string.IsNullOrEmpty(remoteError))
                ModelState.AddModelError(string.Empty, $"External login did not complete: {remoteError}");

            // Password-reset success notice from ForgotPassword.
            if (reset == "ok")
                ResetSucceeded = true;

            returnUrl ??= Url.Content("~/");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid) return Page();

            // Look the account up by email: some accounts (seeded or created by an older
            // flow) have a UserName that differs from the email, and PasswordSignInAsync's
            // string overload searches by UserName only - which would reject valid logins.
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user is null)
            {
                // Same generic message as a wrong password so the page cannot be used
                // to probe which addresses are registered.
                await _audit.RecordLoginAsync(null, Input.Email, false, "Invalid credentials");
                ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, Input.Password, Input.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                await _audit.RecordLoginAsync(user.Id, user.Email, true);
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                await _audit.RecordLoginAsync(user.Id, user.Email, false, "Locked out");
                return RedirectToPage("./Lockout");
            }

            await _audit.RecordLoginAsync(user.Id, user.Email, false, "Invalid credentials");
            ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
            return Page();
        }
    }
}
