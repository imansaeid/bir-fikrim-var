using bir_fikrim_var.Models;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bir_fikrim_var.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesApiController : ControllerBase
    {
        private readonly MYDBCONTXT _context;

        public LikesApiController(MYDBCONTXT context)
        {
            _context = context;
        }

        // GET: api/Likes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetLikes()
        {
            // 1. Veritabanındaki tüm Like kayıtlarını asenkron olarak al
            var likes = await _context.Likes.ToListAsync();

            // 2. Mapster ile entity listesini DTO listesine dönüştür
            var dtoList = likes.Adapt<List<LikeDto>>();

            // 3. DTO listesini 200 OK ile döndür
            return Ok(dtoList);
        }

        // GET: api/Likes/5

        [HttpGet("{id}")]
        public async Task<ActionResult<LikeDto>> GetLike(int id)
        {
            // 1. Veritabanından primary key ile Like entity'sini asenkron olarak getir
            var like = await _context.Likes.FindAsync(id);

            // 2. Eğer böyle bir kayıt yoksa 404 Not Found dön
            if (like == null)
                return NotFound();

            // 3. Mapster ile Like entity'sini LikeDto'ya dönüştür
            var dto = like.Adapt<LikeDto>();

            // 4. 200 OK ile DTO'yu JSON olarak client'a gönder
            return Ok(dto);
        }

        // PUT: api/Likes/5
      

[HttpPost]

        // POST: api/Likes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LikeDto>> PostLike(CreateLikeDto createDto)
        {
            // 1. Eğer CreatedDate boşsa, şimdiye ayarla
            if (createDto.CreatedDate == null)
                createDto.CreatedDate = DateTime.Now;

            // 2. Mapster ile DTO'dan Like entity'si oluştur
            var like = createDto.Adapt<Like>();

            // 3. Veritabanına yeni Like entity'sini ekle
            _context.Likes.Add(like);

            // 4. Değişiklikleri veritabanına kaydet
            await _context.SaveChangesAsync();

            // 5. Kaydedilen entity'yi LikeDto'ya dönüştür ve client'a dön
            var dto = like.Adapt<LikeDto>();

            // 6. 201 Created ile birlikte kaydın detaylarını döndür
            return CreatedAtAction("GetLike", new { id = like.LikeId }, dto);
        }
        // DELETE: api/Likes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLike(int id)
        {
            var like = await _context.Likes.FindAsync(id);
            if (like == null)
            {
                return NotFound();
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LikeExists(int id)
        {
            return _context.Likes.Any(e => e.LikeId == id);
        }
    }
}
