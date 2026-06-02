using FitnessRentalSystem.API.Data;
using FitnessRentalSystem.API.DTOs.FitnessEquipment;
using FitnessRentalSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessRentalSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FitnessEquipmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FitnessEquipmentsController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FitnessEquipmentGetDto>>> GetFitnessEquipments(
    string? name,
    string? brand,
    string? equipmentType,
    bool? isAvailable,
    int pageNumber = 1,
    int pageSize = 10,
    string? sortBy = "name",
    string sortDirection = "asc")
        {
            var query = _context.FitnessEquipments.AsQueryable();

          

            if (!string.IsNullOrWhiteSpace(name))
            {
                var searchName = name.Trim().ToLower();

                query = query.Where(e =>
                    e.Name.ToLower().Contains(searchName));
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                var searchBrand = brand.Trim().ToLower();

                query = query.Where(e =>
                    e.Brand.ToLower().Contains(searchBrand));
            }

            if (!string.IsNullOrWhiteSpace(equipmentType))
            {
                var searchType = equipmentType.Trim().ToLower();

                query = query.Where(e =>
                    e.EquipmentType.ToLower().Contains(searchType));
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(e =>
                    e.IsAvailable == isAvailable.Value);
            }

            

            query = sortBy?.ToLower() switch
            {
                "brand" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Brand)
                    : query.OrderBy(e => e.Brand),

                "equipmenttype" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.EquipmentType)
                    : query.OrderBy(e => e.EquipmentType),

                "rentalpriceperday" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.RentalPricePerDay)
                    : query.OrderBy(e => e.RentalPricePerDay),

                "createdat" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.CreatedAt)
                    : query.OrderBy(e => e.CreatedAt),

                _ => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Name)
                    : query.OrderBy(e => e.Name)
            };

            

            var result = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new FitnessEquipmentGetDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Brand = e.Brand,
                    EquipmentType = e.EquipmentType,
                    Functionality = e.Functionality,
                    Weight = e.Weight,
                    RentalPricePerDay = e.RentalPricePerDay,
                    IsAvailable = e.IsAvailable,
                    ImageUrl = e.ImageUrl,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(result);
        }
        

        [HttpGet("{id}")]
        public async Task<ActionResult<FitnessEquipmentGetDto>> GetFitnessEquipment(int id)
        {
            var equipment = await _context.FitnessEquipments
                .Where(e => e.Id == id)
                .Select(e => new FitnessEquipmentGetDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Brand = e.Brand,
                    EquipmentType = e.EquipmentType,
                    Functionality = e.Functionality,
                    Weight = e.Weight,
                    RentalPricePerDay = e.RentalPricePerDay,
                    IsAvailable = e.IsAvailable,
                    ImageUrl = e.ImageUrl,
                    CreatedAt = e.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (equipment == null)
                return NotFound();

            return Ok(equipment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<FitnessEquipmentGetDto>> PostFitnessEquipment(
            FitnessEquipmentCreateDto dto)
        {
            var equipment = new FitnessEquipment
            {
                Name = dto.Name,
                Brand = dto.Brand,
                EquipmentType = dto.EquipmentType,
                Functionality = dto.Functionality,
                Weight = dto.Weight,
                RentalPricePerDay = dto.RentalPricePerDay,
                IsAvailable = dto.IsAvailable,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.FitnessEquipments.Add(equipment);
            await _context.SaveChangesAsync();

            var result = new FitnessEquipmentGetDto
            {
                Id = equipment.Id,
                Name = equipment.Name,
                Brand = equipment.Brand,
                EquipmentType = equipment.EquipmentType,
                Functionality = equipment.Functionality,
                Weight = equipment.Weight,
                RentalPricePerDay = equipment.RentalPricePerDay,
                IsAvailable = equipment.IsAvailable,
                ImageUrl = equipment.ImageUrl,
                CreatedAt = equipment.CreatedAt
            };

            return CreatedAtAction(
                nameof(GetFitnessEquipment),
                new { id = equipment.Id },
                result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutFitnessEquipment(
            int id,
            FitnessEquipmentUpdateDto dto)
        {
            var equipment = await _context.FitnessEquipments.FindAsync(id);

            if (equipment == null)
                return NotFound();

            equipment.Name = dto.Name;
            equipment.Brand = dto.Brand;
            equipment.EquipmentType = dto.EquipmentType;
            equipment.Functionality = dto.Functionality;
            equipment.Weight = dto.Weight;
            equipment.RentalPricePerDay = dto.RentalPricePerDay;
            equipment.IsAvailable = dto.IsAvailable;
            equipment.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFitnessEquipment(int id)
        {
            var equipment = await _context.FitnessEquipments.FindAsync(id);

            if (equipment == null)
                return NotFound();

            _context.FitnessEquipments.Remove(equipment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}