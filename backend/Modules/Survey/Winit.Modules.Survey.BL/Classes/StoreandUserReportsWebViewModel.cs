using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Modules.User.Model.Interfaces;
using iTextSharp.text;
using Newtonsoft.Json;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.Survey.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Dapper;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Survey.BL.Classes
{
    public class StoreandUserReportsWebViewModel: StoreandUserReportsBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;
        public StoreandUserReportsWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
        ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
            IAppUser appUser) : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService, appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
        }
        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
        }
      
        public override async Task<List<IStoreUserInfo>> GetStoreUserInfoSummaryGridiview()
        {
            return await GetStoreUserInfoSummaryGridiviewFromAPIAsync();
        }
        public override async Task<List<IStoreUserInfo>> GetStoreUserInfoSummaryGridiviewExportData()
        {
            return await GetStoreUserInfoSummaryGridiviewExportDataFromAPIAsync();
        }
        public override async Task<List<ISelectionItem>> GetUsersData(string OrgUID)
        {
            return await GetUsersDataFromAPIAsync(OrgUID);
        }
        public override async Task<List<ISelectionItem>> GetStores_CustomersData(string orgUID)
        {
            return await GetStores_CustomersDataFromAPIAsync(orgUID);
        }
        public override async Task<List<ILocation>> GetStateDetails(List<string> locationTypes)
        {
            return await GetStateDetailsFromAPIAsync(locationTypes);
        }
        public override async Task<List<ISelectionItem>> GetRolesData()
        {
            return await GetRolesDataFromAPIAsync();
        }
        private async Task<List<ISelectionItem>> GetRolesDataFromAPIAsync()
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetRolesDropDown",
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
            return [];
        }
        private async Task<List<ILocation>> GetStateDetailsFromAPIAsync(List<string> locationTypes)
        {
            try
            {
                ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>> apiResponse = await _apiService.FetchDataAsync
                 <List<Winit.Modules.Location.Model.Classes.Location>>(
                 $"{_appConfigs.ApiBaseUrl}Location/GetLocationByTypes",
                 HttpMethod.Post, locationTypes);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<ILocation>();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<ISelectionItem>> GetUsersDataFromAPIAsync(string OrgUID)
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
        private async Task<List<ISelectionItem>> GetStores_CustomersDataFromAPIAsync(string orgUID)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetCustomerDropDown?franchiseeOrgUID={orgUID}",
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
        //private async Task<List<IStoreUserInfo>> GetStoreUserInfoSummaryGridiviewExportDataFromAPIAsync()
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest
        //        {
        //            FilterCriterias = new List<FilterCriteria>(), // ✅ initialize to avoid null reference
        //            SortCriterias = this.SortCriterias,
        //            IsCountRequired = true
        //        };

        //        if (StoreUserInfoFilterCriterias != null && StoreUserInfoFilterCriterias.Any(fc => fc.Value != null))
        //        {
        //            pagingRequest.FilterCriterias = StoreUserInfoFilterCriterias;
        //        }
        //        else
        //        {
        //            var fromDate = DateTime.Today;
        //            var toDate = fromDate.AddDays(1);

        //            var from = fromDate.ToString("yyyy-MM-dd HH:mm:ss");
        //            var to = toDate.ToString("yyyy-MM-dd HH:mm:ss");

        //            pagingRequest.FilterCriterias.Add(new FilterCriteria(
        //                "BH.visit_date",
        //                new[] { from, to },
        //                FilterType.Between
        //            ));
        //        }

        //        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
        //            $"{_appConfigs.ApiBaseUrl}UserStoreActivity/GetStoreUserSummary",
        //            HttpMethod.Post, pagingRequest);

        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {
        //            string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
        //            PagedResponse<StoreUserInfo> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<StoreUserInfo>>(data);
        //            if (selectionORGs.PagedData != null)
        //            {
        //                TotalItems = selectionORGs.TotalCount;
        //                return selectionORGs.PagedData.OfType<IStoreUserInfo>().ToList();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Consider logging ex.Message for debugging
        //    }
        //    return [];
        //}

        private async Task<List<IStoreUserInfo>> GetStoreUserInfoSummaryGridiviewExportDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest
                {
                    FilterCriterias = new List<FilterCriteria>(),
                    SortCriterias = this.SortCriterias,
                    IsCountRequired = false,
                    PageNumber = 1,
                    PageSize = int.MaxValue  // Set to max value to get all records
                };

                // Use the same filter logic as the grid method
                if (isFilterApplied)
                {
                    pagingRequest.FilterCriterias = StoreUserInfoFilterCriterias;
                }
                else
                {
                    pagingRequest.FilterCriterias = PagingRequest.FilterCriterias;
                }

                ApiResponse<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserInfo>> apiResponse =
                    await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserInfo>>(
                        $"{_appConfigs.ApiBaseUrl}UserStoreActivity/GetStoreUserSummary",
                        HttpMethod.Post,
                        pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.PagedData.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting store user info summary for export: {ex.Message}", ex);
            }
            return [];
        }

        public async Task<List<IStoreUserInfo>> GetStoreUserInfoSummaryGridiviewFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                if (isFilterApplied)
                {
                    pagingRequest.FilterCriterias = StoreUserInfoFilterCriterias;
                }
                else
                {
                    pagingRequest.FilterCriterias = PagingRequest.FilterCriterias;
                }
                pagingRequest.IsCountRequired = true;
                pagingRequest.SortCriterias = new List<SortCriteria>
                {
                    new SortCriteria("ModifiedTime",SortDirection.Desc)
                };
                pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserInfo>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserInfo>>(
                    $"{_appConfigs.ApiBaseUrl}UserStoreActivity/GetStoreUserSummary",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    
                    TotalItems = apiResponse.Data.TotalCount;
                    return apiResponse.Data.PagedData.ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
       
    }
}
