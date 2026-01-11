using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webovka.Models;
using System.Linq;

namespace Webovka.Controllers
{
    public class HomeController : BaseController
    {
        private MyContext context;

        public HomeController()
        {
            context = new MyContext();
        }

        public IActionResult Index()
        {
            var produkty = context.Products
                                   .Include(p => p.Category)
                                   .ToList();

            return View(produkty);
        }
    }
}