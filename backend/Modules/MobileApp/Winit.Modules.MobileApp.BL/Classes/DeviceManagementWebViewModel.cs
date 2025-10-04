using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mobile.BL.Classes
{
    public class DeviceManagementWebViewModel:DeviceManagementBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        public DeviceManagementWebViewModel(IServiceProvider serviceProvider,
             IFilterHelper filter,
             ISortHelper sorter,
             IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService)
         : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;

            DeviceManagementFilterCriterials = new List<FilterCriteria>();
        }
       
        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
        }
        #region Database or Services Methods
        public override async Task<List<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>> GetDeviceManagement(string orgUID)
        {
            return await GetDeviceManagementFromAPIAsync(orgUID);
        }
        public override async Task<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> GetDeviceManagementforEditDetailsData(string orgUID)
    
        {
            return await GetDeviceManagementEditDataFromAPIAsync(orgUID);
        }
        public override async Task<bool> UpdateDeviceManagement_data(IAppVersionUser appVersionUser)

        {
            return await UpdateDeviceManagementDataFromAPIAsync(appVersionUser);
        }
        public override async Task<List<ISelectionItem>> GetSalesmanData(string OrgUID)
        {
            return await GetSalesmanDataFromAPIAsync(OrgUID);
        }

        #endregion
        #region Api Calling Methods
        private async Task<List<ISelectionItem>> GetSalesmanDataFromAPIAsync(string OrgUID)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetEmpDropDown?orgUID={OrgUID}",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> Response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return Response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> GetDeviceManagementEditDataFromAPIAsync(string orguid)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;

                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                ApiResponse<Winit.Modules.Mobile.Model.Classes.AppVersionUser> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Mobile.Model.Classes.AppVersionUser>(
                    $"{_appConfigs.ApiBaseUrl}AppVersion/GetAppVersionDetailsByUID?UID={orguid}",
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
        private async Task<List<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>> GetDeviceManagementFromAPIAsync(string orgUID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = DeviceManagementFilterCriterials;
                pagingRequest.SortCriterias = this.SortCriterias;
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.IsCountRequired = true;

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}AppVersion/GetAppVersionDetails?OrgUID={orgUID}",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                   ApiResponse<PagedResponse<Winit.Modules.Mobile.Model.Classes.AppVersionUser>> selectionORGs = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Mobile.Model.Classes.AppVersionUser>>>(apiResponse.Data);
                    if (selectionORGs.Data.PagedData != null)
                    {
                        TotalItemsCount = selectionORGs.Data.TotalCount;
                        return selectionORGs.Data.PagedData.OfType<IAppVersionUser>().ToList();
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        private async Task<bool> UpdateDeviceManagementDataFromAPIAsync(IAppVersionUser appVersionUser)
        {
            try
            {
                ApiResponse<string> apiResponse = null;

                string jsonBody = JsonConvert.SerializeObject(appVersionUser);
                apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}AppVersion/UpdateAppVersionDetails", HttpMethod.Post, appVersionUser);

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
