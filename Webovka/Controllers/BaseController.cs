using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http; 

namespace Webovka.Controllers
{
    public abstract class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var userId = this.HttpContext.Session.GetString("UserId");
            var userName = this.HttpContext.Session.GetString("UserName");
            var userRole = this.HttpContext.Session.GetString("UserRole");

            this.ViewBag.IsAuthenticated = userId != null;
            this.ViewBag.CurrentUserName = userName;
            this.ViewBag.CurrentUserRole = userRole;
        }
    }
}