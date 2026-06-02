namespace FitnessRentalSystem.API.DTOs.EquipmentRentalDto
{
    public class EquipmentRentalGetDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int FitnessEquipmentId { get; set; }

        public DateTime RentDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string? UserEmail { get; set; }

        public string? EquipmentName { get; set; }
    }
}
