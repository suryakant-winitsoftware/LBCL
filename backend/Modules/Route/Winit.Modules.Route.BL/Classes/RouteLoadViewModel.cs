using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.Route.BL.Classes
{
    public class RouteLoadViewModel : RouteLoadWebViewModel
    {
        public RouteLoadViewModel(IServiceProvider serviceProvider,
     IFilterHelper filter,
     ISortHelper sorter,
     IListHelper listHelper,
     IAppUser appUser,

     IAppConfig appConfigs,
     Base.BL.ApiService apiService
 ) : base(serviceProvider, filter, sorter, listHelper, appUser, appConfigs, apiService)
        {

            _filter = filter;
            _sorter = sorter;
            _serviceProvider = serviceProvider;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;

        }
        public override async Task PopulateViewModel(string apiParam=null)
        {
            await GetRouteLoadTruckTemplates();
            await GetRoutes(apiParam);
        }

        public override async Task ApplyFilter(List<FilterCriteria> filterCriterias)
        {
            try
            {
                FilterCriterias.Clear();
                FilterCriterias.AddRange(filterCriterias);
                foreach (var criteria in FilterCriterias)
                {
                    Console.WriteLine($"Name: {criteria.Name}, Value: {criteria.Value}, Type: {criteria.Type}");
                }
                await GetRouteLoadTruckTemplates();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public  async Task GetRouteLoadTruckTemplates()
        {
            try
            {

                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;


                //PagingRequest pagingRequest = new PagingRequest();
                //pagingRequest.PageSize = 10;
                //pagingRequest.PageNumber = 1;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}RouteLoadTruckTemplate/SelectAllRouteLoadTruckTemplateDetails",HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<RouteLoadTruckTemplateUI> fetchedapiData = JsonConvert.DeserializeObject<PagedResponse<RouteLoadTruckTemplateUI>>(data);
                    if (fetchedapiData.PagedData != null)
                    {
                        FilterRouteLoadTruckTemplateView = fetchedapiData.PagedData.Cast<IRouteLoadTruckTemplateUI>().ToList();
                        DisplayRouteLoadTruckTemplateView = fetchedapiData.PagedData.Cast<IRouteLoadTruckTemplateUI>().ToList();

                    }
                }

            }

            catch (Exception ex)
            {
                // Handle exceptions
            }

        }

       
        public  async Task GetRoutes(string OrgUID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();

                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Route/SelectAllRouteDetails?OrgUID={OrgUID}",
                    HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Route.Model.Classes.Route> fetchedapiData = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Route.Model.Classes.Route>>(data);
                    if (fetchedapiData?.PagedData != null)
                    {
                        RouteList = fetchedapiData.PagedData.ToList();
                    }

                }

            }

            catch (Exception ex)
            {
                // Handle exceptions
            }

        }

        
        public override async Task<bool> DeleteRouteLoadTruckTemplate(string selectedUID)
        {


            try
            {
                if (selectedUID.Any())
                {
                    string jsonBody = JsonConvert.SerializeObject(selectedUID);
                    string apiUrl = $"{_appConfigs.ApiBaseUrl}RouteLoadTruckTemplate/DeleteRouteDetail?UID={selectedUID}";

                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Delete, null);

                    if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                    {
                        string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                        await GetRouteLoadTruckTemplates();
                        return apiResponse.IsSuccess;

                    }
                }
            }
            catch (Exception ex)
            {
               
                Console.WriteLine(ex.ToString());
            }

            return false;
        }
    }
}

