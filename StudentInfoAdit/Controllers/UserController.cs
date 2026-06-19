using System;
using System.Linq;
using System.Web.Mvc;
using StudentInfoAdit.Models;
using StudentInfoAdit.Helpers;
namespace StudentInfoAdit.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private StudentDBContext db = new StudentDBContext();

        public ActionResult Index()
        {
            if (Session["Role"] == null ||
                Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var users = db.Users.ToList();

            var resetUsers = db.PasswordResetRequests
                .Where(x => x.IsProcessed == false)
                .Select(x => x.Username)
                .ToList();

            ViewBag.ResetUsers = resetUsers;

            return View(users);
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (Session["Role"] == null ||
                Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string username, string password, string role)
        {
            if (Session["Role"] == null ||
                Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            if (db.Users.Any(x => x.Username == username))
            {
                ViewBag.Error = "Username already exists.";
                return View();
            }
            string salt = PasswordHelper.GenerateSalt();
            string hash = PasswordHelper.HashPassword(password, salt);
            User user = new User
            {
                Username = username,
                PasswordSalt = salt,
                PasswordHash = hash,
                Role = role,
                IsActive = true,
                IsLocked = false,
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult ResetPassword(string username)
        {
            var user = db.Users.FirstOrDefault(x => x.Username == username);

            if (user != null)
            {
                string salt = PasswordHelper.GenerateSalt();
                string hash = PasswordHelper.HashPassword(username, salt);

                user.PasswordSalt = salt;
                user.PasswordHash = hash;

                user.IsLocked = false;
                user.FailedLoginAttempts = 0;

                var request = db.PasswordResetRequests
                    .FirstOrDefault(x => x.Username == username && x.IsProcessed == false);

                if (request != null)
                {
                    request.IsProcessed = true;
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}