using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Models;
using WelfareLink.Services;

namespace WelfareLink.Controllers
{
    public class AdminController : Controller
    {
        private readonly WelfareLinkDbContext _context;
        private readonly WelfareApiClient _apiClient;

        public AdminController(WelfareLinkDbContext context, WelfareApiClient apiClient)
        {
            _context = context;
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var users = await _context.Users
                .Include(u => u.Citizen)
                .Where(u => u.UserId != currentUserId)
                .ToListAsync();

            ViewBag.AdminCount = await _context.Users.CountAsync(u => u.Role == "Admin" && u.IsActive);
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateOfficer()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOfficer(User user)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.Role == "Citizen")
            {
                ModelState.AddModelError("", "Cannot create citizen through this form");
                return View(user);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(user);
            }

            user.IsActive = true;
            user.CreatedAt = DateTime.Now;

            // Call API to create user and log audit trail
            var (createdUser, error) = await _apiClient.CreateUserAsync(user);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(user);
            }

            TempData["Success"] = "Officer created successfully";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(User user)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(user);
            }

            user.Role = "Admin";
            user.IsActive = true;
            user.CreatedAt = DateTime.Now;

            // Call API to create user and log audit trail
            var (createdUser, error) = await _apiClient.CreateUserAsync(user);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(user);
            }

            TempData["Success"] = "Admin created successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(int userId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (userId == currentUserId)
            {
                TempData["Error"] = "You cannot block your own account.";
                return RedirectToAction("Index");
            }

            var (success, error) = await _apiClient.BlockUserAsync(userId);
            if (success)
            {
                TempData["Success"] = "User blocked successfully";
            }
            else
            {
                TempData["Error"] = error ?? "Failed to block user";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(int userId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var (success, error) = await _apiClient.UnblockUserAsync(userId);
            if (success)
            {
                TempData["Success"] = "User unblocked successfully";
            }
            else
            {
                TempData["Error"] = error ?? "Failed to unblock user";
            }

            return RedirectToAction("Index");
        }
    }
}
