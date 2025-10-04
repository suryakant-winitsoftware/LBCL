using Newtonsoft.Json;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SchemeApprovalEngineBaseViewModel : ISchemeApprovalEngineBaseViewModel
    {

        protected readonly Winit.Shared.Models.Common.IAppConfig _appConfig;
        protected readonly ApiService _apiService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppUser _appUser;
        protected readonly ILoadingService _loadingService;
        protected readonly IAlertService _alertService;
        protected readonly IAppSetting _appSetting;
        protected readonly CommonFunctions _commonFunctions;
        public SchemeApprovalEngineBaseViewModel(Winit.Shared.Models.Common.IAppConfig appConfig, ApiService apiService,
            IServiceProvider serviceProvider, IAppUser appUser, ILoadingService loadingService, IAlertService alertService,
            IAppSetting appSetting, CommonFunctions commonFunctions)
        {
            _apiService = apiService;
            _appConfig = appConfig;
            _serviceProvider = serviceProvider;
            _appUser = appUser;
            _loadingService = loadingService;
            _alertService = alertService;
            _appSetting = appSetting;
            _commonFunctions = commonFunctions;

            UserRoleCode = appUser.Role.Code;
            IsReassignButtonNeeded = _appSetting.IsReassignNeededInScheme;
        }
        public IStore SelectedChannelPartner { get; set; }
        public string UserRoleCode { get; set; }
        public string UserType { get; set; }
        public int RequestId { get; set; }
        public int RuleId { get; set; }
        public List<IAllApprovalRequest> AllApprovalLevelList { get; set; } = [];
        public bool IsReassignButtonNeeded { get; set; }
        public Dictionary<string, List<EmployeeDetail>>? ApprovalUserCodes { get; set; }
        public void GetUserTypeWhileCreatingScheme(bool isPositiveScheme, out string userType, out int ruleId)
        {
            if (SchemeConstants.SARoleCode.Equals(_appUser.Role.Code, StringComparison.OrdinalIgnoreCase))
            {
                if (isPositiveScheme)
                {
                    ruleId = GetRuleIdByName(SchemeConstants.RuleNameForSA_SchemeWithPositiveMargin);
                    userType = SchemeConstants.SA_UserTypeWithePositiveScheme;
                }
                else
                {
                    ruleId = GetRuleIdByName(SchemeConstants.RuleNameForSA_SchemeWithNegativeMargin);
                    userType = SchemeConstants.SA_UserTypeWitheNegativeScheme;
                }
            }
            else if (SchemeConstants.ASMRoleCode.Equals(_appUser.Role.Code, StringComparison.OrdinalIgnoreCase))
            {
                if (isPositiveScheme)
                {
                    ruleId = GetRuleIdByName(SchemeConstants.RuleNameForASM_SchemeWithPositiveMargin);
                    //ruleId = SchemeConstants.ASM_SchemeWithPositiveMargin;
                    userType = SchemeConstants.ASM_UserTypeWithePositiveScheme;
                }
                else
                {
                    ruleId = GetRuleIdByName(SchemeConstants.RuleNameForASM_SchemeWithNegativeMargin);
                    // ruleId = SchemeConstants.ASM_SchemeWithNegativeMargin;
                    userType = SchemeConstants.ASM_UserTypeWitheNegativeScheme;
                }
            }
            else if (SchemeConstants.BUHSRoleCode.Equals(_appUser.Role.Code, StringComparison.OrdinalIgnoreCase))
            {
                if (isPositiveScheme)
                {
                    ruleId = GetRuleIdByName(SchemeConstants.RuleNameForBUHS_SchemeWithPositiveMargin);
                    // ruleId = SchemeConstants.BUHS_SchemeWithPositiveMargin;
                    userType = SchemeConstants.BUHS_UserTypeWithePositiveScheme;
                }
                else
                {
                    ruleId = GetRuleIdByName(SchemeConstants.RuleNameForBUHS_SchemeWithNegativeMargin);
                    // ruleId = SchemeConstants.BUHS_SchemeWithNegativeMargin;
                    userType = SchemeConstants.BUHS_UserTypeWitheNegativeScheme;
                }
            }
            else
            {
                if (isPositiveScheme)
                {
                    ruleId = GetRuleIdByName(SchemeConstants.RuleNameForSchemeWithPositiveMargin);
                    // ruleId = SchemeConstants.SchemeWithPositiveMargin;
                    userType = SchemeConstants.UserTypeWithePositiveScheme;
                }
                else
                {
                    ruleId = GetRuleIdByName(SchemeConstants.RuleNameForSchemeWithNegativeMargin);
                    //ruleId = SchemeConstants.SchemeWithNegativeMargin;
                    userType = SchemeConstants.UserTypeWitheNegativeScheme;
                }
            }
        }
        public void SetUserTypeWhileCreatingScheme()
        {
            if (SchemeConstants.SARoleCode.Equals(_appUser.Role.Code, StringComparison.OrdinalIgnoreCase))
            {
                RuleId = GetRuleIdByName(SchemeConstants.RuleNameForSA_StandingProvisionRule);
                // RuleId = SchemeConstants.SA_StandingProvisionRule;
                UserType = SchemeConstants.SA_StandingProvisionUserType;
            }
            else
            {
                RuleId = GetRuleIdByName(SchemeConstants.RuleNameForBUHS_StandingProvisionRule);
                //RuleId = SchemeConstants.BUHS_StandingProvisionRule;
                UserType = SchemeConstants.BUHS_StandingProvisionUserType;
            }
        }
        public int GetRuleIdByName(string ruleName)
        {
            try
            {
                return _appUser.ApprovalRuleMaster.Where(item => item.RuleName == ruleName).Select(item => item.RuleId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return default;
            }
        }
        public async Task PopulateApprovalEngine(string linkedItemUID)
        {
            AllApprovalLevelList = await GetAllApproveListDetails(linkedItemUID);
            if (AllApprovalLevelList != null && AllApprovalLevelList.Count > 0)
            {
                RequestId = int.Parse(AllApprovalLevelList?.FirstOrDefault()?.RequestID ?? "0");

                var approvalUserDetail = AllApprovalLevelList?.FirstOrDefault()?.ApprovalUserDetail;
                ApprovalUserCodes = string.IsNullOrEmpty(approvalUserDetail)
                ? new Dictionary<string, List<EmployeeDetail>>()
                : DeserializeApprovalUserCodes(approvalUserDetail) ?? new Dictionary<string, List<EmployeeDetail>>();
            }
        }
        private Dictionary<string, List<EmployeeDetail>> DeserializeApprovalUserCodes(string approvalUserDetail)
        {
            try
            {
                // First, attempt to deserialize assuming values are List<EmployeeDetail>
                return JsonConvert.DeserializeObject<Dictionary<string, List<EmployeeDetail>>>(approvalUserDetail);
            }
            catch (JsonSerializationException)
            {
                // If it fails, handle the case where values are List<string>
                var stringListDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(approvalUserDetail);

                if (stringListDictionary != null)
                {
                    // Convert List<string> to List<EmployeeDetail>
                    return stringListDictionary.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Select(code => new EmployeeDetail { EmpCode = code, EmpName = code }).ToList()
                    );
                }

                // If all deserialization attempts fail, return null
                return null;
            }
        }
        public async Task<List<IAllApprovalRequest>> GetAllApproveListDetails(string UID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<AllApprovalRequest>> AllApprovalLevelDetails = await
                    _apiService.FetchDataAsync<List<AllApprovalRequest>>
                    ($"{_appConfig.ApiBaseUrl}Store/GetApprovalDetailsByStoreUID?LinkItemUID={UID}", HttpMethod.Get);
                if (AllApprovalLevelDetails != null && AllApprovalLevelDetails.IsSuccess && AllApprovalLevelDetails!.Data != null)
                {
                    return AllApprovalLevelDetails!.Data!.ToList<IAllApprovalRequest>();
                }
                return [default];
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Dictionary<string, List<string>> UserRole_Code { get; set; }
        protected ApprovalRequestItem? PrepareApprovalRequestItem(string hierarchyType, string HierarchyUid)
        {
            ApprovalRequestItem approvalRequestItem = new ApprovalRequestItem();
            approvalRequestItem.HierarchyType = hierarchyType;
            approvalRequestItem.HierarchyUid = HierarchyUid;
            approvalRequestItem.RuleId = RuleId;
            approvalRequestItem.Payload = new Dictionary<string, object>
            {
                { "RequesterId", _appUser.Emp.Code },
                {
                  "UserRoleCode" , _appUser.Role.Code
                },
                { "Remarks", "Need approval" },
                { "Customer", new Customer { FirmType = UserType } }
            };

            return approvalRequestItem;
        }
        private async Task GetAllUserHierarchyFromAPIAsync(string userHierarchyType, string hierarchyUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<UserHierarchy>> apiResponse = await
                _apiService.FetchDataAsync<List<UserHierarchy>>
                    ($"{_appConfig.ApiBaseUrl}MaintainUser/GetUserHierarchyForRule/{userHierarchyType}/{hierarchyUID}/{RuleId}", HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data is not null)
                {
                    UserRole_Code = new Dictionary<string, List<string>>();

                    foreach (var item in apiResponse.Data)
                    {
                        if (UserRole_Code.ContainsKey(item.RoleCode))
                        {
                            UserRole_Code[item.RoleCode].Add(item.EmpCode);
                        }
                        else
                        {
                            UserRole_Code[item.RoleCode] = new List<string> { item.EmpCode };
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        //public async Task<bool> SaveApprovalRequestDetails(string requestId, string linkedItemUID, string linkedItemType, string userHierarchyType, string hierarchyUID)
        //{

        //    try
        //    {
        //        await GetAllUserHierarchyFromAPIAsync(userHierarchyType, hierarchyUID);
        //        ApiResponse<string>? apiResponse = null;
        //        var allApprovalRequest = new AllApprovalRequest
        //        {
        //            LinkedItemType = linkedItemType,
        //            LinkedItemUID = linkedItemUID,
        //            RequestID = requestId,
        //            ApprovalUserDetail = JsonConvert.SerializeObject(UserRole_Code)
        //        };
        //        apiResponse = await _apiService.FetchDataAsync(
        //            $"{_appConfig.ApiBaseUrl}Store/CreateAllApprovalRequest", HttpMethod.Post, allApprovalRequest);


        //        if (apiResponse != null)
        //        {
        //            return apiResponse.IsSuccess;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return false;

        //}
        //protected void SetApprovalEngineRuleByChannelPartner()
        //{
        //    if (SelectedChannelPartner != null)
        //    {
        //        switch (SelectedChannelPartner.BroadClassification.ToLower())
        //        {
        //            case StoreConstants.Distibutor:
        //                RuleId = SchemeConstants.DistributorRule;
        //                UserType = "DistributorScheme";
        //                break;
        //            case StoreConstants.Trade:
        //                RuleId = SchemeConstants.TradeRule;
        //                UserType = "TradeScheme";
        //                break;
        //            default:
        //                UserType = "StandingScheme";
        //                RuleId = SchemeConstants.StandingRule;
        //                break;

        //        }
        //    }
        //}
    }
}
