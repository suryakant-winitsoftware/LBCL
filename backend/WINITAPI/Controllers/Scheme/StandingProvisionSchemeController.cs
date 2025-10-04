using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.Scheme.Model.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.SKU.Model.Classes;


namespace WINITAPI.Controllers.AwayPeriod
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class StandingProvisionSchemeController : WINITBaseController
    {
        private readonly IStandingProvisionSchemeBL _standingProvisionSchemeBL;
        private readonly ISchemesBL _schemesBL;

        public StandingProvisionSchemeController(IServiceProvider serviceProvider, 
            IStandingProvisionSchemeBL standingProvisionSchemeBL, ISchemesBL schemesBL) : base(serviceProvider)
        {
            _standingProvisionSchemeBL = standingProvisionSchemeBL;
            _schemesBL = schemesBL;
        }

        [HttpPost]
        [Route("SelectAllStandingConfiguration")]
        public async Task<ActionResult> SelectAllStandingConfiguration(PagingRequest pagingRequest)
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

                var pagedResponse = await _standingProvisionSchemeBL.SelectAllStandingConfiguration(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Failed to retrieve StandingProvisionSchemeDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetStandingProvisionSchemeDetailsByUID")]
        public async Task<ActionResult> GetStandingProvisionSchemeDetailsByUID(string UID)
        {
            try
            {
                var details = await _standingProvisionSchemeBL.GetStandingConfigurationMasterByUID(UID);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve StandingProvisionSchemeDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CUStandingProvisionScheme")]
        public async Task<ActionResult> CUStandingProvisionScheme([FromBody] IStandingProvisionSchemeMaster standingProvisionSchemeMaster)
        {
            try
            {
                if (standingProvisionSchemeMaster == null || standingProvisionSchemeMaster.StandingProvisionScheme == null || string.IsNullOrEmpty(standingProvisionSchemeMaster.StandingProvisionScheme.UID))
                {
                    return CreateErrorResponse("Invalid Input Data ");
                }

                var retVal = await _standingProvisionSchemeBL.CUStandingProvisionScheme(standingProvisionSchemeMaster);
                if (retVal > 0)
                {
                    if (standingProvisionSchemeMaster.IsNew)
                    {
                        bool isSuccess = await _schemesBL.CreateApproval(linkedItemType: "StandingProvision",
                               linkedItemUID: standingProvisionSchemeMaster.StandingProvisionScheme.UID, approvalRequestItem: standingProvisionSchemeMaster.ApprovalRequestItem);
                        if (isSuccess)
                        {
                            int cnt = await _standingProvisionSchemeBL.UpdateStandingConfiguration(standingProvisionSchemeMaster.StandingProvisionScheme);
                        }
                    }
                    else if (standingProvisionSchemeMaster.ApprovalStatusUpdate != null)
                    {
                        bool isSuccess = await _schemesBL.UpdateApproval(approvalStatusUpdate: standingProvisionSchemeMaster.ApprovalStatusUpdate);
                    }
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StandingProvisionScheme details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateStandingConfiguration")]
        public async Task<ActionResult> UpdateStandingConfiguration([FromBody] IStandingProvisionScheme standingProvisionScheme)
        {
            try
            {
                if (standingProvisionScheme == null)
                {
                    return CreateErrorResponse("Invalid Input Data ");
                }
                standingProvisionScheme.ServerModifiedTime = DateTime.UtcNow;
                var retVal = await _standingProvisionSchemeBL.UpdateStandingConfiguration(standingProvisionScheme);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Update StandingProvisionScheme details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }



        [HttpDelete]
        [Route("DeleteStandingProvisionSchemeDetail")]
        public async Task<ActionResult> DeleteStandingProvisionSchemeDetail([FromQuery] string UID)
        {
            try
            {
                var retVal = await _standingProvisionSchemeBL.DeleteStandingConfiguration(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost("GetStandingSchemesByOrgUidAndSKUUid")]
        public async Task<IActionResult> GetStandingSchemesByOrgUidAndSKUUid([FromQuery] string orgUID, [FromQuery] DateTime orderDate, [FromBody] List<SKUFilter> skuFilterList)
        {
            try
            {
                var details = await _standingProvisionSchemeBL.GetStandingSchemesByOrgUidAndSKUUid(orgUID, orderDate, skuFilterList);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Standing scheme by orguid and skus : {@orgUID}, {@skus}", orgUID, System.Text.Json.JsonSerializer.Serialize(skuFilterList));
                return CreateErrorResponse($"An error occurred while processing the request.{ex.Message}");
            }
        }
        [HttpPost("GetStandingSchemesByPOUid")]
        public async Task<IActionResult> GetStandingSchemesByPOUid([FromQuery] string POUid, [FromBody] List<SKUFilter> skuFilterList)
        {
            try
            {
                var details = await _standingProvisionSchemeBL.GetStandingSchemesByPOUid(POUid, skuFilterList);
                if (details != null)
                {
                    return CreateOkApiResponse(details);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Standing scheme by orguid and skus : {@POUid}, {@skus}",  System.Text.Json.JsonSerializer.Serialize(skuFilterList));
                return CreateErrorResponse($"An error occurred while processing the request.{ex.Message}");
            }
        }
        [HttpPut]
        [Route("ChangeEndDate")]
        public async Task<ActionResult> ChangeEndDate([FromBody] IStandingProvisionScheme standingProvisionScheme)
        {
            try
            {
                int cnt = await _standingProvisionSchemeBL.ChangeEndDate(standingProvisionScheme);
                if (cnt > 0)
                {
                    if (standingProvisionScheme.SchemeExtendHistory != null)
                        cnt += await _schemesBL.InsertSchemeExtendHistory(standingProvisionScheme.SchemeExtendHistory);
                }
                return CreateOkApiResponse(cnt);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex.Message);
            }
        }
    }
}