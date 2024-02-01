using FreshFarmWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmWeb.Pages
{
    public class _2FALoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AuthDbContext dbContext;
        private readonly IHttpContextAccessor contxt;
        private readonly ILogger<LoginModel> logger;

        public _2FALoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AuthDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<LoginModel> logger)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.dbContext = dbContext;
            contxt = httpContextAccessor;
            this.logger = logger;
        }

        [BindProperty]
        public string Code { get; set; }

        public async Task<IActionResult> OnPostAsync(string Email)
        {
            if (ModelState.IsValid)
            {
                var identityResult = await signInManager.TwoFactorSignInAsync("Email", Code, false, true);
                if (identityResult.Succeeded)
                {
    
                    // Create a secured session upon successful login
                    await CreateSecuredSession(Email);

                return RedirectToPage("Index");

                }
                else
                {
                    return RedirectToPage("Login");
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnGetAsync(string email)
        {
            if (email == null)
            {
                return RedirectToPage("Login");
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToPage("Login");

            }

            return Page();
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
    }
}
