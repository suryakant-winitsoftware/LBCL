using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.Currency.BL.Classes
{
    public class MaintainCurrencyWebViewModel :MaintainCurrencyBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly ILanguageService _languageService;
        private IStringLocalizer<LanguageKeys> _localizer;
        public List<FilterCriteria> FilterCriterias { get; set; }
        //private readonly IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public MaintainCurrencyWebViewModel(IServiceProvider serviceProvider,
               IFilterHelper filter,
               ISortHelper sorter,
               //   IAppUser appUser,
               IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService
             ) : base(serviceProvider, filter, sorter, /*appUser,*/ listHelper, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            //  _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _localizer = Localizer;
            _languageService = languageService;
            //WareHouseItemViewList = new List<IOrgType>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public override async Task PopulateViewModel()
        {
            await base.PopulateViewModel();
        }
        public override async Task<List<Winit.Modules.Currency.Model.Interfaces.ICurrency>> GetCurrencyDetailsData()
        {
            return await GetCurrencyDetailsDataDataFromAPIAsync();
        }

        //public override async Task<List<ILocation>?> GetCountryDetailsSelectionItems(List<string> locationTypes)
        //{
        //    return await GetCountrySelectionItemsFromLocationAPIAsync(locationTypes);
        //}

        public override async Task<Winit.Modules.Currency.Model.Interfaces.ICurrency> GetCurrencyViewDetailsData(string UID)
        {
            return await GetCurrencyViewDetailsDataDataFromAPIAsync(UID);

        }
        public override async Task<bool> CreateUpdateCurrencyDetailsData(ICurrency Currency, bool Operation)
        {
            return await CreateUpdateCurrencyDetailsDataFromAPIAsync(Currency, Operation);

        }
        private async Task<List<Winit.Modules.Currency.Model.Interfaces.ICurrency>> GetCurrencyDetailsDataDataFromAPIAsync()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                // pagingRequest.PageNumber = PageNumber;
                // pagingRequest.PageSize = PageSize;
                pagingRequest.FilterCriterias = CurrencyDetailsFilterCriteria;

                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Currency/GetCurrencyDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<Winit.Modules.Currency.Model.Classes.Currency>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Currency.Model.Classes.Currency>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.OfType<Winit.Modules.Currency.Model.Interfaces.ICurrency>().ToList();
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }

        private async Task<Winit.Modules.Currency.Model.Interfaces.ICurrency> GetCurrencyViewDetailsDataDataFromAPIAsync(string UID)
        {
            try
            {
                ApiResponse<Winit.Modules.Currency.Model.Classes.Currency> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.Currency.Model.Classes.Currency>(
                    $"{_appConfigs.ApiBaseUrl}Currency/GetCurrencyById?UID={UID}",
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

        private async Task<bool> CreateUpdateCurrencyDetailsDataFromAPIAsync(ICurrency Currency, bool Operation)
        {
            try
            {
                ApiResponse<string> apiResponse;
                if (Operation)
                {
                    Currency.UID = Guid.NewGuid().ToString();
                    apiResponse = await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}Currency/CreateCurrency",
                        HttpMethod.Post, Currency);
                }
                else
                {
                    apiResponse = await _apiService.FetchDataAsync(
                  $"{_appConfigs.ApiBaseUrl}Currency/UpdateCurrency",
                  HttpMethod.Put, Currency);
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
                // Handle exceptions
                // Handle exceptions
            }
        }

        public override async Task<string> DeleteCurrency(object uID)
        {
            return await DeleteCurrencyFromGrid(uID);
        }

        private async Task<string> DeleteCurrencyFromGrid(object uID)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Currency/DeleteCurrency?UID={uID}",
                    HttpMethod.Delete, uID);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return "Currency successfully deleted.";
                }
                else if (apiResponse != null && apiResponse.Data != null)
                {
                    ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                    return $"{"Error failed to delete customers._error:"}{data.ErrorMessage}";
                }
                else
                {
                    return @_localizer["an_unexpected_error_occurred"];
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        //private async Task<List<ILocation>?> GetCountrySelectionItemsFromLocationAPIAsync(List<string> locationTypes)
        //{
        //    try
        //    {
        //        // Make the API call
        //        ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>> apiResponse = await _apiService.FetchDataAsync
        //            <List<Winit.Modules.Location.Model.Classes.Location>>(
        //            $"{_appConfigs.ApiBaseUrl}Location/GetLocationByTypes",
        //            HttpMethod.Post, locationTypes);
        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {
        //            return apiResponse.Data.ToList<ILocation>();
        //        }
        //        return null;
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }
        //}
    }
}
