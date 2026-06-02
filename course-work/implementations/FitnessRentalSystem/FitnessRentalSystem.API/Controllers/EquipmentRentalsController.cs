using FitnessRentalSystem.API.Data;
using FitnessRentalSystem.API.DTOs.EquipmentRentalDto;
using FitnessRentalSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessRentalSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EquipmentRentalsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipmentRentalsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipmentRentalGetDto>>> GetEquipmentRentals(
    int? userId,
    int? fitnessEquipmentId,
    string? userEmail,
    string? equipmentName,
    string? status,
    DateTime? rentDate,
    int pageNumber = 1,
    int pageSize = 10,
    string? sortBy = "rentDate",
    string sortDirection = "asc")
        {
            var query = _context.EquipmentRentals
                .Include(r => r.User)
                .Include(r => r.FitnessEquipment)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            if (fitnessEquipmentId.HasValue)
                query = query.Where(r => r.FitnessEquipmentId == fitnessEquipmentId.Value);

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                var searchEmail = userEmail.Trim().ToLower();

                query = query.Where(r =>
                    r.User.Email.ToLower().Contains(searchEmail));
            }

            if (!string.IsNullOrWhiteSpace(equipmentName))
            {
                var searchEquipment = equipmentName.Trim().ToLower();

                query = query.Where(r =>
                    r.FitnessEquipment.Name.ToLower().Contains(searchEquipment));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var searchStatus = status.Trim().ToLower();

                query = query.Where(r =>
                    r.Status.ToLower().Contains(searchStatus));
            }

            if (rentDate.HasValue)
                query = query.Where(r =>
                    r.RentDate.Date == rentDate.Value.Date);

            query = sortBy?.ToLower() switch
            {
                "status" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.Status)
                    : query.OrderBy(r => r.Status),

                "totalprice" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.TotalPrice)
                    : query.OrderBy(r => r.TotalPrice),

                "createdat" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),

                _ => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.RentDate)
                    : query.OrderBy(r => r.RentDate)
            };

            var result = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new EquipmentRentalGetDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    FitnessEquipmentId = r.FitnessEquipmentId,
                    RentDate = r.RentDate,
                    ReturnDate = r.ReturnDate,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    UserEmail = r.User.Email,
                    EquipmentName = r.FitnessEquipment.Name
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EquipmentRentalGetDto>> GetEquipmentRental(int id)
        {
            var rental = await _context.EquipmentRentals
                .Include(r => r.User)
                .Include(r => r.FitnessEquipment)
                .Where(r => r.Id == id)
                .Select(r => new EquipmentRentalGetDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    FitnessEquipmentId = r.FitnessEquipmentId,
                    RentDate = r.RentDate,
                    ReturnDate = r.ReturnDate,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    UserEmail = r.User.Email,
                    EquipmentName = r.FitnessEquipment.Name
                })
                .FirstOrDefaultAsync();

            if (rental == null)
                return NotFound();

            return Ok(rental);
        }
        [HttpPost]
        public async Task<ActionResult<EquipmentRentalGetDto>> PostEquipmentRental(
    EquipmentRentalCreateDto dto)
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var equipment = await _context.FitnessEquipments
                .FirstOrDefaultAsync(e => e.Id == dto.FitnessEquipmentId);

            if (equipment == null)
                return BadRequest("Fitness equipment not found.");

            var userExists = await _context.Users
                .AnyAsync(u => u.Id == dto.UserId);

            if (!userExists)
                return BadRequest("User not found.");

            var totalDays =
                   (dto.ReturnDate.Value - dto.RentDate).TotalDays;

            if (totalDays <= 0)
                totalDays = 1;

            var totalPrice = (decimal)totalDays * equipment.RentalPricePerDay;

            var rental = new EquipmentRental
            {
                UserId = dto.UserId,
                FitnessEquipmentId = dto.FitnessEquipmentId,
                RentDate = dto.RentDate,
                ReturnDate = dto.ReturnDate,
                TotalPrice = totalPrice,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.EquipmentRentals.Add(rental);

            await _context.SaveChangesAsync();

            var result = await _context.EquipmentRentals
                .Include(r => r.User)
                .Include(r => r.FitnessEquipment)
                .Where(r => r.Id == rental.Id)
                .Select(r => new EquipmentRentalGetDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    FitnessEquipmentId = r.FitnessEquipmentId,
                    RentDate = r.RentDate,
                    ReturnDate = r.ReturnDate,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    UserEmail = r.User.Email,
                    EquipmentName = r.FitnessEquipment.Name
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetEquipmentRental),
                new { id = rental.Id },
                result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEquipmentRental(
            int id,
            EquipmentRentalUpdateDto dto)
        {
            var rental = await _context.EquipmentRentals.FindAsync(id);

            if (rental == null)
                return NotFound();

            rental.UserId = dto.UserId;
            rental.FitnessEquipmentId = dto.FitnessEquipmentId;
            rental.RentDate = dto.RentDate;
            rental.ReturnDate = dto.ReturnDate;
            rental.TotalPrice = dto.TotalPrice;
            rental.Status = dto.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEquipmentRental(int id)
        {
            var rental = await _context.EquipmentRentals.FindAsync(id);

            if (rental == null)
                return NotFound();

            _context.EquipmentRentals.Remove(rental);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}