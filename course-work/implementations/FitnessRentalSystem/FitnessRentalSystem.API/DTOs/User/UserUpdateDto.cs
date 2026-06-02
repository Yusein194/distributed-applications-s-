using System.ComponentModel.DataAnnotations;

namespace FitnessRentalSystem.API.DTOs.User
{
    public class UserUpdateDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User";

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}
