using System.Linq;
using System.Web.Mvc;
using StudentInfoAdit.Models;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private StudentDBContext db = new StudentDBContext();
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
               return RedirectToAction("Login", "Account");

            string role = "";

             if (Session["Role"] != null)   
               role = Session["Role"].ToString();
             
             if (role == "Admin")
               return RedirectToAction("Admin"); 

             if (role == "Teacher")
               return RedirectToAction("Teacher"); 

             if (role == "Student")
               return RedirectToAction("Student");

             if (role == "Accountant")
               return RedirectToAction("Accountant");
 
          return RedirectToAction("Login", "Account");
    }
       
        public ActionResult ERP()
        {
           if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
              return RedirectToAction("Login", "Account");
           return View("Index"); 
        } 

        public ActionResult EnableERP()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            Session["ERPMode"] = true;
            return RedirectToAction("Admin");
        }

        public ActionResult DisableERP()
        {
            if (Session["Role"] == null || Session["Role"].ToString()!= "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            Session["ERPMode"] = false;
            return RedirectToAction("Admin");
        }

        public ActionResult Admin()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                return RedirectToAction("Login", "Account");

            ViewBag.TotalStudents = db.Students.Count();

            ViewBag.TotalStaff = db.Staff.Count();

            ViewBag.TotalFees =
                db.FeePayments.Sum(x => (decimal?)x.AmountPaid) ?? 0m;

            ViewBag.TotalNotices = db.Notices.Count();

            ViewBag.PendingResetCount =
                db.PasswordResetRequests
                  .Count(x => x.IsProcessed == false);

            return View();
        }

        public ActionResult Teacher()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Teacher")
                return RedirectToAction("Login", "Account");
            ViewBag.TotalStudents = db.Students.Count();
            ViewBag.TodayAttendance = db.Attendances.Count();
            return View(); 
        }

        public ActionResult Student()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Student")
                return RedirectToAction("Login", "Account");                               
            ViewBag.Notices = db.Notices.Count();
            return View();
        }
 
        public ActionResult Accountant()
        { 
            if (Session["Role"] == null || Session["Role"].ToString() != "Accountant")
                return RedirectToAction("Login", "Account");
            decimal totalFees = db.FeeStructures.Sum(x => (decimal?)x.TotalFee) ?? 0m;
            decimal paidFees = db.FeePayments.Sum(x => (decimal?)x.AmountPaid) ?? 0m;
            ViewBag.TotalFees = paidFees;
            ViewBag.PendingFees = totalFees - paidFees;
            return View();
        } 
    }
} 