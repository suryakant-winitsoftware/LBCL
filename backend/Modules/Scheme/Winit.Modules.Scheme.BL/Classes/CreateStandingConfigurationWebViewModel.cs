using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.User.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class CreateStandingConfigurationWebViewModel : CreateStandingConfigurationBaseViewModel
    {
        public CreateStandingConfigurationWebViewModel(Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, ApiService apiService,
            IAppConfig appConfig, CommonFunctions commonFunctions, IAppUser appUser, ILoadingService loadingService,
                          IStandingProvisionSchemeMaster standingProvisionSchemeDTO, IAlertService alertService,
                          IStandingProvisionScheme standingProvisionScheme, IServiceProvider serviceProvider, IAppSetting appSetting
            , IAddProductPopUpDataHelper addProductPopUpDataHelper, IToast toast) : base(appConfig,
                              apiService, serviceProvider, appUser, loadingService, alertService, appSetting, commonFunctions, addProductPopUpDataHelper, toast)
        {



            //base._appUser = appUser;
            //base._loadingService = loadingService;


            standingProvisionSchemeDTO.StandingProvisionScheme = standingProvisionScheme;
            standingProvisionSchemeDTO.SchemeBranches = [];
            standingProvisionSchemeDTO.SchemeBroadClassifications = [];
            standingProvisionSchemeDTO.SchemeOrgs = [];
            standingProvisionSchemeDTO.StandingProvisionSchemeDivisions = [];

            StandingProvisionSchemeMaster = standingProvisionSchemeDTO;
            ExcludedModels = [];
        }



        public async Task<ApiResponse<string>> Save()
        {
            ValidateDetails(out string message, isVal: out bool isVal);
            if (!isVal)
            {
                return new ApiResponse<string>(data: null, statusCode: 400, errorMessage: message);
            }
            ValidateProductType(out message, isVal: out isVal);
            if (!isVal)
            {
                return new ApiResponse<string>(data: null, statusCode: 400, errorMessage: message);
            }
            ValidateApplicableToProducts(out message, isVal: out isVal);
            if (!isVal)
            {
                return new ApiResponse<string>(data: null, statusCode: 400, errorMessage: message);
            }
            PrepareProvisionMaster();
            if (IsNew)
            {
                StandingProvisionSchemeMaster.ApprovalRequestItem = PrepareApprovalRequestItem(UserHierarchyTypeConst.Emp, _appUser.Emp.UID);
                StandingProvisionSchemeMaster.IsNew = true;  // added by aziz
            }
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>($"{_appConfig.ApiBaseUrl}StandingProvisionScheme/CUStandingProvisionScheme",
                HttpMethod.Post, StandingProvisionSchemeMaster);
            return apiResponse;
        }
        //private ApprovalRequestItem? PrepareApprovalRequestItem(IStandingProvisionSchemeMaster standingProvisionSchemeMaster)
        //{
        //    ApprovalRequestItem approvalRequestItem = new ApprovalRequestItem();
        //    approvalRequestItem.HierarchyType = UserHierarchyTypeConst.Emp;
        //    approvalRequestItem.HierarchyUid = _appUser.Emp.UID;
        //    approvalRequestItem.RuleId = RuleId;
        //    approvalRequestItem.Payload = new Dictionary<string, object>
        //    {
        //        { "RequesterId", _appUser.Emp.Code },
        //        { "Remarks", "Need approval" },
        //        { "Customer", new Customer { FirmType = UserType } }
        //    };

        //    return approvalRequestItem;
        //}
        protected override async Task GetStandardProvisionMasterBYUID(string UID)
        {
            ApiResponse<IStandingProvisionSchemeMaster> apiResponse = await _apiService.FetchDataAsync<IStandingProvisionSchemeMaster>($"{_appConfig.ApiBaseUrl}" +
                $"StandingProvisionScheme/GetStandingProvisionSchemeDetailsByUID?UID={UID}",
                HttpMethod.Get, StandingProvisionSchemeMaster);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                StandingProvisionSchemeMaster = apiResponse.Data;
            }
        }

        public async Task<bool> UpdateorDeleteExcludedItems()
        {
            StandingProvisionSchemeMaster.StandingProvisionScheme.ModifiedBy = _appUser.Emp.UID;
            StandingProvisionSchemeMaster.StandingProvisionScheme.ModifiedTime = DateTime.Now;
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfig.ApiBaseUrl}StandingProvisionScheme/UpdateStandingConfiguration",
                    HttpMethod.Put, StandingProvisionSchemeMaster.StandingProvisionScheme);
            return (apiResponse != null && apiResponse.IsSuccess);
        }
        public async Task<int> DeleteExcludedItems()
        {
            List<string> cpy = [];
            ExcludedModels.ForEach(p =>
            {
                if (p.IsSelected == false)
                {
                    cpy.Add(p.UID);
                }
            });
            StandingProvisionSchemeMaster.StandingProvisionScheme.ExcludedModels = string.Join(",", cpy);
            StandingProvisionSchemeMaster.StandingProvisionScheme.ModifiedBy = _appUser.Emp.UID;
            StandingProvisionSchemeMaster.StandingProvisionScheme.ModifiedTime = DateTime.Now;
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfig.ApiBaseUrl}StandingProvisionScheme/UpdateStandingConfiguration",
                    HttpMethod.Put, StandingProvisionSchemeMaster.StandingProvisionScheme);
            return (apiResponse != null && apiResponse.IsSuccess) ? CommonFunctions.GetIntValue(apiResponse!.Data!) : 0;
        }
    }
}
