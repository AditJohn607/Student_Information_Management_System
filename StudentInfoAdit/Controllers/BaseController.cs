using System.Web.Mvc;
using System.Web;

namespace StudentInfoAdit.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(System.DateTime.UtcNow.AddDays(-1));
            base.OnActionExecuting(filterContext);
        }
    }
}