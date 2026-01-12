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

        public IActionResult Index()
        {
            int? orderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (orderId == null) return RedirectToAction("Index", "Cart");

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefault(o => o.Id == orderId && o.State == "New");

            if (order == null || order.OrderItems == null || !order.OrderItems.Any())
                return RedirectToAction("Index", "Cart");

            string userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(order.CustomerName)) order.CustomerName = user.FirstName + " " + user.LastName;
                    if (string.IsNullOrEmpty(order.CustomerEmail)) order.CustomerEmail = user.Email;
                    if (string.IsNullOrEmpty(order.CustomerPhone)) order.CustomerPhone = user.PhoneNumber;
                    if (string.IsNullOrEmpty(order.CustomerAddress)) order.CustomerAddress = user.Address;
                }
            }

            return View(order);
        }

        [HttpPost]
        public IActionResult Complete(Order formData)
        {
            int? orderId = HttpContext.Session.GetInt32("CurrentOrderId");

            var dbOrder = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefault(o => o.Id == orderId);

            if (dbOrder == null || dbOrder.OrderItems == null || !dbOrder.OrderItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (string.IsNullOrWhiteSpace(formData.CustomerName) ||
                string.IsNullOrWhiteSpace(formData.CustomerEmail) ||
                string.IsNullOrWhiteSpace(formData.CustomerAddress) ||
                string.IsNullOrWhiteSpace(formData.CustomerPhone))
            {
                ViewBag.Error = "Prosím vyplňte všechny údaje.";

                dbOrder.CustomerName = formData.CustomerName;
                dbOrder.CustomerEmail = formData.CustomerEmail;
                dbOrder.CustomerAddress = formData.CustomerAddress;
                dbOrder.CustomerPhone = formData.CustomerPhone;

                return View("Index", dbOrder);
            }

            dbOrder.CustomerName = formData.CustomerName;
            dbOrder.CustomerEmail = formData.CustomerEmail;
            dbOrder.CustomerPhone = formData.CustomerPhone;
            dbOrder.CustomerAddress = formData.CustomerAddress;

            dbOrder.State = "Ordered";
            dbOrder.OrderDate = DateTime.Now;

            foreach (var item in dbOrder.OrderItems)
            {
                if (item.ProductVariant != null)
                {
                    item.ProductVariant.StockQuantity -= item.Quantity;
                    if (item.ProductVariant.StockQuantity < 0) item.ProductVariant.StockQuantity = 0;
                }
            }

            _context.SaveChanges();
            HttpContext.Session.Remove("CurrentOrderId");

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}