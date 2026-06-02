using System.ComponentModel.DataAnnotations;

namespace FitnessRentalSystem.API.DTOs.EquipmentRentalDto
{
    public class EquipmentRentalUpdateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int FitnessEquipmentId { get; set; }

        [Required]
        public DateTime RentDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";
    }
}
