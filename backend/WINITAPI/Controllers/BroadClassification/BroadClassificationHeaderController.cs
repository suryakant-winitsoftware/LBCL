using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.Model.Constant;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.BroadClassification
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BroadClassificationHeaderController : WINITBaseController
    {
        private readonly Winit.Modules.BroadClassification.BL.Interfaces.IBroadClassificationHeaderBL _broadClassificationHeaderBL;
        public BroadClassificationHeaderController(IServiceProvider serviceProvider,
            Winit.Modules.BroadClassification.BL.Interfaces.IBroadClassificationHeaderBL broadClassificationHeaderBL
            )
            : base(serviceProvider)
        {
            _broadClassificationHeaderBL = broadClassificationHeaderBL;
        }
        [HttpPost]
        [Route("GetBroadClassificationHeaderDetails")]
        public async Task<ActionResult> GetBroadClassificationHeaderDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader> pagedResponse = null;

                pagedResponse = await _broadClassificationHeaderBL.GetBroadClassificationHeaderDetails(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Fail to retrieve BroadClassificationHeader");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetBroadClassificationHeaderDetailsByUID")]
        public async Task<ActionResult> GetBroadClassificationHeaderDetailsByUID(string UID)
        {
            try
            {
                Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader broadClassificationHeader = await _broadClassificationHeaderBL.GetBroadClassificationHeaderDetailsByUID(UID);
                if (broadClassificationHeader != null)
                {
                    return CreateOkApiResponse(broadClassificationHeader);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve BroadClassificationHeader with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateBroadClassificationHeader")]
        public async Task<ActionResult> CreateBroadClassificationHeader([FromBody] Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader broadClassificationHeader)
        {
            try
            {
                var retVal = await _broadClassificationHeaderBL.CreateBroadClassificationHeader(broadClassificationHeader);
                if (retVal > 0)
                {
                    PerformAuditTrial(broadClassificationHeader, AuditTrailCommandType.Create);
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create BroadClassificationHeader details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateBroadClassificationHeader")]
        public async Task<ActionResult> UpdateBroadClassificationHeader([FromBody] Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader broadClassificationHeader)
        {
            try
            {
                var existingProductDetails = await _broadClassificationHeaderBL.GetBroadClassificationHeaderDetailsByUID(broadClassificationHeader.UID);
                if (existingProductDetails != null)
                {
                    var retVal = await _broadClassificationHeaderBL.UpdateBroadClassificationHeader(broadClassificationHeader);
                    if (retVal > 0)
                    {
                        PerformAuditTrial(broadClassificationHeader, AuditTrailCommandType.Update);
                    }
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating BroadClassificationHeader");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        private void PerformAuditTrial(Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader broadClassificationHeader, string commondType)
        {
            try
            {

                var newData = new Dictionary<string, object>();
                newData["BroadClassificationHeader"] = DictionaryConverter.ToDictionary(broadClassificationHeader);
                var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                    linkedItemType: LinkedItemType.BroadClassificationHeader,
                    linkedItemUID: broadClassificationHeader?.UID,
                    commandType: commondType,
                    docNo: broadClassificationHeader.UID,
                    jobPositionUID: null,
                    empUID: broadClassificationHeader?.CreatedBy,
                    empName: User.FindFirst(ClaimTypes.Name)?.Value,
                    newData: newData,
                    originalDataId: null,
                    changeData: null
                );

                LogAuditTrailInBackground(auditTrailEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(broadClassificationHeader)}");
            }
        }
        [HttpDelete]
        [Route("DeleteBroadClassificationHeader")]
        public async Task<ActionResult> DeleteBroadClassificationHeader(Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader broadClassificationHeader)
        {
            try
            {
                var retVal = await _broadClassificationHeaderBL.DeleteBroadClassificationHeader(broadClassificationHeader.UID);
                if (retVal > 0)
                {
                    PerformAuditTrial(broadClassificationHeader, AuditTrailCommandType.Delete);
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}