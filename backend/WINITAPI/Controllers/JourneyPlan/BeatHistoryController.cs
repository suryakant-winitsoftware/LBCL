using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Enums;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace WINITAPI.Controllers.JobPosition
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BeatHistoryController : WINITBaseController
    {
        private readonly Winit.Modules.JourneyPlan.BL.Interfaces.IBeatHistoryBL _beatHistoryBL;

        public BeatHistoryController(IServiceProvider serviceProvider, 
            Winit.Modules.JourneyPlan.BL.Interfaces.IBeatHistoryBL beatHistoryBL) 
            : base(serviceProvider)
        {
            _beatHistoryBL = beatHistoryBL;
        }
        [HttpPost]
        [Route("SelectAllJobPositionDetails")]
        public async Task<ActionResult> SelectAllBeatHistoryDetails(PagingRequest pagingRequest)
        {

            try
            {
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> pagedResponseBeatHistoryList = null;
                pagedResponseBeatHistoryList = await _beatHistoryBL.SelectAllBeatHistoryDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseBeatHistoryList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseBeatHistoryList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve BeatHistory");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetBeatHistoryByUID")]
        public async Task<ActionResult> GetBeatHistoryByUID(string UID)
        {
            try
            {
                Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistoryDetails = await _beatHistoryBL.GetBeatHistoryByUID(UID);
                if (beatHistoryDetails != null)
                {
                    return CreateOkApiResponse(beatHistoryDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve BeatHistoryDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateBeatHistory")]
        public async Task<ActionResult> CreateBeatHistory([FromBody] Winit.Modules.JourneyPlan.Model.Classes.BeatHistory  beatHistory)
        {
            try
            {
                beatHistory.ServerAddTime = DateTime.Now;
                beatHistory.ServerModifiedTime = DateTime.Now;
                var jobPositionDetails = await _beatHistoryBL.CreateBeatHistory(beatHistory);
                return (jobPositionDetails > 0) ? CreateOkApiResponse(jobPositionDetails) : throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create BeatHistory details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpPut]
        [Route("UpdateBeatHistory")]
        public async Task<ActionResult> UpdateBeatHistory([FromBody] Winit.Modules.JourneyPlan.Model.Classes.BeatHistory beatHistory)
        {
            try
            {
                var existingDetails = await _beatHistoryBL.GetBeatHistoryByUID(beatHistory.UID);
                if (existingDetails != null)
                {
                    beatHistory.ServerModifiedTime = DateTime.Now;  
                    var updateJobPositionDetails = await _beatHistoryBL.UpdateBeatHistory(beatHistory);
                    return (updateJobPositionDetails > 0) ? CreateOkApiResponse(updateJobPositionDetails) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating BeatHistory Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteBeatHistory")]
        public async Task<ActionResult> DeleteBeatHistory([FromQuery] string UID)
        {
            try
            {
                var result = await _beatHistoryBL.DeleteBeatHistory(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Fail");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpGet]
        [Route("GetSelectedBeatHistoryByRouteUID")]
        public async Task<ActionResult> GetSelectedBeatHistoryByRouteUID(string RouteUID)
        {
            try
            {
                Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory SelectedbeatHistoryDetails = await _beatHistoryBL.GetSelectedBeatHistoryByRouteUID(RouteUID);
                if (SelectedbeatHistoryDetails != null)
                {
                    return CreateOkApiResponse(SelectedbeatHistoryDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SelectedbeatHistoryDetails with RouteUID: {@RouteUID}", RouteUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAllBeatHistoriesByRouteUID")]
        public async Task<ActionResult> GetAllBeatHistoriesByRouteUID(string RouteUID, int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                Log.Information($"GetAllBeatHistoriesByRouteUID called - RouteUID: {RouteUID}, PageNumber: {pageNumber}, PageSize: {pageSize}");
                
                // If pagination parameters are provided, use the paginated version
                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    Log.Information($"Using paginated version - PageNumber: {pageNumber.Value}, PageSize: {pageSize.Value}");
                    var pagedBeatHistories = await _beatHistoryBL.GetAllBeatHistoriesByRouteUID(RouteUID, pageNumber.Value, pageSize.Value);
                    return CreateOkApiResponse(pagedBeatHistories);
                }
                else
                {
                    Log.Information("Using non-paginated version - returning all records");
                    // Otherwise, return all records without pagination
                    var allBeatHistories = await _beatHistoryBL.GetAllBeatHistoriesByRouteUID(RouteUID);
                    return CreateOkApiResponse(allBeatHistories);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve all beat histories with RouteUID: {@RouteUID}", RouteUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetCustomersByBeatHistoryUID")]
        public async Task<ActionResult> GetCustomersByBeatHistoryUID(string BeatHistoryUID)
        {
            try
            {
                Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedbeatHistoryDetails = (Winit.Modules.Store.Model.Interfaces.IStoreItemView)await _beatHistoryBL.GetCustomersByBeatHistoryUID( BeatHistoryUID);
                if (SelectedbeatHistoryDetails != null)
                {
                    return CreateOkApiResponse(SelectedbeatHistoryDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SelectedbeatHistoryDetails with BeatHistoryUID: {@BeatHistoryUID}", BeatHistoryUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateStoreHistoryStatus")]
        public async Task<ActionResult> UpdateStoreHistoryStatus(string StoreHistoryUID, string Status)
        {
            try
            {
                return CreateOkApiResponse(_beatHistoryBL.UpdateStoreHistoryStatus(StoreHistoryUID,Status));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating BeatHistory Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetStoreHistoriesByUserJourneyUID")]
        public async Task<ActionResult> GetStoreHistoriesByUserJourneyUID(string userJourneyUID)
        {
            try
            {
                var storeHistories = await _beatHistoryBL.GetStoreHistoriesByUserJourneyUID(userJourneyUID);
                return CreateOkApiResponse(storeHistories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving StoreHistories for UserJourney: {@UserJourneyUID}", userJourneyUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
