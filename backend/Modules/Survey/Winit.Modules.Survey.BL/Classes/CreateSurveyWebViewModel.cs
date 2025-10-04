using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Survey.Model.Classes;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Modules.User.Model.Classes;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Survey.BL.Classes
{
    public class CreateSurveyWebViewModel:CreateSurveyBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        IAppUser _appUser;


        public CreateSurveyWebViewModel(IServiceProvider serviceProvider,
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
        public override async Task GetSurveyData()
        {
            await base.GetSurveyData();
        }
        public override async Task<Model.Interfaces.ISurvey> GetSurveyDetailsforEdit(string uid)
        {
            return await GetSurveyDetailsforEditFromAPIAsync(uid);
        }
        public override async Task<Model.Interfaces.ISurvey> GetSurveyDetailsByCode(string code)
        {
            return await GetSurveyDetailsByCodeFromAPIAsync(code);
        }
        public override async Task<List<Model.Classes.Survey>> GetSurveyDetails()
        {
            return await GetGetSurveyDetailsFromAPIAsync();
        }
        public override async Task<int> CreateSurvey(ISurvey survey, bool Iscreate)
        {
            return await CreateSurveyFromAPIAsync(survey, Iscreate);
        }
        public async Task<List<Model.Classes.Survey>> GetGetSurveyDetailsFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = SurveyFilterCriterias;
                pagingRequest.SortCriterias = this.SortCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Survey/GetAllSurveyDeatils",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Model.Classes.Survey>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Model.Classes.Survey>>>(apiResponse.Data);

                    if (pagedResponse?.Data?.PagedData != null)
                    {
                        TotalItemsCount = pagedResponse.Data.TotalCount;
                        return pagedResponse.Data.PagedData.ToList();
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return new List<Model.Classes.Survey>(); // Return an empty list to prevent null reference issues
        }
        private async Task<int> CreateSurveyFromAPIAsync(ISurvey survey, bool IsCreate)
        {
            try
            {
                ApiResponse<string> apiResponse = null;
                if (IsCreate)
                {
                    // string jsonBody = JsonConvert.SerializeObject(survey, Formatting.Indented);
                    apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}Survey/CUDSurvey", HttpMethod.Post, survey);
                }
                else
                {
                    survey.ModifiedBy = _appUser.Emp.UID;
                    survey.ModifiedTime = DateTime.Now;
                    survey.ServerModifiedTime = DateTime.Now;
                    // string jsonBody = JsonConvert.SerializeObject(survey, Formatting.Indented);
                    apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}Survey/CUDSurvey", HttpMethod.Put, survey);
                }

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
            return 0;
        }
        private async Task<Model.Interfaces.ISurvey> GetSurveyDetailsforEditFromAPIAsync(string uid)
        {
            ApiResponse<ISurvey> apiResponse = null;
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    Console.WriteLine("UID cannot be null or empty.");
                    return null;
                }
               apiResponse = await _apiService.FetchDataAsync<Model.Interfaces.ISurvey>(
                     $"{_appConfigs.ApiBaseUrl}Survey/GetSurveyByUID/{uid}",
                     HttpMethod.Get);

                if (apiResponse != null )
                {
                    return apiResponse.Data;
                        
                }
                else
                {
                    Console.WriteLine("API response is empty or unsuccessful.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching survey details: {ex.Message}");
            }
            return null;
        }
        private async Task<Model.Interfaces.ISurvey> GetSurveyDetailsByCodeFromAPIAsync(string code)
        {
            ApiResponse<ISurvey> apiResponse = null;
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    Console.WriteLine("UID cannot be null or empty.");
                    return null;
                }
                apiResponse = await _apiService.FetchDataAsync<Model.Interfaces.ISurvey>(
                      $"{_appConfigs.ApiBaseUrl}Survey/GetSurveyByCode/{code}",
                      HttpMethod.Get);

                if (apiResponse != null)
                {
                    return apiResponse.Data;

                }
                else
                {
                    Console.WriteLine("API response is empty or unsuccessful.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching survey details: {ex.Message}");
            }
            return null;
        }
    }
}
