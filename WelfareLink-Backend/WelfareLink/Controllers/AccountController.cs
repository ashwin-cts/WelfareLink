using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WelfareLink.Models;
using WelfareLink.Services;
using WelfareLink.ViewModels;

namespace WelfareLink.Controllers
{
    public class AccountController : Controller
    {
        private readonly WelfareApiClient _apiClient;

        public AccountController(WelfareApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(userRole))
            {
                return RedirectBasedOnRole(userRole);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string userType)
        {
            var (user, error) = await _apiClient.LoginAsync(username, password, userType);

            if (user == null)
            {
                TempData["Error"] = error ?? "Invalid username or password";
                return RedirectToAction("Login");
            }

            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);

            if (user.CitizenId.HasValue)
            {
                HttpContext.Session.SetInt32("CitizenId", user.CitizenId.Value);
                var citizen = await _apiClient.GetCitizenByIdAsync(user.CitizenId.Value);
                if (!string.IsNullOrEmpty(citizen?.Gender))
                    HttpContext.Session.SetString("CitizenGender", citizen.Gender);
            }

            return RedirectBasedOnRole(user.Role);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult CheckSession()
        {
            var isLoggedIn = HttpContext.Session.GetString("UserRole") != null;
            return Json(new { expired = !isLoggedIn });
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var (user, error) = await _apiClient.GetUserAsync(userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            var model = new EditProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var (user, error) = await _apiClient.UpdateProfileAsync(userId.Value, model.FullName, model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Failed to update profile.");
                return View(model);
            }

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
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var (success, error) = await _apiClient.ChangePasswordAsync(userId.Value, model.CurrentPassword, model.NewPassword);
            if (!success)
            {
                ModelState.AddModelError("CurrentPassword", error ?? "Failed to change password.");
                return View(model);
            }

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
                "Auditor" => RedirectToAction("Dashboard", "Auditor"),
                "GovernmentAuditor" => RedirectToAction("Dashboard", "Auditor"),
                _ => RedirectToAction("Login", "Account")
            };
        }
    }
}
