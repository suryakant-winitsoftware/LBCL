
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class ManageStandingProvisionWebViewModel : ManageStandingProvisionBaseViewModel
    {
        ApiService _apiService { get; }
        IAppConfig _appConfigs { get; }
        IAddProductPopUpDataHelper _addProductPopUpV1DataHelper { get; }
        public ManageStandingProvisionWebViewModel(ApiService apiService, IAppConfig appConfig,
            Winit.Modules.Common.Model.Interfaces.IDataManager dataManager,
            IAddProductPopUpDataHelper addProductPopUpV1DataHelper)
        {
            _apiService = apiService;
            _appConfigs = appConfig;
            _dataManager = dataManager;
            _addProductPopUpV1DataHelper = addProductPopUpV1DataHelper;
        }

        public override async Task PopulateViewModel()
        {
            SKUGroup = await _addProductPopUpV1DataHelper.GetSKUGroup(new());
            SKUGroupType = await _addProductPopUpV1DataHelper.GetSKUGroupType(new());
            PagingRequest.SortCriterias = [DefaultSortCriteria];
            PagingRequest.PageSize = PageSize = 10;
            PagingRequest.PageNumber = PageNumber = 1;
            await GetStandingProvisionDetails();
        }
        public override async Task GetStandingProvisionDetails()
        {
            Shared.Models.Common.ApiResponse<PagedResponse<IStandingProvisionScheme>> apiResponse = await
                _apiService.FetchDataAsync<PagedResponse<IStandingProvisionScheme>>
                ($"{_appConfigs.ApiBaseUrl}StandingProvisionScheme/SelectAllStandingConfiguration",
                HttpMethod.Post, PagingRequest);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    StandingProvisionSchemes = apiResponse?.Data?.PagedData?.ToList();
                    TotalItems = CommonFunctions.GetIntValue(apiResponse?.Data?.TotalCount);
                    SetTableData();
                }
            }
        }
        public override async Task GetschemeExtendHistoryBySchemeUID(string standingProvisionUID)
        {
            Shared.Models.Common.ApiResponse<List<ISchemeExtendHistory>> apiResponse = await
                _apiService.FetchDataAsync<List<ISchemeExtendHistory>>
                ($"{_appConfigs.ApiBaseUrl}ManageScheme/GetschemeExtendHistoryBySchemeUID/{standingProvisionUID}",
                HttpMethod.Get);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
                {
                    SchemeExtendHistories = apiResponse?.Data;
                }
            }
        }
        public async Task<ApiResponse<string>> ChangeEndDate(IStandingProvisionScheme scheme)
        {
            scheme.EndDateUpdatedOn = DateTime.Now;
            scheme.ModifiedTime = DateTime.Now;

            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}StandingProvisionScheme/ChangeEndDate",
                    HttpMethod.Put, scheme);
            return apiResponse;
        }
    }
}
