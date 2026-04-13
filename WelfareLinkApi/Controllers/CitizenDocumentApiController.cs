using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitizenDocumentApiController : ControllerBase
    {
        private readonly ICitizenService _citizenService;
        private readonly ICitizenDocumentService _documentService;
        private readonly IWebHostEnvironment _environment;

        public CitizenDocumentApiController(
            ICitizenService citizenService,
            ICitizenDocumentService documentService,
            IWebHostEnvironment environment)
        {
            _citizenService = citizenService;
            _documentService = documentService;
            _environment = environment;
        }

        // GET: api/citizendocumentapi/citizen/{citizenId}
        [HttpGet("citizen/{citizenId}")]
        public async Task<IActionResult> GetByCitizenId(int citizenId, [FromQuery] string? status = null)
        {
            var documents = await _documentService.GetDocumentsByCitizenIdAsync(citizenId);

            if (!string.IsNullOrEmpty(status))
                documents = documents.Where(d => d.VerificationStatus.Equals(status, StringComparison.OrdinalIgnoreCase))
                                     .OrderByDescending(d => d.UploadedDate);
            else
                documents = documents.OrderByDescending(d => d.UploadedDate);

            return Ok(documents);
        }

        // GET: api/citizendocumentapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null) return NotFound();
            return Ok(document);
        }

        // POST: api/citizendocumentapi/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] int citizenId, [FromForm] string docType, [FromForm] string documentName, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Error = "Please provide a file to upload." });

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { Error = "Invalid file type. Allowed: PDF, JPG, JPEG, PNG, DOC, DOCX." });

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { Error = "File size cannot exceed 10 MB." });

            var document = new CitizenDocument
            {
                CitizenId = citizenId,
                DocType = docType,
                DocumentName = documentName
            };

            var success = await _documentService.UploadDocumentAsync(document, file);
            if (!success) return BadRequest(new { Error = "Failed to upload document." });

            return Ok(new { Message = "Document uploaded successfully.", DocumentId = document.DocumentID });
        }

        // PUT: api/citizendocumentapi/{id}/reupload
        [HttpPut("{id}/reupload")]
        public async Task<IActionResult> Reupload(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Error = "Please provide a file." });

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null) return NotFound();

            var success = await _documentService.ReuploadDocumentAsync(id, file);
            if (!success) return BadRequest(new { Error = "Failed to reupload document." });

            return Ok(new { Message = "Document reuploaded successfully." });
        }

        // PATCH: api/citizendocumentapi/{id}/verify
        [HttpPatch("{id}/verify")]
        public async Task<IActionResult> UpdateVerificationStatus(int id, [FromBody] string status)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null) return NotFound();

            var success = await _documentService.UpdateVerificationStatusAsync(id, status);
            if (!success) return BadRequest(new { Error = "Failed to update verification status." });

            return Ok(new { Message = $"Document #{id} verification status updated to {status}." });
        }

        // GET: api/citizendocumentapi/{id}/file
        [HttpGet("{id}/file")]
        public async Task<IActionResult> GetFile(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null || string.IsNullOrEmpty(document.FileURI))
                return NotFound(new { Error = "Document or file not found." });

            var webRoot = _environment.WebRootPath
                          ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

            var filePath = Path.Combine(
                webRoot,
                document.FileURI.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Error = "File not found on server." });

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };

            return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
        }

        // DELETE: api/citizendocumentapi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null) return NotFound();

            var success = await _documentService.DeleteDocumentAsync(id);
            if (!success) return BadRequest(new { Error = "Failed to delete document." });

            return Ok(new { Message = $"Document #{id} deleted successfully." });
        }
    }
}
