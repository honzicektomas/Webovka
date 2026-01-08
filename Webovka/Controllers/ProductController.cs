using Microsoft.AspNetCore.Mvc;

namespace Webovka.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
