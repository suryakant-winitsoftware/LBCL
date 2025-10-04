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
    public class UserJourneyController : WINITBaseController
    {
        private readonly Winit.Modules.JourneyPlan.BL.Interfaces.IUserJourneyBL _userJourneyBL;

        public UserJourneyController(IServiceProvider serviceProvider, 
            Winit.Modules.JourneyPlan.BL.Interfaces.IUserJourneyBL userJourneyBL) 
            : base(serviceProvider)
        {
            _userJourneyBL = userJourneyBL;
        }
        [HttpPost]
        [Route("SelectAlUserJourneyDetails")]
        public async Task<ActionResult> SelectAlUserJourneyDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney> pagedResponse = null;
                pagedResponse = await _userJourneyBL.SelectAlUserJourneyDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve UserJourney");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpPost]
        [Route("SelectTodayJourneyPlanDetails")]
        public async Task<ActionResult> SelectTodayJourneyPlanDetails(PagingRequest pagingRequest, string Type, DateTime VisitDate, string JobPositionUID, string OrgUID)
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
                PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan> PagedResponse = null;
                PagedResponse = await _userJourneyBL.SelectTodayJourneyPlanDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired,Type,VisitDate,JobPositionUID,OrgUID);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve TodayJourneyPlan");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("SelecteatHistoryInnerGridDetails")]
        public async Task<ActionResult> SelecteatHistoryInnerGridDetails(string BeatHistoryUID)
        {
            try
            {
                IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.ITodayJourneyPlanInnerGrid> beatHistoryList = null;

                beatHistoryList = await _userJourneyBL.SelecteatHistoryInnerGridDetails(BeatHistoryUID);
                if (beatHistoryList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(beatHistoryList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve HistoryInnerGridDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetUserJourneyGridDetails")]
        public async Task<ActionResult> GetUserJourneyGridDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid> PagedResponse = null;
                PagedResponse = await _userJourneyBL.GetUserJourneyGridDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponse == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(PagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve UserJourney");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetUserJourneyDetailsByUID")]
        public async Task<ActionResult> GetUserJourneyDetailsByUID(string UID)
        {
            try
            {
                Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView userJourneyView = await _userJourneyBL.GetUserJourneyDetailsByUID(UID);
                if (userJourneyView != null)
                {
                    return CreateOkApiResponse(userJourneyView);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve UserJourneyView with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

    }
}
