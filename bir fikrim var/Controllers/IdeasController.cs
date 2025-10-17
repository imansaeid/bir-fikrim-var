using bir_fikrim_var.Models;
using Microsoft.AspNetCore.Mvc;

namespace BirFikrimVar.Controllers
{
    public class IdeasController : Controller
    {
        private readonly HttpClient _http;

        public IdeasController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        public async Task<IActionResult> Index(string sortBy = "newest")
        {
            var ideas = await _http.GetFromJsonAsync<List<IdeaDto>>("api/IdeasApi");

            ideas = sortBy switch
            {
                "likes" => ideas.OrderByDescending(i => i.LikeCount).ToList(),
                "comments" => ideas.OrderByDescending(i => i.CommentCount).ToList(),
                _ => ideas.OrderByDescending(i => i.CreatedDate).ToList(),
            };

            ViewBag.SortBy = sortBy;
            return View(ideas);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var idea = await _http.GetFromJsonAsync<IdeaDto>($"api/IdeasApi/{id}");
                if (idea == null) return NotFound();

                var comments = await _http.GetFromJsonAsync<List<CommentDto>>($"api/CommentsApi/idea/{id}");
                var likeCount = await _http.GetFromJsonAsync<int>($"api/LikesApi/count/{id}");
                bool userLiked = false;
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    userLiked = await _http.GetFromJsonAsync<bool>($"api/LikesApi/check/{id}/{userId}");
                }

                var viewModel = new IdeasDetailsViewModel
                {
                    Idea = idea,
                    Comments = comments ?? new List<CommentDto>(),
                    LikeCount = likeCount,
                    UserLiked = userLiked
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Failed to load idea details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int ideaId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["Error"] = "You must be logged in to like an idea.";
                return RedirectToAction("Login", "Users");
            }

            var createLikeDto = new CreateLikeDto { IdeaId = ideaId, UserId = userId.Value };
            var response = await _http.PostAsJsonAsync("api/LikesApi", createLikeDto);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                await _http.DeleteAsync($"api/LikesApi/{ideaId}/{userId.Value}");
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateIdeaDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Users");

            dto.UserId = userId.Value;
            var response = await _http.PostAsJsonAsync("api/IdeasApi", dto);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View(dto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var idea = await _http.GetFromJsonAsync<IdeaDto>($"api/IdeasApi/{id}");
            if (idea == null) return NotFound();

            var dto = new UpdateIdeaDto { Title = idea.Title, Content = idea.Content };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateIdeaDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var response = await _http.PutAsJsonAsync($"api/IdeasApi/{id}", dto);
            if (response.IsSuccessStatusCode)
            {
                var userId = GetCurrentUserId();
                return RedirectToAction("Profile", "Users", new { id = userId });
            }

            ModelState.AddModelError(string.Empty, "Unable to save changes.");
            return View(dto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var idea = await _http.GetFromJsonAsync<IdeaDto>($"api/IdeasApi/{id}");
            if (idea == null) return NotFound();
            return View(idea);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _http.DeleteAsync($"api/IdeasApi/{id}");
            if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
            return Problem("Failed to delete idea.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ideaId, string content)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return RedirectToAction("Login", "Users");
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { id = ideaId });
            }

            var dto = new CreateCommentDto { IdeaId = ideaId, UserId = userId.Value, Content = content };
            await _http.PostAsJsonAsync("api/CommentsApi", dto);
            return RedirectToAction("Details", new { id = ideaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, int ideaId, string content)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Users");

            var response = await _http.PutAsJsonAsync($"api/CommentsApi/{commentId}", new { Content = content, UserId = userId.Value });
            if (!response.IsSuccessStatusCode) TempData["Error"] = "Failed to update comment.";

            return RedirectToAction("Details", new { id = ideaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int ideaId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Users");

            var response = await _http.DeleteAsync($"api/CommentsApi/{commentId}?userId={userId.Value}");
            if (!response.IsSuccessStatusCode) TempData["Error"] = "Failed to delete comment.";

            return RedirectToAction("Details", new { id = ideaId });
        }
    }
}
