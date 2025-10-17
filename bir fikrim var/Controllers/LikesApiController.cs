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
    public class LikesApiController : ControllerBase
    {
        private readonly MYDBCONTXT _context;

        public LikesApiController(MYDBCONTXT context)
        {
            _context = context;
        }

        [HttpGet("count/{ideaId}")]
        public async Task<ActionResult<int>> GetLikeCount(int ideaId)
        {
            return await _context.Likes.CountAsync(like => like.IdeaId == ideaId);
        }

        [HttpGet("check/{ideaId}/{userId}")]
        public async Task<ActionResult<bool>> CheckUserLiked(int ideaId, int userId)
        {
            return await _context.Likes.AnyAsync(like => like.IdeaId == ideaId && like.UserId == userId);
        }

        [HttpPost]
        public async Task<ActionResult<LikeResponseDto>> PostLike(CreateLikeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.IdeaId == dto.IdeaId && l.UserId == dto.UserId);

            if (existingLike != null)
            {
                return Conflict("User has already liked this idea.");
            }

            var like = dto.Adapt<Like>();
            like.CreatedDate = DateTime.Now;

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            var createdLike = await _context.Likes
                .Include(l => l.User)
                .FirstAsync(l => l.LikeId == like.LikeId);

            var result = createdLike.Adapt<LikeResponseDto>();

            return CreatedAtAction(nameof(GetLikes), new { id = like.LikeId }, result);
        }

        [HttpDelete("{ideaId}/{userId}")]
        public async Task<IActionResult> UnlikeIdea(int ideaId, int userId)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.IdeaId == ideaId && l.UserId == userId);

            if (like == null)
            {
                return NotFound("Like not found.");
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeResponseDto>>> GetLikes()
        {
            var res = await _context.Likes
                .Include(like => like.User)
                .Select(like => like.Adapt<LikeResponseDto>())
                .ToListAsync();

            return res;
        }
    }
}

