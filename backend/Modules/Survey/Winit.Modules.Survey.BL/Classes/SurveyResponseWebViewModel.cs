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
using Nest;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Survey.BL.Classes
{
    public class SurveyResponseWebViewModel : SurveyResponseBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;
        public SurveyResponseWebViewModel(IServiceProvider serviceProvider,
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

        public override async Task ViewSurveyResponsePopulateViewModel(string SurveyUID)
        {
            await base.ViewSurveyResponsePopulateViewModel(SurveyUID);

        }
        public override async Task<List<IViewSurveyResponse>> GetSurveyResponseDetailsGridiview()
        {
            return await GetSurveyResponseDetailsGridiviewFromAPIAsync();
        }
        public override async Task<List<IViewSurveyResponse>> GetTabDataAfterFilterApplyFromAPI()
        {
            return await GetTabDataAfterFilterApplyFromAPIAsync();
        }
        public override async Task<List<IViewSurveyResponseExport>> GetSurveyResponseDetailsExportToExcel()
        {
            return await GetSurveyResponseDetailsExportToExcelFromAPIAsync();
        }
        public override async Task<ISurveyResponseViewDTO> GetViewSurveyResponseDetailsGridiview(string SurveyUID)
        {
            return await GetViewSurveyResponseDetailsGridiviewFromAPIAsync(SurveyUID);
        }
        public override async Task<List<IViewSurveyResponse>> GetTabDataForInitialpageLoadFromAPI()
        {
            return await GetTabDataForInitialpageLoadFromAPIAsync();
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
        public override async Task<List<ISelectionItem>> GetQuestionsData(string selectedSurveyUid)
        {
            return await GetQuestionsDataFromAPIAsync(selectedSurveyUid);
        }
        private async Task<List<ISelectionItem>> GetQuestionsDataFromAPIAsync(string selectedSurveyUid)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetQuestionsDropDown?surveyUID={selectedSurveyUid}",
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
        public async Task<List<IViewSurveyResponse>> GetSurveyResponseDetailsGridiviewFromAPIAsync()
        {
            try
            {
               
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SurveyResponse/GetViewSurveyResponse",
                    HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Survey.Model.Classes.ViewSurveyResponse>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Survey.Model.Classes.ViewSurveyResponse>>>(apiResponse.Data);
                    TotalItems = pagedResponse.Data.TotalCount;
                    return pagedResponse.Data.PagedData.ToList<IViewSurveyResponse>();
                    //string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    //PagedResponse<Winit.Modules.Survey.Model.Classes.ViewSurveyResponse> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Survey.Model.Classes.ViewSurveyResponse>>(data);
                    //if (selectionORGs.PagedData != null)
                    //{
                    //    TotalItems = selectionORGs.TotalCount;
                    //    return selectionORGs.PagedData.OfType<IViewSurveyResponse>().ToList();
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
        public async Task<List<IViewSurveyResponse>> GetTabDataAfterFilterApplyFromAPIAsync()
        {
            try
            {
                //PagingRequest.FilterCriterias.Add(DefaultCriteria);
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SurveyResponse/GetViewSurveyResponse",
                    HttpMethod.Post,PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Survey.Model.Classes.ViewSurveyResponse> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Survey.Model.Classes.ViewSurveyResponse>>(data);
                    if (selectionORGs.PagedData != null)
                    {
                        TotalTabItems = selectionORGs.TotalCount;
                        return selectionORGs.PagedData.OfType<IViewSurveyResponse>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
        public async Task<List<IViewSurveyResponse>> GetTabDataForInitialpageLoadFromAPIAsync()
        {
            try
            {
                PagingRequest.FilterCriterias.Add(DefaultCriteria);
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SurveyResponse/GetViewSurveyResponse",
                    HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.Survey.Model.Classes.ViewSurveyResponse> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Survey.Model.Classes.ViewSurveyResponse>>(data);
                    if (selectionORGs.PagedData != null)
                    {
                        TotalTabItems = selectionORGs.TotalCount;
                        return selectionORGs.PagedData.OfType<IViewSurveyResponse>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
        public async Task<List<IViewSurveyResponseExport>> GetSurveyResponseDetailsExportToExcelFromAPIAsync()
        {
            try
            {
               // PagingRequest.FilterCriterias.Add(DefaultCriteria);

                // Make the API call
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SurveyResponse/GetViewSurveyResponseForExport",
                    HttpMethod.Post, PagingRequest);

                // Validate response
                if (apiResponse != null && apiResponse.IsSuccess && !string.IsNullOrEmpty(apiResponse.Data))
                {
                    // Deserialize the response into the appropriate type
                    var pagedResponse = JsonConvert.DeserializeObject<ApiResponse<List<ViewSurveyResponseExport>>>(apiResponse.Data);

                    // Ensure data exists
                    if (pagedResponse?.Data != null)
                    {
                        return pagedResponse.Data.Cast<IViewSurveyResponseExport>().ToList();
                    }
                }

                // Return an empty list if no data is available
                return new List<IViewSurveyResponseExport>();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("Error occurred while fetching survey response details.", ex);
            }
        }

        public async Task<ISurveyResponseViewDTO> GetViewSurveyResponseDetailsGridiviewFromAPIAsync(string SurveyUID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.IsCountRequired = true;

                pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SurveyResponse/ViewSurveyResponseByUID/{SurveyUID}",
                    HttpMethod.Get, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<Winit.Modules.Survey.Model.Classes.SurveyResponseViewDTO> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<Winit.Modules.Survey.Model.Classes.SurveyResponseViewDTO>>(apiResponse.Data);
                    if (pagedResponse != null && pagedResponse.IsSuccess)
                    {
                        //var surveyResponseData = pagedResponse.Data;
                        //ISurveyResponseViewDTO surveyResponseViewDTO = new SurveyResponseViewDTO();
                        //surveyResponseViewDTO.QuestionAnswers = surveyResponseData.QuestionAnswers;
                        //return surveyResponseViewDTO;
                        return new SurveyResponseViewDTO
                        {
                            QuestionAnswers = pagedResponse.Data.QuestionAnswers
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while fetching view survey response details.", ex);
            }
            return new SurveyResponseViewDTO
            {
                QuestionAnswers = new List<QuestionAnswer>()
            };
        }
        public async Task<ApiResponse<string>> TicketStatusUpdate(string surveyUID, string status,string remarks)
        {
            var apiResponse = await _apiService.FetchDataAsync(
                       $"{_appConfigs.ApiBaseUrl}SurveyResponse/TicketStatusUpdate?uid={surveyUID}&status={status}&empUID={_appUser.Emp.UID}&remarks={remarks}",
                       HttpMethod.Put);
            return apiResponse;
        }
    }
}
