using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webovka.Models;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Webovka.Controllers
{
    public class ProductController : BaseController
    {
        private MyContext _context = new MyContext();

        public IActionResult Index(int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
                ViewBag.CategoryName = _context.Categories.Find(categoryId.Value)?.Name;
            }

            return View(query.ToList());
        }

        public IActionResult Detail(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Images) // <--- PŘIDAT INCLUDE PRO OBRÁZKY
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            // --- OPRAVA ZDE: Přidáno .ToList() před OrderBy(Guid) ---
            var randomProducts = _context.Products
                .Where(p => p.Id != id)
                .ToList() // <--- TOTO ZDE CHYBĚLO
                .OrderBy(r => Guid.NewGuid())
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = randomProducts;

            return View(product);
        }
    }
}