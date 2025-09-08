using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bir_fikrim_var.Models;
using Mapster;

namespace bir_fikrim_var.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MYDBCONTXT _context;

        public UsersController(MYDBCONTXT context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UpdateUserDTO dto)
        {
            // 1) Var mı yok mu kontrol et
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
                return NotFound(); // 404

            // 2) DTO -> Entity map (sadece FullName/Email/Password alanlarını günceller)
            //    Mapster .Adapt ile mevcut tracked entity'nin üstüne yazıyoruz.
            dto.Adapt(user);

            // 3) Değişiklikleri kaydet
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Aynı anda başka biri silmiş/güncellemiş olabilir
                var stillExists = await _context.Users.AnyAsync(u => u.UserId == id);
                if (!stillExists)
                    return NotFound();

                throw; // beklenmeyen durum: bubble up
            }

            // 4) 204 - içerik yok (başarılı güncelleme)
            return NoContent();
        }
    

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser(CreateUserDto getFromUser)
        {
            //we have to adapt the things we get from userdto to user
            var user = getFromUser.Adapt<User>();

            //save to database 
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // respond if ok or no 
            var result = user.Adapt<UserDto>();

            return CreatedAtAction("GetUser", new { id = user.UserId }, result);
        }

        // DELETE: api/Users/5
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

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
