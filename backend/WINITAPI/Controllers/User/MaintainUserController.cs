using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.Currency.BL.Interfaces;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Tax.BL.Interfaces;
using Winit.Modules.User.Model.Interface;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITAPI.Common;


namespace WINITAPI.Controllers.Bank
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaintainUserController : WINITBaseController
    {
        private readonly Winit.Modules.User.BL.Interfaces.IMaintainUserBL _maintainUserBL;
        private readonly Winit.Modules.Emp.BL.Interfaces.IEmpBL _empBL;
        private readonly RSAHelperMethods _rSAHelperMethods;
        ICurrencyBL _currencyBL;
        ISettingBL _settingBL;
        ITaxMasterBL _taxMasterBL;
        IOrgBL _orgBL;
        private readonly IStoreBL _storeBl;

        public MaintainUserController(IServiceProvider serviceProvider,
            Winit.Modules.User.BL.Interfaces.IMaintainUserBL maintainUserBL,
            RSAHelperMethods rSAHelperMethods, IEmpBL empBL,
            ICurrencyBL currencyBL, ISettingBL settingBL, ITaxMasterBL taxMasterBL, IOrgBL orgBL, IStoreBL storeBl)
            : base(serviceProvider)
        {
            _maintainUserBL = maintainUserBL;
            _rSAHelperMethods = rSAHelperMethods;
            _empBL = empBL;
            _currencyBL = currencyBL;
            _settingBL = settingBL;
            _taxMasterBL = taxMasterBL;
            _orgBL = orgBL;
            _storeBl = storeBl;
        }
        [HttpPost]
        [Route("SelectAllMaintainUserDetails")]
        public async Task<ActionResult> SelectAllMaintainUserDetails(PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.User.Model.Interfaces.IMaintainUser> PagedResponse = null;
                PagedResponse = await _maintainUserBL.SelectAllMaintainUserDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve MaintainUser details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("CUDEmployee")]
        public async Task<ActionResult> CUDEmployee([FromBody] Winit.Modules.User.Model.Classes.EmpDTOModel empDTO)
        {
            var retVal = -1;
            try
            {
                if (empDTO != null)
                {
                    //string encryptPassword = string.Empty;
                    var emp = await _empBL.GetEmpByUID(empDTO.Emp.UID);
                    if (emp != null) empDTO.Emp.ActionType = ActionType.Update;
                    else empDTO.Emp.ActionType = ActionType.Add;
                    if (!string.IsNullOrEmpty(empDTO.Emp.EncryptedPassword) && empDTO.Emp.ActionType == ActionType.Add)
                    {
                        empDTO.Emp.EncryptedPassword = _rSAHelperMethods.EncryptText(empDTO.Emp.EncryptedPassword);
                    }
                    retVal = await _maintainUserBL.CUDEmployee(empDTO, empDTO.Emp.EncryptedPassword);
                    PerformAuditTrial(empDTO);
                }
                return (retVal > 0) ? CreateOkApiResponse(retVal) : throw new Exception("Insert Failed");

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create User details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        private void PerformAuditTrial(Winit.Modules.User.Model.Classes.EmpDTOModel empDTOModel)
        {
            try
            {
                var newData = new Dictionary<string, object>();

                newData["Emp"] = DictionaryConverter.ToDictionary(empDTOModel.Emp);
                newData["EmpInfo"] = DictionaryConverter.ToDictionary(empDTOModel.EmpInfo);

                var auditTrailEntry = _auditTrailHelper.CreateAuditTrailEntry(
                    linkedItemType: LinkedItemType.EmployeeDetails,
                linkedItemUID: empDTOModel.Emp?.UID,
                    commandType: empDTOModel.Emp.ActionType.ToString(),
                    docNo: empDTOModel.Emp?.Code,
                    jobPositionUID: null,
                    empUID: empDTOModel.Emp?.CreatedBy,
                    empName: User.FindFirst(ClaimTypes.Name)?.Value,
                    newData: newData,
                    originalDataId: null,
                    changeData: null
                );

                LogAuditTrailInBackground(auditTrailEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing audit trail for purchase Order with data {System.Text.Json.JsonSerializer.Serialize(empDTOModel)}");
            }
        }
        [HttpGet]
        [Route("SelectMaintainUserDetailsByUID")]
        public async Task<ActionResult> SelectMaintainUserDetailsByUID(string empUID)
        {
            try
            {
                IEmpDTO empDTO = await _maintainUserBL.SelectMaintainUserDetailsByUID(empUID);

                if (empDTO != null)
                {
                    return CreateOkApiResponse(empDTO);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve User Details: {@UID}", empUID);
                return CreateErrorResponse("An error occurred while processing the request. " + ex.Message);
            }
        }
        [HttpPost]
        [Route("SelectUserRolesDetails")]
        public async Task<ActionResult> SelectUserRolesDetails(PagingRequest pagingRequest, string empUID)
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
                PagedResponse<Winit.Modules.User.Model.Interfaces.IUserRoles> PagedResponse = null;
                PagedResponse = await _maintainUserBL.SelectUserRolesDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired, empUID);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve User Details details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost]
        [Route("SelectUserFranchiseeMappingDetails")]
        public async Task<ActionResult> SelectUserFranchiseeMappingDetails(PagingRequest pagingRequest, string JobPositionUID, string OrgTypeUID, string ParentUID)
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
                PagedResponse<Winit.Modules.User.Model.Interfaces.IUserFranchiseeMapping> PagedResponse = null;
                PagedResponse = await _maintainUserBL.SelectUserFranchiseeMappingDetails(pagingRequest.SortCriterias,
                pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                pagingRequest.IsCountRequired, JobPositionUID, OrgTypeUID, ParentUID);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve UserFranchiseeMapping details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetAllUserMasterDataByLoginID")]
        public async Task<ActionResult> GetAllUserMasterDataByLoginID(string LoginID)
        {
            try
            {
                var data = await _maintainUserBL.GetAllUserMasterDataByLoginID(LoginID);
                if (data == null)
                {
                    return NotFound();
                }
                else
                {
                    data.Currency = (await _currencyBL.GetOrgCurrencyListBySelectedOrg(data.JobPosition.OrgUID)).ToList();
                    var settings = await _settingBL.SelectAllSettingDetails(sortCriterias: null, pageNumber: 0,
                    pageSize: 0, filterCriterias: null, isCountRequired: false);
                    if (settings != null && settings.PagedData != null & settings.PagedData.Any())
                    {
                        data.Settings = settings.PagedData.ToList();
                    }
                    var tax = await _taxMasterBL.GetTaxMaster([data.JobPosition.OrgUID]);
                    if (tax != null)
                    {
                        data.TaxMaster = tax.ToDictionary(e => e.Tax.UID, e => e.Tax);
                    }
                    data.OrgUIDs = await _orgBL.GetOrgHierarchyParentUIDsByOrgUID([data.JobPosition.OrgUID]);
                    if (data.Role != null && data.Role.Code == "ASM")
                    {
                        data.AsmDivisions = (await _storeBl.GetDivisionsByAsmEmpUID(data.Emp.UID)).ToList();
                    }
                }
                return CreateOkApiResponse(data);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve User details. Exception: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }
        [HttpPost]
        [Route("GetAplicableOrgs")]
        public async Task<ActionResult> GetAplicableOrgs(string orgUID)
        {
            try
            {
                IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> selectionAplicableOrgs = null;
                selectionAplicableOrgs = await _maintainUserBL.GetAplicableOrgs(orgUID);
                if (selectionAplicableOrgs == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(selectionAplicableOrgs);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve AplicableOrgs details");
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
                userHierarchies = await _maintainUserBL.GetUserHierarchyForRule(hierarchyType, hierarchyUID, ruleId);
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
    }
}
