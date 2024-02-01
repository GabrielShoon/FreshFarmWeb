using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmWeb.ViewModels;
using FreshFarmWeb.Model;
using Microsoft.AspNetCore.DataProtection;

namespace FreshFarmWeb.Pages
{
    public class RegisterModel : PageModel
    {

        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Ensure the Photo is not null before accessing its properties
                if (RModel.Photo != null)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + RModel.Photo.FileName;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await RModel.Photo.CopyToAsync(fileStream);
                    }

                    var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                    var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                    var user = new ApplicationUser()
                    {
                        UserName = RModel.Email,
                        Email = RModel.Email,
                        FullName = RModel.FullName,
                        CreditCard = protector.Protect(RModel.CreditCard),
                        Gender = RModel.Gender,
                        PhoneNumber = RModel.PhoneNumber,
                        DeliveryAddress = RModel.DeliveryAddress,
                        PhotoPath = uniqueFileName,
                        AboutMe = RModel.AboutMe,
                        EmailConfirmed = true,
                        TwoFactorEnabled = true
                    };

                    var result = await userManager.CreateAsync(user, RModel.Password);

                    if (result.Succeeded)
                    {
                        
                        return RedirectToPage("Login");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Please upload a photo.");
                }
            }
			return Page();
        }

    }
}
