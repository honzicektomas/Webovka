using Microsoft.AspNetCore.Mvc;
using Webovka.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Webovka.Controllers
{
    [Secured]
    public class AdminController : BaseController
    {
        private MyContext _context = new MyContext();

        private bool isAdmin()
        {
            return this.HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // ==========================================
        // 1. DASHBOARD (Rozcestník)
        // ==========================================
        public IActionResult Index()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            return View();
        }

        // ==========================================
        // 2. SPRÁVA PRODUKTŮ
        // ==========================================
        public IActionResult Products()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            var produkty = _context.Products.Include(p => p.Category).ToList();
            return View(produkty);
        }

        public IActionResult Create()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product model, IFormFile imageFile)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/uploads", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }
                model.MainImageUrl = "/images/uploads/" + fileName;
            }
            else
            {
                model.MainImageUrl = "/images/materialy/img/kosik.png";
            }

            _context.Products.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Products");
        }

        public IActionResult Delete(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            var p = _context.Products.Find(id);
            if (p != null)
            {
                _context.Products.Remove(p);
                _context.SaveChanges();
            }
            return RedirectToAction("Products");
        }

        // ==========================================
        // 3. SPRÁVA VARIANT (Barvy / Velikosti)
        // ==========================================
        public IActionResult ProductVariants(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            var product = _context.Products
                .Include(p => p.Variants)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        public IActionResult AddVariant(int productId, string color, string colorHex, string size, int stock)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            var variant = new ProductVariant
            {
                ProductId = productId,
                Color = color,       // Název (např. Červená)
                ColorHex = colorHex, // Kód (např. #ff0000) - z Color Pickeru
                Size = size,
                StockQuantity = stock
            };

            _context.ProductVariants.Add(variant);
            _context.SaveChanges();

            return RedirectToAction("ProductVariants", new { id = productId });
        }

        public IActionResult DeleteVariant(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            var v = _context.ProductVariants.Find(id);
            if (v != null)
            {
                int pid = v.ProductId;
                _context.ProductVariants.Remove(v);
                _context.SaveChanges();
                return RedirectToAction("ProductVariants", new { id = pid });
            }
            return RedirectToAction("Products");
        }
        public IActionResult Categories()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            // Načteme kategorie i s rodičem, abychom viděli strukturu
            var cats = _context.Categories.Include(c => c.ParentCategory).ToList();
            return View(cats);
        }

        public IActionResult CreateCategory()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            ViewBag.AllCategories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category model)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            _context.Categories.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Categories");
        }

        public IActionResult DeleteCategory(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            var c = _context.Categories.Find(id);
            if (c != null)
            {
                _context.Categories.Remove(c);
                _context.SaveChanges();
            }
            return RedirectToAction("Categories");
        }
        public IActionResult Orders()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        public IActionResult OrderDetail(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}