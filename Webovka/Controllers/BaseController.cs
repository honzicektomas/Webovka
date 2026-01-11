using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http; // Pro Session

namespace Webovka.Controllers
{
    public abstract class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            this.ViewBag.CurrentUserName = this.HttpContext.Session.GetString("UserName");
            this.ViewBag.CurrentUserRole = this.HttpContext.Session.GetString("UserRole");
            this.ViewBag.IsAuthenticated = this.HttpContext.Session.GetString("UserId") != null;
        }
    }
}