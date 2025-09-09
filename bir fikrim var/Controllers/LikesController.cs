using bir_fikrim_var.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;

namespace bir_fikrim_var.Controllers
{
    public class LikesController : Controller
    {
        private readonly HttpClient _httpClient;

        public LikesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // GET: Likes
        public async Task<IActionResult> Index()
        {
            var likes = await _httpClient.GetFromJsonAsync<List<LikeDto>>("api/Likes");
            return View(likes);
        }

        // GET: Likes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var like = await _httpClient.GetFromJsonAsync<LikeDto>($"api/Likes/{id}");
            if (like == null) return NotFound();
            return View(like);
        }

        // GET: Likes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Likes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLikeDto dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/Likes", dto);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: Likes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var like = await _httpClient.GetFromJsonAsync<LikeDto>($"api/Likes/{id}");
            if (like == null) return NotFound();
            return View(like);
        }

        // POST: Likes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Likes/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            return Problem("Could not delete like.");
        }
    }
}
