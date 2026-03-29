using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WelfareLink.Data;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Services;

namespace WelfareLink.Controllers
{
    public class CitizenController : Controller
    {
        private readonly ICitizenService _citizenService;
        private readonly ICitizenDocumentService _documentService;
        private readonly IWelfareProgramService _programService;
        private readonly IWelfareApplicationService _applicationService;
        private readonly WelfareLinkDbContext _context;

        public CitizenController(
            ICitizenService citizenService,
            ICitizenDocumentService documentService,
            IWelfareProgramService programService,
            IWelfareApplicationService applicationService,
            WelfareLinkDbContext context)
        {
            _citizenService = citizenService;
            _documentService = documentService;
            _programService = programService;
            _applicationService = applicationService;
            _context = context;
        }

        // GET: Citizen/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);

            if (citizenProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var documents = await _documentService.GetDocumentsByCitizenIdAsync(citizenProfile.Id);

            var viewModel = new CitizenDashboardViewModel
            {
                CitizenProfile = citizenProfile,
                Documents = documents,
                PendingDocuments = documents.Count(d => d.VerificationStatus == "Pending"),
                ApprovedDocuments = documents.Count(d => d.VerificationStatus == "Approved"),
                RejectedDocuments = documents.Count(d => d.VerificationStatus == "Rejected")
            };

            return View(viewModel);
        }

        // GET: Citizen/CreateProfile
        public IActionResult CreateProfile()
        {
            return View(new CreateCitizenViewModelWithCredentials());
        }

        // POST: Citizen/CreateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(CreateCitizenViewModelWithCredentials model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            // Create User account first
            var user = new User
            {
                Username = model.Username,
                Password = model.Password,
                Role = "Citizen",
                FullName = model.Name,
                Email = model.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create Citizen profile
            var citizen = new Citizen
            {
                UserId = user.UserId,
                Name = model.Name,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                ContactInfo = model.ContactInfo,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            var success = await _citizenService.CreateCitizenProfileAsync(citizen);

            if (success)
            {
                // Update user with CitizenId
                user.CitizenId = citizen.Id;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile created successfully! Please login.";
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError(string.Empty, "Failed to create profile.");
            return View(model);
        }

        // GET: Citizen/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizen = await _citizenService.GetCitizenByUserIdAsync(userId.Value);

            if (citizen == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var viewModel = new EditCitizenViewModel
            {
                Id = citizen.Id,
                Name = citizen.Name,
                DateOfBirth = citizen.DateOfBirth,
                Address = citizen.Address,
                ContactInfo = citizen.ContactInfo,
                Status = citizen.Status
            };

            return View(viewModel);
        }

        // POST: Citizen/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditCitizenViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var citizen = await _citizenService.GetCitizenByIdAsync(model.Id);

            if (citizen == null)
            {
                ModelState.AddModelError(string.Empty, "Citizen profile not found.");
                return View(model);
            }

            citizen.Name = model.Name;
            citizen.DateOfBirth = model.DateOfBirth;
            citizen.Address = model.Address;
            citizen.ContactInfo = model.ContactInfo;
            citizen.Status = model.Status;

            var success = await _citizenService.UpdateCitizenProfileAsync(citizen);

            if (success)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError(string.Empty, "Failed to update profile.");
            return View(model);
        }

        // GET: Citizen/ApplicationDetails/5
        public async Task<IActionResult> ApplicationDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null)
                return RedirectToAction(nameof(CreateProfile));

            var application = await _applicationService.GetApplicationByIdAsync(id);
            if (application == null || application.CitizenID != citizenProfile.Id)
                return NotFound();

            return View(application);
        }

        // GET: Citizen/EditApplication/5
        public async Task<IActionResult> EditApplication(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null)
                return RedirectToAction(nameof(CreateProfile));

            var application = await _applicationService.GetApplicationByIdAsync(id);
            if (application == null || application.CitizenID != citizenProfile.Id)
                return NotFound();

            if (application.Status != "Pending" && application.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "This application cannot be edited in its current status.";
                return RedirectToAction(nameof(ApplicationDetails), new { id });
            }

            var programs = await _programService.GetAllProgramsAsync();
            ViewBag.ProgramList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(programs, "ProgramID", "Title", application.ProgramID);
            return View(application);
        }

        // POST: Citizen/EditApplication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditApplication(int id, [Bind("ApplicationID,CitizenID,ProgramID,SubmittedDate,Status")] WelfareApplication application)
        {
            if (id != application.ApplicationID)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null)
                return RedirectToAction(nameof(CreateProfile));

            var original = await _applicationService.GetApplicationByIdAsync(id);
            if (original == null || original.CitizenID != citizenProfile.Id)
                return NotFound();

            if (original.Status != "Pending" && original.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "This application cannot be edited in its current status.";
                return RedirectToAction(nameof(ApplicationDetails), new { id });
            }

            if (ModelState.IsValid)
            {
                // If was Rejected, re-submitting resets status to Pending
                application.CitizenID = citizenProfile.Id;
                application.Status = original.Status == "Rejected" ? "Pending" : original.Status;
                application.SubmittedDate = original.Status == "Rejected"
                    ? DateOnly.FromDateTime(DateTime.Today)
                    : original.SubmittedDate;

                await _applicationService.UpdateApplicationAsync(application);
                TempData["SuccessMessage"] = original.Status == "Rejected"
                    ? "Application re-submitted successfully. Status reset to Pending."
                    : "Application updated successfully.";
                return RedirectToAction(nameof(ApplicationDetails), new { id });
            }

            var programs = await _programService.GetAllProgramsAsync();
            ViewBag.ProgramList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(programs, "ProgramID", "Title", application.ProgramID);
            return View(application);
        }

        // GET: Citizen/DeleteApplication/5
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null)
                return RedirectToAction(nameof(CreateProfile));

            var application = await _applicationService.GetApplicationByIdAsync(id);
            if (application == null || application.CitizenID != citizenProfile.Id)
                return NotFound();

            if (application.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending applications can be deleted.";
                return RedirectToAction(nameof(ApplicationDetails), new { id });
            }

            return View(application);
        }

        // POST: Citizen/DeleteApplication/5
        [HttpPost, ActionName("DeleteApplication")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteApplicationConfirmed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null)
                return RedirectToAction(nameof(CreateProfile));

            var application = await _applicationService.GetApplicationByIdAsync(id);
            if (application == null || application.CitizenID != citizenProfile.Id)
                return NotFound();

            if (application.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending applications can be deleted.";
                return RedirectToAction(nameof(ApplicationDetails), new { id });
            }

            await _applicationService.DeleteApplicationAsync(id);
            TempData["SuccessMessage"] = $"Application #{id} deleted successfully.";
            return RedirectToAction(nameof(MyApplications));
        }

        // GET: Citizen/MyApplications
        public async Task<IActionResult> MyApplications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var all = await _applicationService.GetAllApplicationsAsync();
            var myApplications = all.Where(a => a.CitizenID == citizenProfile.Id).OrderByDescending(a => a.SubmittedDate);
            return View(myApplications);
        }

        // GET: Citizen/ProgramList
        public async Task<IActionResult> ProgramList()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var programs = await _programService.GetAllProgramsAsync();
            var applications = await _applicationService.GetAllApplicationsAsync();
            var appliedProgramIds = applications
                .Where(a => a.CitizenID == citizenProfile.Id)
                .Select(a => a.ProgramID)
                .ToHashSet();

            ViewBag.CitizenId = citizenProfile.Id;
            ViewBag.AppliedProgramIds = appliedProgramIds;
            return View(programs);
        }

        // POST: Citizen/ApplyForProgram
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyForProgram(int programId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
            {
                return RedirectToAction("Login", "Account");
            }

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            var application = new WelfareApplication
            {
                CitizenID = citizenProfile.Id,
                ProgramID = programId,
                SubmittedDate = DateOnly.FromDateTime(DateTime.Today),
                Status = "Pending"
            };

            var created = await _applicationService.CreateApplicationAsync(application);
            TempData["SuccessMessage"] = $"Application #{created.ApplicationID} submitted successfully!";
            return RedirectToAction(nameof(ProgramList));
        }
    }

    // View Models
    public class CitizenDashboardViewModel
    {
        public Citizen CitizenProfile { get; set; }
        public IEnumerable<CitizenDocument> Documents { get; set; }
        public int PendingDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int RejectedDocuments { get; set; }
    }

    public class CreateCitizenViewModelWithCredentials
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Contact Information (Phone/Email)")]
        public string ContactInfo { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
    }

    public class CreateCitizenViewModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Contact Information (Phone/Email)")]
        public string ContactInfo { get; set; }
    }

    public class EditCitizenViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Information")]
        public string ContactInfo { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
    }
}
