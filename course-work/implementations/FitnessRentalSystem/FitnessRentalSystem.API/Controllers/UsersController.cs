using FitnessRentalSystem.API.Data;
using FitnessRentalSystem.API.DTOs.User;
using FitnessRentalSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessRentalSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGetDto>>> GetUsers(
            string? firstName,
            string? lastName,
            string? email,
            string? role,
            bool? isActive,
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = "firstName",
            string sortDirection = "asc")
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(u => u.FirstName.Contains(firstName));

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(u => u.LastName.Contains(lastName));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => u.Email.Contains(email));

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role.Contains(role));

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            query = sortBy?.ToLower() switch
            {
                "lastname" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.LastName)
                    : query.OrderBy(u => u.LastName),

                "email" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),

                "role" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.Role)
                    : query.OrderBy(u => u.Role),

                "createdat" => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt),

                _ => sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.FirstName)
                    : query.OrderBy(u => u.FirstName)
            };

            var result = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserGetDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserGetDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserGetDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserGetDto>> PostUser(UserCreateDto dto)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
                return BadRequest("Email already exists.");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash),
                Role = dto.Role,
                PhoneNumber = dto.PhoneNumber,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserGetDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.Id },
                result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(
            int id,
            UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != id);

            if (emailExists)
                return BadRequest("Email already exists.");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.Role = dto.Role;
            user.PhoneNumber = dto.PhoneNumber;
            user.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}