using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChatBackend.Services;
using ChatBackend.DTOs;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FilesController> _logger;
        
        public FilesController(IFileService fileService, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }
        
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<FileUploadDto>> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");
                    
                // Validate file size (max 50MB)
                const long maxFileSize = 50 * 1024 * 1024; // 50MB
                if (file.Length > maxFileSize)
                    return BadRequest("File size exceeds maximum limit of 50MB");
                    
                // Validate file type (you can customize this based on your requirements)
                var allowedTypes = new[]
                {
                    "image/jpeg", "image/png", "image/gif", "image/webp",
                    "application/pdf", "text/plain", "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.ms-excel",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
                
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return BadRequest("File type not allowed");
                
                var uploadResult = await _fileService.UploadFileAsync(file);
                return Ok(uploadResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return BadRequest("File URL is required");
                    
                var result = await _fileService.DeleteFileAsync(fileUrl);
                if (!result)
                    return BadRequest("Failed to delete file");
                    
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpGet("presigned-url")]
        public async Task<ActionResult<string>> GetPresignedUrl([FromQuery] string fileKey, [FromQuery] int expirationMinutes = 60)
        {
            try
            {
                if (string.IsNullOrEmpty(fileKey))
                    return BadRequest("File key is required");
                    
                var expiration = TimeSpan.FromMinutes(expirationMinutes);
                var presignedUrl = await _fileService.GetPresignedUrlAsync(fileKey, expiration);
                
                return Ok(new { PresignedUrl = presignedUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating presigned URL");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
