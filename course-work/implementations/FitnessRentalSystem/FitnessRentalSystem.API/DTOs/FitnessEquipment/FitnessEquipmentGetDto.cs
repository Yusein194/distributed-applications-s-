namespace FitnessRentalSystem.API.DTOs.FitnessEquipment
{
    public class FitnessEquipmentGetDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string EquipmentType { get; set; } = string.Empty;

        public string? Functionality { get; set; }

        public double Weight { get; set; }

        public decimal RentalPricePerDay { get; set; }

        public bool IsAvailable { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
