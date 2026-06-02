using System.ComponentModel.DataAnnotations;

namespace FitnessRental.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
