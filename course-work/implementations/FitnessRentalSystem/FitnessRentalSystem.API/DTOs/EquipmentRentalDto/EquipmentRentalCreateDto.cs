using System.ComponentModel.DataAnnotations;

namespace FitnessRentalSystem.API.DTOs.EquipmentRentalDto
{
    public class EquipmentRentalCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int FitnessEquipmentId { get; set; }

        [Required]
        public DateTime RentDate { get; set; }

        
        public DateTime? ReturnDate { get; set; }

        [Range(0, 100000)]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";
    }
}
