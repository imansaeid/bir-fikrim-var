using bir_fikrim_var.Models;
using Microsoft.AspNetCore.Mvc;

namespace BirFikrimVar.Controllers
{
    public class UsersController : Controller
    {
        private readonly HttpClient _http;

        public UsersController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Profile(int? id)
        {
            var loggedInUserId = HttpContext.Session.GetInt32("UserId");

            if (!id.HasValue)
            {
                if (loggedInUserId == null)
                {
                    return RedirectToAction("Login");
                }
                id = loggedInUserId;
            }

            var user = await _http.GetFromJsonAsync<UserDto>($"api/UsersApi/{id}");
            if (user == null)
            {
                return NotFound();
            }

            var ideas = await _http.GetFromJsonAsync<List<IdeaDto>>($"api/IdeasApi/user/{id}") ?? new List<IdeaDto>();

            var model = new UserProfileDto
            {
                User = user,
                Ideas = ideas,
                IsOwnProfile = (loggedInUserId.HasValue && loggedInUserId.Value == id.Value)
            };

            return View(model);
        }

        public async Task<IActionResult> Index()
        {
            var users = await _http.GetFromJsonAsync<List<UserDto>>("api/UsersApi");
            return View(users);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _http.PostAsJsonAsync("api/UsersApi/register", dto);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Registration failed. Please try again.");
            }
            return View(dto);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserDto dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _http.PostAsJsonAsync("api/UsersApi/login", dto);
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserDto>();
                    if (user != null)
                    {
                        HttpContext.Session.SetInt32("UserId", user.UserId);
                        HttpContext.Session.SetString("FullName", user.FullName);
                        HttpContext.Session.SetString("Email", user.Email);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError("", "Invalid email or password.");
            return View(dto);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _http.GetFromJsonAsync<UserDto>($"api/UsersApi/{id}");
            if (user == null) return NotFound();

            var dto = new UpdateUserDTO
            {
                FullName = user.FullName,
                Email = user.Email,
                Password = ""
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateUserDTO dto)
        {
            if (ModelState.IsValid)
            {
                var response = await _http.PutAsJsonAsync($"api/UsersApi/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(dto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _http.GetFromJsonAsync<UserDto>($"api/UsersApi/{id}");
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _http.DeleteAsync($"api/UsersApi/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return Problem("Could not delete user.");
        }
    }
}
