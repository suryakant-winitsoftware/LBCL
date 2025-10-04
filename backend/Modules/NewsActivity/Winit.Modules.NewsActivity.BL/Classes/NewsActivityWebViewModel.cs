using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.NewsActivity.BL.Classes
{
    public class NewsActivityWebViewModel : NewsActivityBaseViewModel
    {
        protected IAppConfig _appConfig { get; }
        protected ApiService _apiService { get; }
        public NewsActivityWebViewModel(IAppConfig appConfig, IAppUser appUser, ApiService apiService, INewsActivity newsActivity,
            CommonFunctions commonFunctions, IDataManager? dataManager) : base(appConfig, apiService)
        {
            _apiService = apiService;
            _appUser = appUser;
            _appConfig = appConfig;
            _commonFunctions = commonFunctions;
            _dataManager = dataManager;
            NewsActivity = newsActivity;
        }

        public async Task<ApiResponse<string>> CreateNewsActivity()
        {
            return await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}NewsActivity/CreateNewsActivity", HttpMethod.Post, NewsActivity);
        }
        public async Task<ApiResponse<string>> UpdateNewsActivity()
        {
            NewsActivity.ModifiedBy = _appUser.Emp.UID;
            NewsActivity.ModifiedTime = DateTime.Now;
            return await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}NewsActivity/UpdateNewsActivity", HttpMethod.Put, NewsActivity);
        }
        public override async Task GetNewsActivityBYUID(string uid)
        {
            var data = _dataManager.GetData(uid);
            if(data == null) {
            ApiResponse<INewsActivity> apiResponse = await _apiService.FetchDataAsync<INewsActivity>($"{_appConfig.ApiBaseUrl}NewsActivity/GetNewsActivitysByUID?UID={uid}", HttpMethod.Get);
            if (apiResponse != null)
            {
                NewsActivity = apiResponse.Data;
            }
            }
            else
            {
                NewsActivity = (INewsActivity)data;
            }

        }
        public async Task DeleteNewsActivityByUID(string UID)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}NewsActivity/DeleteNewsActivityByUID?UID={UID}", HttpMethod.Delete);
        }
    }
}
