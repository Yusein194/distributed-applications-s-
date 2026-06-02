using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FitnessRentalSystem.API.Models
{
    public class FitnessEquipment
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

        [Column(TypeName = "decimal(10,2)")]
        public decimal RentalPricePerDay { get; set; }

        public bool IsAvailable { get; set; } = true;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<EquipmentRental> Rentals { get; set; } = new List<EquipmentRental>();

    }
}
