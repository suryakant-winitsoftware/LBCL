using Microsoft.Extensions.Localization;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.Bank.BL.Classes;

public class ViewBankDetailsWebViewModel : ViewBankDetailsBaseViewModel
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
    public ViewBankDetailsWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
         //   IAppUser appUser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, IStringLocalizer<LanguageKeys> Localizer,
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
        _propertiesToSearch.Add("Code");
        _propertiesToSearch.Add("Name");
    }
    public override async Task PopulateViewModel()
    {
        LoadResources(null, _languageService.SelectedCulture);
        await base.PopulateViewModel();
    }
    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
        _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
    public override async Task<List<Winit.Modules.Bank.Model.Interfaces.IBank>> GetBankDetailsData()
    {
        return await GetBankDetailsDataDataFromAPIAsync();
    }
    public override async Task<List<ILocation>?> GetCountryDetailsSelectionItems(List<string> locationTypes)
    {
        return await GetCountrySelectionItemsFromLocationAPIAsync(locationTypes);
    }        
    public override async Task<Winit.Modules.Bank.Model.Interfaces.IBank> GetBankViewDatailsData(string UID)
    {
        return await GetBankViewDetailsDataDataFromAPIAsync(UID);
    }
    public override async Task<bool> CreateUpdateBankDetailsData(IBank bank, bool Operation)
    {
        return await CreateUpdateBankDetailsDataFromAPIAsync(bank, Operation);
    }
    private async Task<List<Winit.Modules.Bank.Model.Interfaces.IBank>> GetBankDetailsDataDataFromAPIAsync()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = BankDetailsFilterCriteria;
            pagingRequest.IsCountRequired = true;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Bank/GetBankDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ApiResponse<PagedResponse<Winit.Modules.Bank.Model.Classes.Bank>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Bank.Model.Classes.Bank>>>(apiResponse.Data);
                return pagedResponse.Data.PagedData.OfType<Winit.Modules.Bank.Model.Interfaces.IBank>().ToList();
            }
        }
        catch (Exception)
        {
            // Handle exceptions
            // Handle exceptions
        }
        return null;
    }
    private async Task<Winit.Modules.Bank.Model.Interfaces.IBank> GetBankViewDetailsDataDataFromAPIAsync(string UID)
    {
        try
        {
            ApiResponse<Winit.Modules.Bank.Model.Classes.Bank> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.Bank.Model.Classes.Bank>(
                $"{_appConfigs.ApiBaseUrl}Bank/GetBankDetailsByUID?UID={UID}",
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
    
    private async Task<bool> CreateUpdateBankDetailsDataFromAPIAsync(IBank bank, bool Operation)
    {
        try
        {
            ApiResponse<string> apiResponse;
            if (Operation) 
            {
                bank.UID = Guid.NewGuid().ToString();
                apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Bank/CreateBankDetails",
                    HttpMethod.Post, bank);
            }
            else
            {
                 apiResponse = await _apiService.FetchDataAsync(
               $"{_appConfigs.ApiBaseUrl}Bank/UpdateBankDetails",
               HttpMethod.Put, bank);
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
    public override async Task<string> DeleteVehicle(object uID)
    {
        return await DeleteVehicleFromGrid(uID);
    }

    private async Task<string> DeleteVehicleFromGrid(object uID)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Bank/DeleteBankDetail?UID={uID}",
                HttpMethod.Delete, uID);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return @_localizer["bank_successfully_deleted"];
            }
            else if (apiResponse != null && apiResponse.Data != null)
            {
                ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                return $"{@_localizer["error_failed_to_delete_customers._error:"]} {data.ErrorMessage}";
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
    private async Task<List<ILocation>?> GetCountrySelectionItemsFromLocationAPIAsync(List<string> locationTypes)
    {
        try
        {
            ApiResponse<List<Winit.Modules.Location.Model.Classes.Location>> apiResponse = await _apiService.FetchDataAsync
                < List<Winit.Modules.Location.Model.Classes.Location>>(
                $"{_appConfigs.ApiBaseUrl}Location/GetLocationByTypes",
                HttpMethod.Post, locationTypes);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
               return apiResponse.Data.ToList<ILocation>();
            }
            return null;
        }
        catch (Exception e)
        {
            throw;
        }
    }
}
