using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.Model.Constant;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.Role
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : WINITBaseController
    {
        private readonly Winit.Modules.Role.BL.Interfaces.IRoleBL _roleBL;
        public RoleController(IServiceProvider serviceProvider, Winit.Modules.Role.BL.Interfaces.IRoleBL roleBL)
            : base(serviceProvider)
        {
            _roleBL = roleBL;
        }
        [HttpPost]
        [Route("SelectAllRoles")]
        public async Task<ActionResult> SelectAllRoles([FromBody] PagingRequest pagingRequest)
        {
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            try
            {
                return CreateOkApiResponse(await _roleBL.SelectAllRole(pagingRequest.SortCriterias, pagingRequest.PageNumber,
                                            pagingRequest.PageSize, pagingRequest.FilterCriterias, pagingRequest.IsCountRequired));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Role");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CreateRole")]
        public async Task<ActionResult> CreateRole([FromBody] Winit.Modules.Role.Model.Classes.Role role)
        {
            try
            {
                var result = await _roleBL.CreateRoles(role);

                if (result != null)  // Ensure the first method executed successfully
                {
                    PerformAuditTrial(role, AuditTrailCommandType.Create); // Call additional method
                }

                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to Create Role");
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        private void PerformAuditTrial(Winit.Modules.Role.Model.Classes.Role role, string commondType)
        {
            try
            {
                var newData = new Dictionary<string, object>();
                newData["MaintainUserRole"] = DictionaryConverter.ToDictionary(role);
                var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                    linkedItemType: LinkedItemType.MaintainUserRole,
                    linkedItemUID: role?.UID,
                    commandType: commondType,
                    docNo: role?.Code,
                    jobPositionUID: null,
                    empUID: role?.CreatedBy,
                    empName: User.FindFirst(ClaimTypes.Name)?.Value,
                    newData: newData,
                    originalDataId: null,
                    changeData: null
                );

                LogAuditTrailInBackground(auditTrailEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(role)}");
            }
        }

        [HttpPut]
        [Route("UpdateRoles")]
        public async Task<ActionResult> UpdateRoles([FromBody] Winit.Modules.Role.Model.Classes.Role role)
        {
            try
            {
                var result = await _roleBL.UpdateRoles(role);

                if (result != null)  // Ensure the first method executed successfully
                {
                    PerformAuditTrial(role, AuditTrailCommandType.Update); // Call additional method
                }
                return CreateOkApiResponse(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to update Role");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetAllModulesMaster")]
        public async Task<ActionResult> GetAllModulesMaster(string Platform)
        {
            try
            {
                return CreateOkApiResponse(await _roleBL.GetAllModulesMaster(Platform));
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Role");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetModulesMasterByPlatForm")]
        public async Task<ActionResult> GetModulesMasterByPlatForm(string Platform)
        {
            try
            {
                return CreateOkApiResponse(await _roleBL.GetModulesMasterByPlatForm(Platform));
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Module Masters");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectAllPermissionsByRoleUID")]
        public async Task<ActionResult> SelectAllPermissionsByRoleUID(string roleUID, string platform, bool isPrincipalTypePermission)
        {
            try
            {
                return CreateOkApiResponse(await _roleBL.SelectAllPermissions(roleUID, platform, isPrincipalTypePermission));
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve Permissions");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUDPermissionMaster")]
        public async Task<ActionResult> CUDPermissionMaster(PermissionMaster permissionMaster)
        {
            try
            {
                return CreateOkApiResponse(await _roleBL.CUDPermissionMaster(permissionMaster));
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to save Permissions");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectRolesByOrgUID")]
        public async Task<ActionResult> SelectRolesByOrgUID(string orgUID, bool IsAppUser)
        {
            try
            {
                IEnumerable<Winit.Modules.Role.Model.Interfaces.IRole> roleList = await _roleBL.SelectRolesByOrgUID(orgUID, IsAppUser);
                if (roleList != null)
                {
                    return CreateOkApiResponse(roleList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve RoleDetails with orgUID: {@orgUID}", orgUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectRolesByUID")]
        public async Task<ActionResult> SelectRolesByUID(string uid)
        {
            try
            {
                Winit.Modules.Role.Model.Interfaces.IRole role = await _roleBL.SelectRolesByUID(uid);
                if (role != null)
                {
                    return CreateOkApiResponse(role);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Role with UID: {@UID}", uid);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetPermissionByRoleAndPage")]
        public async Task<ActionResult> GetPermissionByRoleAndPage(string roleUID, string relativePath, bool isPrincipleRole)
        {
            try
            {
                IPermission permission = await _roleBL.GetPermissionByRoleAndPage(roleUID, relativePath, isPrincipleRole);
                if (permission != null)
                {
                    return CreateOkApiResponse(permission);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Role with UID: {@UID}", roleUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateMenuByPlatForm")]
        public async Task<ActionResult> UpdateMenuByPlatForm([FromQuery] string platForm)
        {
            try
            {
                return CreateOkApiResponse(await _roleBL.UpdateMenuByPlatForm(platForm));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to Update Menu");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
