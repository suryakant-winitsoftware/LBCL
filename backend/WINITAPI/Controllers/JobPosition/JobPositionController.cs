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

namespace WINITAPI.Controllers.JobPosition
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobPositionController : WINITBaseController
    {
        private readonly Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL _jobPositionBL;

        public JobPositionController(IServiceProvider serviceProvider,
            Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL jobPositionBL)
            : base(serviceProvider)
        {
            _jobPositionBL = jobPositionBL;
        }
        [HttpPost]
        [Route("SelectAllJobPositionDetails")]
        public async Task<ActionResult> SelectAllJobPositionDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition> pagedResponseJobPositionList = null;

                pagedResponseJobPositionList = await _jobPositionBL.SelectAllJobPositionDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseJobPositionList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseJobPositionList);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve JobPositionDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetJobPositionByUID")]
        public async Task<ActionResult> GetJobPositionByUID(string UID)
        {
            try
            {
                Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPositionDetails = await _jobPositionBL.GetJobPositionByUID(UID);
                if (JobPositionDetails != null)
                {
                    return CreateOkApiResponse(JobPositionDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve JobPositionDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateJobPosition1")]
        public async Task<ActionResult> UpdateJobPosition1([FromBody] Winit.Modules.JobPosition.Model.Classes.JobPositionApprovalDTO jobPositionApprovalDTO)
        {
            try
            {
                var existingDetails = await _jobPositionBL.GetJobPositionByUID(jobPositionApprovalDTO.JobPosition.UID);
                if (existingDetails != null)
                {
                    jobPositionApprovalDTO.JobPosition.ServerModifiedTime = DateTime.Now;
                    var updateJobPositionDetails = await _jobPositionBL.UpdateJobPosition1(jobPositionApprovalDTO);
                    PerformAuditTrialJobPositionRoleMapping(jobPositionApprovalDTO.JobPosition);
                    return (updateJobPositionDetails > 0) ? CreateOkApiResponse(updateJobPositionDetails) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating JobPosition Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateJobPosition")]
        public async Task<ActionResult> CreateJobPosition([FromBody] Winit.Modules.JobPosition.Model.Classes.JobPosition jobPosition)
        {
            try
            {
                jobPosition.ServerAddTime = DateTime.Now;
                jobPosition.ServerModifiedTime = DateTime.Now;
                var jobPositionDetails = await _jobPositionBL.CreateJobPosition(jobPosition);
                return (jobPositionDetails > 0) ? CreateOkApiResponse(jobPositionDetails) : throw new Exception("Create Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create JobPosition details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpPut]
        [Route("UpdateJobPosition")]
        public async Task<ActionResult> UpdateJobPosition([FromBody] Winit.Modules.JobPosition.Model.Classes.JobPosition jobPosition)
        {
            try
            {
                var existingDetails = await _jobPositionBL.GetJobPositionByUID(jobPosition.UID);
                if (existingDetails != null)
                {
                    jobPosition.ServerModifiedTime = DateTime.Now;
                    var updateJobPositionDetails = await _jobPositionBL.UpdateJobPosition(jobPosition);
                    if (updateJobPositionDetails>0)
                    {
                        PerformAuditTrialJobPositionRoleMapping(jobPosition);
                    }
                    return (updateJobPositionDetails > 0) ? CreateOkApiResponse(updateJobPositionDetails) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating JobPosition Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        private void PerformAuditTrialJobPositionRoleMapping(Winit.Modules.JobPosition.Model.Interfaces.IJobPosition jobPosition)
        {
            try
            {
                var newData = new Dictionary<string, object>();
                newData["JobPositionRoleMapping"] = DictionaryConverter.ToDictionary(jobPosition);
                var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                    linkedItemType: LinkedItemType.EmployeeRoleMapping,
                    linkedItemUID: jobPosition?.UID,
                    commandType: AuditTrailCommandType.Create,
                    docNo: jobPosition.BranchUID,
                    jobPositionUID: null,
                    empUID: jobPosition?.CreatedBy,
                    empName: User.FindFirst(ClaimTypes.Name)?.Value,
                    newData: newData,
                    originalDataId: null,
                    changeData: null
                );

                LogAuditTrailInBackground(auditTrailEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(jobPosition)}");
            }
        }
        [HttpPut]
        [Route("UpdateJobPositionLocationTypeAndValue")]
        public async Task<ActionResult> UpdateJobPositionLocationTypeAndValue([FromBody] Winit.Modules.JobPosition.Model.Classes.JobPosition jobPosition)
        {
            try
            {
                var existingDetails = await _jobPositionBL.GetJobPositionLocationTypeAndValueByUID(jobPosition.UID);
                if (existingDetails != null)
                {
                    jobPosition.ServerModifiedTime = DateTime.Now;
                    var updateJobPositionLocationTypeAndValueDetails = await _jobPositionBL.UpdateJobPositionLocationTypeAndValue(jobPosition);
                    PerformAuditTrialForJobPositionLocationTypeAndValue(jobPosition);
                    return (updateJobPositionLocationTypeAndValueDetails > 0) ? CreateOkApiResponse(updateJobPositionLocationTypeAndValueDetails) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating JobPosition Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        private void PerformAuditTrialForJobPositionLocationTypeAndValue(Winit.Modules.JobPosition.Model.Classes.JobPosition jobPosition)
        {
            try
            {
                var newData = new Dictionary<string, object>();
                newData["JobPositionLocationTypeAndValue"] = DictionaryConverter.ToDictionary(jobPosition);
                var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                    linkedItemType: LinkedItemType.EmployeeLocationMTypeAndValue,
                    linkedItemUID: jobPosition?.UID,
                    commandType: AuditTrailCommandType.Create,
                    docNo: jobPosition?.EmpCode,
                    jobPositionUID: null,
                    empUID: jobPosition?.CreatedBy,
                    empName: User.FindFirst(ClaimTypes.Name)?.Value,
                    newData: newData,
                    originalDataId: null,
                    changeData: null
                );

                LogAuditTrailInBackground(auditTrailEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(jobPosition)}");
            }
        }

        [HttpDelete]
        [Route("DeleteJobPosition")]
        public async Task<ActionResult> DeleteJobPosition([FromQuery] string UID)
        {
            try
            {
                var result = await _jobPositionBL.DeleteJobPosition(UID);
                return (result > 0) ? CreateOkApiResponse(result) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectJobPositionByEmpUID")]
        public async Task<ActionResult> SelectJobPositionByEmpUID(string EmpUID)
        {
            try
            {
                Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPositionDetails = await _jobPositionBL.SelectJobPositionByEmpUID(EmpUID);
                if (JobPositionDetails != null)
                {
                    return CreateOkApiResponse(JobPositionDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve JobPositionDetails with EmpUID: {@EmpUID}", EmpUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }
    }
}
