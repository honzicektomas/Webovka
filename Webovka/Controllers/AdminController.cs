using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webovka.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Webovka.Controllers
{
    public class AdminController : BaseController
    {
        private MyContext _context = new MyContext();

        private bool isAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        private string UploadFile(IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/images/uploads/" + fileName;
        }

        public IActionResult Index()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            return View();
        }

        public IActionResult Products()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            var products = _context.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        [HttpPost]
        public IActionResult Create(Product product, IFormFile mainImage, List<IFormFile> galleryImages,
                                    string defaultColor, string defaultColorHex, string defaultSize, int defaultStock)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            if (mainImage != null && mainImage.Length > 0)
            {
                product.MainImageUrl = UploadFile(mainImage);
            }
            else
            {
                product.MainImageUrl = "/images/materialy/img/kosik.png";
            }

            if (galleryImages != null && galleryImages.Any())
            {
                product.Images = new List<ProductImage>();
                foreach (var file in galleryImages)
                {
                    if (file.Length > 0)
                    {
                        product.Images.Add(new ProductImage { ImageUrl = UploadFile(file) });
                    }
                }
            }

            _context.Products.Add(product);
            _context.SaveChanges();

            if (!string.IsNullOrEmpty(defaultColor) && !string.IsNullOrEmpty(defaultSize))
            {
                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    Color = defaultColor,
                    ColorHex = defaultColorHex ?? "#000000",
                    Size = defaultSize,
                    StockQuantity = defaultStock
                };
                _context.ProductVariants.Add(variant);
                _context.SaveChanges();
            }

            return RedirectToAction("Products");
        }

        public IActionResult Edit(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            var product = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        public IActionResult EditProductInfo(Product product, IFormFile mainImage)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            var existing = _context.Products.Find(product.Id);
            if (existing == null) return NotFound();

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.OldPrice = product.OldPrice;
            existing.CategoryId = product.CategoryId;

            if (mainImage != null && mainImage.Length > 0)
            {
                existing.MainImageUrl = UploadFile(mainImage);
            }

            _context.SaveChanges();
            return RedirectToAction("Edit", new { id = product.Id });
        }

        [HttpPost]
        public IActionResult AddVariant(int productId, string color, string colorHex, string size, int stock)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            var variant = new ProductVariant
            {
                ProductId = productId,
                Color = color,
                ColorHex = colorHex,
                Size = size,
                StockQuantity = stock
            };
            _context.ProductVariants.Add(variant);
            _context.SaveChanges();

            return RedirectToAction("Edit", new { id = productId });
        }

        [HttpPost]
        public IActionResult UpdateStock(int variantId, int newStock)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            var v = _context.ProductVariants.Find(variantId);
            if (v != null)
            {
                v.StockQuantity = newStock;
                _context.SaveChanges();
                return RedirectToAction("Edit", new { id = v.ProductId });
            }
            return RedirectToAction("Products");
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
                return RedirectToAction("Edit", new { id = pid });
            }
            return RedirectToAction("Products");
        }

        [HttpPost]
        public IActionResult UploadGallery(int productId, List<IFormFile> galleryImages)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");

            if (galleryImages != null && galleryImages.Any())
            {
                foreach (var file in galleryImages)
                {
                    if (file.Length > 0)
                    {
                        var url = UploadFile(file);
                        _context.ProductImages.Add(new ProductImage { ProductId = productId, ImageUrl = url });
                    }
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = productId });
        }

        public IActionResult DeleteImage(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            var img = _context.ProductImages.Find(id);
            if (img != null)
            {
                int pid = img.ProductId;
                _context.ProductImages.Remove(img);
                _context.SaveChanges();
                return RedirectToAction("Edit", new { id = pid });
            }
            return RedirectToAction("Products");
        }

        public IActionResult Delete(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Products");
        }

        public IActionResult Categories()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            return View(_context.Categories.Include(c => c.ParentCategory).ToList());
        }

        public IActionResult CreateCategory()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category cat)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            _context.Categories.Add(cat);
            _context.SaveChanges();
            return RedirectToAction("Categories");
        }

        public IActionResult DeleteCategory(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            var cat = _context.Categories.Find(id);
            if (cat != null) { _context.Categories.Remove(cat); _context.SaveChanges(); }
            return RedirectToAction("Categories");
        }

        public IActionResult Orders()
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            var orders = _context.Orders.Where(o => o.State == "Ordered").OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        public IActionResult OrderDetail(int id)
        {
            if (!isAdmin()) return RedirectToAction("Index", "Home");
            var order = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.ProductVariant).ThenInclude(v => v.Product).FirstOrDefault(o => o.Id == id);
            return View(order);
        }
    }
}