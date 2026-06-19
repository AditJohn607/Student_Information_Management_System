using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using StudentInfoAdit.Models;
using StudentInfoAdit.Helpers;

namespace StudentInfoAdit.Controllers
{
    public class AccountController : Controller
    {
        private StudentDBContext db = new StudentDBContext();
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {                                                                                                           
                ModelState.AddModelError("", "Username and password are required");
                return View();
            }
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)                                                                                                      
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Account is inactive");
                return View();
            }
           
            if (user.IsLocked)
            {
                ModelState.AddModelError("", "Account locked due to multiple failed attempts");
                return View();
            } 

            bool isValid = PasswordHelper.VerifyPassword(
                password,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (isValid)
            {   
                user.FailedLoginAttempts = 0;
                user.LastLogin = DateTime.Now;
                db.SaveChanges();
                                                                                                                                                                          
                FormsAuthentication.SetAuthCookie(user.Username, false);

                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;
                Session["Role"] = user.Role;
                Session["ERPMode"] = false;
                return RedirectToAction("Index", "Dashboard");
            }

            else
            {
                user.FailedLoginAttempts++;                    

                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                }
                
                db.SaveChanges();   
                 
                ModelState.AddModelError("", "Invalid username or password");      
                return View();
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();  
            Session.Clear();              
            Session.Abandon();            
            return RedirectToAction("Login", "Account");   
        }
  
       /* public ActionResult CreateAdmin()
        { 
            string password = "admin123";
            string salt = PasswordHelper.GenerateSalt();
            string hash = PasswordHelper.HashPassword(password, salt);
            var user = new User
            {
                Username = "aditjohn",
                PasswordSalt = salt,
                PasswordHash = hash,
                Role = "Admin",
                IsActive = true,
                FailedLoginAttempts = 0,
                IsLocked = false,
                CreatedAt = DateTime.Now,
            };
 
            db.Users.Add(user);
            db.SaveChanges();

            return Content("Admin created successfully");
        }*/

        public ActionResult GeneratePassword()
        {
            string salt = PasswordHelper.GenerateSalt();
            string hash = PasswordHelper.HashPassword("admin123", salt);
            return Content(
                "Salt: " + salt +
                "<br/><br/>Hash: " + hash
            );
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = Convert.ToInt32(Session["UserId"]);

            var user = db.Users.FirstOrDefault(x => x.UserId == userId);

            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            bool isValid = PasswordHelper.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt);

            if (!isValid)
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "New password and confirm password do not match.";
                return View();
            }

            string newSalt = PasswordHelper.GenerateSalt();
            string newHash = PasswordHelper.HashPassword(newPassword, newSalt);

            user.PasswordSalt = newSalt;
            user.PasswordHash = newHash;

            user.FailedLoginAttempts = 0;
            user.IsLocked = false;

            db.SaveChanges();

            ViewBag.Message = "Password updated successfully.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult ForgotPassword(string username)
        {
            var user = db.Users
                         .FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                ViewBag.Error = "Username not found";
                return View();
            }

            bool requestExists =
                db.PasswordResetRequests.Any(x =>
                    x.Username == username &&
                    x.IsProcessed == false);

            if (!requestExists)
            {
                PasswordResetRequestModel request =
                    new PasswordResetRequestModel();

                request.Username = username;
                request.RequestDate = DateTime.Now;
                request.IsProcessed = false;

                db.PasswordResetRequests.Add(request);
                db.SaveChanges();
            }

            ViewBag.Message =
                "Password reset request submitted.";

            return View();
        }
    }
}