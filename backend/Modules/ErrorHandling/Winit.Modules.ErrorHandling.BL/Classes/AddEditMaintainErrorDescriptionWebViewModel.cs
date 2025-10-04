using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.ErrorHandling.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ErrorHandling.BL.Classes
{
    public class AddEditMaintainErrorDescriptionWebViewModel : AddEditMaintainErrorDescriptionBaseViewModel
    {
        protected Winit.Shared.Models.Common.IAppConfig _appConfigs;
        protected Winit.Modules.Base.BL.ApiService _apiService;
        public AddEditMaintainErrorDescriptionWebViewModel(
             IServiceProvider serviceProvider,
             IFilterHelper filter,
             ISortHelper sorter,
             IListHelper listHelper,
             IAppUser appUser,
             IAppSetting appSetting,
             IDataManager dataManagerWinit, Shared.Models.Common.IAppConfig appConfig,
             Winit.Modules.Base.BL.ApiService apiService) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManagerWinit)
        {
            _appConfigs = appConfig;
            _apiService = apiService;
        }

        protected override async Task<bool> CUDErrorDescriptionDetails(IErrorDetailsLocalization errorDetail)
        {
            try
            {
                string jsonbody=JsonConvert.SerializeObject(errorDetail);
                ApiResponse<string> apiResponse =
                    await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}KnowledgeBase/CUDErrorDetailsLocalization",
                        HttpMethod.Post, new List<IErrorDetailsLocalization> { errorDetail });
                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
                return default;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected override async Task<IErrorDetailsLocalization?> GetErrorDescriptionDetailsByUID(string errorUID)
        {
            try
            {
                ApiResponse<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetailsLocalization> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetailsLocalization>(
                        $"{_appConfigs.ApiBaseUrl}KnowledgeBase/GetErrorDetailsLocalizationbyUID?errorDetailsLocalizationUID={errorUID}",
                        HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                return default;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected override async Task<bool> UpdateErrorDescriptionDetails(IErrorDetailsLocalization errorDetail)
        {
            try
            {
                ApiResponse<string> apiResponse =
                    await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}KnowledgeBase/CUDErrorDetailsLocalization",
                        HttpMethod.Put, errorDetail);
                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }
                return default;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
