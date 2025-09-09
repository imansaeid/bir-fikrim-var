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
    public class UsersApiController : ControllerBase
    {
        private readonly MYDBCONTXT _context;

        public UsersApiController(MYDBCONTXT context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            // 1) Veritabanındaki tüm User kayıtlarını çek
            var users = await _context.Users.ToListAsync();

            // 2) Entity -> DTO dönüşümü (Mapster)
            var dtoList = users.Adapt<List<UserDto>>();

            // 3) 200 OK ile DTO listesini döndür
            return Ok(dtoList);
        }

        // GET: api/Users/5
        [HttpGet("{id}")] // Bu endpoint /api/Users/{id} formatındaki GET isteklerini yakalar
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            // Veritabanında belirtilen id'ye sahip kullanıcıyı primary key üzerinden bul
            var user = await _context.Users.FindAsync(id);

            // Eğer kullanıcı bulunamazsa 404 Not Found döner
            if (user == null)
                return NotFound();

            // Mapster .Adapt ile User entity -> UserReadDto dönüştürülür
            // Bu sayede sadece UserReadDto'daki alanlar response'a dahil olur (ör: Password DTO'da yoksa dışarı çıkmaz)
            var dto = user.Adapt<UserDto>();

            // 200 OK ile birlikte UserReadDto JSON olarak client'a gönderilir
            return Ok(dto);
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
