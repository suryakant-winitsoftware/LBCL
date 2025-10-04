using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.Models.Common;
using WINITServices.Interfaces.CacheHandler;


namespace WINITAPI.Controllers.ApprovalEngine
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApprovalEngineController : WINITBaseController
    {
        private readonly Winit.Modules.ApprovalEngine.BL.Interfaces.IApprovalEngineBL _IApprovalEngineBL;

        public ApprovalEngineController(IServiceProvider serviceProvider, 
            Winit.Modules.ApprovalEngine.BL.Interfaces.IApprovalEngineBL approvalEngineBL 
            ) : base(serviceProvider)
        {
            _IApprovalEngineBL=approvalEngineBL;
        }

        [HttpGet]
        [Route("GetApprovalLog")]
        public async Task<ActionResult> GetApprovalLog([FromQuery] string requestId)
        {
            try
            {
                IEnumerable<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalLog> approvalLog = await _IApprovalEngineBL.GetApprovalLog(requestId);
                if (approvalLog != null)
                {
                    return CreateOkApiResponse(approvalLog);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Approval Log with RequestId: {@requestId}", requestId);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetRoleNames")]
        public async Task<ActionResult> GetRoleNames()
        {
            try
            {
                List<ISelectionItem> approvalLogDictionary = await _IApprovalEngineBL.GetRoleNames();
                if (approvalLogDictionary != null)
                {
                    return CreateOkApiResponse(approvalLogDictionary);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve roe Role Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetRuleId")]
        public async Task<ActionResult> GetRuleId([FromQuery] string type, [FromQuery] string typeCode)
        {
            try
            {
                int ruleId = await _IApprovalEngineBL.GetRuleId(type, typeCode);
                if (ruleId != 0)
                {
                    return CreateOkApiResponse(ruleId);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve rule id");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("DropDownsForApprovalMapping")]
        public async Task<ActionResult> DropDownsForApprovalMapping()
        {
            try
            {
                List<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMap> approvalLog = await _IApprovalEngineBL.DropDownsForApprovalMapping();
                if (approvalLog != null)
                {
                    return CreateOkApiResponse(approvalLog);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve the Data");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("IntegrateRule")]
        public async Task<ActionResult> IntegrateRule([FromBody] Winit.Modules.ApprovalEngine.Model.Classes.ApprovalRuleMapping approRuleMap)
        {
            try
            {
                int retValue = await _IApprovalEngineBL.IntegrateCreatedRule(approRuleMap);
                if (retValue > 0)
                {
                    return CreateOkApiResponse(retValue);
                }
                else { throw new Exception("Insert Failed"); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Create AllApprovalRequest  details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }


        [HttpGet]
        [Route("GetAllChangeRequest")]
        public async Task<ActionResult> GetAllChangeRequest()
        {
            try
            {
                IEnumerable<IViewChangeRequestApproval> viewChangeRequestApproval = await _IApprovalEngineBL.GetAllChangeRequest();
                if (viewChangeRequestApproval != null)
                {
                    return CreateOkApiResponse(viewChangeRequestApproval);
                }
                else { return NotFound(); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Request Change Approval Log ");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetApprovalDetailsByLinkedItemUid")]
        public async Task<ActionResult> GetApprovalDetailsByLinkedItemUid([FromQuery] string requestUid)
        {
            try
            {
                IAllApprovalRequest allApprovalRequest = await _IApprovalEngineBL.GetApprovalDetailsByLinkedItemUid(requestUid);
                if (allApprovalRequest != null)
                {
                    return CreateOkApiResponse(allApprovalRequest);
                }
                else { return NotFound(); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Request Change data");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetApprovalHierarchy")]
        public async Task<ActionResult> GetApprovalHierarchy([FromQuery] string ruleId)
        {
            try
            {
                IEnumerable<IApprovalHierarchy> allApprovalRequest = await _IApprovalEngineBL.GetApprovalHierarchy(ruleId);
                if (allApprovalRequest != null)
                {
                    return CreateOkApiResponse(allApprovalRequest);
                }
                else { return NotFound(); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Request Change data");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetChangeRequestDataByUid")]
        public async Task<ActionResult> GetChangeRequestDataByUid([FromQuery] string requestUid)
        {
            try
            {
                IViewChangeRequestApproval allApprovalRequest = await _IApprovalEngineBL.GetChangeRequestDataByUid(requestUid);
                if (allApprovalRequest != null)
                {
                    return CreateOkApiResponse(allApprovalRequest);
                }
                else { return NotFound(); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Request Change data");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateChangesInMainTable")]
        public async Task<ActionResult> UpdateChangesInMainTable([FromBody] IViewChangeRequestApproval viewChangeRequestApproval)
        {
            try
            {
                int retVal = await _IApprovalEngineBL.UpdateChangesInMainTable(viewChangeRequestApproval);
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else { return NotFound(); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Request Change data");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        #region Approval Delete Logic

        [HttpDelete]
        [Route("DeleteApprovalRequest")]
        public async Task<ActionResult> DeleteApprovalRequest([FromQuery] string requestId)
        {
            try
            {
                int ruleId = await _IApprovalEngineBL.DeleteApprovalRequest(requestId);
                if (ruleId != 0)
                {
                    return CreateOkApiResponse(ruleId);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
            finally
            {

            }
        }
        #endregion
        [HttpGet]
        [Route("GetApprovalRuleMasterData")]
        public async Task<ActionResult> GetApprovalRuleMasterData()
        {
            try
            {


                List<IApprovalRuleMaster> approvalRuleMaster = await _IApprovalEngineBL.GetApprovalRuleMasterData();
                if (approvalRuleMaster == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(approvalRuleMaster);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Setting  Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetUserHierarchyForRule/{hierarchyType}/{hierarchyUID}/{ruleId}")]
        public async Task<IActionResult> GetUserHierarchyForRule(string hierarchyType, string hierarchyUID, int ruleId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hierarchyType) || string.IsNullOrWhiteSpace(hierarchyUID))
                {
                    return BadRequest("Invalid input parameters.");
                }

                IEnumerable<IUserHierarchy> userHierarchies = null;
                userHierarchies = await _IApprovalEngineBL.GetUserHierarchyForRule(hierarchyType, hierarchyUID, ruleId);
                if (userHierarchies == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(userHierarchies);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve GetUserHierarchyForRule details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetChangeRequestData")]
        public async Task<ActionResult> GetChangeRequestData([FromBody] PagingRequest pagingRequest)
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

                var pagedResponse = await _IApprovalEngineBL.GetChangeRequestData(pagingRequest.SortCriterias,
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
                Log.Error(ex, "Failed to retrieve SalesPromotionSchemes");
                return StatusCode(500, "An error occurred while processing the request. " + ex.Message);
            }
        }
    }

}
