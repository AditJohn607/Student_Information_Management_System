using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using StudentInfoAdit.Models;
namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class NoticeController : BaseController
    {
        private StudentDBContext db = new StudentDBContext();

        public ActionResult Index()
        {
            var notices = db.Notices.OrderByDescending(n => n.PublishDate).ToList();
            return View(notices);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NoticeModel model)
        {
            if (ModelState.IsValid)
            {
                db.Notices.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var notice = db.Notices.Find(id);
            if (notice == null)
            {
                return HttpNotFound();
            }

            return View(notice);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NoticeModel notice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(notice).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(notice);
        }

        public ActionResult Details(int id)
        {
            var notice = db.Notices.Find(id);
            if(notice == null)
            {
                return HttpNotFound();
            }
            return View(notice);
        }

        public ActionResult Delete(int id)
        {
            var notice = db.Notices.Find(id);

            if (notice != null)
            {
                db.Notices.Remove(notice);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var notice = db.Notices.Find(id);
            if (notice != null)
            { 
                db.Notices.Remove(notice);
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
    }
}

// Parent Teacher Meeting(PTM) will be held on 13/06/2026(Saturday) from 8:00-2:00pm for classes Nursery-XII. Each ward has to come with his/her at least one parent.