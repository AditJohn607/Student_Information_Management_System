using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using StudentInfoAdit.Models;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class FeeController : Controller
    {
        private StudentDBContext db = new StudentDBContext();

        public ActionResult Index()
        {
            ViewBag.Student = null;
            ViewBag.Fee = null;
            ViewBag.TotalPaid = 0;
            ViewBag.Balance = 0;
            ViewBag.Message = null;

            return View();
        }

        [HttpPost]
        public ActionResult Search(string studentName)
        {
            if (string.IsNullOrWhiteSpace(studentName))
            {
                ViewBag.Message = "Please enter student name";
                return View("Index"); 
            }

            int id;

            var student = db.Students.FirstOrDefault(s =>
                s.StudentName.Contains(studentName));

            if (student == null && int.TryParse(studentName, out id))
            {
                student = db.Students
                            .FirstOrDefault(s =>
                                s.StudentId == id);
            }

            if (student == null)
            {
                ViewBag.Message = "Student not found";
                return View("Index");
            }

            var fee = db.FeeStructures
                        .Include(f => f.Payments)
                        .FirstOrDefault(f => f.StudentId == student.StudentId);

            decimal totalPaid = 0;
            decimal balance = 0;

            if (fee != null && fee.Payments != null)
            {
                totalPaid = fee.Payments.Sum(p => p.AmountPaid);
                balance = fee.TotalFee - totalPaid;
            }

            ViewBag.Student = student;
            ViewBag.Fee = fee;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.Balance = balance;
            return RedirectToAction("Details", new { studentId = student.StudentId });
        }

        [HttpPost]
        public ActionResult Create(FeeStructure model)
        {
            if (ModelState.IsValid)
            {
                db.FeeStructures.Add(model);
                db.SaveChanges();

                return RedirectToAction(
                    "Details",
                    new
                    {
                        studentId = model.StudentId
                    });
            }

            ViewBag.Students =
                new SelectList(
                    db.Students,
                    "StudentId",
                    "StudentName"
                );

            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Students = new SelectList(
                db.Students.OrderBy(x => x.StudentName),
                "StudentId",
                "StudentName"
            );

            return View();
        }

        [HttpPost]
        public ActionResult AddPayment(int feeId, decimal amountPaid, string paymentMode)
        {
            var fee = db.FeeStructures.FirstOrDefault(f => f.FeeId == feeId);

            if (fee == null)
            {
                TempData["Message"] = "Fee record not found";
                return RedirectToAction("Index");
            }

            var payment = new FeePayment
            { 
                FeeId = feeId,
                AmountPaid = amountPaid,
                PaymentDate = DateTime.Now,
                PaymentMode = paymentMode
            };
       
            db.FeePayments.Add(payment);
            db.SaveChanges();

            TempData["Message"] = "Payment added successfully";
            return RedirectToAction("Index");
        }

        public ActionResult Dashboard()
        {
            ViewBag.TotalCollection =
                db.FeePayments.Sum(x => (decimal?)x.AmountPaid) ?? 0;

            ViewBag.TotalStudents =
                db.Students.Count();

            ViewBag.TotalTransactions =
                db.FeePayments.Count();

            decimal totalFee =
                db.FeeStructures.Sum(x => (decimal?)x.TotalFee) ?? 0;

            decimal totalPaid =
                db.FeePayments.Sum(x => (decimal?)x.AmountPaid) ?? 0;

            ViewBag.TotalPending =
                totalFee - totalPaid;

            var monthlyData =
                db.FeePayments
                  .ToList()
                  .GroupBy(x => x.PaymentDate.HasValue
                        ? x.PaymentDate.Value.ToString("MMM")
                        : "Unknown")
                  .Select(g => new
                  {
                      Month = g.Key,
                      Amount = g.Sum(x => x.AmountPaid)
                  })
                  .ToList();

            ViewBag.Months =
                monthlyData.Select(x => x.Month).ToList();

            ViewBag.MonthlyAmounts =
                monthlyData.Select(x => x.Amount).ToList();

            ViewBag.PaymentModes =
                new int[]
        {
            db.FeePayments.Count(x => x.PaymentMode == "Cash"),
            db.FeePayments.Count(x => x.PaymentMode == "UPI"),
            db.FeePayments.Count(x => x.PaymentMode == "Card")
        };

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Details(int studentId)
        {
            var student = db.Students
                            .FirstOrDefault(x =>
                                x.StudentId == studentId);

            if (student == null)
            {
                return HttpNotFound();
            }

            var fee = db.FeeStructures
                        .Include(x => x.Payments)
                        .FirstOrDefault(x =>
                            x.StudentId == studentId);

            if (fee == null)
            {
                ViewBag.Message = "No fee structure created for this student yet.";

                return View("Details",
                    new FeeViewModel
                    {
                        Student = student,
                        Fee = null,
                        TotalPaid = 0,
                        Balance = 0
                    });
            }

            decimal totalPaid = 0;

            if (fee.Payments != null)
            {
                totalPaid =
                    fee.Payments.Sum(x =>
                        x.AmountPaid);
            }

            FeeViewModel model =
                new FeeViewModel();
            model.Student = student;
            model.Fee = fee;
            model.TotalPaid = totalPaid;
            model.Balance = fee.TotalFee - totalPaid;
            model.Payments = fee.Payments.ToList();

            return View(model);
        }
       
        public ActionResult Receipt(int paymentId)
        {
            var payment = db.FeePayments
                            .Include(x => x.FeeStructure)
                            .Include(x => x.FeeStructure.Student)
                            .FirstOrDefault(x => x.PaymentId == paymentId);
            if (payment == null)
            {
                return HttpNotFound();
            }
            var fee = payment.FeeStructure;
            var payments = db.FeePayments
                             .Where(x => x.FeeId == fee.FeeId)
                             .ToList();

            decimal totalPaid = payments.Sum(x => x.AmountPaid);
            FeeViewModel receiptModel = new FeeViewModel();
            receiptModel.Student = fee.Student;
            receiptModel.Fee = fee;
            receiptModel.TotalPaid = totalPaid;
            receiptModel.Balance = fee.TotalFee - totalPaid;
            receiptModel.Payments = payments;
            return View(receiptModel);
        }
    }
}