using Microsoft.AspNetCore.Mvc;

namespace BirFikrimVar.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
