using bir_fikrim_var.Models;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BirFikrimVar.Controllers
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

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<IdeaDto>>> GetIdeasByUser(int userId)
        {
            var ideas = await _context.Ideas
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedDate)
                .Select(i => new IdeaDto
                {
                    IdeaId = i.IdeaId,
                    Title = i.Title,
                    Content = i.Content,
                    CreatedDate = i.CreatedDate,
                    UserId = i.UserId,
                    LikeCount = i.Likes.Count,
                    CommentCount = i.Comments.Count,
                    AuthorName = i.User.FullName
                })
                .ToListAsync();

            return ideas;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IdeaDto>>> GetIdeas()
        {
            var ideas = await _context.Ideas
                .Include(i => i.User)
                .Select(i => new IdeaDto
                {
                    IdeaId = i.IdeaId,
                    Title = i.Title,
                    Content = i.Content,
                    AuthorName = i.User.FullName,
                    CreatedDate = i.CreatedDate,
                    LikeCount = i.Likes.Count(),
                    CommentCount = i.Comments.Count()
                })
                .ToListAsync();

            return Ok(ideas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdeaDto>> GetIdea(int id)
        {
            var idea = await _context.Ideas
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.IdeaId == id);

            if (idea == null)
            {
                return NotFound();
            }

            var ideaDto = new IdeaDto
            {
                IdeaId = idea.IdeaId,
                Title = idea.Title,
                Content = idea.Content,
                CreatedDate = idea.CreatedDate,
                UserId = idea.UserId,
                AuthorName = idea.User?.FullName
            };

            return ideaDto;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutIdea(int id, UpdateIdeaDto dto)
        {
            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null || id != idea.IdeaId)
            {
                return BadRequest();
            }

            dto.Adapt(idea);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IdeaExists(id))
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

        [HttpPost]
        public async Task<ActionResult<Idea>> PostIdea(CreateIdeaDto dto)
        {
            var idea = dto.Adapt<Idea>();
            idea.CreatedDate = DateTime.Now;

            _context.Ideas.Add(idea);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIdea", new { id = idea.IdeaId }, idea);
        }

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
