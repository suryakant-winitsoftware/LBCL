using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserStoreActivityController : WINITBaseController
    {
        private readonly Winit.Modules.Survey.BL.Interfaces.IStoreandUserReportsBL _storeandUserReportsBL;

        public UserStoreActivityController(IServiceProvider serviceProvider,
            Winit.Modules.Survey.BL.Interfaces.IStoreandUserReportsBL storeandUserReportsBL) : base(serviceProvider)
        {
            _storeandUserReportsBL = storeandUserReportsBL;
        }
        [HttpPost]
        [Route("GetStoreUserSummary")]
        public async Task<ActionResult> GetStoreUserSummary(
            PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserInfo> pagedResponsestoreUserInfoList = null;
                pagedResponsestoreUserInfoList = await _storeandUserReportsBL.GetStoreUserSummary(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponsestoreUserInfoList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponsestoreUserInfoList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Store User Summary Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetStoreUserActivityDetails")]
        public async Task<ActionResult> GetStoreUserActivityDetails(
           PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails> pagedResponseStoreUserVisitList = null;
                pagedResponseStoreUserVisitList = await _storeandUserReportsBL.GetStoreUserActivityDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreUserVisitList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreUserVisitList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Store User Activity Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateStoreRollingStats")]
        public async Task<ActionResult> CreateStoreRollingStats(List<IStoreRollingStatsModel> storeRollingStatsModelList)
        {
            try
            {
                var retVal = await _storeandUserReportsBL.CreateStoreRollingStats(storeRollingStatsModelList);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Created failed");

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StoreRollingStatss");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }



    }
}
