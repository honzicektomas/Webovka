using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Webovka.Models;
using System.Linq;

namespace Webovka.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            // Zde později načteme data z DB
            using (var db = new MyContext())
            {
                // Zatím pošleme prázdný seznam, aby stránka nepadala
                return View(new List<OrderItem>());
            }
        }
    }
}