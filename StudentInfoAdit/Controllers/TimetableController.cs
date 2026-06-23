using System;
using System.Linq;
using System.Web.Mvc;
using StudentInfoAdit.Models;
using System.Collections.Generic;

namespace StudentInfoAdit.Controllers
{
    public class TimetableController : Controller
    {
        private StudentDBContext db = new StudentDBContext();

        public ActionResult Index(string selectedClass)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            string role = Session["Role"] == null? "": Session["Role"].ToString(); ;

            if (role == "Student")
            {
                int studentId = Convert.ToInt32(Session["StudentId"]);

                var student = db.Students
                                .FirstOrDefault(x => x.StudentId == studentId);

                if (student != null)
                {
                    selectedClass = student.StudentClass.ToString() + "th";
                }
            }

            ViewBag.ClassList = new List<SelectListItem>
{
    new SelectListItem { Text = "1st", Value = "1st" },
    new SelectListItem { Text = "2nd", Value = "2nd" },
    new SelectListItem { Text = "3rd", Value = "3rd" },
    new SelectListItem { Text = "4th", Value = "4th" },
    new SelectListItem { Text = "5th", Value = "5th" },
    new SelectListItem { Text = "6th", Value = "6th" },
    new SelectListItem { Text = "7th", Value = "7th" },
    new SelectListItem { Text = "8th", Value = "8th" },
    new SelectListItem { Text = "9th", Value = "9th" },
    new SelectListItem { Text = "10th", Value = "10th" },
    new SelectListItem { Text = "11th", Value = "11th" },
    new SelectListItem { Text = "12th", Value = "12th" }
};

            if (string.IsNullOrEmpty(selectedClass))
            {
                selectedClass = "5th";
            }

            var data = db.Timetables
                         .Where(x => x.Class.Trim().ToLower() == selectedClass.Trim().ToLower())
                         .ToList();

            var days = new List<string>
    { 
        "Monday",
        "Tuesday",
        "Wednesday",   
        "Thursday",
        "Friday"
    };

            var periods = data.Select(x => x.PeriodNo).Distinct().OrderBy(x => x).ToList();
      
            var grid = new Dictionary<string, TimetableModel>();

            foreach (var item in data)
            {
                string key = item.DayOfWeek + "-" + item.PeriodNo;
                grid[key] = item;
            }
                                                                                                                                                                                                                                           
            var model = new TimetableGridViewModel
            {
                Days = days,
                Periods = periods,
                GridData = grid,
                SelectedClass = selectedClass
            };
            return View(model);
        }
        
        public ActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
                return RedirectToAction("Login", "Account");
            var data = db.Timetables.FirstOrDefault(x => x.TimetableId == id);
            if (data != null)
            {
                db.Timetables.Remove(data);
                db.SaveChanges();
            }          
            return RedirectToAction("Index");
        }
                                                                  
        private bool IsAdmin()
        {
            return Session["Role"] != null &&
                   Session["Role"].ToString() == "Admin";
        }
      
        private void LoadDays()
        {
            ViewBag.Days = new List<SelectListItem>
            {
                new SelectListItem { Text = "Monday", Value = "Monday" },
                new SelectListItem { Text = "Tuesday", Value = "Tuesday" },
                new SelectListItem { Text = "Wednesday", Value = "Wednesday" },
                new SelectListItem { Text = "Thursday", Value = "Thursday" },
                new SelectListItem { Text = "Friday", Value = "Friday" }
            };
        }
                                                                                                                            
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ClassList = new List<SelectListItem>
            {
               new SelectListItem { Text = "1st", Value = "1st" },  
               new SelectListItem { Text = "2nd", Value = "2nd" },  
               new SelectListItem { Text = "3rd", Value = "3rd" },  
               new SelectListItem { Text = "4th", Value = "4th" },  
               new SelectListItem { Text = "5th", Value = "5th" },  
               new SelectListItem { Text = "6th", Value = "6th" },  
               new SelectListItem { Text = "7th", Value = "7th" },  
               new SelectListItem { Text = "8th", Value = "8th" },  
               new SelectListItem { Text = "9th", Value = "9th" },  
               new SelectListItem { Text = "10th", Value = "10th" },
               new SelectListItem { Text = "11th", Value = "11th" },
               new SelectListItem { Text = "12th", Value = "12th" } 
            };

            ViewBag.Days = new List<SelectListItem>
    {
        new SelectListItem { Text = "Monday", Value = "Monday" },
        new SelectListItem { Text = "Tuesday", Value = "Tuesday" },
        new SelectListItem { Text = "Wednesday", Value = "Wednesday" },
        new SelectListItem { Text = "Thursday", Value = "Thursday" },
        new SelectListItem { Text = "Friday", Value = "Friday" }
    };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TimetableModel model)
        {
            if (ModelState.IsValid)
            {
                 db.Timetables.Add(model);
                 db.SaveChanges();
                 return RedirectToAction("Index");
            }

            LoadDays();
            ViewBag.ClassList = new List<SelectListItem>{
               new SelectListItem { Text = "1st", Value = "1st" },  
               new SelectListItem { Text = "2nd", Value = "2nd" },  
               new SelectListItem { Text = "3rd", Value = "3rd" },  
               new SelectListItem { Text = "4th", Value = "4th" },  
               new SelectListItem { Text = "5th", Value = "5th" },  
               new SelectListItem { Text = "6th", Value = "6th" },  
               new SelectListItem { Text = "7th", Value = "7th" },  
               new SelectListItem { Text = "8th", Value = "8th" },  
               new SelectListItem { Text = "9th", Value = "9th" },  
               new SelectListItem { Text = "10th", Value = "10th" },
               new SelectListItem { Text = "11th", Value = "11th" },
               new SelectListItem { Text = "12th", Value = "12th" } 
    };
                                                                        
            return View(model);
        }
    }
}