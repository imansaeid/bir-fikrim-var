using bir_fikrim_var.Models;
using Microsoft.AspNetCore.Mvc;

namespace BirFikrimVar.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _http;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index()
        {
            var ideas = await _http.GetFromJsonAsync<List<IdeaDto>>("api/IdeasApi");

            // Safety check
            if (ideas == null)
                ideas = new List<IdeaDto>();

            return View(ideas);
        }
    }
}
