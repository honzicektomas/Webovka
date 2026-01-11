using Microsoft.AspNetCore.Mvc;

namespace Webovka.Controllers
{
    public class ProductController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
