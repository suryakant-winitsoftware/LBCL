using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Winit.Shared.Models.Common;
using System.Collections.Generic;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Serilog;
using Winit.Modules.ErrorHandling.Model.Classes;

namespace WINITAPI.Controllers.ErrorHandling
{
    [Route("api/[controller]")]
    [ApiController]
     [Authorize]
    public class KnowledgeBaseController : WINITBaseController
    {
        private readonly Winit.Modules.ErrorHandling.BL.Interfaces.IKnowledgeBaseBL _knowledgeBaseBL;

        public KnowledgeBaseController(IServiceProvider serviceProvider, 
            Winit.Modules.ErrorHandling.BL.Interfaces.IKnowledgeBaseBL knowledgeBaseBL) 
            : base(serviceProvider)
        {
            _knowledgeBaseBL = knowledgeBaseBL;
        }
        [HttpPost]
        [Route("GetErrorDetailsAsync")]
        public async Task<ActionResult> GetErrorDetailsAsync()
        {
            try
            {
                Dictionary<string, IErrorDetail> errorDetails = await _knowledgeBaseBL.GetErrorDetailsAsync();
                if (errorDetails == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(errorDetails);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve EmpDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetErrorDetails")]
        public async Task<ActionResult> GetErrorDetails(PagingRequest pagingRequest)
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
                PagedResponse<IErrorDetailModel> pagedResponse = null;
                pagedResponse = await _knowledgeBaseBL.GetErrorDetails(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Fail to retrieve ErrorDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetErrorDetailsByUID")]
        public async Task<ActionResult> GetErrorDetailsByUID(string UID)
        {
            try
            {
                Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetailModel errorDetails = await _knowledgeBaseBL.GetErrorDetailsByUID(UID);
                if (errorDetails != null)
                {
                    return CreateOkApiResponse(errorDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Error Details with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateErrorDetails")]
        public async Task<ActionResult> CreateErrorDetails([FromBody] ErrorDetailModel errorDetail)
        {
            try
            {

                var retVal = await _knowledgeBaseBL.CreateErrorDetails(errorDetail);
                return (retVal > 0) ? Created("Created", retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Error Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateErrorDetails")]
        public async Task<ActionResult> UpdateErrorDetails([FromBody] ErrorDetailModel errorDetail)
        {
            try
            {
                var existingDetails = await _knowledgeBaseBL.GetErrorDetailsByUID(errorDetail.UID);
                if (existingDetails != null)
                {

                    var retVal = await _knowledgeBaseBL.UpdateErrorDetails(errorDetail);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Error Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("GetErrorDescriptionDetailsByErroCode")]
        public async Task<ActionResult> GetErrorDescriptionDetailsByErroCode(string errorCode)
        {
            try
            {
                Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDescriptionDetails errorDescriptionDetails = await _knowledgeBaseBL.GetErrorDescriptionDetailsByErroCode(errorCode);
                if (errorDescriptionDetails != null)
                {
                    return CreateOkApiResponse(errorDescriptionDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Error Description with ErrorCode: {@ErrorCode}", errorCode);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CUDErrorDetailsLocalization")]
        public async Task<ActionResult> CUDErrorDetailsLocalization([FromBody] List<ErrorDetailsLocalization> errorDetailsLocalizations)
        {
            try
            {
                var retVal = await _knowledgeBaseBL.CUDErrorDetailsLocalization(errorDetailsLocalizations);
                return (retVal > 0) ? Created("Created", retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to ErrorDetailsLocalization Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetErrorDetailsLocalizationbyUID")]
        public async Task<ActionResult> GetErrorDetailsLocalizationbyUID(string errorDetailsLocalizationUID)
        {
            try
            {
                var retVal = await _knowledgeBaseBL.GetErrorDetailsLocalizationbyUID(errorDetailsLocalizationUID);
                return (retVal != null) ? CreateOkApiResponse(retVal) : throw new Exception("GetErrorDetailsLocalizationbyUID Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to GetErrorDetailsLocalizationbyUID Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
