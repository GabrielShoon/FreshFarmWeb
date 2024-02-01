using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmWeb.ViewModels;
using FreshFarmWeb.Model;
using System.Net;
using System.Text.Json;
using System.Net.Mail;

namespace FreshFarmWeb.Pages
{
	public class LoginModel : PageModel
	{
		[BindProperty]
		public Login LModel { get; set; }

		private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuthDbContext dbContext;
        private readonly IHttpContextAccessor contxt;
        private readonly ILogger<LoginModel> logger;

		public LockoutStatusViewModel LockoutStatus { get; set; }

		public class LockoutStatusViewModel
		{
			public bool IsLockedOut { get; set; }
			public DateTimeOffset? LockoutEndDate { get; set; }
		}

		// Property to track the remaining login attempts
		public int RemainingLoginAttempts { get; set; }

		public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AuthDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<LoginModel> logger)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.dbContext = dbContext;
			contxt = httpContextAccessor;
			this.logger = logger;

			// Initialize LockoutStatus property
			LockoutStatus = new LockoutStatusViewModel();
		}
		
		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			try
			{
				var user = await userManager.FindByEmailAsync(LModel.Email);

				if (user == null)
				{
					// Handle the case where the user is not found
					ModelState.AddModelError("", "User not found. Please check your email and try again.");
					return Page();
				}

				if (user != null && await userManager.IsLockedOutAsync(user))
				{
					// Set lockout status properties
					LockoutStatus = new LockoutStatusViewModel
					{
						IsLockedOut = true,
						LockoutEndDate = await userManager.GetLockoutEndDateAsync(user)
					};

					// Convert UTC time to Singapore time
					LockoutStatus.LockoutEndDate = LockoutStatus.LockoutEndDate?.ToOffset(new TimeSpan(8, 0, 0)); // UTC+8

					ModelState.AddModelError("", "Account is locked. Please try again later.");
					return Page();
				}

				if (ModelState.IsValid)
				{
					if (ValidateCaptcha())
					{
						var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, false);

						if (identityResult.Succeeded)
						{
							// Reset access failed count upon successful login
							await userManager.ResetAccessFailedCountAsync(user);

							// Log login activity
							await LogActivityAsync("Login");

							// Create a secured session upon successful login
							await CreateSecuredSession(LModel.Email);

							return RedirectToPage("Index");
						}
						else if (identityResult.IsLockedOut)
						{
							// Lock the account if login fails multiple times
							await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(1));
							ModelState.AddModelError("", "Account is locked. Please try again later.");
						}
                        else if (identityResult.RequiresTwoFactor)
                        {
                            var existingUser = await userManager.FindByEmailAsync(LModel.Email);
                            if (existingUser == null)
                            {
                                ModelState.AddModelError("Email", "Email Does Not Exist.");
                                return Page();
                            }

                            var code = await userManager.GenerateTwoFactorTokenAsync(existingUser, "Email");

                            var message = $"Your one-time verification code is: {code}";

                            var client = new SmtpClient("smtp.gmail.com", 587)
                            {
                                Credentials = new NetworkCredential("gabrielshoonxd@gmail.com", "mjln njck kpoe dhfa"),
                                EnableSsl = true
                            };

                            MailMessage mail = new MailMessage("freshfarm@gmail.com", LModel.Email, "Your 2FA Code", message);
                            client.Send(mail);
                            return RedirectToPage("2FALogin", new { Email = LModel.Email });
                        }
                        else
						{
							// Increment access failed count
							await userManager.AccessFailedAsync(user);

							// Update remaining login attempts
							RemainingLoginAttempts = userManager.Options.Lockout.MaxFailedAccessAttempts - user.AccessFailedCount;

							if (user.AccessFailedCount >= userManager.Options.Lockout.MaxFailedAccessAttempts)
							{
								// Lock the account if login fails multiple times
								await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(1));
								ModelState.AddModelError("", "Account is locked. Please try again later.");
							}
							else
							{
								ModelState.AddModelError("", "Username or Password incorrect");
							}
						}
					}
					return Page();
				}

				return Page(); // Add this return statement
			}
			catch (Exception ex)
			{
				logger.LogError($"Exception during login: {ex}");
				throw;
			}
		}


		public class MyObject
		{
			public bool success { get; set; }
		}

		public bool ValidateCaptcha()
		{
			bool result = false;

			//When user submits the recaptcha form, the user gets a response POST parameter. 
			//captchaResponse consist of the user click pattern. Behaviour analytics! AI :) 
			string captchaResponse = Request.Form["g-recaptcha-response"];

			//To send a GET request to Google along with the response and Secret key.
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=6LenfmApAAAAALDFQTQCjrL9I-ug1G6Jhd__EoLY&response=" + captchaResponse);

			try
			{

				//Codes to receive the Response in JSON format from Google Server
				using (WebResponse wResponse = req.GetResponse())
				{
					using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
					{
						//The response in JSON format
						string jsonResponse = readStream.ReadToEnd();

						var jsonObject = JsonSerializer.Deserialize<MyObject>(jsonResponse);

						//Convert the string "False" to bool false or "True" to bool true
						result = Convert.ToBoolean(jsonObject.success);//

					}
				}

				return result;
			}
			catch (WebException ex)
			{
				throw ex;
			}
		}

		private async Task CreateSecuredSession(string userEmail)
        {
            // Retrieve the user after a successful sign-in
            var user = await signInManager.UserManager.FindByEmailAsync(userEmail);

            if (user != null)
            {
                contxt.HttpContext.Session.SetString("FullName", user.FullName);
                contxt.HttpContext.Session.SetString("Email", user.Email);
                contxt.HttpContext.Session.SetString("Gender", user.Gender);
                contxt.HttpContext.Session.SetString("PhoneNumber", user.PhoneNumber);
                contxt.HttpContext.Session.SetString("DeliveryAddress", user.DeliveryAddress);
                contxt.HttpContext.Session.SetString("AboutMe", user.AboutMe);
                contxt.HttpContext.Session.SetString("CreditCard", user.CreditCard);

                
                contxt.HttpContext.Session.SetString("IsAuthenticated", "true");
            }
            else
            {
                logger.LogWarning($"User not found during secured session creation.");
            }
        }

        private async Task LogActivityAsync(string activity)
        {
            var signInResult = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, false);

            if (signInResult.Succeeded)
            {
                // Retrieve the user after a successful sign-in
                var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);

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
                else
                {
                    logger.LogWarning($"User not found during login activity logging.");
                }
            }
            else
            {
                logger.LogWarning($"Login failed. Unable to capture login activity.");
            }
        }
		


	}
}
