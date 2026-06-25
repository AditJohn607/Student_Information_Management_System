using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StudentInfoAdit.Models;
using System.Data.Entity;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class AssignmentController : BaseController
    {
        private StudentDBContext db = new StudentDBContext();

        public ActionResult Index()
        {
            string role = Session["Role"] != null
                ? Session["Role"].ToString()
                : "";

            string username = Session["Username"] != null
                ? Session["Username"].ToString()
                : "";

            if (role == "Admin")
            {
                return View(db.Assignments
                              .OrderByDescending(x => x.CreatedDate)
                              .ToList());
            }

            if (role == "Teacher")
            {
                return View(db.Assignments
                              .Where(x => x.CreatedBy == username
                                       || x.CreatedBy == "admin")
                              .OrderByDescending(x => x.CreatedDate)
                              .ToList());
            }

            if (role == "Parent")
            {
                return View(db.Assignments
                              .OrderByDescending(x => x.DueDate)
                              .ToList());
            }

            return RedirectToAction("Index", "Dashboard");
        }

        public ActionResult Create()
        {
            string role = Session["Role"] != null? Session["Role"].ToString(): "";

            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(AssignmentModel model, HttpPostedFileBase AssignmentFile)
        {
            string role = Session["Role"] != null
                ? Session["Role"].ToString()
                : "";

            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index");
            }

            if (AssignmentFile != null && AssignmentFile.ContentLength > 0)
            {
                string folder = Server.MapPath("~/Uploads/Assignments");

                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }

                string fileName = Guid.NewGuid().ToString()
                                  + System.IO.Path.GetExtension(AssignmentFile.FileName);

                string fullPath = System.IO.Path.Combine(folder, fileName);

                AssignmentFile.SaveAs(fullPath);

                model.AttachmentPath = "/Uploads/Assignments/" + fileName;
            }

            model.CreatedBy = Session["Username"] != null
                ? Session["Username"].ToString()
                : role;

            model.CreatedDate = DateTime.Now;

            db.Assignments.Add(model);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Save(
            AssignmentModel assignment,
            HttpPostedFileBase AssignmentFile)
        {
            try
            {
                if (AssignmentFile != null &&
                    AssignmentFile.ContentLength > 0)
                {
                    string extension =
                        Path.GetExtension(AssignmentFile.FileName)
                        .ToLower();

                    if (extension != ".pdf" &&
                        extension != ".doc" &&
                        extension != ".docx" &&
                        extension != ".jpg" &&
                        extension != ".jpeg" &&
                        extension != ".png")
                    {
                        TempData["Message"] =
                            "Only PDF, DOC, DOCX, JPG and PNG files are allowed.";
                        return RedirectToAction("Create");
                    }

                    string folder =
                        Server.MapPath("~/Uploads/Assignments/");

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string fileName =
                        Guid.NewGuid().ToString() + extension;

                    string fullPath =
                        Path.Combine(folder, fileName);

                    AssignmentFile.SaveAs(fullPath);

                    assignment.AttachmentPath =
                        "/Uploads/Assignments/" + fileName;
                }

                assignment.CreatedBy =
                    Session["Username"] != null
                    ? Session["Username"].ToString()
                    : "Unknown";

                assignment.CreatedDate = DateTime.Now;
                db.Assignments.Add(assignment);
                db.SaveChanges();

                TempData["Message"] =
                    "Assignment created successfully.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            string role = Session["Role"] != null
                ? Session["Role"].ToString()
                : "";

            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index");
            }

            var assignment = db.Assignments
                               .FirstOrDefault(x => x.AssignmentId == id);
            
            if (assignment == null)
            {
                return HttpNotFound();
            }

            string username = Session["Username"] != null? Session["Username"].ToString(): "";

            if (role == "Teacher" &&
                assignment.CreatedBy != username)
            {
                return RedirectToAction("Index");
            }

            return View(assignment);
        }

        [HttpPost]
        public ActionResult Edit(AssignmentModel model, HttpPostedFileBase AttachmentFile)
        {
            string role = Session["Role"] != null
                ? Session["Role"].ToString()
                : "";

            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index");
            }

            var assignment = db.Assignments
                               .FirstOrDefault(x => x.AssignmentId == model.AssignmentId);

            if (assignment == null)
            {
                return HttpNotFound();
            }

            assignment.Title = model.Title;
            assignment.Description = model.Description;
            assignment.Subject = model.Subject;
            assignment.StudentClass = model.StudentClass;
            assignment.DueDate = model.DueDate;

            if (AttachmentFile != null && AttachmentFile.ContentLength > 0)
            {
                string folder = Server.MapPath("~/Uploads/Assignments/");

                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }

                string fileName = Guid.NewGuid().ToString() +
                                  System.IO.Path.GetExtension(AttachmentFile.FileName);

                string fullPath = System.IO.Path.Combine(folder, fileName);

                AttachmentFile.SaveAs(fullPath);

                assignment.AttachmentPath = "/Uploads/Assignments/" + fileName;
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            string role = Session["Role"] != null? Session["Role"].ToString(): "";

            string username = Session["Username"] != null? Session["Username"].ToString(): "";

            var assignment = db.Assignments.FirstOrDefault(x => x.AssignmentId == id);

            if (assignment == null)
            {
                return HttpNotFound();
            }

            if (role == "Teacher" &&
                assignment.CreatedBy != username)
            {
                return RedirectToAction("Index");
            }

            if (assignment != null)
            {
                if (!string.IsNullOrEmpty(
                    assignment.AttachmentPath))
                {
                    string path =
                        Server.MapPath(
                            assignment.AttachmentPath);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                db.Assignments.Remove(assignment);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var assignment =
                db.Assignments
                  .FirstOrDefault(x =>
                      x.AssignmentId == id);
            if (assignment == null)
            {
                return HttpNotFound();
            }

            return View(assignment);
        }

        public ActionResult Submit(int id)
{
    if (Session["Role"] == null ||
        Session["Role"].ToString() != "Parent")
    {
        return RedirectToAction("Index");
    }

    var assignment = db.Assignments
                       .FirstOrDefault(x => x.AssignmentId == id);

    if (assignment == null)
    {
        return HttpNotFound();
    }

    string username = Session["Username"].ToString();

    var submission = db.AssignmentSubmissions
        .FirstOrDefault(x => x.AssignmentId == id
                          && x.SubmittedBy == username);

    ViewBag.ExistingSubmission = submission;

    return View(assignment);
}

        [HttpPost]
        public ActionResult Submit(int AssignmentId, HttpPostedFileBase SubmissionFile)
        {
            string role = Session["Role"] != null
                ? Session["Role"].ToString()
                : "";

            if (role != "Parent")
            {
                return RedirectToAction("Index");
            }

            if (SubmissionFile != null && SubmissionFile.ContentLength > 0)
            {
                string folder = Server.MapPath("~/Uploads/AssignmentSubmissions");

                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }
                string fileName = Guid.NewGuid().ToString()
                                + System.IO.Path.GetExtension(SubmissionFile.FileName);
                string fullPath = System.IO.Path.Combine(folder, fileName);
                SubmissionFile.SaveAs(fullPath);
                string username = Session["Username"].ToString();
                var existingSubmission = db.AssignmentSubmissions
                    .FirstOrDefault(x => x.AssignmentId == AssignmentId
                                      && x.SubmittedBy == username);
                string filePath = "/Uploads/AssignmentSubmissions/" + fileName;

                if (existingSubmission == null)
                {
                    AssignmentSubmissionModel submission =
                        new AssignmentSubmissionModel();
                    submission.AssignmentId = AssignmentId;
                    submission.SubmittedBy = username;
                    submission.FilePath = filePath;
                    submission.SubmissionDate = DateTime.Now;
                    db.AssignmentSubmissions.Add(submission);
                }
                else
                {
                    if (!string.IsNullOrEmpty(existingSubmission.FilePath))
                    {
                        string oldPath = Server.MapPath(existingSubmission.FilePath);

                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    existingSubmission.FilePath = filePath;
                    existingSubmission.SubmissionDate = DateTime.Now;
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult SubmissionList()
        {
            string role = Session["Role"] != null
                ? Session["Role"].ToString()
                : "";

            string username = Session["Username"] != null
                ? Session["Username"].ToString()
                : "";

            if (role == "Admin")
            {
                var submissions = db.AssignmentSubmissions
                                    .Include("Assignment")
                                    .OrderByDescending(x => x.SubmissionDate)
                                    .ToList();

                return View(submissions);
            }

            if (role == "Teacher")
            {
                var submissions = db.AssignmentSubmissions
                                    .Include("Assignment")
                                    .Where(x => x.Assignment.CreatedBy == username
                                             || x.Assignment.CreatedBy == "admin")
                                    .OrderByDescending(x => x.SubmissionDate)
                                    .ToList();

                return View(submissions);
            }

            return RedirectToAction("Index");
        }
    }
}