using Microsoft.AspNetCore.Components;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SKU.Model.UIInterfaces;

namespace WinIt.Pages.SKUGroupTypeHierarchy;

partial class SKUGroupTypeHierarchy
{
    [Parameter]
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView> HierarchyGrids { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView> OnBtnExpandClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView,Task<bool>> OnCreateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView,Task<bool>> OnDeleteSKUGroupType { get; set; }
    [Parameter]
    public Func<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView, Task<bool>> OnUpdateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView, Task<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView>> OnCreateClone { get; set; }
    
    public string title { get; set; }
    public Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView SelectedElement { get; set; }
    public Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView Context { get; set; }

    public async Task ToggleCollapse(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView hierarchy)
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

    public async void CreateSkuGroup(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        bool IsNew = await ValidateCode(sKUGroupTypeItemView);

        if (IsNew)
        {
            bool status = await OnCreateClick.Invoke(Context);
            if (status)
            {
                _tost.Add(@Localizer["sku_grouptype_hierarchy"], @Localizer["sku_group_type_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _tost.Add(@Localizer["sku_grouptype_hierarchy"], @Localizer["sku_group_type_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
            var parentgrid = HierarchyGrids.Find(i => i.UID.Equals(Context.ParentUID));
            if (parentgrid != null)
            {
                if (parentgrid.ChildGrids != null) parentgrid.ChildGrids.Add(Context);
                parentgrid.IsCreatePopUpOpen = false;
            }
        }
        else
        {
            _tost.Add(@Localizer["sku_grouptype_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }

    public async Task DeleteClicked(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        bool status = await OnDeleteSKUGroupType.Invoke(sKUGroupTypeItemView);
        if (status)
        {
            var deletedgrid = HierarchyGrids.Find(i => i.UID.Equals(sKUGroupTypeItemView.UID, StringComparison.OrdinalIgnoreCase));
            if (deletedgrid != null)
            {
                HierarchyGrids.Remove(deletedgrid);
            }
            _tost.Add(@Localizer["sku_grouptype_hierarchy"], @Localizer["sku_group_type_deleted_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _tost.Add(@Localizer["sku_grouptype_hierarchy"], @Localizer["sku_grouptype_deleting_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
    }

    public async Task UpdatePopIconClicked(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        Context = await OnCreateClone.Invoke(sKUGroupTypeItemView);
        title = @Localizer["edit_sku_group_type_hierarchy"]; sKUGroupTypeItemView.IsUpdatePopUpOpen = true;
    }
    public async Task CreatePopIconClicked(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        title = @Localizer["add_sku_group_type_hierarchy"];
        sKUGroupTypeItemView.IsCreatePopUpOpen = true;
        SelectedElement = sKUGroupTypeItemView;
        Context = new Winit.Modules.SKU.Model.Classes.SKUGroupTypeItemView();
        Context.ParentUID = sKUGroupTypeItemView.UID;
        Context.ParentName = sKUGroupTypeItemView.Name;
        Context.ItemLevel = sKUGroupTypeItemView.ItemLevel + 1;
    }
    public async Task DeletePopIconClicked(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView)
    {
        title = @Localizer["delete_sku_group_type_hierarchy"];
        sKUGroupTypeItemView.IsDeletePopUpOpen = true;
        SelectedElement = sKUGroupTypeItemView;
        await Task.CompletedTask;
    }

    public async Task UpdateSKUGroupTypeClick()
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
        }
        Context.IsUpdatePopUpOpen = false;
        StateHasChanged();
    }
    private Task<bool> ValidateCode(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView iSKUGroupTypeItemView)
    {
        if (iSKUGroupTypeItemView != null && iSKUGroupTypeItemView.ChildGrids != null && iSKUGroupTypeItemView.ChildGrids.Any())
        {
            var item = iSKUGroupTypeItemView.ChildGrids.Find(e => e.Code == Context.Code);
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
