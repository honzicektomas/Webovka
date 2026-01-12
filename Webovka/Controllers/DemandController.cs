using Microsoft.AspNetCore.Mvc;
using Webovka.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace Webovka.Controllers
{
    public class DemandController : BaseController
    {
        private MyContext _context = new MyContext();

        // ZOBRAZENÍ FORMULÁŘE (Beze změny)
        public IActionResult Index()
        {
            int? orderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (orderId == null) return RedirectToAction("Index", "Cart");

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefault(o => o.Id == orderId && o.State == "New");

            if (order == null || !order.OrderItems.Any()) return RedirectToAction("Index", "Cart");

            // Předvyplnění (pokud je user login)
            if (ViewBag.IsAuthenticated == true)
            {
                // ... (tvůj kód pro předvyplnění)
            }

            return View(order);
        }

        // DOKONČENÍ - TADY BOLA CHYBA
        [HttpPost]
        public IActionResult Complete(Order formData)
        {
            int? orderId = HttpContext.Session.GetInt32("CurrentOrderId");

            // 1. Načteme skutečný košík z DB
            var dbOrder = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefault(o => o.Id == orderId);

            if (dbOrder == null || !dbOrder.OrderItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // 2. VALIDACE: Zkontrolujeme, jestli jsou vyplněna pole
            // Pokud je něco prázdné, vrátíme uživatele zpět
            if (string.IsNullOrWhiteSpace(formData.CustomerName) ||
                string.IsNullOrWhiteSpace(formData.CustomerEmail) ||
                string.IsNullOrWhiteSpace(formData.CustomerAddress) ||
                string.IsNullOrWhiteSpace(formData.CustomerPhone))
            {
                ViewBag.Error = "Prosím vyplňte všechna pole.";

                // Musíme znovu nastavit data do dbOrder, aby se vypsaly ve formuláři
                dbOrder.CustomerName = formData.CustomerName;
                dbOrder.CustomerEmail = formData.CustomerEmail;
                dbOrder.CustomerAddress = formData.CustomerAddress;
                dbOrder.CustomerPhone = formData.CustomerPhone;

                // Vrátíme view Index (formulář) s chybou
                return View("Index", dbOrder);
            }

            // 3. VŠE OK -> ULOŽÍME
            dbOrder.CustomerName = formData.CustomerName;
            dbOrder.CustomerEmail = formData.CustomerEmail;
            dbOrder.CustomerPhone = formData.CustomerPhone;
            dbOrder.CustomerAddress = formData.CustomerAddress;

            dbOrder.State = "Ordered"; // Změna stavu
            dbOrder.OrderDate = DateTime.Now;

            // Odečtení skladu
            foreach (var item in dbOrder.OrderItems)
            {
                if (item.ProductVariant != null)
                {
                    item.ProductVariant.StockQuantity -= item.Quantity;
                    if (item.ProductVariant.StockQuantity < 0) item.ProductVariant.StockQuantity = 0;
                }
            }

            _context.SaveChanges();

            // Smazat session (košík je hotový)
            HttpContext.Session.Remove("CurrentOrderId");

            // 4. PŘESMĚROVÁNÍ NA SUCCESS
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}