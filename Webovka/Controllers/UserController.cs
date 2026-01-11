using Microsoft.AspNetCore.Mvc;
using Webovka.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace Webovka.Controllers
{
    public class UserController : BaseController
    {
        private MyContext context;


        public UserController()
        {
            context = new MyContext();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
           
            var user = context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

            if (user != null)
            {
                this.HttpContext.Session.SetString("UserId", user.Id.ToString());
                this.HttpContext.Session.SetString("UserName", user.FirstName);
                this.HttpContext.Session.SetString("UserRole", user.Role);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Špatný email nebo heslo.";
            return View();
            
        }

        [HttpPost]
        public IActionResult Register(User model)
        {

                if (context.Users.Any(u => u.Email == model.Email))
                {
                    ViewBag.RegisterError = "Email už existuje.";
                    return View("Login"); 
                }

                model.Role = "Customer";
                model.RegisteredAt = DateTime.Now;

                context.Users.Add(model);
                context.SaveChanges();

                this.HttpContext.Session.SetString("UserId", model.Id.ToString());
                this.HttpContext.Session.SetString("UserName", model.FirstName);
                this.HttpContext.Session.SetString("UserRole", model.Role);

                return RedirectToAction("Index", "Home");
            
        }

        public IActionResult Logout()
        {
            this.HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}