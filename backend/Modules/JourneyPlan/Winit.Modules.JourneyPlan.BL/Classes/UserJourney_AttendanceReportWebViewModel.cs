using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public class UserJourney_AttendanceReportWebViewModel :UserJourney_AttendanceReportBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public UserJourney_AttendanceReportWebViewModel(IServiceProvider serviceProvider,
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
        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
        }
        public override async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid>> GetUserJourneyGridData()
        {
            return await GetUserJourneyGridDataFromAPIAsync();
        }
        public override async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView> GetUserJourneyReportforView(string UID)
        {
            return await GetUserJourneyReportforViewFromAPIAsnyc(UID);
        }
        public override async Task<List<ISelectionItem>> GetSalesmanData(string OrgUID)
        {
            return await GetSalesmanDataFromAPIAsync(OrgUID);
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
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid>> GetUserJourneyGridDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
               // pagingRequest.PageNumber = PageNumber;
              //  pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = UserJourney_AttendanceReportFilterCriterias;
                pagingRequest.SortCriterias = this.SortCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}UserJourney/GetUserJourneyGridDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.JourneyPlan.Model.Classes.UserJourneyGrid> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.JourneyPlan.Model.Classes.UserJourneyGrid>>(data);
                    if (selectionORGs.PagedData != null)
                    {
                        TotalItemsCount = selectionORGs.TotalCount;
                        return selectionORGs.PagedData.OfType<IUserJourneyGrid>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView> GetUserJourneyReportforViewFromAPIAsnyc(string UID)
        {
            try
            {
                ApiResponse<Winit.Modules.JourneyPlan.Model.Classes.UserJourneyView> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.JourneyPlan.Model.Classes.UserJourneyView>(
                    $"{_appConfigs.ApiBaseUrl}UserJourney/GetUserJourneyDetailsByUID?UID={UID}",
                    HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView userJourneyView = new UserJourneyView();
                    userJourneyView.User = apiResponse.Data.User;
                    userJourneyView.JourneyDate = apiResponse.Data.JourneyDate;
                    userJourneyView.EOTStatus = apiResponse.Data.EOTStatus;
                    userJourneyView.StartTime = apiResponse.Data.StartTime;
                    userJourneyView.EndTime = apiResponse.Data.EndTime;
                    userJourneyView.JourneyTime = apiResponse.Data.JourneyTime;
                    userJourneyView.JourneyDate = apiResponse.Data.JourneyDate;
                    userJourneyView.StartOdometerReading = apiResponse.Data.StartOdometerReading;
                    userJourneyView.EndOdometerReading = apiResponse.Data.EndOdometerReading;
                    userJourneyView.TotalReading = apiResponse.Data.TotalReading;
                    userJourneyView.is_synchronizing = apiResponse.Data.is_synchronizing;
                    userJourneyView.is_location_enabled = apiResponse.Data.is_location_enabled;
                    userJourneyView.has_mobile_network = apiResponse.Data.has_mobile_network;
                    userJourneyView.has_internet = apiResponse.Data.has_internet;
                    userJourneyView.internet_type = apiResponse.Data.internet_type;
                    userJourneyView.download_speed = apiResponse.Data.download_speed;
                    userJourneyView.upload_speed = apiResponse.Data.upload_speed;
                    userJourneyView.BatteryStatus = apiResponse.Data.BatteryStatus;
                    userJourneyView.battery_percentage_target = apiResponse.Data.battery_percentage_target;
                    userJourneyView.battery_percentage_available = apiResponse.Data.battery_percentage_available;
                    userJourneyView.ImagePath = apiResponse.Data.ImagePath;
                   // userJourneyView.sKUuserJourneyViewList = apiResponse.Data.sKUuserJourneyViewList.OfType<ISKUuserJourneyView>().ToList();
                    return userJourneyView;
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
    }
}
