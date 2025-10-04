using Microsoft.AspNetCore.Components;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SKU.Model.UIInterfaces;

namespace WinIt.Pages.StoreGroupTypeHierarchy;

partial class StoreGroupTypeHierarchy
{
    [Parameter]
    public List<Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView> HierarchyGrids { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView> OnBtnExpandClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView, Task<bool>> OnCreateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView,Task<bool>> OnDeleteStoreGroupType { get; set; }
    [Parameter]
    public Func<Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView, Task<bool>> OnUpdateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView, Task<Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView>> OnCreateClone { get; set; }

    public string title { get; set; }
    public Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView SelectedElement { get; set; }
    public Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView Context { get; set; }

    public async Task ToggleCollapse(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView hierarchy)
    {
        if (!hierarchy.IsOpen)
        {
            await OnBtnExpandClick.InvokeAsync(hierarchy);
        }
        if (hierarchy.ChildGrids != null && hierarchy.ChildGrids.Count > 0)
        {
            hierarchy.IsOpen = !hierarchy.IsOpen;
            StateHasChanged();
        }
    }

    public async void CreateSkuGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        bool IsNew = await ValidateCode(storeGroupTypeItemView);
        if (IsNew)
        {
            bool status = await OnCreateClick.Invoke(Context);
            if (status)
            {
                _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["store_group_type_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["store_group_type_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
            var parentgrid = HierarchyGrids.Find(i => i.UID == Context.ParentUID);
            if (parentgrid != null)
            {
                if (parentgrid.ChildGrids != null) parentgrid.ChildGrids.Add(Context);
                parentgrid.IsCreatePopUpOpen = false;
            }
        }
        else
        {
            _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }

    public async Task DeleteClicked(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        bool status = await OnDeleteStoreGroupType.Invoke(storeGroupTypeItemView);
        var deletedgrid = HierarchyGrids.Find(i => i.UID.Equals(storeGroupTypeItemView.UID, StringComparison.OrdinalIgnoreCase));
        if (deletedgrid != null)
        {
            HierarchyGrids.Remove(deletedgrid);
        }
        if (status)
        {
            _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["store_group_type_deleted_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["store_group_type_deleting_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
    }

    public async Task UpdatePopIconClicked(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        Context = await OnCreateClone.Invoke(storeGroupTypeItemView);
        title = @Localizer["edit_store_group_type_hierarchy"]; storeGroupTypeItemView.IsUpdatePopUpOpen = true;
    }
    public async Task CreatePopIconClicked(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        title = @Localizer["add_store_group_type_hierarchy"];
        storeGroupTypeItemView.IsCreatePopUpOpen = true;
        SelectedElement = storeGroupTypeItemView;
        Context = new Winit.Modules.Store.Model.Classes.StoreGroupTypeItemView();
        Context.ParentUID = storeGroupTypeItemView.UID;
        Context.ParentName = storeGroupTypeItemView.Name;
        Context.LevelNo = storeGroupTypeItemView.LevelNo + 1;
    }
    public async Task DeletePopIconClicked(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        title = @Localizer["delete_store_group_type_hierarchy"];
        storeGroupTypeItemView.IsDeletePopUpOpen = true;
        SelectedElement = storeGroupTypeItemView;
    }

    public async Task UpdateStoreGroupTypeClick()
    {
        bool IsUpdated = await OnUpdateClick.Invoke(Context);
        if (IsUpdated)
        {
            var ogGrid = HierarchyGrids.Find(i => i.UID == Context.UID);
            if (ogGrid != null)
            {
                ogGrid.Name = Context.Name;
                ogGrid.IsUpdatePopUpOpen = false;
            }
            Context.IsUpdatePopUpOpen = false;
            _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["store_group_type_updated_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _tost.Add(@Localizer["store_grouptype_hierarchy"], @Localizer["store_group_type_update_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        StateHasChanged();
    }
    private Task<bool> ValidateCode(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView)
    {
        if (storeGroupTypeItemView != null && storeGroupTypeItemView.ChildGrids != null && storeGroupTypeItemView.ChildGrids.Any())
        {
            var item = storeGroupTypeItemView.ChildGrids.Find(e => e.Code == Context.Code);
            if (item != null)
            {
                return Task.FromResult(false);
            }
        }
        return Task.FromResult(true);
    }
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
    }
}

