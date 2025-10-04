using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.Model.Constant;
using Winit.Modules.BroadClassification.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.BroadClassification
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BroadClassificationLineController : WINITBaseController
    {
        private readonly Winit.Modules.BroadClassification.BL.Interfaces.IBroadClassificationLineBL _broadClassificationLineBL;
        public BroadClassificationLineController(IServiceProvider serviceProvider,
            Winit.Modules.BroadClassification.BL.Interfaces.IBroadClassificationLineBL broadClassificationBL)
            : base(serviceProvider)
        {
            _broadClassificationLineBL = broadClassificationBL;
        }
        [HttpPost]
        [Route("GetBroadClassificationLineDetails")]
        public async Task<ActionResult> GetBroadClassificationLineDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine> pagedResponse = null;

                pagedResponse = await _broadClassificationLineBL.GetBroadClassificationLineDetails(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Fail to retrieve BroadClassificationLine");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetBroadClassificationLineDetailsByUID")]
        public async Task<ActionResult> GetBroadClassificationLineDetailsByUID(string UID)
        {
            try
            {
                List<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine> broadClassificationLines = await _broadClassificationLineBL.GetBroadClassificationLineDetailsByUID(UID);
                if (broadClassificationLines != null)
                {
                    return CreateOkApiResponse(broadClassificationLines);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve BroadClassificationLine with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateBroadClassificationLine")]
        public async Task<ActionResult> CreateBroadClassificationLine([FromBody] Winit.Modules.BroadClassification.Model.Classes.BroadClassificationLine broadClassificationLine)
        {
            try
            {
                var retVal = await _broadClassificationLineBL.CreateBroadClassificationLine(broadClassificationLine);
                if (retVal > 0)
                {
                    PerformAuditTrial(broadClassificationLine, AuditTrailCommandType.Create);
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create BroadClassificationLine details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        private void PerformAuditTrial(Winit.Modules.BroadClassification.Model.Classes.BroadClassificationLine broadClassificationLine, string commondType)
        {
            try
            {
                var newData = new Dictionary<string, object>();

                //string auditTrailData = JsonSerializer.Serialize(purchaseOrderMaster, options);

                newData["BroadClassificationLine"] = DictionaryConverter.ToDictionary(broadClassificationLine);
                var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                    linkedItemType: LinkedItemType.BroadClassificationLine,
                    linkedItemUID: broadClassificationLine?.UID,
                    commandType: commondType,
                    docNo: broadClassificationLine.ClassificationCode,
                    jobPositionUID: null,
                    empUID: broadClassificationLine?.CreatedBy,
                    empName: User.FindFirst(ClaimTypes.Name)?.Value,
                    newData: newData,
                    originalDataId: null,
                    changeData: null
                );

                LogAuditTrailInBackground(auditTrailEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(broadClassificationLine)}");
            }
        }
        [HttpPut]
        [Route("UpdateBroadClassificationLine")]
        public async Task<ActionResult> UpdateBroadClassificationLine([FromBody] Winit.Modules.BroadClassification.Model.Classes.BroadClassificationLine broadClassificationline)
        {
            try
            {
                var retVal = await _broadClassificationLineBL.UpdateBroadClassificationLine(broadClassificationline);
                if (retVal>0)
                {
                    PerformAuditTrial(broadClassificationline, AuditTrailCommandType.Update);
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating BroadClassificationLine");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteBroadClassificationLine")]
        public async Task<ActionResult> DeleteBroadClassificationHeader([FromBody] BroadClassificationLine broadClassificationline)
        {
            try
            {
                var retVal = await _broadClassificationLineBL.DeleteBroadClassificationLine(broadClassificationline.UID);
                if (retVal>0)
                {
                    PerformAuditTrialForDeleteBroadClassificationLine(broadClassificationline, AuditTrailCommandType.Delete);
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        private void PerformAuditTrialForDeleteBroadClassificationLine(BroadClassificationLine broadClassificationLine, string commondType)
        {
            try
            {
                var newData = new Dictionary<string, object>();

                //string auditTrailData = JsonSerializer.Serialize(purchaseOrderMaster, options);

                newData["BroadClassificationLine"] = DictionaryConverter.ToDictionary(broadClassificationLine);
                var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                    linkedItemType: LinkedItemType.BroadClassificationLine,
                    linkedItemUID: broadClassificationLine?.UID,
                    commandType: commondType,
                    docNo: broadClassificationLine.ClassificationCode,
                    jobPositionUID: null,
                    empUID: broadClassificationLine?.CreatedBy,
                    empName: User.FindFirst(ClaimTypes.Name)?.Value,
                    newData: newData,
                    originalDataId: null,
                    changeData: null
                );

                LogAuditTrailInBackground(auditTrailEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(broadClassificationLine)}");
            }
        }
    }
}