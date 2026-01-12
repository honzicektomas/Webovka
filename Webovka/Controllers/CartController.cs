using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webovka.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Webovka.Controllers
{
    // Dědíme od BaseController, abychom měli přístup k Session informacím o userovi
    public class CartController : BaseController
    {
        private MyContext _context = new MyContext();

        // Pomocná metoda: Získá ID přihlášeného uživatele
        private int? GetCurrentUserId()
        {
            var idString = HttpContext.Session.GetString("UserId");
            if (int.TryParse(idString, out int id))
            {
                return id;
            }
            return null;
        }

        // Pomocná metoda: Získá nebo vytvoří "Košíkovou" objednávku pro uživatele
        private Order GetOrCreateCartOrder(int userId)
        {
            // Hledáme objednávku tohoto uživatele, která ještě není odeslaná (Stav = "New")
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefault(o => o.UserId == userId && o.State == "New");

            if (order == null)
            {
                // Pokud košík neexistuje, vytvoříme ho
                order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    State = "New", // "New" znamená, že je to zatím jen košík
                    TotalPrice = 0,
                    // Vyplníme povinná pole (zatím placeholderem, při objednávce se přepíšou)
                    CustomerName = "",
                    CustomerEmail = "",
                    CustomerPhone = "",
                    CustomerAddress = ""
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
            }

            return order;
        }

        // ZOBRAZIT KOŠÍK
        public IActionResult Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                // Pokud není přihlášen, pošleme ho na přihlášení s vzkazem
                return RedirectToAction("Index", "User");
            }

            var order = GetOrCreateCartOrder(userId.Value);

            // Pošleme položky objednávky do View
            return View(order.OrderItems.ToList());
        }

        // PŘIDAT DO KOŠÍKU
        [HttpPost]
        public IActionResult AddToCart(int variantId, int quantity)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                // Nepřihlášený uživatel nemůže nakupovat (podle nového zadání)
                return RedirectToAction("Index", "User");
            }

            var order = GetOrCreateCartOrder(userId.Value);

            // Zkontrolujeme, jestli už položka v košíku není
            var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ProductVariantId == variantId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                // Najdeme cenu varianty
                var variant = _context.ProductVariants.Include(v => v.Product).FirstOrDefault(v => v.Id == variantId);
                if (variant == null) return NotFound();

                var newItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductVariantId = variantId,
                    Quantity = quantity,
                    UnitPrice = variant.Product.Price
                };
                _context.OrderItems.Add(newItem);
            }

            // Přepočítat celkovou cenu (jednoduchý součet)
            // Uložíme změny, abychom mohli spočítat sumu z DB nebo paměti
            _context.SaveChanges();

            UpdateOrderTotal(order.Id);

            return RedirectToAction("Index");
        }

        // ODEBRAT Z KOŠÍKU
        public IActionResult Remove(int itemId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Index", "User");

            var item = _context.OrderItems.Find(itemId);
            if (item != null)
            {
                // Bezpečnostní kontrola: patří položka aktuálnímu uživateli?
                var order = _context.Orders.Find(item.OrderId);
                if (order != null && order.UserId == userId)
                {
                    _context.OrderItems.Remove(item);
                    _context.SaveChanges();
                    UpdateOrderTotal(order.Id);
                }
            }
            return RedirectToAction("Index");
        }

        // Pomocná metoda pro aktualizaci celkové ceny objednávky
        private void UpdateOrderTotal(int orderId)
        {
            var order = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.TotalPrice = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
                _context.SaveChanges();
            }
        }
    }
}