using System.ComponentModel.DataAnnotations;

namespace FreshFarmWeb.ViewModels
{
	public class ResetPassword
	{
		[Required]
		// [DataType(DataType.Password)]
		[MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{12,}$",
			ErrorMessage = "Passwords must be at least 12 characters long and contain at least an uppercase letter, lowercase letter, digit, and a symbol")]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
		public string ConfirmPassword { get; set; }
	}
}
