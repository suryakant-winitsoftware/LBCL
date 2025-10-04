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
using Dapper;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Survey.BL.Classes
{
    public class StoreQuestionFrequencyWebViewModel: StoreQuestionFrequencyBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;
        public StoreQuestionFrequencyWebViewModel(IServiceProvider serviceProvider,
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
      
        public override async Task<List<IStoreQuestionFrequency>> GetStoreQuestionFrequencyGridiview()
        {
            return await GetStoreQuestionFrequencyGridiviewFromAPIAsync();
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

        //public async Task<List<IStoreQuestionFrequency>> GetStoreQuestionFrequencyGridiviewFromAPIAsync()
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest();
        //        pagingRequest.PageNumber = PageNumber;
        //        pagingRequest.PageSize = PageSize;
        //        if (isFilterApplied)
        //        {
        //            pagingRequest.FilterCriterias = StoreQuestionFrequencyFilterCriterias;
        //        }
        //        else
        //        {
        //            pagingRequest.FilterCriterias = PagingRequest.FilterCriterias;
        //        }
        //        pagingRequest.IsCountRequired = true;
        //        pagingRequest.SortCriterias = new List<SortCriteria>
        //        {
        //            new SortCriteria("customercode",SortDirection.Desc)
        //        };
        //        pagingRequest.SortCriterias = this.SortCriterias;
        //        ApiResponse<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>>(
        //            $"{_appConfigs.ApiBaseUrl}SurveyResponse/GetStoreQuestionFrequencyDetails",
        //            HttpMethod.Post, pagingRequest);
        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {

        //            TotalItems = apiResponse.Data.TotalCount;
        //            return apiResponse.Data.PagedData.ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return null;
        //}
        public async Task<List<IStoreQuestionFrequency>> GetStoreQuestionFrequencyGridiviewFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();

                // If export is clicked, set high page size to fetch all data
                if (IsExportClicked)
                {
                    pagingRequest.PageNumber = 1;
                    pagingRequest.PageSize = int.MaxValue; // or a reasonably high number like 100000
                    pagingRequest.IsCountRequired = false; // not needed for export
                }
                else
                {
                    pagingRequest.PageNumber = PageNumber;
                    pagingRequest.PageSize = PageSize;
                    pagingRequest.IsCountRequired = true;
                }

                // Apply filters
                pagingRequest.FilterCriterias = isFilterApplied
                    ? StoreQuestionFrequencyFilterCriterias
                    : PagingRequest.FilterCriterias;

                // Apply sorting
                pagingRequest.SortCriterias = SortCriterias ?? new List<SortCriteria>
        {
            new SortCriteria("customercode", SortDirection.Desc)
        };

                // Call API
                ApiResponse<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>> apiResponse =
                    await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>>(
                        $"{_appConfigs.ApiBaseUrl}SurveyResponse/GetStoreQuestionFrequencyDetails",
                        HttpMethod.Post,
                        pagingRequest);

                // Return result
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    if (!IsExportClicked)
                        TotalItems = apiResponse.Data.TotalCount;

                    return apiResponse.Data.PagedData.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching store question frequency data.", ex);
            }

            return null;
        }

    }
}
