using System.Web.Mvc;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class ProfileController : Controller 
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}