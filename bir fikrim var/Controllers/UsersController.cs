using Microsoft.AspNetCore.Mvc;
using bir_fikrim_var.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace bir_fikrim_var.Controllers
{
    public class UsersController : Controller
    {
        private readonly HttpClient _httpClient;

        public UsersController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/UsersApi");
            return View(users);
        }
      
        // GET: Users/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _httpClient.GetFromJsonAsync<UserDto>($"api/Users/{id}");
            if (user == null) return NotFound();
            return View(user);
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/Users/register", dto);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserDto dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/Users/login", dto);
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserDto>();
                    // optionally: set session/cookie here
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Invalid email or password.");
            }
            return View(dto);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _httpClient.GetFromJsonAsync<UserDto>($"api/Users/{id}");
            if (user == null) return NotFound();

            var dto = new UpdateUserDTO
            {
                FullName = user.FullName,
                Email = user.Email,
                Password = "" // leave blank for security
            };

            return View(dto);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateUserDTO dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Users/{id}", dto);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _httpClient.GetFromJsonAsync<UserDto>($"api/Users/{id}");
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Users/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            return Problem("Could not delete user.");
        }
    }
}
