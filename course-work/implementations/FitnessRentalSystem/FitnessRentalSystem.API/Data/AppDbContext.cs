using FitnessRentalSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessRentalSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<FitnessEquipment> FitnessEquipments { get; set; }
        public DbSet<EquipmentRental> EquipmentRentals { get; set; }
    }
}
