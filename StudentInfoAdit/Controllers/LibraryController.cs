using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using StudentInfoAdit.Models;

namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {    
        private readonly StudentDBContext db = new StudentDBContext();
 
        public ActionResult IssueBook()
        {
            var model = new IssueBookViewModel
            {
                StudentList = db.Students
                    .OrderBy(s => s.StudentName)
                    .Select(s => new SelectListItem 
                    {
                        Value = s.StudentId.ToString(),
                        Text = s.StudentName
                    })
                    .ToList(),
                
                BookList = db.Books
                    .Where(b => b.AvailableCopies > 0)
                    .OrderBy(b => b.BookName)
                    .Select(b => new SelectListItem
                    {
                        Value = b.BookId.ToString(),
                        Text = b.BookName +
                               " (" + b.AvailableCopies + " Available)"
                    })
                    .ToList(),
                   
                DueDate = DateTime.Today.AddDays(14)
            }; 
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IssueBook(IssueBookViewModel model)
        {
            bool alreadyIssued = db.BookIssueds.Any(x =>
                x.StudentId == model.StudentId &&
                x.Returned == false);

            if (alreadyIssued)
            {
                ModelState.AddModelError("", "This student already has an issued book.");
            }
             
            var book = db.Books.FirstOrDefault(x => x.BookId == model.BookId);
            if (book == null)
            {
                ModelState.AddModelError("", "Book not found.");
            }
               
            else if (book.AvailableCopies <= 0)
            {
                ModelState.AddModelError("", "No copies available.");
            }

            if (!ModelState.IsValid)
            {   
                model.StudentList = db.Students
                    .OrderBy(s => s.StudentName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.StudentId.ToString(),
                        Text = s.StudentName
                    })
                    .ToList();

                model.BookList = db.Books
                    .Where(b => b.AvailableCopies > 0)
                    .OrderBy(b => b.BookName)
                    .Select(b => new SelectListItem
                    {
                        Value = b.BookId.ToString(),
                        Text = b.BookName
                    })
                    .ToList();
                return View(model);
            }
       
            var issue = new BookIssuedModel
            {
                StudentId = model.StudentId,
                BookId = model.BookId,
                IssueDate = DateTime.Now,
                DueDate = model.DueDate,
                Returned = false
            };

            db.BookIssueds.Add(issue);
            book.AvailableCopies--;
            db.SaveChanges();
            TempData["Success"] = "Book issued successfully.";
            return RedirectToAction("IssuedBooks");
        }

        public ActionResult IssuedBooks()
        {
            var books = db.BookIssueds
                .Include("Student")
                .Include("Book")
                .OrderByDescending(x => x.IssueDate)
                .ToList();
            return View(books);
        }

        public ActionResult ReturnBook(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var issue = db.BookIssueds.Include("Book").FirstOrDefault(x => x.IssueId == id);

            if (issue == null)
            {
                return HttpNotFound();
            }
            
            if (!issue.Returned)
            {
                issue.Returned = true;
                issue.Book.AvailableCopies++;
                db.SaveChanges();
                TempData["Success"] = "Book returned successfully.";
            }

            return RedirectToAction("IssuedBooks");
        }

        public ActionResult ActiveIssues()
        {
            var books = db.BookIssueds
                .Include("Student")
                .Include("Book")
                .Where(x => x.Returned == false)
                .OrderBy(x => x.DueDate)
                .ToList();
            return View(books);
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