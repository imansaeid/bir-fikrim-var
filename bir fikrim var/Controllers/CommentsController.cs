using bir_fikrim_var.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;

namespace bir_fikrim_var.Controllers
{
    public class CommentsController : Controller
    {
        private readonly HttpClient _httpClient;

        public CommentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var comments = await _httpClient.GetFromJsonAsync<List<CommentDto>>("api/Comments");
            return View(comments);
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var comment = await _httpClient.GetFromJsonAsync<CommentDto>($"api/Comments/{id}");
            if (comment == null) return NotFound();
            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCommentDto dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/Comments", dto);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _httpClient.GetFromJsonAsync<CommentDto>($"api/Comments/{id}");
            if (comment == null) return NotFound();

            // Fill DTO for editing
            var dto = new CreateCommentDto
            {
                Content = comment.Content,
                UserId = comment.UserId,
                IdeaId = comment.IdeaId
            };

            return View(dto);
        }

        // POST: Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCommentDto dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Comments/{id}", dto);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _httpClient.GetFromJsonAsync<CommentDto>($"api/Comments/{id}");
            if (comment == null) return NotFound();
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Comments/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            return Problem("Could not delete comment.");
        }
    }
}
