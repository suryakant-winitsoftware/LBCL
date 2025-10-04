using Microsoft.AspNetCore.Components;
using Winit.Modules.SKUClass.BL.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.LocationHierarchy;

partial class LocationHierarchyBasePage
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsAddLocation { get; set; }
    public Winit.Modules.Location.Model.Interfaces.ILocationItemView Context { get; set; } = new Winit.Modules.Location.Model.Classes.LocationItemView();
    public List<ISelectionItem> LocationTypeSelectionItems { get; set; }
    public List<ISelectionItem> SupplierSelectionItems { get; set; }
    public List<ISelectionItem> FilterLocationTypeSelectionItems { get; set; }
    public Winit.Modules.Common.BL.SelectionManager selectionManagerLocationgroupTypeSM = null;
    public bool IsLocationTypeDDOpen { get; set; }
    private Winit.UIComponents.Web.Filter.Filter? FilterRef;
    public List<FilterModel> FilterColumns = new();
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Location Hierarchy",
        BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="Location Hierarchy"},
         }
    };

    protected async override Task OnInitializedAsync()
    {
        try
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            PrepareFilter();
            await _LocationViewModel.PopulateViewModel();
            FilterLocationTypeSelectionItems = await _LocationViewModel.GetLocationTypeSelectionItems(0, false, null, true);
            selectionManagerLocationgroupTypeSM = new Winit.Modules.Common.BL.SelectionManager
            (FilterLocationTypeSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
            await SetHeaderName();
            StateHasChanged();
            HideLoader();
        }
        catch
        {
            HideLoader();
            throw;
        }
    }
    public async Task SetHeaderName()
    {
        _IDataService.BreadcrumList = new();
        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["location_hierarchy"], IsClickable = false });
        _IDataService.HeaderText = @Localizer["location_hierarchy"];
        await CallbackService.InvokeAsync(_IDataService);
    }
    public void AddLocationPOPupClosed()
    {
        IsAddLocation = false;
    }
    public async Task GetChildGrid(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        await _LocationViewModel.GetChildGrid(locationItemView);
        StateHasChanged();
    }
    public async Task<bool> CreateLocation(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        bool status = await _LocationViewModel.CreateLocation(locationItemView);
        if (status && locationItemView.LocationTypeName == _appSetting.LocationLevel)
        {
            await _LocationViewModel.CreateLocationHierarchy(locationItemView);
        }
        base.StateHasChanged();
        return status;
    }
    public async Task<bool> DeleteLocation(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        return await _LocationViewModel.DeleteLocation(locationItemView);
    }

    public async Task<Winit.Modules.Location.Model.Interfaces.ILocationItemView> getClonedILocationItemView(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        return await _LocationViewModel.CreateClone(locationItemView);
    }
    public async Task<bool> UpdateLocation(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        if (locationItemView.LocationTypeName == _appSetting.LocationLevel)
        {
            await _LocationViewModel.CreateLocationHierarchy(locationItemView);
        }
        return await _LocationViewModel.UpdateLocation(locationItemView);
    }

    public async Task LocationTypeDDClicked(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView)
    {
        var sKuGroupTypeSelectionItems = await _LocationViewModel.GetLocationTypeSelectionItems(locationItemView.ItemLevel, IsAddLocation, locationItemView.ParentLocationTypeUID);
        if (sKuGroupTypeSelectionItems != null)
        {
            Context = locationItemView;
            if (locationItemView.LocationTypeUID != null) sKuGroupTypeSelectionItems.Find(e => e.UID == locationItemView.LocationTypeUID).IsSelected = true;
            LocationTypeSelectionItems = sKuGroupTypeSelectionItems;
            IsLocationTypeDDOpen = true;
        }
    }
    public async Task LocationTypeSelected(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        IsLocationTypeDDOpen = false;
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedSKugroupType = dropDownEvent.SelectionItems.First();
            Context.LocationTypeUID = selectedSKugroupType.UID;
            Context.LocationTypeName = selectedSKugroupType.Label;
            if (IsAddLocation && selectedSKugroupType != null)
            {
                await _LocationViewModel.LocationTypeSelectedForParent(Context, selectedSKugroupType.UID);
            }
            StateHasChanged();
        }
    }
    public async Task CreateClickedFromHome()
    {
        bool IsNew = !_LocationViewModel.LocationItemViews.Exists(e => e.Code == Context.Code);

        if (IsNew)
        {
            bool status = await CreateLocation(Context);
            if (status)
            {
                _LocationViewModel.LocationItemViews.Add(Context);
                _tost.Add(@Localizer["location_hierarchy"], @Localizer["location_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                IsAddLocation = false;
            }
            else
            {
                _tost.Add(@Localizer["location_hierarchy"], @Localizer["location_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
        }
        else
        {
            _tost.Add(@Localizer["location_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }
    private void PrepareFilter()
    {
        FilterColumns.AddRange(new List<FilterModel>
            {
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["code/name"],
                    ColumnName = "locationhierarchy_level_code_name"},
            });
    }
    private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
    {
        await _LocationViewModel.ApplyFilter(keyValuePairs);
    }

}

