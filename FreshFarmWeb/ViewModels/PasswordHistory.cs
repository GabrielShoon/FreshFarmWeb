namespace FreshFarmWeb.ViewModels
{
	public class PasswordHistory
	{
		public int Id { get; set; }

		public string UserId { get; set; }

		public string PasswordHash { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
