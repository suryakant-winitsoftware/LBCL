using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Survey.BL.Classes
{
    public class ActivityModuleWebViewModel:ActivityModuleBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;


        public ActivityModuleWebViewModel(IServiceProvider serviceProvider,
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
        public override async Task GetActivityModuleData()
        {
            await base.GetActivityModuleData();
        }
      
        public override async Task<List<Model.Classes.ActivityModule>> GetActivityModuleDataDetails()
        {
            return await GetActivityModuleDataDetailsFromAPIAsync();
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
        public async Task<List<Model.Classes.ActivityModule>> GetActivityModuleDataDetailsFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();

                // Adjust paging for export
                if (IsExportClicked)
                {
                    pagingRequest.PageNumber = 1;
                    pagingRequest.PageSize = int.MaxValue; // Or a large number like 100000
                    pagingRequest.IsCountRequired = false;
                }
                else
                {
                    pagingRequest.PageNumber = PageNumber;
                    pagingRequest.PageSize = PageSize;
                    pagingRequest.IsCountRequired = true;
                }

                pagingRequest.FilterCriterias = ActivityModuleFilterCriterias;
                pagingRequest.SortCriterias = this.SortCriterias;

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ActivityModule/GetAllActivityModuleDeatils",
                    HttpMethod.Post,
                    pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    var pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Model.Classes.ActivityModule>>>(apiResponse.Data);

                    if (pagedResponse?.Data?.PagedData != null)
                    {
                        if (!IsExportClicked)
                            TotalItemsCount = pagedResponse.Data.TotalCount;

                        return pagedResponse.Data.PagedData.ToList();
                    }
                }
            }
            catch (Exception)
            {
                // Optionally log the exception
            }

            return new List<Model.Classes.ActivityModule>(); // Safe fallback
        }

    }
}
