using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Newtonsoft.Json;
using System.Net.Http;
using Winit.Modules.ApprovalEngine.Model.Classes;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreCheckReportWebViewModel : StoreCheckReportBaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppConfig _appConfigs;
        private readonly Winit.Modules.Base.BL.ApiService _apiService;

        public StoreCheckReportWebViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService) : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
        }

        public override async Task<List<IStoreCheckReport>> GetStoreCheckReportData()
        {
            return await GetStoreCheckReportDataFromAPIAsync();
        }

        public override async Task<List<ISelectionItem>> GetRouteData(string OrgUID)
        {
            return await GetRouteDataFromAPIAsync(OrgUID);
        }

        public override async Task<List<ISelectionItem>> GetSalesmanData(string OrgUID)
        {
            return await GetSalesmanDataFromAPIAsync(OrgUID);
        }

        public override async Task<List<ISelectionItem>> GetCustomerData(string OrgUID)
        {
            return await GetCustomerDataFromAPIAsync(OrgUID);
        }

        private async Task<List<IStoreCheckReport>> GetStoreCheckReportDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    FilterCriterias = StoreCheckReportFilterCriterias,
                    SortCriterias = SortCriterias,
                    IsCountRequired = true
                };

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}StoreCheckReport/GetStoreCheckReportDetails",
                    HttpMethod.Post,
                    pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<StoreCheckReport> storeCheckReports = JsonConvert.DeserializeObject<PagedResponse<StoreCheckReport>>(data);

                    if (storeCheckReports.PagedData != null)
                    {
                        TotalItemsCount = storeCheckReports.TotalCount;
                        return storeCheckReports.PagedData.OfType<IStoreCheckReport>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }
            return new List<IStoreCheckReport>();
        }

        private async Task<List<ISelectionItem>> GetRouteDataFromAPIAsync(string OrgUID)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetRouteDropDown?orgUID={OrgUID}",
                    HttpMethod.Post);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }
            return new List<ISelectionItem>();
        }

        private async Task<List<ISelectionItem>> GetSalesmanDataFromAPIAsync(string OrgUID)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetEmpDropDown?orgUID={OrgUID}",
                    HttpMethod.Post);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }
            return new List<ISelectionItem>();
        }

        private async Task<List<ISelectionItem>> GetCustomerDataFromAPIAsync(string OrgUID)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetCustomerDropDown?franchiseeOrgUID={OrgUID}",
                    HttpMethod.Post);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }
            return new List<ISelectionItem>();
        }

        public  async Task GetStoreCheckReportItemsAsync(string uid)
        {
            ApiResponse<List<IStoreCheckReportItem>> apiResponse = await _apiService.FetchDataAsync< List < IStoreCheckReportItem >> (
                 $"{_appConfigs.ApiBaseUrl}StoreCheckReport/GetStoreCheckReportItems?uid={uid}",
                 HttpMethod.Get);
            StoreCheckReportsitem.Clear();
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                StoreCheckReportsitem.AddRange(apiResponse.Data);
            }
        }
    }
}
