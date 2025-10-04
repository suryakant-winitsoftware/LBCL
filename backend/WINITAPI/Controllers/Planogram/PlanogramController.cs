using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.Planogram.BL.Interfaces;
using Winit.Modules.Planogram.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace WINITAPI.Controllers.Planogram
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanogramController : WINITBaseController
    {
        private readonly IPlanogramBL _planogramBL;

        public PlanogramController(IServiceProvider serviceProvider, IPlanogramBL planogramBL) : base(serviceProvider)
        {
            _planogramBL = planogramBL;
        }

        #region PlanogramSetup CRUD Operations

        [HttpGet]
        [Route("GetAllPlanogramSetups")]
        public async Task<ActionResult> GetAllPlanogramSetups([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var setups = await _planogramBL.GetAllPlanogramSetupsAsync(pageNumber, pageSize);
                return CreateOkApiResponse(setups);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve all planogram setups");
                return CreateErrorResponse($"An error occurred while retrieving planogram setups: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetPlanogramSetupByUID/{uid}")]
        public async Task<ActionResult> GetPlanogramSetupByUID(string uid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return BadRequest("UID is required");
                }

                var setup = await _planogramBL.GetPlanogramSetupByUIDAsync(uid);
                
                if (setup == null)
                {
                    return NotFound($"Planogram setup with UID {uid} not found");
                }

                return CreateOkApiResponse(setup);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve planogram setup with UID: {uid}", uid);
                return CreateErrorResponse($"An error occurred while retrieving planogram setup: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetPlanogramSetupsByCategory/{categoryCode}")]
        public async Task<ActionResult> GetPlanogramSetupsByCategory(string categoryCode)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryCode))
                {
                    return BadRequest("Category code is required");
                }

                var setups = await _planogramBL.GetPlanogramSetupsByCategoryAsync(categoryCode);
                return CreateOkApiResponse(setups);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve planogram setups for category: {categoryCode}", categoryCode);
                return CreateErrorResponse($"An error occurred while retrieving planogram setups: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("CreatePlanogramSetup")]
        public async Task<ActionResult> CreatePlanogramSetup([FromBody] IPlanogramSetup planogramSetup)
        {
            try
            {
                if (planogramSetup == null)
                {
                    return BadRequest("Planogram setup data is required");
                }

                if (string.IsNullOrEmpty(planogramSetup.CategoryCode))
                {
                    return BadRequest("Category code is required");
                }

                // Generate UID if not provided
                if (string.IsNullOrEmpty(planogramSetup.UID))
                {
                    planogramSetup.UID = Guid.NewGuid().ToString();
                }

                // Set creation metadata
                planogramSetup.CreatedTime = DateTime.Now;
                planogramSetup.CreatedBy = GetCurrentUserUID();
                planogramSetup.ServerAddTime = DateTime.Now;

                var result = await _planogramBL.CreatePlanogramSetupAsync(planogramSetup);
                
                if (!string.IsNullOrEmpty(result))
                {
                    return CreateOkApiResponse(new { uid = result, message = "Planogram setup created successfully" });
                }

                return CreateErrorResponse("Failed to create planogram setup");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create planogram setup");
                return CreateErrorResponse($"An error occurred while creating planogram setup: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("UpdatePlanogramSetup")]
        public async Task<ActionResult> UpdatePlanogramSetup([FromBody] IPlanogramSetup planogramSetup)
        {
            try
            {
                if (planogramSetup == null || string.IsNullOrEmpty(planogramSetup.UID))
                {
                    return BadRequest("Planogram setup data with UID is required");
                }

                // Set modification metadata
                planogramSetup.ModifiedTime = DateTime.Now;
                planogramSetup.ModifiedBy = GetCurrentUserUID();
                planogramSetup.ServerModifiedTime = DateTime.Now;

                var result = await _planogramBL.UpdatePlanogramSetupAsync(planogramSetup);
                
                if (result)
                {
                    return CreateOkApiResponse(new { message = "Planogram setup updated successfully" });
                }

                return NotFound($"Planogram setup with UID {planogramSetup.UID} not found");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update planogram setup with UID: {uid}", planogramSetup?.UID);
                return CreateErrorResponse($"An error occurred while updating planogram setup: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("DeletePlanogramSetup/{uid}")]
        public async Task<ActionResult> DeletePlanogramSetup(string uid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return BadRequest("UID is required");
                }

                var result = await _planogramBL.DeletePlanogramSetupAsync(uid);
                
                if (result)
                {
                    return CreateOkApiResponse(new { message = "Planogram setup deleted successfully" });
                }

                return NotFound($"Planogram setup with UID {uid} not found");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete planogram setup with UID: {uid}", uid);
                return CreateErrorResponse($"An error occurred while deleting planogram setup: {ex.Message}");
            }
        }

        #endregion

        #region PlanogramCategory Operations

        [HttpGet]
        [Route("GetPlanogramCategories")]
        public async Task<ActionResult> GetPlanogramCategories()
        {
            try
            {
                var categories = await _planogramBL.GetPlanogramCategoriesAsync();
                return CreateOkApiResponse(categories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve planogram categories");
                return CreateErrorResponse($"An error occurred while retrieving planogram categories: {ex.Message}");
            }
        }

        #endregion

        #region PlanogramRecommendation Operations

        [HttpGet]
        [Route("GetPlanogramRecommendations/{categoryCode}")]
        public async Task<ActionResult> GetPlanogramRecommendations(string categoryCode)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryCode))
                {
                    return BadRequest("Category code is required");
                }

                var recommendations = await _planogramBL.GetPlanogramRecommendationsByCategoryAsync(categoryCode);
                return CreateOkApiResponse(recommendations);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve planogram recommendations for category: {categoryCode}", categoryCode);
                return CreateErrorResponse($"An error occurred while retrieving planogram recommendations: {ex.Message}");
            }
        }

        #endregion

        #region PlanogramExecution Operations

        [HttpPost]
        [Route("CreatePlanogramExecutionHeader")]
        public async Task<ActionResult> CreatePlanogramExecutionHeader([FromBody] IPlanogramExecutionHeader header)
        {
            try
            {
                if (header == null)
                {
                    return BadRequest("Planogram execution header data is required");
                }

                if (string.IsNullOrEmpty(header.StoreUID))
                {
                    return BadRequest("Store UID is required");
                }

                // Generate UID if not provided
                if (string.IsNullOrEmpty(header.UID))
                {
                    header.UID = Guid.NewGuid().ToString();
                }

                // Set creation metadata
                header.CreatedTime = DateTime.Now;
                header.CreatedBy = GetCurrentUserUID();
                header.ServerAddTime = DateTime.Now;

                var result = await _planogramBL.CreatePlanogramExecutionHeaderAsync(header);
                
                if (!string.IsNullOrEmpty(result))
                {
                    return CreateOkApiResponse(new { uid = result, message = "Planogram execution header created successfully" });
                }

                return CreateErrorResponse("Failed to create planogram execution header");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create planogram execution header");
                return CreateErrorResponse($"An error occurred while creating planogram execution header: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("CreatePlanogramExecutionDetail")]
        public async Task<ActionResult> CreatePlanogramExecutionDetail([FromBody] IPlanogramExecutionDetail detail)
        {
            try
            {
                if (detail == null)
                {
                    return BadRequest("Planogram execution detail data is required");
                }

                if (string.IsNullOrEmpty(detail.PlanogramExecutionHeaderUID))
                {
                    return BadRequest("Planogram execution header UID is required");
                }

                // Generate UID if not provided
                if (string.IsNullOrEmpty(detail.UID))
                {
                    detail.UID = Guid.NewGuid().ToString();
                }

                // Set creation metadata
                detail.CreatedTime = DateTime.Now;
                detail.CreatedBy = GetCurrentUserUID();
                detail.ServerAddTime = DateTime.Now;

                var result = await _planogramBL.CreatePlanogramExecutionDetailAsync(detail);
                
                if (!string.IsNullOrEmpty(result))
                {
                    return CreateOkApiResponse(new { uid = result, message = "Planogram execution detail created successfully" });
                }

                return CreateErrorResponse("Failed to create planogram execution detail");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create planogram execution detail");
                return CreateErrorResponse($"An error occurred while creating planogram execution detail: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetPlanogramExecutionDetails/{headerUID}")]
        public async Task<ActionResult> GetPlanogramExecutionDetails(string headerUID)
        {
            try
            {
                if (string.IsNullOrEmpty(headerUID))
                {
                    return BadRequest("Header UID is required");
                }

                var details = await _planogramBL.GetPlanogramExecutionDetailsByHeaderUIDAsync(headerUID);
                return CreateOkApiResponse(details);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve planogram execution details for header: {headerUID}", headerUID);
                return CreateErrorResponse($"An error occurred while retrieving planogram execution details: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("UpdatePlanogramExecutionStatus/{uid}")]
        public async Task<ActionResult> UpdatePlanogramExecutionStatus(string uid, [FromBody] bool isCompleted)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return BadRequest("UID is required");
                }

                var result = await _planogramBL.UpdatePlanogramExecutionDetailStatusAsync(uid, isCompleted);
                
                if (result)
                {
                    return CreateOkApiResponse(new { message = "Planogram execution status updated successfully" });
                }

                return NotFound($"Planogram execution detail with UID {uid} not found");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update planogram execution status for UID: {uid}", uid);
                return CreateErrorResponse($"An error occurred while updating planogram execution status: {ex.Message}");
            }
        }

        #endregion

        #region Bulk Operations

        [HttpPost]
        [Route("BulkCreatePlanogramSetups")]
        public async Task<ActionResult> BulkCreatePlanogramSetups([FromBody] List<IPlanogramSetup> planogramSetups)
        {
            try
            {
                if (planogramSetups == null || !planogramSetups.Any())
                {
                    return BadRequest("Planogram setups data is required");
                }

                var results = new List<object>();
                var errors = new List<object>();

                foreach (var setup in planogramSetups)
                {
                    try
                    {
                        // Generate UID if not provided
                        if (string.IsNullOrEmpty(setup.UID))
                        {
                            setup.UID = Guid.NewGuid().ToString();
                        }

                        // Set creation metadata
                        setup.CreatedTime = DateTime.Now;
                        setup.CreatedBy = GetCurrentUserUID();
                        setup.ServerAddTime = DateTime.Now;

                        var result = await _planogramBL.CreatePlanogramSetupAsync(setup);
                        
                        if (!string.IsNullOrEmpty(result))
                        {
                            results.Add(new { uid = result, categoryCode = setup.CategoryCode, success = true });
                        }
                        else
                        {
                            errors.Add(new { categoryCode = setup.CategoryCode, error = "Failed to create" });
                        }
                    }
                    catch (Exception innerEx)
                    {
                        errors.Add(new { categoryCode = setup.CategoryCode, error = innerEx.Message });
                    }
                }

                return CreateOkApiResponse(new 
                { 
                    message = $"Bulk operation completed. Success: {results.Count}, Errors: {errors.Count}",
                    successful = results,
                    failed = errors
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to perform bulk create of planogram setups");
                return CreateErrorResponse($"An error occurred during bulk create: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("BulkDeletePlanogramSetups")]
        public async Task<ActionResult> BulkDeletePlanogramSetups([FromBody] List<string> uids)
        {
            try
            {
                if (uids == null || !uids.Any())
                {
                    return BadRequest("UIDs are required");
                }

                var results = new List<object>();
                var errors = new List<object>();

                foreach (var uid in uids)
                {
                    try
                    {
                        var result = await _planogramBL.DeletePlanogramSetupAsync(uid);
                        
                        if (result)
                        {
                            results.Add(new { uid = uid, success = true });
                        }
                        else
                        {
                            errors.Add(new { uid = uid, error = "Not found or failed to delete" });
                        }
                    }
                    catch (Exception innerEx)
                    {
                        errors.Add(new { uid = uid, error = innerEx.Message });
                    }
                }

                return CreateOkApiResponse(new 
                { 
                    message = $"Bulk delete completed. Success: {results.Count}, Errors: {errors.Count}",
                    successful = results,
                    failed = errors
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to perform bulk delete of planogram setups");
                return CreateErrorResponse($"An error occurred during bulk delete: {ex.Message}");
            }
        }

        #endregion

        #region Search and Filter Operations

        [HttpPost]
        [Route("SearchPlanogramSetups")]
        public async Task<ActionResult> SearchPlanogramSetups([FromBody] PlanogramSearchRequest searchRequest)
        {
            try
            {
                if (searchRequest == null)
                {
                    searchRequest = new PlanogramSearchRequest();
                }

                var results = await _planogramBL.SearchPlanogramSetupsAsync(
                    searchRequest.SearchText,
                    searchRequest.CategoryCodes,
                    searchRequest.MinShelfCm,
                    searchRequest.MaxShelfCm,
                    searchRequest.SelectionType,
                    searchRequest.PageNumber,
                    searchRequest.PageSize
                );

                return CreateOkApiResponse(results);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to search planogram setups");
                return CreateErrorResponse($"An error occurred while searching planogram setups: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private string GetCurrentUserUID()
        {
            // This should be implemented based on your authentication mechanism
            // For now, returning a placeholder
            return HttpContext.User?.Identity?.Name ?? "SYSTEM";
        }

        #endregion
    }

    #region Request Models

    public class PlanogramSearchRequest
    {
        public string SearchText { get; set; }
        public List<string> CategoryCodes { get; set; }
        public decimal? MinShelfCm { get; set; }
        public decimal? MaxShelfCm { get; set; }
        public string SelectionType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    #endregion
}