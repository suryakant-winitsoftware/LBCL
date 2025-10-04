using Microsoft.AspNetCore.Components;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.SKU_Group;

partial class SKUGroupGrid
{
    [Parameter]
    public List<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView> HierarchyGrids { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView> OnBtnExpandClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView, Task<bool>> OnCreateClick { get; set; }
    [Parameter]
    public Func<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView, Task<bool>> OnDeleteSKUGroup { get; set; }
    [Parameter]
    public Func<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView, Task<bool>> OnUpdateClick { get; set; }
    [Parameter]
    public List<ISelectionItem> SupplierSelectionItems { get; set; }
    [Parameter]
    public Func<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView, Task<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView>> OnCreateClone { get; set; }
    [Parameter]
    public EventCallback<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView> OnSKUGroupTypebtnClicked { get; set; }
    public string? Title { get; set; }
    public Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView? SelectedElement { get; set; }
    public Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView Context;

    public async Task ToggleCollapse(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView hierarchy)
    {
        try
        {
            await InvokeAsync(async () =>
            {
                if (!hierarchy.IsOpen && (hierarchy.ChildGrids == null || hierarchy.ChildGrids.Count == 0))
                {
                    ShowLoader();
                    await OnBtnExpandClick.InvokeAsync(hierarchy);
                }
                hierarchy.IsOpen = !hierarchy.IsOpen;
                //StateHasChanged();
            });
        }
        catch (Exception)
        {
            throw;
        }
        finally { HideLoader(); }
    }

    public async void CreateSkuGroup(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        bool IsNew = await ValidateCode(sKUGroupItemView);

        if (IsNew)
        {
            bool status = await OnCreateClick.Invoke(Context);
            if (status)
            {
                _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["sku_group_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["sku_group_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
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
            _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }

    public async Task DeleteClicked(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        bool status = await OnDeleteSKUGroup.Invoke(sKUGroupItemView);
        var deletedgrid = HierarchyGrids.Find(i => i.UID.Equals(sKUGroupItemView.UID, StringComparison.OrdinalIgnoreCase));
        if (deletedgrid != null)
        {
            HierarchyGrids.Remove(deletedgrid);
        }
        if (status)
        {
            _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["sku_group_deleted_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        else
        {
            _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["sku_group_deleting_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
    }

    public async Task UpdatePopIconClicked(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        Context = await OnCreateClone.Invoke(sKUGroupItemView);
        Title = @Localizer["edit_sku_group_hierarchy"]; sKUGroupItemView.IsUpdatePopUpOpen = true;
    }
    public void CreatePopIconClicked(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        Title = @Localizer["add_sku_group_hierarchy"];
        sKUGroupItemView.IsCreatePopUpOpen = true;
        SelectedElement = sKUGroupItemView;
        Context = new Winit.Modules.SKU.Model.UIClasses.SKUGroupItemView();
        Context.ParentUID = sKUGroupItemView.UID;
        Context.ParentName = sKUGroupItemView.Name;
        Context.ItemLevel = sKUGroupItemView.ItemLevel + 1;
        Context.SupplierOrgUID = null;//sKUGroupItemView.SupplierOrgUID;
        Context.SupplierName = sKUGroupItemView.SupplierName;
        Context.ParentSKUGroupTypeUID = sKUGroupItemView.SKUGroupTypeUID;
    }
    public void DeletePopIconClicked(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        Title = @Localizer["delete_sku_group_hierarchy"];
        sKUGroupItemView.IsDeletePopUpOpen = true;
        SelectedElement = sKUGroupItemView;
    }

    public async Task UpdateSKUGroupClick()
    {
        bool IsUpdated = await OnUpdateClick.Invoke(Context);
        if (IsUpdated)
        {
            var ogGrid = HierarchyGrids.Find(i => i.UID == Context.UID);
            if (ogGrid != null)
            {
                //HierarchyGrids.Remove(ogGrid);
                //HierarchyGrids.Add(Context);
                ogGrid.SKUGroupTypeUID = Context.SKUGroupTypeUID;
                ogGrid.SKUGroupTypeName = Context.SKUGroupTypeName;
                ogGrid.Name = Context.Name;
                ogGrid.SupplierOrgUID = Context.SupplierOrgUID;
                ogGrid.SupplierName = Context.SupplierName;
                ogGrid.IsUpdatePopUpOpen = false;
            }
            _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["sku_group_updated_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
            Context.IsUpdatePopUpOpen = false;
        }
        else
        {
            _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["sku_group_update_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }

    public async Task SKUGroupTypeDDClicked()
    {
        await OnSKUGroupTypebtnClicked.InvokeAsync(Context);
    }
    public async Task UpdateSKUGroupTypeDDClicked()
    {
        //Context.Level
        await OnSKUGroupTypebtnClicked.InvokeAsync(Context);
    }
    private Task<bool> ValidateCode(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        if (sKUGroupItemView != null && sKUGroupItemView.ChildGrids != null && sKUGroupItemView.ChildGrids.Any())
        {
            var item = sKUGroupItemView.ChildGrids.Find(e => e.Code == Context.Code);
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
