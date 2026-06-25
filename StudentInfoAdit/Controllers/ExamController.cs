using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using StudentInfoAdit.Models;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class ExamController : BaseController
    {
        private StudentDBContext db = new StudentDBContext();

        public ActionResult Index()
        {
            var exams = db.Exams
                          .OrderByDescending(x => x.ExamDate)
                          .ToList();

            return View(exams);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ExamModel model)
        {
            if (ModelState.IsValid)
            {
                db.Exams.Add(model);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var exam = db.Exams
                         .FirstOrDefault(x => x.ExamId == id);

            if (exam == null)
            {
                return HttpNotFound();
            }

            return View(exam);
        }

        [HttpPost]
        public ActionResult Edit(ExamModel model)
        {
            var exam = db.Exams
                         .FirstOrDefault(x => x.ExamId == model.ExamId);

            if (exam == null)
            {
                return HttpNotFound();
            }

            exam.ExamName = model.ExamName;
            exam.ExamDate = model.ExamDate;
            exam.StudentClass = model.StudentClass;

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var exam = db.Exams.FirstOrDefault(x => x.ExamId == id);

            if (exam != null)
            {
                var results = db.ExamResults
                                .Where(x => x.ExamId == id)
                                .ToList();
                db.ExamResults.RemoveRange(results);
                db.Exams.Remove(exam);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult SaveMarks(int examId, int studentId)
        {
            var student = db.Students.FirstOrDefault(x => x.StudentId == studentId);

            ViewBag.Student = student;
            ViewBag.StudentId = studentId;
            ViewBag.ExamId = examId;

            return View();
        }

        public ActionResult AddMarks(int id)
        {
            var exam = db.Exams
                         .FirstOrDefault(x => x.ExamId == id);

            if (exam == null)
            {
                return HttpNotFound();
            }

            ViewBag.Exam = exam;

            var students = db.Students 
                             .Where(x => x.StudentClass == 
                                         exam.StudentClass)
                             .OrderBy(x => x.StudentName)  
                             .ToList();

            return View(students);
        }

        [HttpPost]
        public ActionResult SaveMarks(
            int examId,
            int studentId,
            string subject,
            int marksObtained,
            int maxMarks)
        {
            var existing = db.ExamResults.FirstOrDefault(x =>
                    x.ExamId == examId &&
                    x.StudentId == studentId &&
                    x.Subject == subject);

            if (existing != null)
            {
                existing.MarksObtained = marksObtained;
                existing.MaxMarks = maxMarks;
            }

            else
            {
                ExamResultModel result = new ExamResultModel();
                result.ExamId = examId;
                result.StudentId = studentId;
                result.Subject = subject;
                result.MarksObtained = marksObtained;
                result.MaxMarks = maxMarks;
                db.ExamResults.Add(result);
            }

            db.SaveChanges();

            return RedirectToAction("AddMarks",
                new { id = examId });
        }

        public ActionResult ViewResult(int examId, int studentId)
        {
            var student = db.Students
                            .FirstOrDefault(x =>
                                x.StudentId == studentId);

            var exam = db.Exams
                         .FirstOrDefault(x =>
                             x.ExamId == examId);

            if (student == null || exam == null)
            {
                return HttpNotFound();
            }

            var results =
                db.ExamResults
                  .Where(x =>
                      x.ExamId == examId &&
                      x.StudentId == studentId)
                  .OrderBy(x => x.Subject)
                  .ToList();

            int totalObtained =
                results.Sum(x => x.MarksObtained);

            int totalMax =
                results.Sum(x => x.MaxMarks);

            decimal percentage = 0;

            if (totalMax > 0)
            {
                percentage =
                    ((decimal)totalObtained / totalMax) * 100;
            }

            string grade;

            if (percentage >= 90)
                grade = "A+";
            else if (percentage >= 80)
                grade = "A"; 
            else if (percentage >= 70)                                
                grade = "B"; 
            else if (percentage >= 60)
                grade = "C"; 
            else
                grade = "D";

            var model = new ExamViewModel
            {
                Student = student,
                Exam = exam,
                Results = results,
                Percentage = percentage,
                Grade = grade
            };

            return View(model);
        }

        public ActionResult Dashboard()
        {
            ViewBag.TotalStudents =
                db.Students.Count();

            ViewBag.TotalExams =
                db.Exams.Count();

            ViewBag.PassCount =
                db.ExamResults.Count(x =>
                    x.MarksObtained >= 40);

            ViewBag.FailCount =
                db.ExamResults.Count(x =>
                    x.MarksObtained < 40);

            double avgPercentage = 0;

            if (db.ExamResults.Any())
            {
                avgPercentage =
                    db.ExamResults.Average(x =>
                        ((double)x.MarksObtained /
                         x.MaxMarks) * 100);
            }

            ViewBag.AveragePercentage =
                avgPercentage;

            var subjectStats =
                db.ExamResults
                  .GroupBy(x => x.Subject)
                  .Select(g => new
                  {
                      Subject = g.Key,
                      AvgMarks = g.Average(x => x.MarksObtained)
                  })
                  .ToList();

            ViewBag.SubjectNames =
                subjectStats.Select(x => x.Subject).ToList();

            ViewBag.SubjectAverages =
                subjectStats.Select(x => x.AvgMarks).ToList();

            var toppers =
                db.ExamResults
                  .GroupBy(x => x.StudentId)
                  .Select(g => new
                  {
                      StudentId = g.Key,
                      TotalMarks = g.Sum(x => x.MarksObtained)
                  })
                  .OrderByDescending(x => x.TotalMarks)
                  .Take(5)
                  .ToList();

            var topperNames =
                toppers.Select(t =>
                    db.Students
                      .Where(s => s.StudentId == t.StudentId)
                      .Select(s => s.StudentName)
                      .FirstOrDefault())
                .ToList();

            ViewBag.TopperNames = topperNames;

            ViewBag.TopperMarks =
                toppers.Select(x => x.TotalMarks).ToList();

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