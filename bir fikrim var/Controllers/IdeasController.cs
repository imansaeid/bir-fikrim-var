using bir_fikrim_var.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;


namespace bir_fikrim_var.Controllers
{
    public class IdeasController : Controller
    {
        private readonly HttpClient _httpClient;

        public IdeasController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // GET: Ideas
        public async Task<IActionResult> Index()
        {
            var ideas = await _httpClient.GetFromJsonAsync<List<IdeaDto>>("api/Ideas");

            foreach (var idea in ideas)
            {
                // Get like count
                idea.LikeCount = await _httpClient.GetFromJsonAsync<int>($"api/LikesApi/count/{idea.IdeaId}");

                // Get comment count
                idea.CommentCount = await _httpClient.GetFromJsonAsync<int>($"api/CommentsApi/count/{idea.IdeaId}");
            }
            return View(ideas);
        }

        // GET: Ideas/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Load idea
            var idea = await _httpClient.GetFromJsonAsync<IdeaDto>($"api/IdeasApi/{id}");
            if (idea == null) return NotFound();

            // Load comments
            var comments = await _httpClient.GetFromJsonAsync<List<CommentDto>>($"api/CommentsApi/idea/{id}");

            // Like count
            var likeCount = await _httpClient.GetFromJsonAsync<int>($"api/LikesApi/count/{id}");

            // Did current user like?
            var userId = HttpContext.Session.GetInt32("UserId");
            bool userLiked = false;
            if (userId.HasValue)
            {
                userLiked = await _httpClient.GetFromJsonAsync<bool>($"api/LikesApi/check/{id}/{userId.Value}");
            }

            var vm = new IdeasDetailsViewModel
            {
                Idea = idea,
                Comments = comments ?? new List<CommentDto>(),
                LikeCount = likeCount,
                UserLiked = userLiked
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int ideaId, string content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Users");

            var dto = new CreateCommentDto
            {
                IdeaId = ideaId,
                UserId = userId.Value,
                Content = content
            };

            await _httpClient.PostAsJsonAsync("api/CommentsApi", dto);
            return RedirectToAction("Details", new { id = ideaId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int ideaId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Users");

            await _httpClient.PostAsJsonAsync("api/LikesApi/toggle", new { IdeaId = ideaId, UserId = userId.Value });

            return RedirectToAction("Details", new { id = ideaId });
        }

        // GET: Ideas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ideas/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateIdeaDto dto)
        {
            // Attach logged-in userId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Users");
            }

            dto.UserId = userId.Value;

            var response = await _httpClient.PostAsJsonAsync("api/IdeasApi", dto);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View(dto);
        }

        // GET: Ideas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var idea = await _httpClient.GetFromJsonAsync<IdeaDto>($"api/Ideas/{id}");
            if (idea == null) return NotFound();

            // Fill DTO for editing
            var dto = new UpdateIdeaDto
            {
                Title = idea.Title,
                Content = idea.Content
                // add other editable fields if your DTO has them
            };

            return View(dto);
        }

        // POST: Ideas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateIdeaDto dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Ideas/{id}", dto);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: Ideas/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var idea = await _httpClient.GetFromJsonAsync<IdeaDto>($"api/Ideas/{id}");
            if (idea == null) return NotFound();
            return View(idea);
        }

        // POST: Ideas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Ideas/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            return Problem("Could not delete idea.");
        }
    }
}