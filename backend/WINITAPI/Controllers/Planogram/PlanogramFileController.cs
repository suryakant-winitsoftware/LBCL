using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Winit.Shared.CommonUtilities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WINITAPI.Controllers.Planogram
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlanogramFileController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PlanogramFileController> _logger;

        public PlanogramFileController(
            IWebHostEnvironment environment,
            ILogger<PlanogramFileController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("UploadImage/{planogramSetupUid}")]
        public async Task<IActionResult> UploadPlanogramImage(
            string planogramSetupUid,
            [Required] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { IsSuccess = false, Message = "No file uploaded" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Only image files are allowed" });
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { IsSuccess = false, Message = "File size must be less than 5MB" });
                }

                // Generate unique file name
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"planogram_{planogramSetupUid}_{DateTime.Now.Ticks}{fileExtension}";
                
                // Create directory if it doesn't exist
                var uploadDir = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "Data", "planogram-photos");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var filePath = Path.Combine(uploadDir, uniqueFileName);
                var relativePath = Path.Combine("Data", "planogram-photos", uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create file_sys record
                var fileUID = $"FS_PLANOGRAM_{DateTime.Now.Ticks}_{GenerateRandomString(7)}";
                var currentUser = GetCurrentUser();
                
                var insertQuery = @"
                    INSERT INTO file_sys (
                        uid, created_by, created_time, modified_by, modified_time,
                        linked_item_type, linked_item_uid, file_sys_type, file_type,
                        is_directory, file_name, display_name, file_size, relative_path,
                        created_by_job_position_uid, created_by_emp_uid, is_default, ss
                    ) VALUES (
                        @uid, @created_by, @created_time, @modified_by, @modified_time,
                        @linked_item_type, @linked_item_uid, @file_sys_type, @file_type,
                        @is_directory, @file_name, @display_name, @file_size, @relative_path,
                        @created_by_job_position_uid, @created_by_emp_uid, @is_default, @ss
                    )";

                // Note: You'll need to implement database insertion here using your data layer
                // This is a placeholder for the database operation
                
                var fileRecord = new
                {
                    uid = fileUID,
                    created_by = currentUser,
                    created_time = DateTime.Now,
                    modified_by = currentUser,
                    modified_time = DateTime.Now,
                    linked_item_type = "planogram_image",
                    linked_item_uid = planogramSetupUid,
                    file_sys_type = "PLANOGRAM_IMAGE",
                    file_type = file.ContentType,
                    is_directory = false,
                    file_name = uniqueFileName,
                    display_name = file.FileName,
                    file_size = file.Length,
                    relative_path = relativePath.Replace("\\", "/"),
                    created_by_job_position_uid = currentUser,
                    created_by_emp_uid = currentUser,
                    is_default = false,
                    ss = 1
                };

                return Ok(new
                {
                    IsSuccess = true,
                    Data = new
                    {
                        FileUID = fileUID,
                        FileName = uniqueFileName,
                        DisplayName = file.FileName,
                        RelativePath = relativePath.Replace("\\", "/"),
                        FileSize = file.Length,
                        ContentType = file.ContentType
                    },
                    Message = "File uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading planogram image");
                return StatusCode(500, new { IsSuccess = false, Message = "Internal server error" });
            }
        }

        [HttpGet("GetImages/{planogramSetupUid}")]
        public async Task<IActionResult> GetPlanogramImages(string planogramSetupUid)
        {
            try
            {
                // Note: Implement database query to get files for this planogram
                // This is a placeholder - you'll need to implement with your data layer
                
                var query = @"
                    SELECT uid, file_name, display_name, relative_path, file_size, 
                           file_type, created_time, created_by
                    FROM file_sys 
                    WHERE linked_item_type = 'planogram_image' 
                      AND linked_item_uid = @planogramSetupUid 
                      AND (ss = 0 OR ss IS NULL)
                    ORDER BY created_time DESC";

                // Placeholder for database query result
                var images = new List<object>();

                return Ok(new
                {
                    IsSuccess = true,
                    Data = images
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting planogram images");
                return StatusCode(500, new { IsSuccess = false, Message = "Internal server error" });
            }
        }

        [HttpDelete("DeleteImage/{fileUid}")]
        public async Task<IActionResult> DeletePlanogramImage(string fileUid)
        {
            try
            {
                // Note: Implement database operations to soft delete the file record
                // and optionally delete the physical file
                
                var updateQuery = @"
                    UPDATE file_sys 
                    SET ss = 1, 
                        modified_time = @modified_time,
                        modified_by = @modified_by
                    WHERE uid = @uid";

                return Ok(new
                {
                    IsSuccess = true,
                    Message = "Image deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting planogram image");
                return StatusCode(500, new { IsSuccess = false, Message = "Internal server error" });
            }
        }

        [HttpGet("Download/{fileUid}")]
        public async Task<IActionResult> DownloadPlanogramImage(string fileUid)
        {
            try
            {
                // Note: Implement database query to get file info
                // Then return the file for download
                
                // Placeholder for getting file info from database
                var fileInfo = new { relative_path = "", file_name = "", file_type = "" };
                
                var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, fileInfo.relative_path);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { IsSuccess = false, Message = "File not found" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, fileInfo.file_type, fileInfo.file_name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading planogram image");
                return StatusCode(500, new { IsSuccess = false, Message = "Internal server error" });
            }
        }

        private string GetCurrentUser()
        {
            // Get current user from JWT token or session
            return User?.Identity?.Name ?? "SYSTEM";
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}