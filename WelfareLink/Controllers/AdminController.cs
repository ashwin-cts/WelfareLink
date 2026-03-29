using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Models;

namespace WelfareLink.Controllers
{
    public class AdminController : Controller
    {
        private readonly WelfareLinkDbContext _context;

        public AdminController(WelfareLinkDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var users = await _context.Users.Include(u => u.Citizen).ToListAsync();
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
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "User blocked successfully";
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

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "User unblocked successfully";
            }

            return RedirectToAction("Index");
        }
    }
}
