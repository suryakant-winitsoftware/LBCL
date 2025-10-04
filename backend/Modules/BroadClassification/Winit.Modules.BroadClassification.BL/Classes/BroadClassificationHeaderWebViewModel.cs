using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.BroadClassification.BL.Classes
{
    public class BroadClassificationHeaderWebViewModel : BroadClassificationHeaderBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly ILanguageService _languageService;
        private IStringLocalizer<LanguageKeys> _localizer;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private IAppUser _appUser;

        public BroadClassificationHeaderWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IAppUser appUser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService
         ) : base(serviceProvider, filter, sorter, listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _localizer = Localizer;
            _languageService = languageService;
            //WareHouseItemViewList = new List<IOrgType>();
        }
        public override async Task<bool> CreateUpdateBroadClassificationHeaderData(IBroadClassificationHeader broadClassificationHeader, bool Operation)
        {
            return await CreateUpdateBroadClassificationHeaderDataasync(broadClassificationHeader, Operation);
        }

        public override async Task<string> DeleteBroadClassificationHeaderData(object uID)
        {
            return await DeleteBroadClassificationHeaderDataAsync(uID);
        }
        public override async Task<List<IBroadClassificationHeader>> GetBroadClassificationHeaderData()
        {
            return await GetBroadClassificationHeaderDataFromAPIAsync();
        }

        public override async Task<IBroadClassificationHeader> GetBroadClassificationHeaderDetailsByUID(string UID)
        {
            return await GetBroadClassificationHeaderDetailsByUIDFromApiAsync(UID);
        }
        public override async Task<List<IListItem>?> GetClassificationsList(string classificationTypes)
        {
            return await GetClassificationlistItemsFromAPIAsync(classificationTypes);
        }

        private async Task<List<IListItem>?> GetClassificationlistItemsFromAPIAsync(string code)
        {
            try
            {
                Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new Winit.Modules.ListHeader.Model.Classes.ListItemRequest();
                listItemRequest.Codes = new List<string>() { code };
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                    HttpMethod.Post, listItemRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>>(apiResponse.Data);
                    if (pagedResponse != null && pagedResponse.Data != null)
                    {
                        TotalItemsCount = pagedResponse.Data.TotalCount;
                        return pagedResponse.Data.PagedData.OfType<IListItem>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }
            return null;
        }

        private async Task<List<IBroadClassificationHeader>> GetBroadClassificationHeaderDataFromAPIAsync()
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}BroadClassificationHeader/GetBroadClassificationHeaderDetails",
                    HttpMethod.Post, PagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader>>>(apiResponse.Data);
                    TotalItems = pagedResponse?.Data?.TotalCount ?? 0;
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>().ToList();
                }
            }
            catch (Exception ex)
            {

                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }



        private async Task<IBroadClassificationHeader> GetBroadClassificationHeaderDetailsByUIDFromApiAsync(string uID)
        {
            try
            {
                ApiResponse<Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader>(
                    $"{_appConfigs.ApiBaseUrl}BroadClassificationHeader/GetBroadClassificationHeaderDetailsByUID?UID={uID}",
                    HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<bool> CreateUpdateBroadClassificationHeaderDataasync(IBroadClassificationHeader broadClassificationHeader, bool operation)
        {
            try
            {
                ApiResponse<string> apiResponse;
                if (!operation)
                {
                    broadClassificationHeader.UID = Guid.NewGuid().ToString();
                    apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}BroadClassificationHeader/BroadClassificationHeader",
                        HttpMethod.Post, broadClassificationHeader);
                }
                else
                {
                    broadClassificationHeader.CreatedBy = _appUser.Emp.CreatedBy;
                    apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfigs.ApiBaseUrl}BroadClassificationHeader/UpdateBroadClassificationHeader",
                  HttpMethod.Put, broadClassificationHeader);
                }
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<string> DeleteBroadClassificationHeaderDataAsync(object uID)
        {
            throw new NotImplementedException();
        }
    }
}
