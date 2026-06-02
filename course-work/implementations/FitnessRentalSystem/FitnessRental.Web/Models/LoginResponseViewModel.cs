namespace FitnessRental.Web.Models
{
    public class LoginResponseViewModel
    {
        public string Token { get; set; } = string.Empty;

        public LoggedUserViewModel User { get; set; } = new();
        public class LoggedUserViewModel
        {
            public int Id { get; set; }

            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string Role { get; set; } = string.Empty;
        }
    }
}
