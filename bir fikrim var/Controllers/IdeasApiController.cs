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
    public class IdeasApiController : ControllerBase
    {
        private readonly MYDBCONTXT _context;

        public IdeasApiController(MYDBCONTXT context)
        {
            _context = context;
        }

        // GET: api/Ideas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Idea>>> GetIdeas()
        {
            var ideas = await _context.Ideas.ToListAsync();
            var dtoList = ideas.Adapt<List<IdeaDto>>();
            return Ok(dtoList);
        }
        // GET: api/Ideas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IdeaDto>> GetIdea(int id)
    {
        var idea = await _context.Ideas.FindAsync(id);
        if (idea == null)
            return NotFound();
        var dto = idea.Adapt<IdeaDto>();
        return Ok(dto);
    }


        // PUT: api/Ideas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIdea(int id, UpdateIdeaDto updateDto)
        {
            // 1. Veritabanından güncellenecek Idea entity'sini getir
            var existingIdea = await _context.Ideas.FindAsync(id);

            // 2. Eğer böyle bir kayıt yoksa 404 Not Found dön
            if (existingIdea == null)
                return NotFound();

            // 3. Mapster ile DTO'daki alanları entity'ye kopyala
            updateDto.Adapt(existingIdea);

                await _context.SaveChangesAsync(); // 5. Değişiklikleri veritabanına kaydet
          
                return NoContent();
            
        }

        // POST: api/Ideas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<IdeaDto>> PostIdea(CreateIdeaDto createDto)
        {
            // 1. Mapster ile CreateİdeaDto'dan Idea entity'si oluştur
            var idea = createDto.Adapt<Idea>();

            // 2. Veritabanına yeni Idea entity'sini ekle
            _context.Ideas.Add(idea);

            // 3. Değişiklikleri veritabanına kaydet
            await _context.SaveChangesAsync();

            // 4. Kaydedilen entity'yi İdeaDto'ya dönüştür ve client'a dön
            var dto = idea.Adapt<IdeaDto>();

            // 5. 201 Created ile birlikte kaydın detaylarını döndür
            return CreatedAtAction("GetIdea", new { id = idea.IdeaId }, dto);
        }

        // DELETE: api/Ideas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIdea(int id)
        {
            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null)
            {
                return NotFound();
            }

            _context.Ideas.Remove(idea);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IdeaExists(int id)
        {
            return _context.Ideas.Any(e => e.IdeaId == id);
        }
    }
}
