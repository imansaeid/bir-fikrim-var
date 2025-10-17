using bir_fikrim_var.Models;
using Microsoft.AspNetCore.Mvc;

namespace BirFikrimVar.Controllers
{
    public class LikesController : Controller
    {
        private readonly HttpClient _http;

        public LikesController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Index()
        {
            var likes = await _http.GetFromJsonAsync<List<LikeResponseDto>>("api/LikesApi");
            return View(likes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLikeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var response = await _http.PostAsJsonAsync("api/LikesApi", dto);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to create like.");
            return View(dto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _http.DeleteAsync($"api/LikesApi/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return Problem("Failed to delete like.");
        }
    }
}
