using Microsoft.AspNetCore.Mvc;
using WelfareLink.Models;
using WelfareLink.Services;
using WelfareLink.ViewModels;

namespace WelfareLink.Controllers
{
    public class CitizenDocumentController : Controller
    {
        private readonly WelfareApiClient _api;

        public CitizenDocumentController(WelfareApiClient api)
        {
            _api = api;
        }

        // GET: CitizenDocument/ViewDocument/5
        public async Task<IActionResult> ViewDocument(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var (bytes, contentType, _) = await _api.GetDocumentFileAsync(id);
            if (bytes == null) return NotFound();

            return File(bytes, contentType!);
        }

        // GET: CitizenDocument/UploadDocument
        public async Task<IActionResult> UploadDocument()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var citizen = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizen == null) return RedirectToAction("CreateProfile", "Citizen");

            return View(new DocumentUploadViewModel { CitizenId = citizen.CitizenId });
        }

        // POST: CitizenDocument/UploadDocument
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(DocumentUploadViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var citizen = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizen == null) return RedirectToAction("CreateProfile", "Citizen");

            model.CitizenId = citizen.CitizenId;

            if (!ModelState.IsValid) return View(model);

            if (model.FileUpload == null || model.FileUpload.Length == 0)
            {
                ModelState.AddModelError("FileUpload", "Please select a file to upload.");
                return View(model);
            }

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            var ext = Path.GetExtension(model.FileUpload.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("FileUpload", "Invalid file type. Allowed: PDF, JPG, JPEG, PNG, DOC, DOCX.");
                return View(model);
            }
            if (model.FileUpload.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("FileUpload", "File size cannot exceed 10 MB.");
                return View(model);
            }

            var (success, error) = await _api.UploadDocumentAsync(citizen.CitizenId, model.DocType, model.DocumentName, model.FileUpload);
            if (success)
            {
                TempData["SuccessMessage"] = "Document uploaded successfully!";
                return RedirectToAction(nameof(DocumentStatus));
            }
            ModelState.AddModelError(string.Empty, error ?? "Failed to upload document.");
            return View(model);
        }

        // GET: CitizenDocument/DocumentStatus
        public async Task<IActionResult> DocumentStatus(string status = "")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var citizen = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizen == null) return RedirectToAction("CreateProfile", "Citizen");

            var documents = await _api.GetDocumentsByCitizenIdAsync(citizen.CitizenId, string.IsNullOrEmpty(status) ? null : status);

            return View(new DocumentStatusViewModel { Documents = documents, FilterStatus = status });
        }

        // POST: CitizenDocument/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int documentId)
        {
            var success = await _api.DeleteDocumentAsync(documentId);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Document deleted successfully!" : "Failed to delete document.";
            return RedirectToAction(nameof(DocumentStatus));
        }

        // GET: CitizenDocument/Reupload/5
        public async Task<IActionResult> Reupload(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var citizen = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizen == null) return RedirectToAction("CreateProfile", "Citizen");

            var document = await _api.GetDocumentByIdAsync(id);
            if (document == null || document.CitizenId != citizen.CitizenId) return NotFound();

            return View(new ReuploadDocumentViewModel
            {
                DocumentID = document.DocumentID,
                DocumentName = document.DocumentName ?? document.DocType,
                DocType = document.DocType,
                CurrentFileURI = document.FileURI,
                VerificationStatus = document.VerificationStatus
            });
        }

        // POST: CitizenDocument/Reupload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reupload(ReuploadDocumentViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var citizen = await _api.GetCitizenByUserIdAsync(userId.Value);
            if (citizen == null) return RedirectToAction("CreateProfile", "Citizen");

            var document = await _api.GetDocumentByIdAsync(model.DocumentID);
            if (document == null || document.CitizenId != citizen.CitizenId) return NotFound();

            if (!ModelState.IsValid) return View(model);

            if (model.FileUpload == null || model.FileUpload.Length == 0)
            {
                ModelState.AddModelError("FileUpload", "Please select a file.");
                return View(model);
            }

            var (success, error) = await _api.ReuploadDocumentAsync(model.DocumentID, model.FileUpload);
            if (success)
            {
                TempData["SuccessMessage"] = "Document reuploaded successfully!";
                return RedirectToAction(nameof(DocumentStatus));
            }
            ModelState.AddModelError(string.Empty, error ?? "Failed to reupload document.");
            return View(model);
        }
    }
}
