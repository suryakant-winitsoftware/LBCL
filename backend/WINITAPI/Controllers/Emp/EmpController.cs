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
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.Emp
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmpController : WINITBaseController
    {
        private readonly Winit.Modules.Emp.BL.Interfaces.IEmpBL _EmpBL;
        public EmpController(IServiceProvider serviceProvider, 
            Winit.Modules.Emp.BL.Interfaces.IEmpBL EmpBL
            ) : base(serviceProvider)
        {
            _EmpBL = EmpBL;
        }

        [HttpPost]
        [Route("GetEmpDetails")]
        public async Task<ActionResult> GetEmpDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmp> pagedResponseEmpList = null;
                pagedResponseEmpList = await _EmpBL.GetEmpDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseEmpList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponseEmpList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve EmpDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetEmpByUID")]
        public async Task<ActionResult> GetEmpByUID(string UID)
        {
            try
            {
                Winit.Modules.Emp.Model.Interfaces.IEmp EmpDetails = await _EmpBL.GetEmpByUID(UID);
                if (EmpDetails != null)
                {
                    return CreateOkApiResponse(EmpDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve EmpDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetBMByBranchUID")]
        public async Task<ActionResult> GetBMByBranchUID(string UID)
        {
            try
            {
                Winit.Modules.Emp.Model.Interfaces.IEmp EmpDetails = await _EmpBL.GetBMByBranchUID(UID);
                if (EmpDetails != null)
                {
                    return CreateOkApiResponse(EmpDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve EmpDetails with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CreateEmp")]
        public async Task<ActionResult> CreateEmp([FromBody] Winit.Modules.Emp.Model.Classes.Emp Emp)
        {
            try
            {
                Emp.ServerModifiedTime = DateTime.Now;
                var retVal = await _EmpBL.CreateEmp(Emp, null);
                return (retVal > 0) ? Created("Created", retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Emp details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPut]
        [Route("UpdateEmp")]
        public async Task<ActionResult> UpdateEmp([FromBody] Winit.Modules.Emp.Model.Classes.Emp updateEmp)
        {
            try
            {
                updateEmp.ServerModifiedTime = DateTime.Now;
                var existingEmpDetails = await _EmpBL.GetEmpByUID(updateEmp.UID);
                if (existingEmpDetails != null)
                {
                    updateEmp.ServerModifiedTime = DateTime.Now;
                    var retVal = await _EmpBL.UpdateEmp(updateEmp, null);
                    return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Update Failed");
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating EmpDetails");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteEmp")]
        public async Task<ActionResult> DeleteEmp([FromQuery] string UID)
        {
            try
            {
                var retVal = await _EmpBL.DeleteEmp(UID);
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Delete Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failure");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetEmpByLoginId")]
        public async Task<ActionResult> GetEmpByLoginId(string LoginId)
        {
            try
            {
                Winit.Modules.Emp.Model.Interfaces.IEmp EmpDetails = await _EmpBL.GetEmpByUID(LoginId);
                if (EmpDetails != null)
                {
                    return CreateOkApiResponse(EmpDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve EmpDetails with UID: {@LoginId}", LoginId);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetReportsToEmployeesByRoleUID")]
        public async Task<ActionResult> GetReportsToEmployeesByRoleUID(string roleUID)
        {
            try
            {
                IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmp> EmpDetails = await _EmpBL.GetReportsToEmployeesByRoleUID(roleUID);
                if (EmpDetails != null)
                {
                    return CreateOkApiResponse(EmpDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve EmpDetails with UID: {@UID}", roleUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetEmployeesByRoleUID")]
        public async Task<ActionResult> GetEmployeesByRoleUID(string orgUID, string roleUID)
        {
            try
            {
                IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmp> EmpDetails = await _EmpBL.GetEmployeesByRoleUID(orgUID, roleUID);
                if (EmpDetails != null)
                {
                    return CreateOkApiResponse(EmpDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve EmpDetails with UID: {@UID}", roleUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetEmployeesSelectionItemByRoleUID")]
        public async Task<ActionResult> GetEmployeesSelectionItemByRoleUID(string orgUID, string roleUID)
        {
            try
            {
                IEnumerable<ISelectionItem> EmpDetails = await _EmpBL.GetEmployeesSelectionItemByRoleUID(orgUID, roleUID);
                if (EmpDetails != null)
                {
                    return CreateOkApiResponse(EmpDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve EmpDetails with UID: {@UID}", roleUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetEmpViewByUID")]
        public async Task<ActionResult> GetEmpViewByUID(string empUID)
        {
            try
            {
                IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmpView> empViewDetails = await _EmpBL.GetEmpViewByUID(empUID);
                if (empViewDetails != null)
                {
                    return CreateOkApiResponse(empViewDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve EmpViewDetails with UID: {@UID}", empUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetASMList")]
        public async Task<ActionResult> GetASMList([FromQuery]string branchUID, [FromQuery] string Code = "ASM")
        {
            try
            {
                List<Winit.Modules.Emp.Model.Interfaces.IEmp> empDetails = await _EmpBL.GetASMList(branchUID, Code);
                if (empDetails != null)
                {
                    return CreateOkApiResponse(empDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Emp");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAllASM")]
        public async Task<ActionResult> GetAllASM()
        {
            try
            {
                List<Winit.Modules.Emp.Model.Interfaces.IEmp> empDetails = await _EmpBL.GetAllASM();
                if (empDetails != null)
                {
                    return CreateOkApiResponse(empDetails);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Emp");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetEscalationMatrix")]
        public async Task<ActionResult> GetEscalationMatrix([FromQuery] string jobPositionUid)
        {
            try
            {
                var result = await _EmpBL.GetEscalationMatrixAsync(jobPositionUid);
                if (result == null || !result.Any())
                    return NotFound();
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Escalation Matrix for JobPositionUID: {@jobPositionUid}", jobPositionUid);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
