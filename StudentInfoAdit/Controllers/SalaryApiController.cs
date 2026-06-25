using System;
using System.Linq;
using System.Web.Mvc;
using StudentInfoAdit.Models;

namespace StudentInfoAdit.Controllers
{
    public class SalaryApiController : Controller
    {
        private StudentDBContext db =
            new StudentDBContext();

        public JsonResult GetSalaryStructures()
        {
            var data =
                db.SalaryStructures.ToList();
            
            return Json(
                data,
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateSalaryStructure(int staffId, decimal basicSalary, decimal hra, decimal da, decimal allowance)
        {
            bool exists = db.SalaryStructures
                            .Any(x => x.StaffId == staffId);

            if (exists)
            {
                return Json(new
                {
                    success = false,
                    message = "Salary structure already exists for this staff member"
                });
            }

            SalaryStructureModel salary = new SalaryStructureModel
            {
                StaffId = staffId,
                BasicSalary = basicSalary,
                HRA = hra,
                DA = da,
                OtherAllowance = allowance
            };

            db.SalaryStructures.Add(salary);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Salary Structure Created"
            });
        }

        [HttpPost]
        public JsonResult UpdateSalaryStructure(
            int salaryId,
            decimal basicSalary,
            decimal hra,
            decimal da,
            decimal allowance)
        {
            var salary = db.SalaryStructures
                           .FirstOrDefault(x => x.SalaryId == salaryId);

            if (salary == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Record not found"
                });
            }

            salary.BasicSalary = basicSalary;
            salary.HRA = hra;
            salary.DA = da;
            salary.OtherAllowance = allowance;

            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Updated Successfully"
            });
        }

        [HttpPost]
        public JsonResult DeleteSalaryStructure(int salaryId)
        {
            var salary = db.SalaryStructures
                           .FirstOrDefault(x => x.SalaryId == salaryId);

            if (salary == null)
            {
                return Json(new
                {
                    success = false
                });
            }

            db.SalaryStructures.Remove(salary);
            db.SaveChanges();

            return Json(new
            {
                success = true
            });
        }

        public JsonResult GetStaff()
        {
            var data = db.Staff
                         .Select(x => new
                         {
                             x.StaffId,
                             x.StaffName
                         })
                         .ToList();

            return Json(
                data,
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GenerateSalary(
            int salaryId,
            int month,
            int year)
        {
            var structure =
                db.SalaryStructures
                  .FirstOrDefault(x => x.SalaryId == salaryId);
            
            if (structure == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Salary structure not found"
                });
            }

            bool exists = db.SalaryPayments.Any(x =>
                x.SalaryId == salaryId &&
                x.PaymentMonth == month &&
                x.PaymentYear == year);

            if (exists)
            {
                return Json(new
                {
                    success = false,
                    message = "Salary already generated for this month"
                });
            }

            decimal grossSalary =
                structure.BasicSalary +
                structure.HRA +
                structure.DA +
                structure.OtherAllowance;

            SalaryPaymentModel payment =
                new SalaryPaymentModel
                {
                    SalaryId = salaryId,
                    PaymentMonth = month,
                    PaymentYear = year,
                    GrossSalary = grossSalary,
                    PaidDate = DateTime.Now
                };

            db.SalaryPayments.Add(payment);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                grossSalary = grossSalary
            });
        }

        public JsonResult GetSalaryPayments()
        {
            var data = db.SalaryPayments
                         .Select(x => new
                         {
                             x.PaymentId,
                             x.SalaryId,
                             x.PaymentMonth,
                             x.PaymentYear,
                             x.GrossSalary,
                             x.PaidDate
                         })
                         .ToList();

            return Json(
                data,
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSalarySlip(int paymentId)
        {
            var payment = db.SalaryPayments
                            .Include("SalaryStructure")
                            .Include("SalaryStructure.Staff")
                            .FirstOrDefault(x => x.PaymentId == paymentId);

            if (payment == null)
            {
                return Json(
                    new
                    {
                        success = false
                    },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(
                new
                {
                    success = true,

                    StaffName = payment.SalaryStructure.Staff.StaffName,

                    Month = payment.PaymentMonth,
                    Year = payment.PaymentYear,

                    BasicSalary = payment.SalaryStructure.BasicSalary,

                    HRA = payment.SalaryStructure.HRA,

                    DA = payment.SalaryStructure.DA,

                    Allowance = payment.SalaryStructure.OtherAllowance,

                    GrossSalary = payment.GrossSalary,

                    PaidDate = payment.PaidDate
                },
                JsonRequestBehavior.AllowGet);
        }
    }
} 