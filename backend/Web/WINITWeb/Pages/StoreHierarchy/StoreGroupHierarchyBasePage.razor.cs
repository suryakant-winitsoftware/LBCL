using WinIt.Pages.Base;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Microsoft.AspNetCore.Components;
using Winit.UIModels.Common.Filter;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
namespace WinIt.Pages.StoreHierarchy;

partial class StoreGroupHierarchyBasePage : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsAddStoreGroup { get; set; }
    public Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView Context { get; set; } = new Winit.Modules.Store.Model.Classes.StoreGroupItemView();
    public List<ISelectionItem> StoreGroupTypeSelectionItems { get; set; }
    public List<ISelectionItem> SupplierSelectionItems { get; set; }
    public List<ISelectionItem> FilterStoreGroupTypeSelectionItems { get; set; }
    public Winit.Modules.Common.BL.SelectionManager? selectionManagerStoreGroupgroupTypeSM = null;
    private bool IsStoreGroupTypeDDOpen;
    public List<FilterModel> FilterColumns = new();
    public IDataService dataService = new DataServiceModel
    {
        BreadcrumList =
        {
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Store Group Hierarchy", IsClickable = false }
        },
        HeaderText = "Store Group Hierarchy"
    };
    protected async override Task OnInitializedAsync()
    {
        try
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            await _StoreGroupViewModel.PopulateViewModel();
            FilterStoreGroupTypeSelectionItems = _StoreGroupViewModel.GetStoreGroupTypeSelectionItems(0, false, null, true);
            selectionManagerStoreGroupgroupTypeSM = new Winit.Modules.Common.BL.SelectionManager
            (FilterStoreGroupTypeSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
            PrepareFilter();
            HideLoader();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            HideLoader();
        }
    }
    
    public void AddStoreGroupPOPupClosed()
    {
        IsAddStoreGroup = false;
    }
    public async Task GetChildGrid(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        await _StoreGroupViewModel.GetChildGrid(storeGroupItemView);
        StateHasChanged();
    }
    public async Task<bool> CreateStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        bool status = await _StoreGroupViewModel.CreateStoreGroup(storeGroupItemView);
        base.StateHasChanged();
        return status;
    }
    public async Task<bool> DeleteStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        return await _StoreGroupViewModel.DeleteStoreGroup(storeGroupItemView);
    }

    public async Task<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView> getClonedIStoreGroupItemView(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        return await _StoreGroupViewModel.CreateClone(storeGroupItemView);
    }
    public async Task<bool> UpdateStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        return await _StoreGroupViewModel.UpdateStoreGroup(storeGroupItemView);
    }

    public void StoreGroupTypeDDClicked(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        var sKuGroupTypeSelectionItems = _StoreGroupViewModel.GetStoreGroupTypeSelectionItems(storeGroupItemView.ItemLevel, IsAddStoreGroup, storeGroupItemView.ParentStoreGroupTypeUID);
        if (sKuGroupTypeSelectionItems != null)
        {
            Context = storeGroupItemView;
            if (storeGroupItemView.StoreGroupTypeUID != null) sKuGroupTypeSelectionItems.Where(e => e.UID == storeGroupItemView.StoreGroupTypeUID).Select(i => i.IsSelected = true);
            StoreGroupTypeSelectionItems = sKuGroupTypeSelectionItems;
            IsStoreGroupTypeDDOpen = true;
        }
    }
    public async Task StoreGroupTypeSelected(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        IsStoreGroupTypeDDOpen = false;
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedSKugroupType = dropDownEvent.SelectionItems.First();
            Context.StoreGroupTypeUID = selectedSKugroupType.UID;
            Context.StoreGroupTypeName = selectedSKugroupType.Label;
            if (IsAddStoreGroup)
            {
                await _StoreGroupViewModel.StoreGroupTypeSelectedForParent(Context, selectedSKugroupType.UID);
            }
            StateHasChanged();
        }
    }
    public async void CreateClickedFromHome()
    {
        bool IsNew = !_StoreGroupViewModel.StoreGroupItemViews.Any(e => e.Code == Context.Code);

        if (IsNew)
        {
            bool status = await CreateStoreGroup(Context);
            if (status)
            {
                _StoreGroupViewModel.StoreGroupItemViews.Add(Context);
                _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["store_group_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                IsAddStoreGroup = false;
            }
            else
            {
                _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["store_group_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
        }
        else
        {
            _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }

    private void PrepareFilter()
    {
        FilterColumns.AddRange(new List<FilterModel>
            {
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["code/name"],
                    ColumnName = @Localizer["store_group_hierarchy_level_code_name"]},
            });
    }
    private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
    {
        await _StoreGroupViewModel.ApplyFilter(keyValuePairs);
    }
}

