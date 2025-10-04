using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http;
using System.Resources;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;

namespace WinIt.Pages;
partial class DashBoardV1 : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

    [CascadingParameter]
    public EventCallback<Winit.Modules.Role.Model.Interfaces.IModulesMasterView> ModuleMasterEvent { get; set; }

    //boolean added by shanmukha
    private bool IsFilterOpen = false;
    protected async override Task OnInitializedAsync()
    {
        try
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();
                //await GetAllMenuDetails();
                
                // await GetAppuserEmp();
              //  await GetAppuserCurrency();
                //object obj = dataManager.GetData(Winit.Shared.Models.Constants.CommonMasterDataConstants.SKUGroup);
               // await LoadSettingMaster();
                NavigationManager.LocationChanged -= ResetbreadCrum;
                await CallbackService.InvokeAsync(null);
                await LoadErrorDetailsAsync();
                LoadResources(null, _languageService.SelectedCulture);
               // _ = PopualteTax();
               // _ = SetOrgHierarchyParentUIDsByOrgUID();
                HideLoader();
            });
        }
        catch (Exception)
        {

            throw;
        }
    }
   

    protected async Task GetAppuserCurrency()
    {
        try
        {
            Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
           $"{_appConfigs.ApiBaseUrl}Currency/GetCurrencyListByOrgUID?orgUID={_iAppUser.SelectedJobPosition.OrgUID}", HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                Winit.Shared.Models.Common.ApiResponse<List<OrgCurrency>> response = JsonConvert.DeserializeObject<ApiResponse<List<OrgCurrency>>>(apiResponse.Data);
                if (response != null)
                {
                    _iAppUser.OrgCurrencyList = response.Data.ToList<IOrgCurrency>();
                    _iAppUser.DefaultOrgCurrency = _iAppUser.OrgCurrencyList.FirstOrDefault(m => m.IsPrimary == true);
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    private async Task PopualteTax()
    {
        try
        {
            ApiResponse<Dictionary<string, ITax>> apiResponse = await _apiService.FetchDataAsync<Dictionary<string, ITax>>(
                $"{_appConfigs.ApiBaseUrl}Tax/GetTaxMaster",
                HttpMethod.Post, new List<string> { _iAppUser.SelectedJobPosition.OrgUID });

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {

                _iAppUser.TaxDictionary = apiResponse.Data;
            }
        }
        catch (Exception)
        {

            throw;
        }
    }
    private async Task LoadSettingMaster()
    {

        PagingRequest pagingRequest = new PagingRequest();

        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}Setting/SelectAllSettingDetails",
            HttpMethod.Post, pagingRequest);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
            PagedResponse<Winit.Modules.Setting.Model.Classes.Setting> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Setting.Model.Classes.Setting>>(data);
            if (pagedResponse != null && pagedResponse.PagedData != null)
            {
                _appSetting.PopulateSettings(pagedResponse.PagedData);
            }
        }
    }
    private async Task SetOrgHierarchyParentUIDsByOrgUID()
    {
        List<string> orgUIds = [_iAppUser.SelectedJobPosition.OrgUID];
        ApiResponse<List<string>> apiResponse = await _apiService.FetchDataAsync<List<string>>(
            $"{_appConfigs.ApiBaseUrl}Org/GetOrgHierarchyParentUIDsByOrgUID",
            HttpMethod.Post, orgUIds);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            _iAppUser.OrgUIDs = apiResponse.Data;  
        }
    }
    private async Task LoadErrorDetailsAsync()
    {

        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
            $"{_appConfigs.ApiBaseUrl}KnowledgeBase/GetErrorDetailsAsync",
            HttpMethod.Post);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
            Dictionary<string, ErrorDetail> errorDetailDictionaryConcrete = JsonConvert.DeserializeObject<Dictionary<string, ErrorDetail>>(data);

            // Convert to Dictionary<string, IErrorDetail>
            Dictionary<string, IErrorDetail> errorDetailDictionary = errorDetailDictionaryConcrete
                .ToDictionary(kv => kv.Key, kv => (IErrorDetail)kv.Value);

            if (errorDetailDictionary != null)
            {
                await _errorHandlerBL.SetErrorDetailDictionary(errorDetailDictionary);
            }
        }
    }
    public void ResetbreadCrum(object sender, LocationChangedEventArgs args)
    {

        CallbackService.InvokeAsync(_IDataService);
    }
    public async Task GetAllMenuDetails()
    {
        Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Role/GetAllModulesMaster", HttpMethod.Get);
        if (apiResponse != null)
        {
            if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
            {
                ApiResponse<ModulesMasterView>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<ModulesMasterView>>(apiResponse.Data);
                if (apiResponse1.StatusCode == 200 && apiResponse1.Data != null)
                {
                    ModulesMasterView view = apiResponse1.Data;
                    _dataManager.SetData(nameof(IModulesMasterView), view);
                    ModulesMasterView masterView = new ModulesMasterView() { Modules = view.Modules, SubModules = view.SubModules, SubSubModules = view.SubSubModules };
                    await ModuleMasterEvent.InvokeAsync(masterView);
                    await InvokeAsync(StateHasChanged);
                    StateHasChanged();
                }
            }
        }
    }
}
