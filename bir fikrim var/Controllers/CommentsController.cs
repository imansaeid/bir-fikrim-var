using bir_fikrim_var.Models;
using Microsoft.AspNetCore.Mvc;

namespace BirFikrimVar.Controllers
{
    public class CommentsController : Controller
    {
        private readonly HttpClient _http;

        public CommentsController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Index()
        {
            var comments = await _http.GetFromJsonAsync<List<CommentDto>>("api/CommentsApi");
            return View(comments);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCommentDto dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _http.PostAsJsonAsync("api/CommentsApi", dto);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            ModelState.AddModelError("", "Failed to create comment.");
            return View(dto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _http.GetFromJsonAsync<CommentDto>($"api/CommentsApi/{id}");
            if (comment == null) return NotFound();

            var dto = new UpdateCommentDTO
            {
                Content = comment.Content
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCommentDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var response = await _http.PutAsJsonAsync($"api/CommentsApi/{id}", dto);

            if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Failed to update comment.");
            return View(dto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _http.GetFromJsonAsync<CommentDto>($"api/CommentsApi/{id}");
            if (comment == null) return NotFound();
            return View(comment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _http.DeleteAsync($"api/CommentsApi/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return Problem("Failed to delete comment.");
        }
    }
}
