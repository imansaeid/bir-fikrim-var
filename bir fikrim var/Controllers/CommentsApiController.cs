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
    public class CommentsApiController : ControllerBase
    {
        private readonly MYDBCONTXT _context;

        public CommentsApiController(MYDBCONTXT context)
        {
            _context = context;
        }

        [HttpGet("idea/{ideaId}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByIdea(int ideaId)
        {
            var comments = await _context.Comments
                .Where(c => c.IdeaId == ideaId)
                .Include(c => c.User)
                .Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    IdeaId = c.IdeaId,
                    UserId = c.UserId,
                    authorName = c.User.FullName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpGet("count/{ideaId}")]
        public async Task<ActionResult<int>> GetCommentCount(int ideaId)
        {
            return await _context.Comments.CountAsync(comment => comment.IdeaId == ideaId);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment.Adapt<CommentDto>());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, UpdateCommentDTO dto)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            if (comment.UserId != dto.UserId)
            {
                return Forbid();
            }

            comment.Content = dto.Content;
            comment.CreatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> PostComment(CreateCommentDto dto)
        {
            var comment = dto.Adapt<Comment>();
            comment.CreatedDate = DateTime.Now;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = comment.CommentId }, comment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id, [FromQuery] int userId)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            if (comment.UserId != userId)
            {
                return Forbid();
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
