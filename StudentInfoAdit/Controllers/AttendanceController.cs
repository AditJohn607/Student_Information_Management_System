using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using StudentInfoAdit.Models;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class AttendanceController : BaseController
    {
        private StudentDBContext db = new StudentDBContext();

        public ActionResult Details(string searchName)
        {
            ViewBag.StudentList = db.Students.OrderBy(x => x.StudentName).ToList();

            var attendanceList = db.Attendances
                       .Include(x => x.Student)
                       .OrderByDescending(x => x.AttendanceDate)
                       .AsQueryable();

            if (Session["Role"] != null &&
                Session["Role"].ToString() == "Student")
            {
                int studentId =
                    Convert.ToInt32(Session["StudentId"]);

                attendanceList =
                    attendanceList.Where(x =>
                        x.StudentId == studentId);
            }

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                attendanceList = attendanceList.Where(x =>
                    x.Student.StudentName.Contains(searchName));
            }

            ViewBag.AttendanceList = attendanceList.ToList();

            return View(new AttendanceModel
            {
                AttendanceDate = DateTime.Today
            });
        }

        [HttpPost]
        public ActionResult SaveAttendance(AttendanceModel model)
        {
            if (model.AttendanceDate.Date > DateTime.Today)
            {
                TempData["Error"] =
                    "Future attendance cannot be marked.";

                return RedirectToAction("Details");
            }

            DateTime attendanceDate = model.AttendanceDate.Date;         
            bool exists = db.Attendances.Any(x =>
                x.StudentId == model.StudentId &&
                DbFunctions.TruncateTime(x.AttendanceDate)
                == DbFunctions.TruncateTime(attendanceDate));

            if (exists)
            {
                TempData["Error"] =
                    "Attendance already exists for this date.";

                return RedirectToAction("Details");
            }
            db.Attendances.Add(model);
            db.SaveChanges();

            TempData["Success"] =
                "Attendance saved successfully.";

            return RedirectToAction("Details");
        }

        public ActionResult Edit(int id)
        {
            var attendance = db.Attendances
                               .Include(x => x.Student)
                               .FirstOrDefault(x => x.Id == id);

            if (attendance == null)
                return HttpNotFound();

            return View(attendance);
        }

        [HttpPost]
        public ActionResult Edit(AttendanceModel model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State =
                    System.Data.Entity.EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction("Details");
            }

            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var attendance =
                db.Attendances.FirstOrDefault(x =>
                    x.Id == id);

            if (attendance == null)
            {
                return HttpNotFound();
            }

            db.Attendances.Remove(attendance);

            db.SaveChanges();

            TempData["Success"] =
                "Attendance deleted successfully.";

            return RedirectToAction(
                "Details");
        }

        public ActionResult Report(int? studentId)
        {
            if (Session["Role"] != null &&
                Session["Role"].ToString() == "Student")
            {
                studentId =
                    Convert.ToInt32(Session["StudentId"]);
            }

            if (studentId == null)
            {
                return RedirectToAction("Details");
            }

            var student =
                db.Students.FirstOrDefault(x =>
                    x.StudentId == studentId.Value);

            if (student == null)
            {
                return HttpNotFound();
            }

            var records =
                db.Attendances
                  .Where(x => x.StudentId == studentId.Value)
                  .OrderByDescending(x => x.AttendanceDate)
                  .ToList();

            int totalDays = records.Count;
            int presentDays = records.Count(x => x.IsPresent);

            double percentage = 0;

            if (totalDays > 0)
            {
                percentage = (double)presentDays * 100 / totalDays;
            }

            AttendanceReportViewModel model =
                new AttendanceReportViewModel
                {
                    Student = student,
                    AttendanceRecords = records,
                    TotalDays = totalDays,
                    PresentDays = presentDays,
                    AttendancePercentage = percentage
                };

            return View(model);
        }

        public ActionResult Dashboard()
        {
            int totalRecords =
                db.Attendances.Count();

            int totalPresent =
                db.Attendances.Count(x => x.IsPresent);

            int totalAbsent =
                db.Attendances.Count(x => !x.IsPresent);

            double percentage = 0;

            if (totalRecords > 0)
            {
                percentage =
                    ((double)totalPresent /
                     totalRecords) * 100;
            }

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPresent = totalPresent;
            ViewBag.TotalAbsent = totalAbsent;
            ViewBag.AttendancePercentage =
                percentage.ToString("0.00");

            var monthlyData =
                db.Attendances
                  .ToList()
                  .GroupBy(x =>
                        x.AttendanceDate
                         .ToString("MMM"))
                  .Select(g => new
                  {
                      Month = g.Key,
                      Count = g.Count()
                  })
                  .ToList();
         
            ViewBag.Months =
                monthlyData
                    .Select(x => x.Month)
                    .ToList();

            ViewBag.MonthCounts =
                monthlyData
                    .Select(x => x.Count)
                    .ToList();

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
    }
}