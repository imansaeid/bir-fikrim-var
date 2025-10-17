using bir_fikrim_var.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BirFikrimVar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        private readonly MYDBCONTXT _context;

        public UsersApiController(MYDBCONTXT context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users.Adapt<List<UserDto>>());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user.Adapt<UserDto>());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UpdateUserDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || id != user.UserId)
            {
                return BadRequest();
            }

            dto.Adapt(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterUserDto dto)
        {
            var user = dto.Adapt<User>();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var result = user.Adapt<UserDto>();
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginUserDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Password == dto.Password);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            var result = user.Adapt<UserDto>();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(new List<object>());
            }

            var users = await _context.Users
                .Where(u => u.FullName.Contains(term) || u.Email.Contains(term))
                .Select(u => new
                {
                    id = u.UserId,
                    name = u.FullName,
                    username = u.Email
                })
                .Take(5)
                .ToListAsync();

            return Ok(users);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
