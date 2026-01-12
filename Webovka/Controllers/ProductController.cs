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

        // Seznam produktů (ten už asi máte, necháme ho beze změn)
        // SEZNAM PRODUKTŮ (KATALOG)
        public IActionResult Index(int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // Pokud je zadáno categoryId, filtrujeme
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = query.ToList();

            // Načteme název kategorie pro nadpis
            if (categoryId.HasValue)
            {
                ViewBag.CategoryName = _context.Categories.Find(categoryId.Value)?.Name;
            }

            return View(products);
        }

        // DETAIL PRODUKTU
        public IActionResult Detail(int id)
        {
            // 1. Načteme produkt, jeho kategorii a varianty
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // 2. Náhodné produkty "Mohlo by se vám líbit"
            // OPRAVA: Přidáno .ToList() před OrderBy
            var randomProducts = _context.Products
                .Where(p => p.Id != id)
                .ToList() // <--- TÍMTO STÁHNEME DATA DO PAMĚTI (vyřeší to error)
                .OrderBy(r => Guid.NewGuid()) // Teď už řadí C#, ne databáze, a to funguje
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = randomProducts;

            return View(product);
        }
    }
}