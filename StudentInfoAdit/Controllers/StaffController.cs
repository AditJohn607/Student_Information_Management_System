using System.Linq;
using System.Web.Mvc;
using StudentInfoAdit.Models;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        StudentDBContext db = new StudentDBContext();
        public ActionResult StaffDetails()
        {
            var data = db.Staff.ToList();
            return View(data);
        }
            
        public ActionResult Create()
        {
            return View("StaffForm", new StaffModel());
        }

        [HttpPost]
        public ActionResult Save(StaffModel model)
        {
            if (model.StaffId == 0) 
            { 
                db.Staff.Add(model);
            }
            else
            { 
                var data = db.Staff.FirstOrDefault(x => x.StaffId == model.StaffId);
                if (data != null)
                {
                    data.StaffName = model.StaffName;
                    data.Role = model.Role;
                    data.Subject = model.Subject;
                    data.DateJoined = model.DateJoined;
                }
            }
            db.SaveChanges();
            return RedirectToAction("StaffDetails");
        }
        
        public ActionResult Edit(int id)
        {
            var data = db.Staff.FirstOrDefault(x => x.StaffId == id);
            return View("StaffForm", data);
        }

        public ActionResult Delete(int id)
        {
            var data = db.Staff.FirstOrDefault(x => x.StaffId == id);
            if (data != null)
            {
                db.Staff.Remove(data);
                db.SaveChanges();
            } 
            return RedirectToAction("StaffDetails");
        }
    }
}