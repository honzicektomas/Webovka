using Microsoft.AspNetCore.Mvc;
using Webovka.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;

namespace Webovka.Controllers
{
    [Secured] // Hlídá přihlášení
    public class AdminController : BaseController
    {
        // Instance contextu nahoře
        private MyContext _context = new MyContext();

        // Pomocná metoda pro kontrolu role
        private bool isAdmin()
        {
            return this.HttpContext.Session.GetString("UserRole") == "Admin";
        }

        public IActionResult Index()
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

            // Logic pro nahrání obrázku
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

            return RedirectToAction("Index");
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
            return RedirectToAction("Index");
        }
    }
}