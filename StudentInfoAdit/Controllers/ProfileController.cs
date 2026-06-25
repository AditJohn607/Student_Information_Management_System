using System.Web.Mvc;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class ProfileController : BaseController 
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}