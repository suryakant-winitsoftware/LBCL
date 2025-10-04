using Microsoft.AspNetCore.Components;

namespace WinIt.Pages.StoreHierarchy;

partial class StoreGroupHierarchy
{
    [Parameter]
    public List<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView> HierarchyGrids { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView> OnBtnExpandClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView, Task<bool>> OnCreateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView, Task<bool>> OnDeleteStoreGroup { get; set; }
    [Parameter]
    public Func<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView, Task<bool>> OnUpdateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView, Task<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView>> OnCreateClone { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView> OnStoreGroupTypebtnClicked { get; set; }
    public string title { get; set; }
    public Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView SelectedElement { get; set; }
    public Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView Context { get; set; }

    public async Task ToggleCollapse(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView hierarchy)
    {
        if (!hierarchy.IsOpen && (hierarchy.ChildGrids == null || !hierarchy.ChildGrids.Any()))
        {
            ShowLoader();
            await InvokeAsync(() => OnBtnExpandClick.InvokeAsync(hierarchy));
        }
        if (hierarchy.ChildGrids != null && hierarchy.ChildGrids.Count > 0)
        {
            hierarchy.IsOpen = !hierarchy.IsOpen;
        }
        StateHasChanged();
        HideLoader();
    }

    public async Task CreateSkuGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        bool IsNew = await ValidateCode(storeGroupItemView);

        if (IsNew)
        {
            bool status = await OnCreateClick.Invoke(Context);
            if (status)
            {
                _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["store_group_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["store_group_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
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
            _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }

    public async Task UpdateStoreGroupClick()
    {
        bool IsUpdated = await OnUpdateClick.Invoke(Context);
        if (IsUpdated)
        {
            var ogGrid = HierarchyGrids.Find(i => i.UID == Context.UID);
            if (ogGrid != null)
            {
                //HierarchyGrids.Remove(ogGrid);
                //HierarchyGrids.Add(Context);
                ogGrid.StoreGroupTypeUID = Context.StoreGroupTypeUID;
                ogGrid.StoreGroupTypeName = Context.StoreGroupTypeName;
                ogGrid.Name = Context.Name;
                ogGrid.IsUpdatePopUpOpen = false;
                _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["store_group_updated_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            Context.IsUpdatePopUpOpen = false;
        }
        else
        {
            _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["store_group_update_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }
    public async Task DeleteClicked(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        bool status = await OnDeleteStoreGroup.Invoke(storeGroupItemView);
        var deletedgrid = HierarchyGrids.Find(i => i.UID.Equals(storeGroupItemView.UID, StringComparison.OrdinalIgnoreCase));
        if (deletedgrid != null)
        {
            HierarchyGrids.Remove(deletedgrid);
        }
        if (status)
        {
            _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["store_group_deleted_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _tost.Add(@Localizer["storegroup_hierarchy"], @Localizer["store_group_deleting_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
    }

    public async Task UpdatePopIconClicked(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        Context = await OnCreateClone.Invoke(storeGroupItemView);
        title = @Localizer["edit_storegroup_hierarchy"]; storeGroupItemView.IsUpdatePopUpOpen = true;
    }
    public void CreatePopIconClicked(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        title = @Localizer["add_storegroup_hierarchy"];
        storeGroupItemView.IsCreatePopUpOpen = true;
        SelectedElement = storeGroupItemView;
        Context = new Winit.Modules.Store.Model.Classes.StoreGroupItemView();
        Context.ParentUID = storeGroupItemView.UID;
        Context.ParentName = storeGroupItemView.Name;
        Context.ItemLevel = storeGroupItemView.ItemLevel + 1;
        Context.ParentStoreGroupTypeUID = storeGroupItemView.StoreGroupTypeUID;
    }
    public async Task DeletePopIconClicked(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        title = @Localizer["delete_store_group_hierarchy"];
        storeGroupItemView.IsDeletePopUpOpen = true;
        SelectedElement = storeGroupItemView;
    }

    public async Task StoreGroupTypeDDClicked()
    {
        await OnStoreGroupTypebtnClicked.InvokeAsync(Context);
    }
    public async Task UpdateStoreGroupTypeDDClicked()
    {
        //Context.Level
        await OnStoreGroupTypebtnClicked.InvokeAsync(Context);
    }
    private Task<bool> ValidateCode(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        if (storeGroupItemView != null && storeGroupItemView.ChildGrids != null && storeGroupItemView.ChildGrids.Any())
        {
            var item = storeGroupItemView.ChildGrids.Find(e => e.Code == Context.Code);
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
