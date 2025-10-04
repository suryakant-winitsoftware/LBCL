using Winit.Shared.Models.Common;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Shared.Models.Enums;
using Newtonsoft.Json;

namespace Winit.Modules.Setting.BL.Classes
{
    public class MaintainSettingWebViewModel: MaintainSettingBaseViewModel
    {

        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private List<string> _propertiesToSearch = new List<string>();
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public MaintainSettingWebViewModel(IServiceProvider serviceProvider,
              IFilterHelper filter,
              ISortHelper sorter,
              IListHelper listHelper,
              IAppUser appUser,
              IAppSetting appSetting,
              IDataManager dataManager,
              IAppConfig appConfigs,
              Base.BL.ApiService apiService
          ) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            _appConfigs = appConfigs;
            _apiService = apiService;
            SettingGridviewList = new List<ISetting>();
            SettingFilterCriterials = new List<FilterCriteria>();
        }
        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
        }
        #region Business Logic 
        #endregion
        #region Database or Services Methods
        public override async Task<List<Winit.Modules.Setting.Model.Interfaces.ISetting>> GetMaintainSetting()
        {
            return await GetMaintainSettingDataFromAPIAsync();
        }
        public override async Task<Winit.Modules.Setting.Model.Interfaces.ISetting> GetMaintainSettingforEditDetailsData(string orguid)

        {
            return await GetMaintainSettingforEditDetailsDataFromAPIAsync(orguid);
        }
        public override async Task<bool> UpdateMaintainSetting_data(ISetting settingUID)

        {
            return await UpdateMaintainSettingDataFromAPIAsync(settingUID);
        }
        #endregion
        #region Api Calling Methods
        private async Task<List<Winit.Modules.Setting.Model.Interfaces.ISetting>> GetMaintainSettingDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = SettingFilterCriterials;
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = SettingFilterCriterials;
                pagingRequest.SortCriterias = this.SortCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Setting/SelectAllSettingDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    ApiResponse<PagedResponse<Winit.Modules.Setting.Model.Classes.Setting>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Setting.Model.Classes.Setting>>>(apiResponse.Data);
                    if (pagedResponse.Data.PagedData != null)
                    {
                        TotalItemsCount = pagedResponse.Data.TotalCount;
                        return pagedResponse.Data.PagedData.OfType<ISetting>().ToList();
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<Winit.Modules.Setting.Model.Interfaces.ISetting> GetMaintainSettingforEditDetailsDataFromAPIAsync(string orguid)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;

                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                ApiResponse<Winit.Modules.Setting.Model.Classes.Setting> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Setting.Model.Classes.Setting>(
                    $"{_appConfigs.ApiBaseUrl}Setting/GetSettingByUID?UID={orguid}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<bool> UpdateMaintainSettingDataFromAPIAsync(ISetting settingUID)
        {
            try
            {
                ApiResponse<string> apiResponse = null;

                string jsonBody = JsonConvert.SerializeObject(settingUID);
                apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Setting/UpdateSetting", HttpMethod.Put, settingUID);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    if (apiResponse.IsSuccess != null && apiResponse.IsSuccess)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        #endregion
    }
}
