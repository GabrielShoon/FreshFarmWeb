using FreshFarmWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmWeb.Pages
{
	public class LogoutModel : PageModel
	{
		private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuthDbContext dbContext;
        private readonly IHttpContextAccessor contxt;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AuthDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.dbContext = dbContext;
            contxt = httpContextAccessor;
        }

        public void OnGet() { }

		public async Task<IActionResult> OnPostLogoutAsync()
		{

            // Log logout activity
            await LogActivityAsync("Logout");

            // Clear session
            contxt.HttpContext.Session.Clear();

            await signInManager.SignOutAsync();
			return RedirectToPage("Login");
		}

		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}

        private async Task LogActivityAsync(string activity)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            if (user != null)
            {
                var auditLog = new AuditLog
                {
                    UserId = user.Id,
                    Activity = activity,
                    Timestamp = DateTime.Now,
                };

                dbContext.AuditLogs.Add(auditLog);
                await dbContext.SaveChangesAsync();
            }
        }

    }
}
