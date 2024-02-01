using System.ComponentModel.DataAnnotations;

namespace FreshFarmWeb.ViewModels
{
	public class Register
	{
        [Required]
        [DataType(DataType.Text)]
		[RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Only letters and spaces are allowed in the full name.")]
        public string FullName { get; set; }

        [Required]
        [DataType(DataType.CreditCard)]
        [RegularExpression(@"^(\d\s?){16}$", ErrorMessage = "Credit card must be 16 digits and may include spaces.")]
        public string CreditCard { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\d{4}\s?\d{4}$", ErrorMessage = "Please enter a valid 8-digit phone number.")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string DeliveryAddress { get; set; }

        [Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

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

        [Required]
        [DataType(DataType.Upload)]
		[AllowedExtensions(new string[] { ".jpg" }, ErrorMessage = "Only .jpg files are allowed.")]
		public IFormFile Photo { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string AboutMe { get; set; }




		// Custom attribute for allowed file extensions
		private class AllowedExtensionsAttribute : ValidationAttribute
		{
			private readonly string[] _extensions;

			public AllowedExtensionsAttribute(string[] extensions)
			{
				_extensions = extensions;
			}

			protected override ValidationResult IsValid(object value, ValidationContext validationContext)
			{
				if (value is IFormFile file)
				{
					var extension = Path.GetExtension(file.FileName);

					if (!_extensions.Contains(extension.ToLower()))
					{
						return new ValidationResult(GetErrorMessage());
					}
				}

				return ValidationResult.Success;
			}

			public string GetErrorMessage()
			{
				return "Invalid file format. Only allowed extensions are: " + string.Join(", ", _extensions);
			}
		}

	}
}