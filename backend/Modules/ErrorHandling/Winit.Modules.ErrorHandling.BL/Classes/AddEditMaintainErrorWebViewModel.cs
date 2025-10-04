using Microsoft.Extensions.Localization;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.ErrorHandling.BL.Classes
{
    public class AddEditMaintainErrorWebViewModel:AddEditMaintainErrorBaseViewModel
    {
        protected Winit.Shared.Models.Common.IAppConfig _appConfigs;
        protected Winit.Modules.Base.BL.ApiService _apiService;
        private readonly ILanguageService _languageService;
        private IStringLocalizer<LanguageKeys> _localizer;
        public AddEditMaintainErrorWebViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            IDataManager dataManagerWinit,Shared.Models.Common.IAppConfig appConfig,
            Winit.Modules.Base.BL.ApiService apiService, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService) :base(serviceProvider,filter, sorter, listHelper,appUser,appSetting,dataManagerWinit, Localizer, languageService)
        {
            _appConfigs = appConfig;
            _apiService = apiService;
            _localizer = Localizer;
            _languageService = languageService;
        }
        protected override async Task<List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>?> GetListItemsByCodes(List<string> codes)
        {
            try
            {
                Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new();
                listItemRequest.Codes = codes;
                ApiResponse < PagedResponse < Winit.Modules.ListHeader.Model.Classes.ListItem >> apiResponse =
                    await _apiService.FetchDataAsync<PagedResponse<ListHeader.Model.Classes.ListItem>>(
                        $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                        HttpMethod.Post, listItemRequest);
                if(apiResponse != null && apiResponse.IsSuccess)
                {
                    return apiResponse.Data.PagedData.ToList<IListItem>();
                }
                return new();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        protected override async Task<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail?> GetErrorDetailsByUID(string errorUID)
        {
            try
            {
                
                ApiResponse<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetail> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetail>(
                        $"{_appConfigs.ApiBaseUrl}KnowledgeBase/GetErrorDetailsByUID?UID={errorUID}",
                        HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess)
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
        protected override async Task<bool> CreateErrorDetails(Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail error)
        {
            try
            {

                ApiResponse<string> apiResponse =
                    await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}KnowledgeBase/CreateErrorDetails",
                        HttpMethod.Post,error);
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
        protected override async Task<bool> UpdateErrorDetails(Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail error)
        {
            try
            {

                ApiResponse<string> apiResponse =
                    await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}KnowledgeBase/UpdateErrorDetails",
                        HttpMethod.Put, error);
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
