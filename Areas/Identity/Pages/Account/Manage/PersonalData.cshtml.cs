using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NihongoAcademy.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public PersonalDataModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound($"Unable to load user.");
            }

            return Page();
        }
    }
}
