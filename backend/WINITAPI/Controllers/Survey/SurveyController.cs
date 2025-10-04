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
using WINITAPI.Controllers.SKU;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SurveyController : WINITBaseController
    {
        private readonly Winit.Modules.Survey.BL.Interfaces.ISurveyBL _surveyBL;

        public SurveyController(IServiceProvider serviceProvider,
            Winit.Modules.Survey.BL.Interfaces.ISurveyBL surveyBL) : base(serviceProvider)
        {
            _surveyBL = surveyBL;
        }
        [HttpPost]
        [Route("GetAllSurveyDeatils")]
        public async Task<ActionResult> GetAllSurveyDeatils(
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
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurvey> pagedResponseSurveyList = null;
                pagedResponseSurveyList = await _surveyBL.GetAllSurveyDeatils(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseSurveyList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseSurveyList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Survey Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetSurveyByUID/{uID}")]
        public async Task<ActionResult> GetSurveyByUID(string uID)
        {
            try
            {
                Winit.Modules.Survey.Model.Interfaces.ISurvey survey = await _surveyBL.GetSurveyByUID(uID);
                if (survey != null)
                {
                    return CreateOkApiResponse(survey);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Survey with UID: {@UID}", uID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpGet]
        [Route("GetSurveyByCode/{code}")]
        public async Task<ActionResult> GetSurveyByCode(string code)
        {
            try
            {
                Winit.Modules.Survey.Model.Interfaces.ISurvey survey = await _surveyBL.GetSurveyByCode(code);
                if (survey != null)
                {
                    return CreateOkApiResponse(survey);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Survey with UID: {@UID}", code);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpPost]
        [Route("CreateSurvey")]
        public async Task<ActionResult> CreateBankDetails([FromBody] ISurvey bank)
        {
            try
            {
                var retVal = await _surveyBL.CreateSurvey(bank);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Bank details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost, HttpPut]
        [Route("CUDSurvey")]
        public async Task<ActionResult> CUDSurveyDetails([FromBody] ISurvey survey)
        {
            try
            {
                var retVal = await _surveyBL.CUDSurvey(survey);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Bank details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("DeleteSurvey/{uID}")]
        public async Task<ActionResult> DeleteSurvey(string uID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(uID))
                {
                    return BadRequest("Invalid survey UID");
                }
                
                var retVal = await _surveyBL.DeleteSurvey(uID);
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else
                {
                    return NotFound("Survey not found or could not be deleted");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete Survey with UID: {@UID}", uID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
