using Microsoft.AspNetCore.Mvc;

namespace Webovka.Controllers
{
    public class DemandController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
