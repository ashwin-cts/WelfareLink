using Microsoft.AspNetCore.Authorization; // ADDED FOR AUTHORIZATION
using Microsoft.AspNetCore.Mvc;
using WelfareLink.CitizenManagement.API.Data;
using WelfareLink.CitizenManagement.API.DTOs;
using WelfareLink.CitizenManagement.API.Interfaces;
using WelfareLink.CitizenManagement.API.Models;


namespace WelfareLink.CitizenManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Base Rule: These roles can read citizen data (GET requests)
    [Authorize(Roles = "Admin,Citizen,WelfareOfficer,ComplianceOfficer,GovernmentAuditor")]
    public class CitizenApiController : ControllerBase
    {
        private readonly ICitizenService _citizenService;
        private readonly ICitizenDocumentService _documentService;
        private readonly IWelfareProgramService _programService;
        private readonly IWelfareApplicationService _applicationService;
        private readonly IWelfareApplicationDocumentService _appDocumentService;
        private readonly WelfareLinkDbContext _context;

        public CitizenApiController(
            ICitizenService citizenService,
            ICitizenDocumentService documentService,
            IWelfareProgramService programService,
            IWelfareApplicationService applicationService,
            IWelfareApplicationDocumentService appDocumentService,
            WelfareLinkDbContext context)
        {
            _citizenService = citizenService;
            _documentService = documentService;
            _programService = programService;
            _applicationService = applicationService;
            _appDocumentService = appDocumentService;
            _context = context;
        }

        // GET: api/citizenapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var citizen = await _citizenService.GetCitizenByIdAsync(id);
            if (citizen == null) return NotFound();
            return Ok(citizen);
        }

        // GET: api/citizenapi/by-user/{userId}
        // GET: api/citizenapi/by-user/{userId}
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            // 1. Fetch from the Service!
            var citizen = await _citizenService.GetCitizenByUserIdAsync(userId);
            if (citizen == null) return NotFound();

            // 2. Map it to include the Email
            return Ok(new
            {
                CitizenId = citizen.CitizenId,
                UserId = citizen.UserId,
                Username = citizen.User?.Username, //
                Name = citizen.Name,
                Email = citizen.User?.Email, // <-- Extracts the Email safely!
                DateOfBirth = citizen.DateOfBirth,
                Address = citizen.Address,
                ContactInfo = citizen.ContactInfo,
                Gender = citizen.Gender,
                Status = citizen.Status,
                CreatedAt = citizen.CreatedAt
            });
        }
      
   
        // GET: api/citizenapi/{citizenId}/dashboard
        [HttpGet("{citizenId}/dashboard")]
        public async Task<IActionResult> GetDashboard(int citizenId)
        {
            // 1. Fetch from the Service!
            var citizen = await _citizenService.GetCitizenByIdAsync(citizenId);
            if (citizen == null) return NotFound();

            var documents = await _documentService.GetDocumentsByCitizenIdAsync(citizenId);

            // 2. Map it to include the Email inside the dashboard
            return Ok(new
            {
                CitizenProfile = new
                {
                    CitizenId = citizen.CitizenId,
                    UserId = citizen.UserId,
                    Name = citizen.Name,
                    Email = citizen.User?.Email, // <-- Extracts the Email safely!
                    DateOfBirth = citizen.DateOfBirth,
                    Address = citizen.Address,
                    ContactInfo = citizen.ContactInfo,
                    Gender = citizen.Gender,
                    Status = citizen.Status,
                    CreatedAt = citizen.CreatedAt
                },
                Documents = documents,
                PendingDocuments = documents.Count(d => d.VerificationStatus == "Pending"),
                ApprovedDocuments = documents.Count(d => d.VerificationStatus == "Approved"),
                RejectedDocuments = documents.Count(d => d.VerificationStatus == "Rejected")
            });
        }

        // GET: api/citizenapi/application/{applicationId}
        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetApplicationDetails(int applicationId)
        {
            var application = await _applicationService.GetApplicationByIdAsync(applicationId);
            if (application == null) return NotFound();
            return Ok(application);
        }


        // POST: api/citizenapi
        [HttpPost]
        // OVERRIDE: Crucial! Users don't have a token yet when they are registering.
        [AllowAnonymous]
        public async Task<IActionResult> CreateProfile([FromBody] CreateCitizenRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (existingUser != null)
                return BadRequest(new { Error = "Username already exists." });
            //DTO transfer to user table when citizen reg
            var user = new User
            {
                Username = request.Username,
                Password = request.Password,
                Role = "Citizen",
                FullName = request.Name,
                Email = request.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var citizen = new Citizen
            {
                UserId = user.UserId,
                Name = request.Name,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                ContactInfo = request.ContactInfo,
                Gender = request.Gender,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            var success = await _citizenService.CreateCitizenProfileAsync(citizen);
            if (!success) return BadRequest(new { Error = "Failed to create citizen profile." });

            user.CitizenId = citizen.CitizenId;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Profile created successfully.", CitizenId = citizen.CitizenId });
        }

        // PUT: api/citizenapi/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Citizen")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateCitizenDto request) // <--- CHANGED TO DTO
        {
            if (id != request.CitizenId) return BadRequest(new { Error = "ID mismatch." });

            try
            {
                // Pass the DTO down to the service
                await _citizenService.UpdateCitizenProfileAsync(request);
                return Ok(new { Message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // PUT: api/citizenapi/application/{id}
        [HttpPut("application/{id}")]
        // OVERRIDE: Only the Citizen resubmits their application. Officers have their own endpoints for approval.
        [Authorize(Roles = "Citizen")]
        public async Task<IActionResult> UpdateApplication(int id, [FromBody] WelfareApplication application)
        {
            if (id != application.ApplicationID) return BadRequest(new { Error = "ID mismatch." });

            var original = await _applicationService.GetApplicationByIdAsync(id);
            if (original == null) return NotFound();

            if (original.Status != "Pending" && original.Status != "Rejected")
                return BadRequest(new { Error = "This application cannot be edited in its current status." });

            application.CitizenID = original.CitizenID;
            application.Status = original.Status == "Rejected" ? "Pending" : original.Status;
            application.SubmittedDate = original.Status == "Rejected"
                ? DateOnly.FromDateTime(DateTime.Today)
                : original.SubmittedDate;

            await _applicationService.UpdateApplicationAsync(application);
            return Ok(new { Message = original.Status == "Rejected" ? "Application re-submitted successfully." : "Application updated successfully." });
        }

        // GET: api/citizenapi/{citizenId}/applications
        [HttpGet("{citizenId}/applications")]
        public async Task<IActionResult> GetApplicationsByCitizenId(int citizenId)
        {
            var applications = await _applicationService.GetAllApplicationsAsync();
            var citizenApps = applications.Where(a => a.CitizenID == citizenId).ToList();
            return Ok(citizenApps);
        }

        // POST: api/citizenapi/apply
        [HttpPost("apply")]
        // OVERRIDE: Only Citizens can apply for programs.
        [Authorize(Roles = "Citizen")]
        public async Task<IActionResult> ApplyForProgram([FromBody] CitizenApplyRequest request)
        {
            if (request.CitizenID <= 0 || request.ProgramID <= 0)
                return BadRequest(new { Error = "CitizenID and ProgramID are required." });

            var program = await _programService.GetProgramByIdAsync(request.ProgramID);
            if (program == null)
                return NotFound(new { Error = "Program not found." });

            // Check that the program is still active
            if (!string.Equals(program.Status, "Active", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Error = "This program is no longer accepting applications." });

            // Check if citizen already applied for this program
            var existingApps = await _applicationService.GetAllApplicationsAsync();
            if (existingApps.Any(a => a.CitizenID == request.CitizenID && a.ProgramID == request.ProgramID))
                return BadRequest(new { Error = "You have already applied for this program." });

            var requiredDocs = program.RequiredDocuments ?? "None";
            bool noDocRequired = requiredDocs.Equals("None", StringComparison.OrdinalIgnoreCase);

            if (!noDocRequired)
            {
                var requiredDocTypes = requiredDocs
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim())
                    .ToList();

                if (!requiredDocTypes.Any())
                    return BadRequest(new { Error = "Program has no valid required document types configured." });

                // Validate selected documents are provided
                if (request.SelectedDocumentIds == null || !request.SelectedDocumentIds.Any())
                    return BadRequest(new { Error = $"This programme requires documents. Please select: {string.Join(", ", requiredDocTypes)}." });

                // Get the citizen's submitted documents (any status — officer will review)
                var citizenDocs = await _documentService.GetDocumentsByCitizenIdAsync(request.CitizenID);
                var selectedDocs = citizenDocs
                    .Where(d => request.SelectedDocumentIds.Contains(d.DocumentID))
                    .ToList();

                // Every required type must have at least one selected document
                var missingDocTypes = requiredDocTypes
                    .Where(rt => !selectedDocs.Any(d => d.DocType.Equals(rt, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (missingDocTypes.Any())
                    return BadRequest(new { Error = $"Please select at least one document for: {string.Join(", ", missingDocTypes)}." });
            }

            // Create the application
            var application = new WelfareApplication
            {
                CitizenID = request.CitizenID,
                ProgramID = request.ProgramID,
                SubmittedDate = DateOnly.FromDateTime(DateTime.Today),
                Status = "Pending"
            };

            var created = await _applicationService.CreateApplicationAsync(application);

            // Link selected documents to the application
            if (!noDocRequired && request.SelectedDocumentIds != null && request.SelectedDocumentIds.Any())
            {
                await _appDocumentService.AddApplicationDocumentsAsync(created.ApplicationID, request.SelectedDocumentIds);
            }

            return Ok(new
            {
                Message = $"Application #{created.ApplicationID} submitted successfully.",
                ApplicationID = created.ApplicationID
            });
        }
    }
}
//public class ChangePasswordRequest
//{
//    public string CurrentPassword { get; set; }
//    public string NewPassword { get; set; }
//}