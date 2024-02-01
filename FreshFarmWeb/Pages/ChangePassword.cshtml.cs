using FreshFarmWeb.Model;
using FreshFarmWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmWeb.Pages
{
	public class ChangePasswordModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly AuthDbContext _dbContext;

		public ChangePasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AuthDbContext dbContext)
		{
			_dbContext = dbContext;
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[BindProperty]
		public NewPassword ChangePassword { get; set; }

		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(User);

				if (user == null)
				{
					return NotFound();
				}

				var passwordHash = user.PasswordHash;

				var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, ChangePassword.CurrentPassword);

				if (!isCurrentPasswordValid)
				{
					ModelState.AddModelError("ChangePassword.CurrentPassword", "Your Current password is incorrect.");
					return Page();
				}

				if (IsPasswordInHistory(user, ChangePassword.ConfirmPassword))
				{
					ModelState.AddModelError("ChangePassword.ConfirmNewPassword", "Avoid reusing previous passwords.");
					return Page();
				}


				var changePasswordResult = await _userManager.ChangePasswordAsync(user, ChangePassword.CurrentPassword, ChangePassword.ConfirmPassword);

				if (changePasswordResult.Succeeded)
				{
					AddPasswordToHistory(user, passwordHash);

					await _signInManager.RefreshSignInAsync(user);
					return RedirectToPage("Index");
				}

				foreach (var error in changePasswordResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			return Page();
		}

		private bool IsPasswordInHistory(ApplicationUser user, string newPassword)
		{
			var userPasswordHistories = GetUserPasswordHistories(user);

			var passwordHasher = new PasswordHasher<ApplicationUser>();
			return userPasswordHistories.Any(passwordHistory =>
				passwordHasher.VerifyHashedPassword(user, passwordHistory.PasswordHash, newPassword) == PasswordVerificationResult.Success);
		}

		private List<PasswordHistory> GetUserPasswordHistories(ApplicationUser user)
		{
			return _dbContext.PasswordHistories
				.Where(ph => ph.UserId == user.Id)
				.ToList();
		}

		private void AddPasswordToHistory(ApplicationUser user, string currentPassword)
		{
			var userPasswordHistories = GetUserPasswordHistories(user);

			var passwordHistory = new PasswordHistory
			{
				UserId = user.Id,
				PasswordHash = currentPassword,
				CreatedAt = DateTime.UtcNow
			};

			_dbContext.PasswordHistories.Add(passwordHistory);

			if (userPasswordHistories.Count >= 2)
			{
				RemoveExcessPasswordEntries(userPasswordHistories);
			}

			_dbContext.SaveChanges();
		}

		private void RemoveExcessPasswordEntries(List<PasswordHistory> userPasswordHistories)
		{
			var entriesToRemove = userPasswordHistories.Skip(2).ToList();
			_dbContext.PasswordHistories.RemoveRange(entriesToRemove);
		}




	}
}
