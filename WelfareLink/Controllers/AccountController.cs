using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WelfareLink.Data;
using WelfareLink.Models;
using WelfareLink.ViewModels;

namespace WelfareLink.Controllers
{
    public class AccountController : Controller
    {
        private readonly WelfareLinkDbContext _context;

        public AccountController(WelfareLinkDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect based on role
            var userRole = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(userRole))
            {
                return RedirectBasedOnRole(userRole);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password, string userType)
        {
            var user = _context.Users.FirstOrDefault(u => 
                u.Username == username && 
                u.Password == password && 
                u.Role == userType );
            if (user == null)
            {
                TempData["Error"] = "Invalid username or password";
                return RedirectToAction("Login");
            }

            if (!user.IsActive)
            {
                TempData["Error"] = "Your account is blocked. Please contact Admin.";
                return RedirectToAction("Login");

            }
            
            if (user != null)
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);

                if (user.CitizenId.HasValue)
                {
                    HttpContext.Session.SetInt32("CitizenId", user.CitizenId.Value);
                    var citizen = _context.Citizens.FirstOrDefault(c => c.CitizenId == user.CitizenId.Value);
                    if (!string.IsNullOrEmpty(citizen?.Gender))
                        HttpContext.Session.SetString("CitizenGender", citizen.Gender);
                }

                return RedirectBasedOnRole(user.Role);
            }

            TempData["Error"] = "Unknown Error";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            // Prevent browser from caching authenticated pages
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("Login");
        }

        // Used by the client-side session timer to verify session is still alive
        [HttpGet]
        public IActionResult CheckSession()
        {
            var isLoggedIn = HttpContext.Session.GetString("UserRole") != null;
            return Json(new { expired = !isLoggedIn });
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            var model = new EditProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email    = user.Email    ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            user.FullName = model.FullName;
            user.Email    = model.Email;
            _context.SaveChanges();

            HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("EditProfile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login");
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            if (user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            user.Password = model.NewPassword;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("ChangePassword");
        }

        private IActionResult RedirectBasedOnRole(string role)
        {
            return role switch
            {
                "Citizen" => RedirectToAction("Dashboard", "Citizen"),
                "WelfareOfficer" => RedirectToAction("HomeIndex", "WelfareApplication"),
                "WelfareManager" => RedirectToAction("Dashboard", "WelfareProgram"),
                "ProgramManager" => RedirectToAction("Dashboard", "WelfareProgram"),
                "Admin" => RedirectToAction("Index", "Admin"),
                "ComplianceOfficer" => RedirectToAction("Dashboard", "ComplianceOfficer"),
                "GovernmentAuditor" => RedirectToAction("Dashboard", "Audit"),
                _ => RedirectToAction("Login", "Account")
            };
        }
    }
}
