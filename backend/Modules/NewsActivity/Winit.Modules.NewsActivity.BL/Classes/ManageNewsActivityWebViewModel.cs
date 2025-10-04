using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.NewsActivity.BL.Classes
{
    public class ManageNewsActivityWebViewModel : ManageNewsActivityBaseViewModel
    {
        protected IAppUser _appUser;
        protected IAppConfig _appConfig;
        protected ApiService _apiService;

        public ManageNewsActivityWebViewModel(IAppUser appUser, IAppConfig appConfig, ApiService apiService)
        {
            _apiService = apiService;
            _appUser = appUser;
            _appConfig = appConfig;
        }
        protected override async Task GetAllNewsActivity()
        {

            ApiResponse<PagedResponse<INewsActivity>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<INewsActivity>>($"{_appConfig.ApiBaseUrl}NewsActivity/SelectAllNewsActivities/{IsFilesysNeeded}",
                HttpMethod.Post, PagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                NewsActivities.Clear();
                NewsActivities.AddRange(apiResponse.Data.PagedData);
                TotalItems = apiResponse.Data.TotalCount;
            }
        }
        public async Task<ApiResponse<string>> DeleteNewsActivityByUID(string uid)
        {

            ApiResponse<string> apiResponse =
                await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}NewsActivity/DeleteNewsActivityByUID?UID={uid}",
                HttpMethod.Post, PagingRequest);
            return apiResponse;
        }
    }
}
