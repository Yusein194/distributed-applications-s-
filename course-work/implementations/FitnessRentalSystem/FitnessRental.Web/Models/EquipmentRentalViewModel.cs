using System.ComponentModel.DataAnnotations;

namespace FitnessRental.Web.Models
{
    public class EquipmentRentalViewModel
    {
        public int Id { get; set; }

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

        public DateTime CreatedAt { get; set; }

        public string? UserEmail { get; set; }

        public string? EquipmentName { get; set; }
    }
}
