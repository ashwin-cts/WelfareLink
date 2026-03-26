using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Models;
using WelfareLink.Services;

namespace WelfareLink.Controllers
{
    public class CitizenController : Controller
    {
        private readonly ICitizenService _citizenService;
        private readonly ICitizenDocumentService _documentService;

        public CitizenController(ICitizenService citizenService, ICitizenDocumentService documentService)
        {
            _citizenService = citizenService;
            _documentService = documentService;
        }

        // GET: Citizen/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // For demo purposes, using a hardcoded UserId = 1
            // In production, get this from authenticated user claims
            int currentUserId = 1;

            var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(currentUserId);

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
            return View();
        }

        // POST: Citizen/CreateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(CreateCitizenViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // For demo purposes, using a hardcoded UserId = 1
            int currentUserId = 1;

            var citizen = new Citizen
            {
                UserId = currentUserId,
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
                TempData["SuccessMessage"] = "Profile created successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError(string.Empty, "Failed to create profile.");
            return View(model);
        }

        // GET: Citizen/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            // For demo purposes, using a hardcoded UserId = 1
            int currentUserId = 1;

            var citizen = await _citizenService.GetCitizenByUserIdAsync(currentUserId);

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
