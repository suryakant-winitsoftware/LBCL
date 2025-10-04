using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Mapping.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Common.BL;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Modules.Mapping.Model.Classes;
using Winit.Shared.Models.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Mapping.Model.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Microsoft.Extensions.Localization;
using Winit.UIComponents.Common.LanguageResources.Web;
using System.Globalization;
using System.Resources;
using Npgsql.Replication.PgOutput.Messages;
using DocumentFormat.OpenXml.Packaging;
using Winit.UIComponents.SnackBar.Services;

namespace Winit.Modules.Mapping.BL.Classes;

public class MappingViewModel : IMappingViewModel
{
    public MappingComponentDTO MappingDTO { get; }
    private List<ILocationType>? LocationTypes { get; set; }
    private List<ILocation>? Locations { get; set; }
    private List<IStoreGroupType>? StoreGroupTypes { get; set; }
    private List<IStoreGroup>? StoreGroups { get; set; }
    //public List<ISelectionItem>? LocationTypesSelectionItems { get; set; }
    //public List<ISelectionItem>? LocationsSelectionItems { get; set; }
    //public List<ISelectionItem>? StoreGroupTypesSelectionItems { get; set; }
    //public List<ISelectionItem>? StoreGroupsSelectionItems { get; set; }
    public bool IsExcluded { get; set; }
    private bool _IsInitialized { get; set; }
    //public List<ISelectionItem> TabSelectionItems { get; set; }
    private SelectionManager? TabSM { get; set; }
    private SelectionManager? LocationTypeSM { get; set; }
    private SelectionManager? LocationsSM { get; set; }
    private SelectionManager? StoreGroupTypeSM { get; set; }
    private SelectionManager? StoreGroupsSM { get; set; }
    private SelectionManager? DistributorTypeSM { get; set; }
    private SelectionManager? DistributorSM { get; set; }
    private SelectionManager? UserTypeSM { get; set; }
    private SelectionManager? UserSM { get; set; }
    public int TabIndex { get; set; }
    //public List<IMappingItemView>? GridDataSource { get; set; }
    //public string? LinkedItemUID { get; set; }
    //public string? LinkedItemType { get; set; }
    public bool IsEdit { get; set; }
    public ISelectionMapMaster SelectionMapMaster = new SelectionMapMaster();

    private IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Base.BL.ApiService _apiService;
    private readonly ILanguageService _languageService;
    private IStringLocalizer<LanguageKeys> _localizer;
    public MappingViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService, IAppUser appUser, IStringLocalizer<LanguageKeys> Localizer, ILanguageService LanguageService)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appConfigs = appConfigs;
        _apiService = apiService;
        _appUser = appUser;
        _localizer = Localizer;
        _languageService = LanguageService;

        MappingDTO = new();
        MappingDTO.DistributorTypeSelectionItems.Add(new SelectionItem { Label = GroupConstant.Distributor, Code = GroupConstant.Distributor, IsSelected = true });
        MappingDTO.UserTypeSelectionItems.Add(new SelectionItem { Label = GroupConstant.User, Code = GroupConstant.User, IsSelected = true });

        MappingDTO.OnLocationClick = AddLocationToGrid;
        MappingDTO.OnStoreGroupClick = AddStoreToGrid;
        MappingDTO.OnDistributorClick = AddDistributorsToGrid;
        MappingDTO.OnUserClick = AddUsersToGrid;

        MappingDTO.OnTabClick = OnTabClick;
        MappingDTO.OnLocationTypeDDClick = async (e) => await LocationTypeDDClick(e);
        MappingDTO.OnStoreGroupTypeDDClick = async (e) => await StoreGroupTypeDDClick(e);
    }
    Winit.UIComponents.SnackBar.IToast _toast => _serviceProvider.GetRequiredService<Winit.UIComponents.SnackBar.IToast>();
    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
        _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
    protected void PrepareMappingDTO()
    {
        //MappingDTO.TabSelectionItems.AddRange(TabSelectionItems);
        //MappingDTO.GridDataSource.AddRange(GridDataSource);
        //MappingDTO.LocationTypesSelectionItems.AddRange(LocationTypesSelectionItems);
        //MappingDTO.LocationsSelectionItems.AddRange(LocationsSelectionItems);
        //MappingDTO.StoreGroupTypesSelectionItems.AddRange(StoreGroupTypesSelectionItems);
        //MappingDTO.StoreGroupsSelectionItems.AddRange(StoreGroupsSelectionItems);
    }

    public async Task PopulateViewModel(string? linkedItemUID = null, string? linkedItemType = null)
    {
        GenerateDataGridColumns();
        MappingDTO.LinkedItemType = linkedItemType ?? string.Empty;
        MappingDTO.LinkedItemUID = linkedItemUID ?? string.Empty;
        Task dist = GetDistributorsFromAPI();
        Task user = GetRoutesFromAPI();
        await Task.WhenAll(dist, user);

        LoadResources(null, _languageService.SelectedCulture);
        LocationTypes = new List<ILocationType>();
        Locations = new List<ILocation>();
        StoreGroupTypes = new List<IStoreGroupType>();
        StoreGroups = new List<IStoreGroup>();
        //LocationTypesSelectionItems = new List<ISelectionItem>();
        //LocationsSelectionItems = new List<ISelectionItem>();
        //StoreGroupTypesSelectionItems = new List<ISelectionItem>();
        //StoreGroupsSelectionItems = new List<ISelectionItem>();
        MappingDTO.TabSelectionItems.AddRange(new List<ISelectionItem>
            {
                new SelectionItem{Label=@_localizer["location"], Code = "Location",IsSelected =true},
                new SelectionItem{Label=@_localizer["customer"], Code = "Customer"},
                new SelectionItem{Label=GroupConstant.Distributor, Code = GroupConstant.Distributor},
                new SelectionItem{Label=GroupConstant.User, Code = GroupConstant.User},
            });

        //GridDataSource = new List<IMappingItemView>();
        _IsInitialized = true;
        TabSM = new SelectionManager(MappingDTO.TabSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
        LocationTypes?.AddRange(await GetLocationTypesFromAPI());
        MappingDTO.LocationTypesSelectionItems.AddRange(ConvertToSelectionItems<ILocationType>(LocationTypes));
        LocationTypeSM = new SelectionManager(MappingDTO.LocationTypesSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
        StoreGroupTypes?.AddRange(await GetStoreGroupTypesFromAPI());
        MappingDTO.StoreGroupTypesSelectionItems.AddRange(ConvertToSelectionItems<IStoreGroupType>(StoreGroupTypes));
        StoreGroupTypeSM = new SelectionManager(MappingDTO.StoreGroupTypesSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
        DistributorTypeSM = new SelectionManager(MappingDTO.DistributorTypeSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
        UserTypeSM = new SelectionManager(MappingDTO.UserTypeSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
        DistributorSM = new SelectionManager(MappingDTO.DistributorSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Multiple);
        UserSM = new SelectionManager(MappingDTO.UserSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Multiple);

        if (!string.IsNullOrEmpty(linkedItemUID))
        {
            ISelectionMapMaster? selectionMapMaster = await GetSelectionMapMasterByLinkedItemUID(linkedItemUID);
            if (selectionMapMaster != null)
            {
                SelectionMapMaster = selectionMapMaster;
                //SelectionMapMaster.SelectionMapCriteria = selectionMapMaster.SelectionMapCriteria;
                IsEdit = true;
                if (SelectionMapMaster.SelectionMapDetails != null)
                    MappingDTO.GridDataSource!.AddRange(ConvertToMappingItemView(SelectionMapMaster.SelectionMapDetails, ActionType.Update));
            }
            else
            {
                IsEdit = false;
                SelectionMapMaster = new SelectionMapMaster();
            }
        }

    }
    protected void GenerateDataGridColumns()
    {
        MappingDTO.DataGridColumns.AddRange(new List<DataGridColumn>
        {
            new DataGridColumn
            {
                Header = _localizer["s_no"],
                GetValue = s => ((IMappingItemView)s).SNO
            },
            new DataGridColumn
            {
                Header = _localizer["type"],
                GetValue = s => ((IMappingItemView)s)?.Type
            },
            new DataGridColumn
            {
                Header = _localizer["value"],
                GetValue = s => ((IMappingItemView)s)?.Value
            },
            new DataGridColumn
            {
                Header = _localizer["is_excluded"],
                GetValue = s => ((IMappingItemView)s).IsExcluded ? _localizer["yes"] : _localizer["no"]
            }
        });
    }
    public void DeleteItemFromGrid(IMappingItemView mappingItemView)
    {
        //_MappingViewModel.GridDataSource?.Remove(mappingItemView);
        mappingItemView.ActionType = Winit.Shared.Models.Enums.ActionType.Delete;
        _toast.Add("Mapping", mappingItemView.Value + _localizer["deleted_from_grid"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
    }
    public void OnTabClick(ISelectionItem selectionItem)
    {
        if (!selectionItem.IsSelected)
        {
            TabSM?.Select(selectionItem);
            TabIndex = TabSM?.GetSelectionItemIndex() ?? 0;
            TabIndex = TabSM?.GetSelectionItemIndex() ?? 0;
            MappingDTO.TabIndex = TabIndex;
        }
    }
    public List<string> AddLocationToGrid()
    {
        List<string> duplicateValues = new List<string>();
        var selectedLocationType = LocationTypeSM?.GetSelectedSelectionItems().FirstOrDefault();
        var selectedLocations = LocationsSM?.GetSelectedSelectionItems();
        if (selectedLocations != null)
            foreach (var item in selectedLocations)
            {
                var check = MappingDTO.GridDataSource?.Find(e => e.Value == item.Code);
                if (check == null)
                {
                    Winit.Modules.Mapping.Model.Interfaces.IMappingItemView mappingItemView = new MappingItemView();
                    mappingItemView.SNO = MappingDTO.GridDataSource?.Count + 1 ?? 0;
                    AddCreateFields(mappingItemView, true);
                    mappingItemView.Code = item.Code;
                    mappingItemView.Type = selectedLocationType?.Label;
                    mappingItemView.TypeUID = selectedLocationType?.Code;
                    mappingItemView.Value = item.Code;
                    mappingItemView.IsExcluded = IsExcluded;
                    mappingItemView.Group = GroupConstant.Location;
                    mappingItemView.ActionType = ActionType.Add;
                    MappingDTO.GridDataSource?.Add(mappingItemView);
                }
                else
                {
                    duplicateValues.Add(check.Value!);
                }
            }
        return duplicateValues;
    }
    public List<string> AddDistributorsToGrid()
    {
        List<string> duplicateValues = new List<string>();
        var selectedType = DistributorTypeSM?.GetSelectedSelectionItems().FirstOrDefault();
        var selected = DistributorSM?.GetSelectedSelectionItems();
        if (selected != null)
            foreach (var item in selected)
            {
                var check = MappingDTO.GridDataSource?.Find(e => e.Value == item.Code);
                if (check == null)
                {
                    Winit.Modules.Mapping.Model.Interfaces.IMappingItemView mappingItemView = new MappingItemView();
                    mappingItemView.SNO = MappingDTO.GridDataSource?.Count + 1 ?? 0;
                    AddCreateFields(mappingItemView, true);
                    mappingItemView.Code = item.Code;
                    mappingItemView.Type = selectedType?.Label;
                    mappingItemView.TypeUID = selectedType?.Code;
                    mappingItemView.Value = item.Code;
                    mappingItemView.IsExcluded = IsExcluded;
                    mappingItemView.Group = GroupConstant.Distributor;
                    mappingItemView.ActionType = ActionType.Add;
                    MappingDTO.GridDataSource?.Add(mappingItemView);
                }
                else
                {
                    duplicateValues.Add(check.Value!);
                }
            }
        return duplicateValues;
    }
    public List<string> AddUsersToGrid()
    {
        List<string> duplicateValues = new List<string>();
        var selectedType = UserTypeSM?.GetSelectedSelectionItems().FirstOrDefault();
        var selected = UserSM?.GetSelectedSelectionItems();
        if (selected != null)
        {
            foreach (var item in selected)
            {
                var check = MappingDTO.GridDataSource?.Find(e => e.Value == item.Code && e.Group == GroupConstant.User);
                if (check == null)
                {
                    Winit.Modules.Mapping.Model.Interfaces.IMappingItemView mappingItemView = new MappingItemView();
                    mappingItemView.SNO = MappingDTO.GridDataSource?.Count + 1 ?? 0;
                    AddCreateFields(mappingItemView, true);
                    mappingItemView.Code = item.Code;
                    mappingItemView.Type = selectedType?.Label;
                    mappingItemView.TypeUID = selectedType?.Code;
                    mappingItemView.Value = item.Code;
                    mappingItemView.IsExcluded = IsExcluded;
                    mappingItemView.Group = GroupConstant.User;
                    mappingItemView.ActionType = ActionType.Add;
                    MappingDTO.GridDataSource?.Add(mappingItemView);
                }
                else
                {
                    duplicateValues.Add(check.Value!);
                }
            }
        }
        return duplicateValues;
    }


    public List<string> AddStoreToGrid()
    {
        List<string> duplicateValues = new List<string>();
        var selectedStoreGroupType = StoreGroupTypeSM?.GetSelectedSelectionItems().FirstOrDefault();
        var selectedStoreGroups = StoreGroupsSM?.GetSelectedSelectionItems();
        if (selectedStoreGroups != null)
        {
            foreach (var item in selectedStoreGroups)
            {
                var check = MappingDTO.GridDataSource?.Find(e => e.Value == item.Code);
                if (check == null)
                {
                    IMappingItemView mappingItemView = new MappingItemView();
                    mappingItemView.SNO = MappingDTO.GridDataSource?.Count + 1 ?? 0;
                    AddCreateFields(mappingItemView, true);
                    mappingItemView.Type = selectedStoreGroupType?.Label;
                    mappingItemView.TypeUID = selectedStoreGroupType?.Code;
                    mappingItemView.Value = item.Code;
                    mappingItemView.IsExcluded = IsExcluded;
                    mappingItemView.Group = GroupConstant.Customer;
                    mappingItemView.ActionType = ActionType.Add;
                    MappingDTO.GridDataSource?.Add(mappingItemView);
                }
                else
                {
                    duplicateValues.Add(check.Value!);
                }
            }
        }
        return duplicateValues;
    }

    public async Task LocationTypeDDClick(ISelectionItem selectedLocationType)
    {
        Locations?.Clear();
        Locations?.AddRange(await GetLocationsFromAPI(selectedLocationType?.UID!));
        MappingDTO.LocationsSelectionItems?.Clear();
        MappingDTO.LocationsSelectionItems?.AddRange(ConvertToSelectionItems(Locations));
        LocationsSM = new SelectionManager(MappingDTO.LocationsSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Multiple);
    }
    public async Task StoreGroupTypeDDClick(ISelectionItem selectedStoreGroupType)
    {
        StoreGroups?.Clear();
        StoreGroups?.AddRange(await GetStoreGroupsFromAPI(selectedStoreGroupType?.UID));
        MappingDTO.StoreGroupsSelectionItems?.Clear();
        MappingDTO.StoreGroupsSelectionItems?.AddRange(ConvertToSelectionItems(StoreGroups));
        StoreGroupsSM = new SelectionManager(MappingDTO.StoreGroupsSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Multiple);
    }

    //APIs
    public async Task<List<ILocationType>> GetLocationTypesFromAPI()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();
            ApiResponse<PagedResponse<LocationType>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<LocationType>>(
                 $"{_appConfigs.ApiBaseUrl}LocationType/SelectAllLocationTypeDetails",
                 HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.PagedData.OfType<ILocationType>().ToList();
            }
            return new List<ILocationType>();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<ILocation>> GetLocationsFromAPI(string locationTypeUID)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>()
                {
                    new Winit.Shared.Models.Enums.FilterCriteria("LocationTypeUID",locationTypeUID,Winit.Shared.Models.Enums.FilterType.Equal)
                };
            ApiResponse<PagedResponse<Winit.Modules.Location.Model.Classes.Location>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Location.Model.Classes.Location>>(
                 $"{_appConfigs.ApiBaseUrl}Location/SelectAllLocationDetails",
                 HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.PagedData.OfType<ILocation>().ToList();
            }
            return null;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<IStoreGroupType>> GetStoreGroupTypesFromAPI()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>();
            ApiResponse<PagedResponse<StoreGroupType>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<StoreGroupType>>(
                 $"{_appConfigs.ApiBaseUrl}StoreGroupType/SelectAllStoreGroupType",
                 HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.PagedData.OfType<IStoreGroupType>().ToList();
            }
            return null;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<IStoreGroup>> GetStoreGroupsFromAPI(string storeGroupTypeUID)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Winit.Shared.Models.Enums.FilterCriteria>()
                {
                    new Winit.Shared.Models.Enums.FilterCriteria("StoreGroupTypeUID",storeGroupTypeUID,Winit.Shared.Models.Enums.FilterType.Equal)
                };
            ApiResponse<PagedResponse<StoreGroup>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<StoreGroup>>(
                 $"{_appConfigs.ApiBaseUrl}StoreGroup/SelectAllStoreGroup",
                 HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.PagedData.OfType<IStoreGroup>().ToList();
            }
            return [];
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task GetDistributorsFromAPI()
    {
        try
        {
            ApiResponse<List<ISelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<ISelectionItem>>(
                 $"{_appConfigs.ApiBaseUrl}Dropdown/GetDistributorDropDown",
                 HttpMethod.Post);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                MappingDTO.DistributorSelectionItems.Clear();
                MappingDTO.DistributorSelectionItems.AddRange(apiResponse.Data);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task GetRoutesFromAPI()
    {
        try
        {
            ApiResponse<List<ISelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<ISelectionItem>>(
                 $"{_appConfigs.ApiBaseUrl}Dropdown/GetRouteDropDown?orgUID={_appUser.SelectedJobPosition.OrgUID}",
                 HttpMethod.Post);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                MappingDTO.UserSelectionItems.Clear();
                MappingDTO.UserSelectionItems.AddRange(apiResponse.Data);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> SaveMappings(ISelectionMapMaster selectionMapMaster)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
                 $"{_appConfigs.ApiBaseUrl}Mapping/CUDSelectiomMapMaster",
                 HttpMethod.Post, selectionMapMaster);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<ISelectionMapMaster?> GetSelectionMapMasterByLinkedItemUID(string linkedItemUID)
    {
        try
        {
            ApiResponse<SelectionMapMasterDTO> apiResponse = await _apiService.FetchDataAsync<SelectionMapMasterDTO>(
                 $"{_appConfigs.ApiBaseUrl}Mapping/GetSelectionMapMasterByLinkedItemUID?linkedItemUID={linkedItemUID}",
                 HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ISelectionMapMaster selectionMapMaster = new SelectionMapMaster
                {
                    SelectionMapCriteria = apiResponse.Data.SelectionMapCriteria,
                    SelectionMapDetails = apiResponse.Data?.SelectionMapDetails?.ToList<ISelectionMapDetails>(),
                };
                return selectionMapMaster;
            }
            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public List<SelectionItem> ConvertToSelectionItems<T>(List<T>? Items)
    {
        List<SelectionItem> selectionItems = new List<SelectionItem>();
        if (Items != null)
            foreach (var item in Items)
            {
                SelectionItem selectionItem = new SelectionItem();
                selectionItem.Code = item?.GetPropertyValue<string>("Code");
                selectionItem.UID = item?.GetPropertyValue<string>("UID");
                selectionItem.Label = item?.GetPropertyValue<string>("Name");
                selectionItems.Add(selectionItem);
            }
        return selectionItems;
    }

    public async Task<bool> SaveMapping()
    {
        try
        {
            if (IsEdit)
            {
                SelectionMapMaster.SelectionMapCriteria!.ActionType = ActionType.Update;
                AddUpdateFields(SelectionMapMaster.SelectionMapCriteria);
            }
            else
            {
                SelectionMapMaster.SelectionMapCriteria = new SelectionMapCriteria();
                SelectionMapMaster.SelectionMapCriteria.ActionType = ActionType.Add;
                AddCreateFields(SelectionMapMaster.SelectionMapCriteria, true);
            }
            SelectionMapMaster.SelectionMapDetails = new List<ISelectionMapDetails>();
            SelectionMapMaster.SelectionMapCriteria.LinkedItemType = MappingDTO.LinkedItemType;
            SelectionMapMaster.SelectionMapCriteria.LinkedItemUID = MappingDTO.LinkedItemUID;
            if (MappingDTO.GridDataSource != null)
                foreach (var row in MappingDTO.GridDataSource)
                {
                    ISelectionMapDetails selectionMap = new SelectionMapDetails
                    {
                        IsExcluded = row.IsExcluded,
                        SelectionMapCriteriaUID = SelectionMapMaster.SelectionMapCriteria.UID,
                        SelectionGroup = row.Group,
                        TypeUID = row.TypeUID,
                        SelectionValue = row.Value,
                        ActionType = row.ActionType,
                        UID = row.UID!
                    };
                    if (IsEdit) AddUpdateFields(selectionMap);
                    else AddCreateFields(selectionMap, true);
                    ProcessSelectionMapCriteriaWithSelectionMapDetails(SelectionMapMaster.SelectionMapCriteria, selectionMap);
                    SelectionMapMaster.SelectionMapDetails.Add(selectionMap);
                }
            return await SaveMappings(SelectionMapMaster);
        }
        catch (Exception)
        {
            throw;
        }
    }
    private void ProcessSelectionMapCriteriaWithSelectionMapDetails(ISelectionMapCriteria selectionMapCriteria,
        ISelectionMapDetails selectionMapDetails)
    {
        switch (selectionMapDetails.SelectionGroup)
        {
            case GroupConstant.Location:
                selectionMapCriteria.HasLocation = true;
                selectionMapCriteria.LocationCount++;
                break;
            case GroupConstant.Customer:
                selectionMapCriteria.HasCustomer = true;
                selectionMapCriteria.CustomerCount++;
                break;
            case GroupConstant.Distributor:
                selectionMapCriteria.HasOrganization = true;
                selectionMapCriteria.OrgCount++;
                break;
            case GroupConstant.User:
                selectionMapCriteria.HasSalesTeam = true;
                selectionMapCriteria.SalesTeamCount++;
                break;
        }
    }
    #region Common Util Methods
    private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired)
    {
        baseModel.CreatedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
    }
    private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT"; //_appUser.Emp.UID;
        baseModel.ModifiedTime = DateTime.Now;
    }
    #endregion
    #region Conversion Methods
    private List<IMappingItemView> ConvertToMappingItemView(List<ISelectionMapDetails> selectionMapDetails, ActionType actionType)
    {
        Dictionary<string, string> allTypesDict = ConvertAllTypesIntoDictionary();
        List<IMappingItemView> mappingItemViews = new List<IMappingItemView>();

        foreach (var selectionMapDetailsItem in selectionMapDetails)
        {


            IMappingItemView mappingItemView = new MappingItemView
            {
                IsExcluded = selectionMapDetailsItem.IsExcluded,
                UID = selectionMapDetailsItem.UID,
                TypeUID = selectionMapDetailsItem.TypeUID,
                Value = selectionMapDetailsItem.SelectionValue,
                Group = selectionMapDetailsItem.SelectionGroup,
                Type = allTypesDict.GetValueOrDefault(selectionMapDetailsItem.TypeUID!),
                ActionType = actionType,
                SNO = mappingItemViews.Count + 1
            };
            mappingItemViews.Add(mappingItemView);

        }
        return mappingItemViews;
    }
    protected Dictionary<string, string> ConvertAllTypesIntoDictionary()
    {
        Dictionary<string, string> allTypesDict = new();
        foreach (ISelectionItem item in MappingDTO.LocationTypesSelectionItems)
        {
            if (!allTypesDict.ContainsKey(item!.Code!))
                allTypesDict[item.Code!] = item!.Label!;
        }
        foreach (ISelectionItem item in MappingDTO.StoreGroupTypesSelectionItems)
        {
            if (!allTypesDict.ContainsKey(item!.Code!))
                allTypesDict[item.Code!] = item!.Label!;
        }
        foreach (ISelectionItem item in MappingDTO.DistributorTypeSelectionItems)
        {
            if (!allTypesDict.ContainsKey(item!.Code!))
                allTypesDict[item.Code!] = item!.Label!;
        }
        foreach (ISelectionItem item in MappingDTO.UserTypeSelectionItems)
        {
            if (!allTypesDict.ContainsKey(item!.Code!))
                allTypesDict[item.Code!] = item!.Label!;
        }


        return allTypesDict;
    }
    #endregion
}

