using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public class ViewTodayJourneyPlanWebViewModel : ViewTodayJourneyPlanBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public ViewTodayJourneyPlanWebViewModel(IServiceProvider serviceProvider,
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

        }
        public override async Task PopulateViewModel(string selectedTab = null)
        {
            await base.PopulateViewModel(selectedTab);
        }
        #region Business Logics  
        #endregion
        #region Database or Services Methods
        public override async Task<List<IAssignedJourneyPlan>> GetAssigned_UnAssignedJourneyPlanData(string selectedTab)
        {
            return await GetAssigned_UnAssignedJourneyPlanDataFromAPIAsync(selectedTab);
        }
        public override async Task<List<ITodayJourneyPlanInnerGrid>> GetTodayJourneyPlanInnerGridData(string BeatHistoryUID)
        {
            return await GetTodayJourneyPlanInnerGridDataFromAPIAsync(BeatHistoryUID);
        }
        public override async Task<List<ISelectionItem>> GetSalesmanData(string OrgUID)
        {
            return await GetSalesmanDataFromAPIAsync(OrgUID);
        }
        public override async Task<List<ISelectionItem>> GetRouteData(string OrgUID)
        {
            return await GetRouteDataFromAPIAsync(OrgUID);
        }
        public override async Task<List<ISelectionItem>> GetVehicleData(string parentUID)
        {
            return await GetVehicleDataFromAPIAsync(parentUID);
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
        private async Task<List<ISelectionItem>> GetRouteDataFromAPIAsync(string OrgUID)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetRouteDropDown?orgUID={OrgUID}",
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
        private async Task<List<ISelectionItem>> GetVehicleDataFromAPIAsync(string parentUID)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetVehicleDropDown?parentUID={parentUID}",
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
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.ITodayJourneyPlanInnerGrid>> GetTodayJourneyPlanInnerGridDataFromAPIAsync(string BeatHistoryUID)
        {

            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias = new List<FilterCriteria>();
                pagingRequest.IsCountRequired = true;
                ApiResponse<IEnumerable<Winit.Modules.JourneyPlan.Model.Classes.TodayJourneyPlanInnerGrid>> apiResponse =
                     await _apiService.FetchDataAsync<IEnumerable<Winit.Modules.JourneyPlan.Model.Classes.TodayJourneyPlanInnerGrid>>(
                     $"{_appConfigs.ApiBaseUrl}UserJourney/SelecteatHistoryInnerGridDetails?BeatHistoryUID={BeatHistoryUID}",
                     HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToHashSet<ITodayJourneyPlanInnerGrid>().ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        public async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan>> GetAssigned_UnAssignedJourneyPlanDataFromAPIAsync(string selectedTab)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                // DateTime dateTime= DateTime.Today;
                pagingRequest.FilterCriterias = TodayJournryplanFilterCriterias;
                pagingRequest.IsCountRequired = true;

                // Get current date in yyyy-MM-dd format
                string currentDate = DateTime.Today.ToString("yyyy-MM-dd");

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}UserJourney/SelectTodayJourneyPlanDetails?VisitDate={currentDate}&Type={selectedTab}",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.JourneyPlan.Model.Classes.AssignedJourneyPlan>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.JourneyPlan.Model.Classes.AssignedJourneyPlan>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan>().ToList();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        #endregion
    }
}
