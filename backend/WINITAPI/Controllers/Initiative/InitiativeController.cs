using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using Winit.Modules.Initiative.BL.Interfaces;
using Winit.Modules.Initiative.Model.Classes;
using Winit.Shared.Models.Common;
using WINITAPI.Controllers;

namespace WINITAPI.Controllers.Initiative
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InitiativeController : WINITBaseController
    {
        private readonly IInitiativeBL _initiativeBL;
        private readonly ILogger<InitiativeController> _logger;

        public InitiativeController(
            IServiceProvider serviceProvider,
            IInitiativeBL initiativeBL,
            ILogger<InitiativeController> logger) : base(serviceProvider)
        {
            _initiativeBL = initiativeBL;
            _logger = logger;
        }

        #region Initiative CRUD Operations

        /// <summary>
        /// Get initiative by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InitiativeDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInitiativeById(int id)
        {
            try
            {
                var initiative = await _initiativeBL.GetInitiativeByIdAsync(id);
                return Ok(initiative);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Initiative not found: {InitiativeId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting initiative {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the initiative" });
            }
        }

        /// <summary>
        /// Get all initiatives with pagination and filtering
        /// </summary>
        [HttpPost("search")]
        [ProducesResponseType(typeof(PagedResponse<InitiativeDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchInitiatives([FromBody] InitiativeSearchRequest request)
        {
            try
            {
                if (request == null)
                {
                    request = new InitiativeSearchRequest();
                }

                var initiatives = await _initiativeBL.GetInitiativesAsync(request);
                return Ok(initiatives);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching initiatives");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while searching initiatives" });
            }
        }

        /// <summary>
        /// Create new initiative
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(InitiativeDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateInitiative([FromBody] CreateInitiativeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Invalid request data" });
                }

                var userCode = GetUserCode();
                var initiative = await _initiativeBL.CreateInitiativeAsync(request, userCode);
                
                return CreatedAtAction(
                    nameof(GetInitiativeById), 
                    new { id = initiative.InitiativeId }, 
                    initiative);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating initiative");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating initiative");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the initiative" });
            }
        }

        /// <summary>
        /// Update existing initiative
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(InitiativeDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateInitiative(int id, [FromBody] CreateInitiativeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Invalid request data" });
                }

                var userCode = GetUserCode();
                var initiative = await _initiativeBL.UpdateInitiativeAsync(id, request, userCode);
                return Ok(initiative);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Initiative not found for update: {InitiativeId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation updating initiative {InitiativeId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating initiative {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the initiative" });
            }
        }

        /// <summary>
        /// Delete initiative
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteInitiative(int id)
        {
            try
            {
                var userCode = GetUserCode();
                var success = await _initiativeBL.DeleteInitiativeAsync(id, userCode);
                
                if (success)
                {
                    return NoContent();
                }
                
                return NotFound(new { message = "Initiative not found or cannot be deleted" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete initiative {InitiativeId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting initiative {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the initiative" });
            }
        }

        #endregion

        #region Initiative Actions

        /// <summary>
        /// Submit initiative for approval
        /// </summary>
        [HttpPost("{id}/submit")]
        [ProducesResponseType(typeof(InitiativeDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitInitiative(int id)
        {
            try
            {
                var userCode = GetUserCode();
                var initiative = await _initiativeBL.SubmitInitiativeAsync(id, userCode);
                return Ok(initiative);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Initiative not found for submit: {InitiativeId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot submit initiative {InitiativeId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting initiative {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while submitting the initiative" });
            }
        }

        /// <summary>
        /// Cancel initiative
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelInitiative(int id, [FromBody] CancelInitiativeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.CancelReason))
                {
                    return BadRequest(new { message = "Cancel reason is required" });
                }

                var userCode = GetUserCode();
                var success = await _initiativeBL.CancelInitiativeAsync(id, request.CancelReason, userCode);
                
                if (success)
                {
                    return Ok(new { message = "Initiative cancelled successfully" });
                }
                
                return NotFound(new { message = "Initiative not found or cannot be cancelled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling initiative {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while cancelling the initiative" });
            }
        }

        /// <summary>
        /// Save initiative as draft
        /// </summary>
        [HttpPost("save-draft")]
        [ProducesResponseType(typeof(InitiativeDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveDraft([FromBody] SaveDraftRequest request)
        {
            try
            {
                if (request?.InitiativeData == null)
                {
                    return BadRequest(new { message = "Invalid request data" });
                }

                var userCode = GetUserCode();
                var initiative = await _initiativeBL.SaveDraftAsync(
                    request.InitiativeId ?? 0, 
                    request.InitiativeData, 
                    userCode);
                
                return Ok(initiative);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving draft");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while saving the draft" });
            }
        }

        #endregion

        #region Customer Management

        /// <summary>
        /// Get initiative customers
        /// </summary>
        [HttpGet("{id}/customers")]
        [ProducesResponseType(typeof(List<InitiativeCustomerDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInitiativeCustomers(int id)
        {
            try
            {
                var customers = await _initiativeBL.GetInitiativeCustomersAsync(id);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting initiative customers for {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving customers" });
            }
        }

        /// <summary>
        /// Update initiative customers
        /// </summary>
        [HttpPut("{id}/customers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateInitiativeCustomers(int id, [FromBody] List<InitiativeCustomerDTO> customers)
        {
            try
            {
                if (customers == null)
                {
                    return BadRequest(new { message = "Invalid customer data" });
                }

                var userCode = GetUserCode();
                var success = await _initiativeBL.UpdateInitiativeCustomersAsync(id, customers, userCode);
                
                if (success)
                {
                    return Ok(new { message = "Customers updated successfully" });
                }
                
                return BadRequest(new { message = "Failed to update customers" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot update customers for initiative {InitiativeId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customers for initiative {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating customers" });
            }
        }

        #endregion

        #region Product Management

        /// <summary>
        /// Get initiative products
        /// </summary>
        [HttpGet("{id}/products")]
        [ProducesResponseType(typeof(List<InitiativeProductDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInitiativeProducts(int id)
        {
            try
            {
                var products = await _initiativeBL.GetInitiativeProductsAsync(id);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting initiative products for {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving products" });
            }
        }

        /// <summary>
        /// Update initiative products
        /// </summary>
        [HttpPut("{id}/products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateInitiativeProducts(int id, [FromBody] List<InitiativeProductDTO> products)
        {
            try
            {
                if (products == null)
                {
                    return BadRequest(new { message = "Invalid product data" });
                }

                var userCode = GetUserCode();
                var success = await _initiativeBL.UpdateInitiativeProductsAsync(id, products, userCode);
                
                if (success)
                {
                    return Ok(new { message = "Products updated successfully" });
                }
                
                return BadRequest(new { message = "Failed to update products" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot update products for initiative {InitiativeId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating products for initiative {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating products" });
            }
        }

        #endregion

        #region Allocation Management

        /// <summary>
        /// Get available allocations
        /// </summary>
        [HttpGet("allocations")]
        [ProducesResponseType(typeof(List<AllocationMasterDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllocations(
            [FromQuery] string salesOrgCode,
            [FromQuery] string brand,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var allocations = await _initiativeBL.GetAvailableAllocationsAsync(salesOrgCode, brand, startDate, endDate);
                return Ok(allocations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting allocations");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving allocations" });
            }
        }

        /// <summary>
        /// Get allocation details
        /// </summary>
        [HttpGet("allocations/{allocationNo}")]
        [ProducesResponseType(typeof(AllocationMasterDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllocationDetails(string allocationNo)
        {
            try
            {
                var allocation = await _initiativeBL.GetAllocationDetailsAsync(allocationNo);
                return Ok(allocation);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Allocation not found: {AllocationNo}", allocationNo);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting allocation {AllocationNo}", allocationNo);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the allocation" });
            }
        }

        /// <summary>
        /// Validate allocation amount
        /// </summary>
        [HttpPost("allocations/validate")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateAllocationAmount([FromBody] ValidateAllocationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Invalid request data" });
                }

                var isValid = await _initiativeBL.ValidateAllocationAmountAsync(
                    request.AllocationNo, 
                    request.ContractAmount, 
                    request.InitiativeId);
                
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating allocation amount");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while validating the allocation" });
            }
        }

        #endregion

        #region File Management

        /// <summary>
        /// Upload file for initiative
        /// </summary>
        [HttpPost("{id}/files")]
        [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(int id, [FromForm] FileUploadRequest request)
        {
            try
            {
                if (request?.File == null || request.File.Length == 0)
                {
                    return BadRequest(new { message = "No file provided" });
                }

                if (string.IsNullOrWhiteSpace(request.FileType))
                {
                    return BadRequest(new { message = "File type is required" });
                }

                using var memoryStream = new MemoryStream();
                await request.File.CopyToAsync(memoryStream);
                var fileContent = memoryStream.ToArray();
                
                var userCode = GetUserCode();
                var filePath = await _initiativeBL.UploadFileAsync(
                    id, 
                    request.FileType, 
                    fileContent, 
                    request.File.FileName, 
                    userCode);
                
                return Ok(new FileUploadResponse 
                { 
                    FilePath = filePath, 
                    FileName = request.File.FileName 
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot upload file for initiative {InitiativeId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid file upload request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file for initiative {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while uploading the file" });
            }
        }

        /// <summary>
        /// Delete file from initiative
        /// </summary>
        [HttpDelete("{id}/files/{fileType}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteFile(int id, string fileType)
        {
            try
            {
                var userCode = GetUserCode();
                var success = await _initiativeBL.DeleteFileAsync(id, fileType, userCode);
                
                if (success)
                {
                    return NoContent();
                }
                
                return BadRequest(new { message = "Failed to delete file" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete file for initiative {InitiativeId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file for initiative {InitiativeId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the file" });
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validate initiative data
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateInitiative([FromBody] ValidateInitiativeRequest request)
        {
            try
            {
                if (request?.InitiativeData == null)
                {
                    return BadRequest(new { message = "Invalid request data" });
                }

                var result = await _initiativeBL.ValidateInitiativeAsync(request.InitiativeData, request.InitiativeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating initiative");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while validating the initiative" });
            }
        }

        #endregion

        #region Helper Methods

        private string GetUserCode()
        {
            // Get user code from claims or context
            var userCode = User?.Identity?.Name ?? "SYSTEM";
            return userCode;
        }

        #endregion
    }

    #region Request/Response Models

    public class CancelInitiativeRequest
    {
        public string CancelReason { get; set; }
    }

    public class SaveDraftRequest
    {
        public int? InitiativeId { get; set; }
        public CreateInitiativeRequest InitiativeData { get; set; }
    }

    public class ValidateAllocationRequest
    {
        public string AllocationNo { get; set; }
        public decimal ContractAmount { get; set; }
        public int? InitiativeId { get; set; }
    }

    public class FileUploadRequest
    {
        public IFormFile File { get; set; }
        public string FileType { get; set; } // posm, default, email
    }

    public class FileUploadResponse
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }

    public class ValidateInitiativeRequest
    {
        public CreateInitiativeRequest InitiativeData { get; set; }
        public int? InitiativeId { get; set; }
    }

    #endregion
}