using FreshFarmWeb.Model;
using FreshFarmWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmWeb.Pages
{
    public class ResetConfirmModel : PageModel
    {
		private readonly UserManager<ApplicationUser> userManager;
		private readonly AuthDbContext dbContext;

		public ResetConfirmModel(UserManager<ApplicationUser> userManager, AuthDbContext dbContext)
		{
			this.dbContext = dbContext;
			this.userManager = userManager;
		}

		public async Task<IActionResult> OnGetAsync(string userId, string token)
		{
			if (userId == null || token == null)
			{
				return RedirectToPage("Login");
			}

			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return RedirectToPage("Login");

			}

			return Page();
		}

		[BindProperty]
		public ResetPassword ResetPassword { get; set; }

		public async Task<IActionResult> OnPostAsync(string userId, string token)
		{
			if (ModelState.IsValid)
			{

				var user = await userManager.FindByIdAsync(userId);
				if (user == null)
				{
					return NotFound();
				}

				var changePasswordResult = await userManager.ResetPasswordAsync(user, token, ResetPassword.ConfirmPassword);

				if (changePasswordResult.Succeeded)
				{					
					await userManager.UpdateAsync(user);
					return RedirectToPage("Login");
				}

				foreach (var error in changePasswordResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return Page();
		}
	}
}
