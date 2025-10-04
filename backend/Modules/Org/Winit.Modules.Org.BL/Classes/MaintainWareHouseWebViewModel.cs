using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.Org.BL.Classes;

public class MaintainWareHouseWebViewModel : MaintainWareHouseBaseViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService _apiService;
    private readonly ILanguageService _languageService;
    private IStringLocalizer<LanguageKeys> _localizer;
    private readonly List<string> _propertiesToSearch = new();
    public MaintainWareHouseWebViewModel(IServiceProvider serviceProvider,
          IFilterHelper filter,
          ISortHelper sorter,
          IAppUser appUser,
          IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService)
      : base(serviceProvider, filter, sorter, appUser, listHelper, appConfigs, apiService)
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
        // Property set for Search
        _propertiesToSearch.Add("Code");
        _propertiesToSearch.Add("Name");
    }

    public override async Task PopulateViewModel()
    {
        await base.PopulateViewModel();
    }
    #region Business Logics  
    #endregion
    #region Database or Services Methods
    public override async Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>> GetMaintainWarteHouseData(string orgUID)
    {

        return await GetMaintainWarteHouseDataFromAPIAsync(orgUID , _appUser.Emp.Code );
    }
    public override async Task<string> DeleteMaintainWareHouseFromGrid(string uid)
    {
        return await DeleteMaintainWareHouseFromAPIAsync(uid);
    }
    public override async Task<List<ISelectionItem>> GetDistributorData()
    {
        return await GetDistributorDataFromAPIAsync();
    }
    #endregion
    #region Api Calling Methods
    private async Task<List<ISelectionItem>?> GetDistributorDataFromAPIAsync()
    {
        try

        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Dropdown/GetDistributorDropDown",
                HttpMethod.Post);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ApiResponse<List<SelectionItem>> Response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                return Response.Data.ToList<ISelectionItem>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }
    private async Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>?> GetMaintainWarteHouseDataFromAPIAsync(string orgUID, string code)
    {
        try
        {
            PagingRequest pagingRequest = new()
            {

                PageNumber = PageNumber,
                PageSize = PageSize,
                FilterCriterias = MaintainWarehouseFilterCriterias,

                SortCriterias = new List<SortCriteria>
            {
                new("OrgModifiedTime",SortDirection.Desc)
            }
            };
            pagingRequest.SortCriterias = SortCriterias;
            pagingRequest.IsCountRequired = true;
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Org/ViewFranchiseeWarehouse?FranchiseeOrgUID={orgUID}",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.WarehouseItemView>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.WarehouseItemView>>>(apiResponse.Data);
                TotalItemsCount = pagedResponse.Data.TotalCount;
                return pagedResponse.Data.PagedData.OfType<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>().ToList();
            }
        }
        catch (Exception)
        {
            // Handle exceptions
            // Handle exceptions
        }
        return null;
    }
    private async Task<string> DeleteMaintainWareHouseFromAPIAsync(string uid)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Org/DeleteViewFranchiseeWarehouse?UID={uid}",
                HttpMethod.Delete, uid);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return @_localizer["warehouse_successfully_deleted."];
            }
            else if (apiResponse != null && apiResponse.Data != null)
            {
                ApiResponse<string> data = JsonConvert.DeserializeObject<ApiResponse<string>>(apiResponse.Data);
                return $"{@_localizer["error_failed_to_delete_customers._error:"]} Error: {data.ErrorMessage}";
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
    protected override async Task<List<Winit.Modules.Org.Model.Interfaces.IOrgType>> GetWarehouseTypeDropdownDataFromAPIAsync()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<FilterCriteria>();
            pagingRequest.IsCountRequired = true;
            //pagingRequest.FilterCriterias.Add(new FilterCriteria("UID", "FRWH", FilterType.Equal));
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Org/GetOrgTypeDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                PagedResponse<Winit.Modules.Org.Model.Classes.OrgType> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Org.Model.Classes.OrgType>>(data);
                if (selectionORGs.PagedData != null)
                {
                    return selectionORGs.PagedData.OfType<IOrgType>().ToList();
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        return null;
    }
    #endregion
}
