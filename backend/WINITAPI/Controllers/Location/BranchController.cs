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


namespace WINITAPI.Controllers.Location
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BranchController : WINITBaseController
    {
        private readonly Winit.Modules.Location.BL.Interfaces.IBranchBL _branchBL;

        public BranchController(IServiceProvider serviceProvider,
            Winit.Modules.Location.BL.Interfaces.IBranchBL branchBL)
            : base(serviceProvider)
        {
            _branchBL = branchBL;
        }
        [HttpPost]
        [Route("SelectAllBranchDetails")]
        public async Task<ActionResult> SelectAllBranchDetails(PagingRequest pagingRequest)
        {

            try
            {
                var hed = Request.Headers;
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.IBranch> PagedResponseBranchList = null;
                PagedResponseBranchList = await _branchBL.SelectAllBranchDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponseBranchList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponseBranchList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Branch Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetBranchByUID")]
        public async Task<ActionResult> GetBranchByUID(string UID)
        {
            try
            {
                Winit.Modules.Location.Model.Interfaces.IBranch branchDetails = await _branchBL.GetBranchByUID(UID);
                if (branchDetails != null)
                {
                    return CreateOkApiResponse(branchDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Branch Details with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("CreateBranch")]
        public async Task<ActionResult> CreateBranch([FromBody] Winit.Modules.Location.Model.Interfaces.IBranch CreateBranch)
        {
            try
            {
                var retVal = await _branchBL.CreateBranch(CreateBranch);
                if (retVal>0)
                {
                    PerformAuditTrial(CreateBranch, AuditTrailCommandType.Create);
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Branch details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateBranch")]
        public async Task<ActionResult> UpdateBranch([FromBody] Winit.Modules.Location.Model.Interfaces.IBranch UpdateBranch)
        {
            try
            {
                var existingbranchDetails = await _branchBL.GetBranchByUID(UpdateBranch.UID);
                if (existingbranchDetails != null)
                {
                    var retVal = await _branchBL.UpdateBranch(UpdateBranch);
                    if (retVal>0)
                    {
                        PerformAuditTrial(UpdateBranch, AuditTrailCommandType.Update);
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
                Log.Error(ex, "Error updating Branch Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        private void PerformAuditTrial(Winit.Modules.Location.Model.Interfaces.IBranch maintainBranchObject, string commondType)
        {
            try
            {
                var newData = new Dictionary<string, object>();
                newData["MaintainBranch"] = DictionaryConverter.ToDictionary(maintainBranchObject);


                var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                    linkedItemType: LinkedItemType.MaintainBranch,
                    linkedItemUID: maintainBranchObject?.UID,
                    commandType: commondType,
                    docNo: maintainBranchObject.Code,
                    jobPositionUID: null,
                    empUID: maintainBranchObject?.CreatedBy,
                    empName: User.FindFirst(ClaimTypes.Name)?.Value,
                    newData: newData,
                    originalDataId: null,
                    changeData: null
                );

                LogAuditTrailInBackground(auditTrailEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(maintainBranchObject)}");
            }
        }
        [HttpDelete]
        [Route("DeleteBranch")]
        public async Task<ActionResult> DeleteBranch([FromQuery] string UID)
        {
            try
            {
                var retVal = await _branchBL.DeleteBranch(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetBranchByLocationHierarchy")]
        public async Task<ActionResult> GetBranchByLocationHierarchy(string state, string city, string locality)
        {
            try
            {
                List<Winit.Modules.Location.Model.Interfaces.IBranch> branchDetails = await _branchBL.GetBranchByLocationHierarchy(state, city, locality);
                if (branchDetails != null)
                {
                    return CreateOkApiResponse(branchDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Branch Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
        [HttpGet]
        [Route("GetBranchByJobPositionUid")]
        public async Task<ActionResult> GetBranchByJobPositionUid(string jobPositionUid)
        {
            try
            {
                List<Winit.Modules.Location.Model.Interfaces.IBranch> branchs = await _branchBL.GetBranchsByJobPositionUid(jobPositionUid);
                if (branchs != null)
                {
                    return CreateOkApiResponse(branchs);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Branch Details with jobPositionUid: {@jobPositionUid}", jobPositionUid);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
    }
}
