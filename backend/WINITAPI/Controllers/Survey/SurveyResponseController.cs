using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SurveyResponseController : WINITBaseController
    {
        private readonly Winit.Modules.Survey.BL.Interfaces.ISurveyResponseBL _surveyResponseBL;

        public SurveyResponseController(IServiceProvider serviceProvider,
            Winit.Modules.Survey.BL.Interfaces.ISurveyResponseBL surveyResponseBL) : base(serviceProvider)
        {
            _surveyResponseBL = surveyResponseBL;
        }
        [HttpPost]
        [Route("GetViewSurveyResponse")]
        public async Task<ActionResult> GetViewSurveyResponse(
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
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse> pagedResponseSurveyResponseList = null;
                pagedResponseSurveyResponseList = await _surveyResponseBL.GetViewSurveyResponse(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseSurveyResponseList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSurveyResponseList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SurveyResponse  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("ViewSurveyResponseByUID/{uID}")]
        public async Task<ActionResult> ViewSurveyResponseByUID(string uID)
        {
            try
            {
                Winit.Modules.Survey.Model.Interfaces.ISurveyResponseViewDTO surveyResponseDTO = await _surveyResponseBL.ViewSurveyResponseByUID(uID);
                if (surveyResponseDTO != null)
                {
                    return CreateOkApiResponse(surveyResponseDTO);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve SurveyResponse with UID: {@UID}", uID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("GetViewSurveyResponseForExport")]
        public async Task<ActionResult> GetViewSurveyResponseForExport(PagingRequest pagingRequest)
        {
            try
            {
                List<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponseExport>surveyResponseExportList = null;
                surveyResponseExportList = await _surveyResponseBL.GetViewSurveyResponseForExport(pagingRequest.FilterCriterias);
                if (surveyResponseExportList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(surveyResponseExportList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve SurveyResponse  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("TicketStatusUpdate")]
        public async Task<ActionResult> TicketStatusUpdate(string uid, string status,string empUID)
        {
            try
            {
                var existingDetails = await _surveyResponseBL.GetSurveyResponseByUID(uid);
                if (existingDetails != null)
                {
                    var retVal = await _surveyResponseBL.TicketStatusUpdate(uid, status, empUID);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetStoreQuestionFrequencyDetails")]
        public async Task<ActionResult> GetStoreQuestionFrequencyDetails(
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
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency> pagedResponseStoreQuestionFrequencyList = null;
                pagedResponseStoreQuestionFrequencyList = await _surveyResponseBL.GetStoreQuestionFrequencyDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseStoreQuestionFrequencyList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseStoreQuestionFrequencyList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Store Question Frequency  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateSurveyResponseList")]
        public async Task<ActionResult> CreateSurveyResponseList()
        {
            try
            {
                var retVal = await _surveyResponseBL.CreateSurveyResponseList();
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
                
               
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        

    }
}
