using System.ComponentModel.DataAnnotations;

namespace FitnessRentalSystem.API.DTOs
{
    public class LoginDto
    {
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
