using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webovka.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Webovka.Controllers
{
    public class CartController : BaseController
    {
        private MyContext _context = new MyContext();

        private Order GetCurrentOrder()
        {
            int? userId = null;
            string userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int parsedId))
            {
                userId = parsedId;
            }

            int? sessionOrderId = HttpContext.Session.GetInt32("CurrentOrderId");
            Order order = null;

            if (sessionOrderId != null)
            {
                order = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                    .FirstOrDefault(o => o.Id == sessionOrderId.Value && o.State == "New");
            }

            if (order == null && userId != null)
            {
                order = _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.UserId == userId && o.State == "New");
            }

            if (order == null)
            {
                order = new Order
                {
                    UserId = userId, 
                    OrderDate = DateTime.Now,
                    State = "New",
                    TotalPrice = 0,
                    CustomerName = "",
                    CustomerEmail = "",
                    CustomerPhone = "",
                    CustomerAddress = ""
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
            }

            HttpContext.Session.SetInt32("CurrentOrderId", order.Id);

            return order;
        }

        public IActionResult Index()
        {
            var order = GetCurrentOrder();
            if (order.OrderItems == null) order.OrderItems = new List<OrderItem>();

            return View(order.OrderItems.ToList());
        }

        [HttpPost]
        public IActionResult AddToCart(int variantId, int quantity)
        {
            var order = GetCurrentOrder();

            if (order.OrderItems == null) order.OrderItems = new List<OrderItem>();

            var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ProductVariantId == variantId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
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

            _context.SaveChanges();
            UpdateOrderTotal(order.Id);

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int itemId)
        {
            var order = GetCurrentOrder();
            if (order.OrderItems != null)
            {
                var item = order.OrderItems.FirstOrDefault(i => i.Id == itemId);
                if (item != null)
                {
                    _context.OrderItems.Remove(item);
                    _context.SaveChanges();
                    UpdateOrderTotal(order.Id);
                }
            }
            return RedirectToAction("Index");
        }

        private void UpdateOrderTotal(int orderId)
        {
            var order = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == orderId);
            if (order != null && order.OrderItems != null)
            {
                order.TotalPrice = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
                _context.SaveChanges();
            }
        }
    }
}