using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FitnessRentalSystem.API.Models
{
    public class EquipmentRental
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int FitnessEquipmentId { get; set; }

        [Required]
        public DateTime RentDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Returned, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public FitnessEquipment FitnessEquipment { get; set; } = null!;
    }
}
