using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessRental.Web.Models
{
    public class FitnessEquipmentViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EquipmentType { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Functionality { get; set; }

        public double Weight { get; set; }

        public decimal RentalPricePerDay { get; set; }

        public bool IsAvailable { get; set; } = true;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
