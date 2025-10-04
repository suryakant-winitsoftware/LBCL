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
using Winit.Modules.Location.Model.Interfaces;
using Nest;

namespace Winit.Modules.Survey.BL.Classes
{
    public class StoreUserVisitReportWebViewModel : StoreUserVisitReportBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;
        public StoreUserVisitReportWebViewModel(IServiceProvider serviceProvider,
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

        public override async Task<List<IStoreUserVisitDetails>> StoreUserVisitReportGridiview()
        {
            return await GetStoreUserVisitReportGridiviewFromAPIAsync();
        }
        public override async Task<List<IStoreUserVisitDetails>> ExporttoreUserVisitReport()
        {
            return await GetExporttoreUserVisitReportFromAPIAsync();
        }
        public override async Task<List<IStoreUserVisitDetails>> StoreTabUserVisitReportGridiview(string status)
        {
            return await GetTabStoreUserVisitReportGridiviewFromAPIAsync(status);
        }
        public override async Task<List<ILocation>> GetStateDetails(List<string> locationTypes)
        {
            return await GetStateDetailsFromAPIAsync(locationTypes);
        }
        public override async Task<List<ISelectionItem>> GetSalesmanData(string OrgUID)
        {
            return await GetSalesmanDataFromAPIAsync(OrgUID);
        }

        public override async Task<List<ISelectionItem>> GetStores_CustomersData(string orgUID)
        {
            return await GetStores_CustomersDataFromAPIAsync(orgUID);
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
        public async Task<List<IStoreUserVisitDetails>> GetStoreUserVisitReportGridiviewFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = StoreUserVisitReportFilterCriterias;
                pagingRequest.IsCountRequired = true;
                pagingRequest.SortCriterias = this.SortCriterias;

                if (!pagingRequest.SortCriterias.Any(q => q.SortParameter == "VisitDateTime"))
                {
                    pagingRequest.SortCriterias.Add(new SortCriteria("VisitDateTime", SortDirection.Desc));
                }
                //ApiResponse<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>>(
                //    $"{_appConfigs.ApiBaseUrl}UserStoreActivity/GetStoreUserActivityDetails",
                //    HttpMethod.Post, pagingRequest);
                //if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                //{

                //    TotalItems = apiResponse.Data.TotalCount;
                //    return apiResponse.Data.PagedData.ToList();
                //}
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}UserStoreActivity/GetStoreUserActivityDetails",
                HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Survey.Model.Classes.StoreUserVisitDetails> visitDetails =
                        JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Survey.Model.Classes.StoreUserVisitDetails>>(data);

                    if (visitDetails.PagedData != null)
                    {
                        TotalItems = visitDetails.TotalCount;
                        return visitDetails.PagedData.OfType<IStoreUserVisitDetails>().ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return [];
        }
        public async Task<List<IStoreUserVisitDetails>> GetExporttoreUserVisitReportFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = StoreUserVisitReportFilterCriterias;
                pagingRequest.IsCountRequired = true;
                pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}UserStoreActivity/GetStoreUserActivityDetails",
                HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Survey.Model.Classes.StoreUserVisitDetails> visitDetails =
                        JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Survey.Model.Classes.StoreUserVisitDetails>>(data);

                    if (visitDetails.PagedData != null)
                    {
                        TotalItems = visitDetails.TotalCount;
                        return visitDetails.PagedData.OfType<IStoreUserVisitDetails>().ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return [];
        }
        public async Task<List<IStoreUserVisitDetails>> GetTabStoreUserVisitReportGridiviewFromAPIAsync(string status)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest
                {
                    FilterCriterias = new List<FilterCriteria>()
                };

                // Include existing filters (like date, user, etc.)
                if (StoreUserVisitReportFilterCriterias != null && StoreUserVisitReportFilterCriterias.Any())
                {
                    pagingRequest.FilterCriterias.AddRange(StoreUserVisitReportFilterCriterias);
                }

                // Add the status filter, if any
                if (!string.IsNullOrEmpty(status))
                {
                    pagingRequest.FilterCriterias.Add(
                        new FilterCriteria(
                            nameof(Winit.Modules.Survey.Model.Classes.StoreUserVisitDetails.Status),
                            status,
                            FilterType.Equal
                        ));
                }

                // API call
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}UserStoreActivity/GetStoreUserActivityDetails",
                    HttpMethod.Post,
                    pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    var visitDetails = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Survey.Model.Classes.StoreUserVisitDetails>>(data);

                    if (visitDetails.PagedData != null)
                    {
                        return visitDetails.PagedData.OfType<IStoreUserVisitDetails>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
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


    }
}
