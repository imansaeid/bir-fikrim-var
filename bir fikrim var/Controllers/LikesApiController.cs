using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mapster;
using bir_fikrim_var.Models;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeResponseDto>>> GetLikes()
        {
            var res = await _context.Likes
                .Include(like => like.User)
                .Select(like => new LikeResponseDto
                {
                    LikeId = like.LikeId,
                    IdeaId = like.IdeaId,
                    UserId = like.UserId,
                    FullName = like.User.FullName,
                    CreatedDate = like.CreatedDate ?? DateTime.UtcNow
                })
                .ToListAsync();
            return res;
        }

        [HttpGet("count/{ideaId}")]
        public async Task<ActionResult<int>> GetLikeCount(int ideaId)
        {
            return await _context.Likes.CountAsync(like => like.IdeaId == ideaId);
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

            var ideaExists = await _context.Ideas.AnyAsync(i => i.IdeaId == dto.IdeaId);
            if (!ideaExists)
            {
                return BadRequest("Idea does not exist.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.UserId);
            if (!userExists)
            {
                return BadRequest("User does not exist.");
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
        public async Task<IActionResult> DeleteLike(int ideaId, int userId)
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

        [HttpGet("check/{ideaId}/{userId}")]
        public async Task<ActionResult<bool>> CheckUserLiked(int ideaId, int userId)
        {
            return await _context.Likes.AnyAsync(like => like.IdeaId == ideaId && like.UserId == userId);
        }
    }
}
