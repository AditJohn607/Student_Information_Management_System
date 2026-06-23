using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StudentInfoAdit.Models;
using System.Collections.Generic;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private bool IsERPEnabled()
        {
            return Session["Role"] != null
                && Session["Role"].ToString() == "Admin"
                && Session["ERPMode"] != null
                && (bool)Session["ERPMode"];

        }

        public ActionResult StudentDetails()
        {
            using (var db = new StudentDBContext())
            {
                var classList = new List<SelectListItem>
{
    new SelectListItem { Text = "5", Value = "5" },
    new SelectListItem { Text = "6", Value = "6" },
    new SelectListItem { Text = "7", Value = "7" },
    new SelectListItem { Text = "8", Value = "8" },
    new SelectListItem { Text = "9", Value = "9" },
    new SelectListItem { Text = "10", Value = "10"}
};

                var model = new StudentViewModel
                {
                    Student = new StudentModel(),
                    Parent = new ParentModel(),
                    StudentList = db.Students.ToList(),
                    ClassList = classList
                };

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Save(StudentViewModel vm, HttpPostedFileBase PhotoFile)
        {
            try
            {
                if (vm.Student == null) return Content("Student is NULL");

                if (vm.Parent == null) vm.Parent = new ParentModel();

                using (var db = new StudentDBContext())
                {
                    string filePath = null;

                    if (PhotoFile != null && PhotoFile.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(PhotoFile.FileName).ToLower();

                        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                        {
                            return Content("Only JPG, JPEG, PNG allowed");
                        }

                        string root = Server.MapPath("~/Uploads/");
                        string folder = Path.Combine(root, "Students");

                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        string fileName = Guid.NewGuid() + ext;
                        string fullPath = Path.Combine(folder, fileName);

                        PhotoFile.SaveAs(fullPath);

                        filePath = "/Uploads/Students/" + fileName;
                    }

                    if (vm.Student.StudentId == 0)
                    {
                        vm.Student.PhotoPath = filePath;

                        db.Students.Add(vm.Student);
                        db.SaveChanges();

                        if (vm.Parent != null && !string.IsNullOrWhiteSpace(vm.Parent.FatherName))
                        {  
                            vm.Parent.StudentId = vm.Student.StudentId;
                            db.Parents.Add(vm.Parent);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var student = db.Students.FirstOrDefault(x => x.StudentId == vm.Student.StudentId);

                        if (student != null)
                        {
                            student.StudentName = vm.Student.StudentName;
                            student.DateOfBirth = vm.Student.DateOfBirth;
                            student.PresentAddress = vm.Student.PresentAddress;
                            student.StudentClass = vm.Student.StudentClass;
                            student.MobileNumber = vm.Student.MobileNumber;
                            if (!string.IsNullOrEmpty(filePath))
                                student.PhotoPath = filePath;
                        }
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("StudentDetails");
            }
            catch (Exception ex)
            {
                return Content(
                    ex.InnerException != null && ex.InnerException.InnerException != null
                    ? ex.InnerException.InnerException.Message
                    : ex.InnerException != null
                        ? ex.InnerException.Message
                        : ex.Message
                );
            }
        }
       
        public ActionResult Edit(int id)
        { 
            using (var db = new StudentDBContext())
            {
                var model = new StudentViewModel
                {
                    Student = db.Students.FirstOrDefault(x => x.StudentId == id),
                    Parent = db.Parents.FirstOrDefault(x => x.StudentId == id) ?? new ParentModel(),
                    StudentList = db.Students.ToList(),
                    ClassList = new List<SelectListItem>                                                                                                 
                    {
                        new SelectListItem { Text = "5", Value = "5" },
                        new SelectListItem { Text = "6", Value = "6" },
                        new SelectListItem { Text = "7", Value = "7" },
                        new SelectListItem { Text = "8", Value = "8" },
                        new SelectListItem { Text = "9", Value = "9" },
                        new SelectListItem { Text = "10", Value = "10"}
                    }
                };
                return View("StudentDetails", model);
            }
        }

        public ActionResult Delete(int id)
        { 
            using (var db = new StudentDBContext())
            {
                var student = db.Students.FirstOrDefault(x => x.StudentId == id);
                if (student != null)
                {
                    if (!string.IsNullOrEmpty(student.PhotoPath))
                    {
                        string path = Server.MapPath(student.PhotoPath);
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);
                    }
                  
                    db.Students.Remove(student);
                }
                var parent = db.Parents.FirstOrDefault(x => x.StudentId == id);
                if (parent != null)
                    db.Parents.Remove(parent);
                db.SaveChanges();
            }
            return RedirectToAction("StudentDetails");
        }
       
        public ActionResult ParentDetails(int id)
        {
            using (var db = new StudentDBContext())
            {
                var parent = db.Parents.FirstOrDefault(x => x.StudentId == id);

                if (parent == null)
                {
                    parent = new ParentModel
                    {
                        StudentId = id
                    };
                }

                var model = new StudentViewModel
                {
                    Student = db.Students.FirstOrDefault(x => x.StudentId == id),
                    Parent = parent
                };

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult SaveParent(StudentViewModel vm)
        {
            using (var db = new StudentDBContext())
            {
                var parent = db.Parents.FirstOrDefault(x => x.StudentId == vm.Parent.StudentId);

                if (parent == null)
                {
                    db.Parents.Add(vm.Parent);
                }
                else
                {
                    parent.FatherName = vm.Parent.FatherName;
                    parent.FOccupation = vm.Parent.FOccupation;
                    parent.FMobileno = vm.Parent.FMobileno;
                    parent.MotherName = vm.Parent.MotherName;
                    parent.MOccupation = vm.Parent.MOccupation;
                    parent.MMobileno = vm.Parent.MMobileno;                                                                                                                                     
                }
                db.SaveChanges();
            }
            return RedirectToAction("StudentDetails");
        }
    }
}

 //Currently i am developing a project of school management system using asp.net mvc 5 c# which has 7 modules which have already been implemented(Student,parent,staff, attendance, exam ,fee, notice). Currently i am developing login feature which has four types of users( admin, parent, teacher, accountant). Admin has the authority to view and modify all the modules and other type of users have access to only some of them. Currently there is only one user as admin who can only view the dashboard there should be a toggle of erp at the top right corner which when clicked goes to the webpage where it gets the access to all the modules. In the erp page also there should be a toggle at the same place to go back to the dashboard view page. I will be sharing the necessary codes so that u can help me out with this