using Microsoft.AspNetCore.Mvc;
using WelfareLink.Models;
using WelfareLink.Services;
using WelfareLink.ViewModels;

namespace WelfareLink.Controllers
{
    public class CitizenController : Controller
    {
        private readonly WelfareApiClient _api;

        public CitizenController(WelfareApiClient api)
        {
            _api = api;
        }

        // GET: Citizen/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
                return RedirectToAction("Login", "Account");

            var data = await _api.GetCitizenDashboardAsync(HttpContext.Session.GetInt32("CitizenId") ?? 0);
            if (data?.CitizenProfile == null)
                return RedirectToAction(nameof(CreateProfile));

            var viewModel = new CitizenDashboardViewModel
            {
                CitizenProfile = data.CitizenProfile,
                Documents = data.Documents,
                PendingDocuments = data.PendingDocuments,
                ApprovedDocuments = data.ApprovedDocuments,
                RejectedDocuments = data.RejectedDocuments
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
            if (!ModelState.IsValid) return View(model);

            var response = await _api.CreateCitizenProfileAsync(model);
            if (response.success)
            {
                TempData["Success"] = "Profile created successfully! Please login.";
                return RedirectToAction("Login", "Account");
            }
            ModelState.AddModelError(string.Empty, response.error ?? "Failed to create profile.");
            return View(model);
        }

        // GET: Citizen/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Citizen")
                return RedirectToAction("Login", "Account");

            var citizen = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizen == null) return RedirectToAction(nameof(CreateProfile));

            return View(new EditCitizenViewModel
            {
                CitizenId = citizen.CitizenId,
                Name = citizen.Name,
                DateOfBirth = citizen.DateOfBirth,
                Address = citizen.Address,
                ContactInfo = citizen.ContactInfo,
                Status = citizen.Status,
                Gender = citizen.Gender
            });
        }

        // POST: Citizen/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditCitizenViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var citizen = await _api.GetCitizenByIdAsync(model.CitizenId);
            if (citizen == null) { ModelState.AddModelError(string.Empty, "Citizen profile not found."); return View(model); }

            citizen.Name = model.Name;
            citizen.DateOfBirth = model.DateOfBirth;
            citizen.Address = model.Address;
            citizen.ContactInfo = model.ContactInfo;
            citizen.Status = model.Status;
            citizen.Gender = model.Gender;

            var error = await _api.UpdateCitizenProfileAsync(citizen);
            if (error == null)
            {
                if (!string.IsNullOrEmpty(model.Gender))
                    HttpContext.Session.SetString("CitizenGender", model.Gender);
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Dashboard));
            }
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        // GET: Citizen/ApplicationDetails/5
        public async Task<IActionResult> ApplicationDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Citizen") return RedirectToAction("Login", "Account");

            var citizenProfile = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null) return RedirectToAction(nameof(CreateProfile));

            var application = await _api.GetApplicationByIdAsync(id);
            if (application == null || application.CitizenID != citizenProfile.CitizenId) return NotFound();

            return View(application);
        }

        // GET: Citizen/EditApplication/5
        public async Task<IActionResult> EditApplication(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Citizen") return RedirectToAction("Login", "Account");

            var citizenProfile = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null) return RedirectToAction(nameof(CreateProfile));

            var application = await _api.GetApplicationByIdAsync(id);
            if (application == null || application.CitizenID != citizenProfile.CitizenId) return NotFound();

            if (application.Status != "Pending" && application.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "This application cannot be edited in its current status.";
                return RedirectToAction(nameof(ApplicationDetails), new { id });
            }

            var programs = await _api.GetAllProgramsAsync();
            ViewBag.ProgramList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(programs, "ProgramID", "Title", application.ProgramID);
            return View(application);
        }

        // POST: Citizen/EditApplication/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditApplication(int id, [Bind("ApplicationID,CitizenID,ProgramID,SubmittedDate,Status")] WelfareApplication application)
        {
            if (id != application.ApplicationID) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Citizen") return RedirectToAction("Login", "Account");

            var citizenProfile = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null) return RedirectToAction(nameof(CreateProfile));

            if (ModelState.IsValid)
            {
                var error = await _api.UpdateCitizenApplicationAsync(application);
                if (error == null)
                {
                    TempData["SuccessMessage"] = "Application updated successfully.";
                    return RedirectToAction(nameof(ApplicationDetails), new { id });
                }
                ModelState.AddModelError(string.Empty, error);
            }

            var programs = await _api.GetAllProgramsAsync();
            ViewBag.ProgramList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(programs, "ProgramID", "Title", application.ProgramID);
            return View(application);
        }

        // GET: Citizen/DeleteApplication/5
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Citizen") return RedirectToAction("Login", "Account");

            var citizenProfile = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizenProfile == null) return RedirectToAction(nameof(CreateProfile));

            var application = await _api.GetApplicationByIdAsync(id);
            if (application == null || application.CitizenID != citizenProfile.CitizenId) return NotFound();

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
            if (userId == null || userRole != "Citizen") return RedirectToAction("Login", "Account");

            await _api.DeleteApplicationAsync(id);
            TempData["SuccessMessage"] = "Application deleted successfully.";
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Citizen/MyApplications
        public async Task<IActionResult> MyApplications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Citizen") return RedirectToAction("Login", "Account");

            var citizenId = HttpContext.Session.GetInt32("CitizenId") ?? 0;
            if (citizenId == 0) return RedirectToAction(nameof(CreateProfile));

            var applications = await _api.GetApplicationsByCitizenIdAsync(citizenId);
            return View(applications);
        }

        // GET: Citizen/ProgramList
        public async Task<IActionResult> ProgramList()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Citizen") return RedirectToAction("Login", "Account");

            var citizenId = HttpContext.Session.GetInt32("CitizenId") ?? 0;
            if (citizenId == 0) return RedirectToAction(nameof(CreateProfile));

            var programs = await _api.GetAllProgramsAsync();
            var activePrograms = programs.Where(p => string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase));

            var applications = await _api.GetApplicationsByCitizenIdAsync(citizenId);
            var appliedProgramIds = new HashSet<int>(applications.Select(a => a.ProgramID));

            var citizenGender = HttpContext.Session.GetString("CitizenGender") ?? "";

            ViewBag.AppliedProgramIds = appliedProgramIds;
            ViewBag.CitizenId = citizenId;
            ViewBag.CitizenGender = citizenGender;

            return View(activePrograms);
        }

        // GET: Citizen/SelectDocuments?programId=5
        public async Task<IActionResult> SelectDocuments(int programId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Citizen") return RedirectToAction("Login", "Account");

            var citizenId = HttpContext.Session.GetInt32("CitizenId") ?? 0;
            if (citizenId == 0) return RedirectToAction(nameof(CreateProfile));

            var programs = await _api.GetAllProgramsAsync();
            var program = programs.FirstOrDefault(p => p.ProgramID == programId);
            if (program == null) return NotFound();

            var requiredDocs = program.RequiredDocuments ?? "None";
            bool noDocRequired = requiredDocs.Equals("None", StringComparison.OrdinalIgnoreCase);
            var requiredDocTypes = noDocRequired
                ? new List<string>()
                : requiredDocs.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).ToList();

            // Show all documents so the citizen can select any to submit with the application
            var documents = await _api.GetDocumentsByCitizenIdAsync(citizenId);

            ViewBag.Program = program;
            ViewBag.Documents = documents;
            ViewBag.ProgramId = programId;
            ViewBag.RequiredDocTypes = requiredDocTypes;
            ViewBag.NoDocRequired = noDocRequired;

            return View();
        }

        // POST: Citizen/SelectDocuments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectDocuments(int programId, IEnumerable<int> selectedDocumentIds)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Citizen") return RedirectToAction("Login", "Account");

            var citizenId = HttpContext.Session.GetInt32("CitizenId") ?? 0;
            if (citizenId == 0) return RedirectToAction(nameof(CreateProfile));

            var docIds = selectedDocumentIds?.ToArray() ?? [];

            var (success, error) = await _api.ApplyForProgramAsync(citizenId, programId, docIds);
            if (success)
            {
                TempData["SuccessMessage"] = "Your application has been submitted successfully! It is now pending review.";
                return RedirectToAction(nameof(MyApplications));
            }

            // Re-populate the view on error
            var programs = await _api.GetAllProgramsAsync();
            var program = programs.FirstOrDefault(p => p.ProgramID == programId);
            var requiredDocs = program?.RequiredDocuments ?? "None";
            bool noDocRequired = requiredDocs.Equals("None", StringComparison.OrdinalIgnoreCase);
            var requiredDocTypes = noDocRequired
                ? new List<string>()
                : requiredDocs.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).ToList();

            var documents = await _api.GetDocumentsByCitizenIdAsync(citizenId);

            ViewBag.Program = program;
            ViewBag.Documents = documents;
            ViewBag.ProgramId = programId;
            ViewBag.RequiredDocTypes = requiredDocTypes;
            ViewBag.NoDocRequired = noDocRequired;
            ViewBag.Error = error;

            return View();
        }
    }
}
