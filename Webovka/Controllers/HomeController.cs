using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webovka.Models;
using System.Linq;

namespace Webovka.Controllers
{
    public class HomeController : Controller
    {
        private MyContext context;

        // Dependency Injection - systém nám sem pošle pøipojenou databázi
        public HomeController()
        {
            context = new MyContext();
        }

        public IActionResult Index()
        {
            // Naèteme všechny produkty z databáze
            // .Include(p => p.Category) - abychom mohli vypsat i název kategorie, kdybychom chtìli
            var produkty = context.Products
                                   .Include(p => p.Category)
                                   .ToList();

            return View(produkty);
        }
    }
}