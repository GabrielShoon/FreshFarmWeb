using FreshFarmWeb.Model;
using FreshFarmWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Net;
using System.Text.Encodings.Web;

namespace FreshFarmWeb.Pages
{
    public class ResetPasswordModel : PageModel
    {
		private readonly UserManager<ApplicationUser> userManager;

		public ResetPasswordModel(UserManager<ApplicationUser> userManager)
		{
			this.userManager = userManager;
		}

		[BindProperty]
		public string Email { get; set; }


		public void OnGet()
        {
        }

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				var existingUser = await userManager.FindByEmailAsync(Email);
				if (existingUser == null)
				{
					ModelState.AddModelError("Email", "Email Does Not Exist.");
					return Page();
				}

				var token = await userManager.GeneratePasswordResetTokenAsync(existingUser);

				var resetLink = Url.Page(
						"/ResetConfirm",
						pageHandler: null,
						values: new { userId = existingUser.Id, token = token },
						protocol: Request.Scheme);

				var client = new SmtpClient("smtp.gmail.com", 587)
				{
					Credentials = new NetworkCredential("gabrielshoonxd@gmail.com", "mjln njck kpoe dhfa"),
					EnableSsl = true
				};

				MailMessage mail = new MailMessage("freshfarm@gmail.com", Email, "Reset Password Link", $"Reset your password by clicking on this link: <a href={HtmlEncoder.Default.Encode(resetLink)}>link</a>");
				mail.IsBodyHtml = true;
				client.Send(mail);
				return RedirectToPage("Login");
			}
			return Page();
		}

	}
}
