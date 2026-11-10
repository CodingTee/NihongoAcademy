using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using NihongoAcademy.Data;
using NihongoAcademy.Models;
using NihongoAcademy.Services;

namespace NihongoAcademy.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = string.Empty;

        public IList<AuthenticationScheme>? ExternalLogins { get; set; }

        /// <summary>
        /// Fixed security questions offered in the registration dropdown. The posted
        /// value must match one of these exactly (server-side whitelist check).
        /// </summary>
        public static readonly string[] AllowedSecurityQuestions = new[]
        {
            "What is your mother's name?",
            "What was your first pet's name?",
            "What city were you born in?"
        };

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please set a security question.")]
            [Display(Name = "Security question")]
            public string SecurityQuestion { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please provide an answer to your security question.")]
            [Display(Name = "Security answer")]
            public string SecurityAnswer { get; set; } = string.Empty;

        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // Server-side whitelist: the posted question must be one of the fixed
                // dropdown options (a malicious client could post anything otherwise).
                if (!AllowedSecurityQuestions.Contains(Input.SecurityQuestion))
                {
                    ModelState.AddModelError("Input.SecurityQuestion", "Please choose one of the listed security questions.");
                    return Page();
                }

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                // Accounts are confirmed automatically at registration (the email
                // verification-code step was removed), so the new account is created
                // already confirmed - no separate confirmation email is needed.
                user.EmailConfirmed = true;
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var roleResult = await _userManager.AddToRoleAsync(user, "Student");
                    if (!roleResult.Succeeded)
                    {
                        await _userManager.DeleteAsync(user);
                        foreach (var error in roleResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        return Page();
                    }
                    _context.UserProfiles.Add(new UserProfile
                    {
                        UserId = userId
                    });
                    await _context.SaveChangesAsync();

                    // Store the security question + a hashed answer as claims. The forgot-password
                    // flow reads these back to verify the person really owns the account before
                    // resetting the password (replacing the removed email verification-code system).
                    await _userManager.AddClaimAsync(user, new Claim("SecurityQuestion", Input.SecurityQuestion));
                    await _userManager.AddClaimAsync(user, new Claim("SecurityAnswerHash", SecurityAnswerHasher.Hash(Input.SecurityAnswer)));

                    // Account created and confirmed, so sign in directly and go to onboarding.
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToPage("Onboarding", new { returnUrl });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
