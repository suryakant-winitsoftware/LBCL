using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.User.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SellInSchemeWebViewModel : SellInSchemeBaseViewModel
    {
        public SellInSchemeWebViewModel(ISellInSchemeDTO SellInSchemeDTO, IAppConfig appConfig,
            ApiService apiService, ILoadingService loadingService, CommonFunctions commonFunctions,
            IToast toast, IAppUser appUser, IServiceProvider serviceProvider, IAlertService alertService, IAppSetting appSetting, IAddProductPopUpDataHelper addProductPopUpDataHelper) :
            base(appConfig, apiService, serviceProvider, appUser, loadingService, alertService, appSetting, commonFunctions,
                addProductPopUpDataHelper, toast)
        {
            _SellInSchemeDTO = SellInSchemeDTO;
            _SellInSchemeDTO.SellInSchemeLines = new();
            _SellInSchemeDTO.SellInHeader = _serviceProvider.CreateInstance<ISellInSchemeHeader>();
            _SellInSchemeDTO.SchemeOrgs = [];
            _SellInSchemeDTO.SchemeBranches = [];
            _SellInSchemeDTO.SchemeBroadClassifications = [];
            ChannelPartner = [];
        }


        #region API Calling Methodes

        public async Task<ApiResponse<string>> Save()
        {
            //var isValidatedHeader = ValidateOnAddProduct_Click();
            //if (!isValidatedHeader.Item1)
            //{
            //    return new ApiResponse<string>(null, 500, isValidatedHeader.Item2)
            //    {
            //        ErrorMessage = isValidatedHeader.Item2
            //    };
            //}
            //var isValidated = Validate();
            //if (!isValidated.Item1)
            //{
            //    return new ApiResponse<string>(null, 500, isValidated.Item2)
            //    {
            //        ErrorMessage = isValidated.Item2
            //    };
            //}
            if (IsNew)
            {

                _SellInSchemeDTO!.SellInHeader.OrgUID = _appUser.SelectedJobPosition.OrgUID;
                _SellInSchemeDTO.SellInHeader.JobPositionUID = _appUser.SelectedJobPosition.UID;
                _SellInSchemeDTO.SellInHeader.EmpUID = _appUser.Emp.UID;
                _SellInSchemeDTO.SellInHeader.ValidUpTo = _SellInSchemeDTO.SellInHeader.EndDate;
                _SellInSchemeDTO.SellInHeader.Status = SchemeConstants.Pending;
                _SellInSchemeDTO.ApprovalRequestItem = PrepareApprovalRequestItem(UserHierarchyTypeConst.Emp, _appUser.Emp.UID);
            }
            _SellInSchemeDTO!.SellInHeader.LineCount = _SellInSchemeDTO.SellInSchemeLines?.Count ?? 0;
            _SellInSchemeDTO.SellInHeader.ModifiedBy = _appUser.Emp.UID;
            _SellInSchemeDTO.SellInHeader.ModifiedTime = DateTime.Now;
            PrePareApplicabletoCustomers();

            return await _apiService.FetchDataAsync<string>(
                   $"{_appConfig.ApiBaseUrl}SellInSchemeHeader/CUSellInHeader",
                   HttpMethod.Post, _SellInSchemeDTO);
        }

        protected override async Task GetSellInMasterByHeaderUID(string UID)
        {
            Winit.Shared.Models.Common.ApiResponse<ISellInSchemeDTO>
              apiResponse = await _apiService.FetchDataAsync<ISellInSchemeDTO>
              (
               $"{_appConfig.ApiBaseUrl}SellInSchemeHeader/GetSellInMasterByHeaderUID?UID={UID}",
               HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                _SellInSchemeDTO = apiResponse.Data;
            }
        }
        protected override async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> GetSKUPrice(List<string> skuUids)
        {
            PagingRequest pagingRequest = new PagingRequest()
            {
                FilterCriterias = new()
                {
                    new Shared.Models.Enums.FilterCriteria("SkuUid",skuUids,Shared.Models.Enums.FilterType.In)
                }
            };
            Winit.Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>>
                apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>>
                (
                 $"{_appConfig.ApiBaseUrl}SKUPrice/SelectAllSKUPriceDetails",
                 HttpMethod.Post, pagingRequest);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse!.Data!.PagedData.ToList();
            }
            return [];
        }

        #endregion

    }
}
