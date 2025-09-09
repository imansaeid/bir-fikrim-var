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
    public class CommentsController : ControllerBase
    {
        private readonly MYDBCONTXT _context;

        public CommentsController(MYDBCONTXT context)
        {
            _context = context;
        }

        // GET: api/Comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            return await _context.Comments.ToListAsync();
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment(int id)
        {
            // 1. Veritabanından primary key ile Comment entity'sini asenkron olarak getir
            var comment = await _context.Comments.FindAsync(id);

            // 2. Eğer böyle bir kayıt yoksa 404 Not Found dön
            if (comment == null)
                return NotFound();

            // 3. Mapster ile Comment entity'sini CommentDto'ya dönüştür
            var dto = comment.Adapt<CommentDto>();

            // 4. 200 OK ile DTO'yu JSON olarak client'a gönder
            return Ok(dto);
        }


        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, CreateCommentDTO updateDto)
        {
            // 1. Veritabanından güncellenecek Comment entity'sini getir
            var existingComment = await _context.Comments.FindAsync(id);

            // 2. Eğer böyle bir kayıt yoksa 404 Not Found dön
            if (existingComment == null)
                return NotFound();

            // 3. Mapster ile DTO'daki alanları mevcut Comment entity'sine kopyala
            updateDto.Adapt(existingComment);

            await _context.SaveChangesAsync();

            return NoContent();
        }





        // POST: api/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CommentDto>> PostComment(CreateCommentDTO createDto)
        {
            // 1. Mapster ile CreateCommentDTO'dan Comment entity'si oluştur
            var comment = createDto.Adapt<Comment>();

            // 2. Veritabanına yeni Comment entity'sini ekle
            _context.Comments.Add(comment);

            // 3. Değişiklikleri veritabanına kaydet
            await _context.SaveChangesAsync();

            // 4. Kaydedilen entity'yi CommentDto'ya dönüştür ve client'a döndür
            var dto = comment.Adapt<CommentDto>();

            // 5. 201 Created ile birlikte kaydın detaylarını döndür
            return CreatedAtAction("GetComment", new { id = comment.CommentId }, dto);
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
