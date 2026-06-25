using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StudentInfoAdit.Models;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class SalaryController : BaseController
    {
        private StudentDBContext db = new StudentDBContext();

        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult SalarySlip(int paymentId)
        {
            var payment = db.SalaryPayments
                            .Include("SalaryStructure")
                            .Include("SalaryStructure.Staff")
                            .FirstOrDefault(x => x.PaymentId == paymentId);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }
    }
}