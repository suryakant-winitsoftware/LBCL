
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;

using WinIt.Pages.Base;

namespace WinIt.Pages.SKU_Group;

partial class SKUGroupBasePage : BaseComponentBase
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    public bool IsAddSKUGroup { get; set; }
    public Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView Context { get; set; } = new Winit.Modules.SKU.Model.UIClasses.SKUGroupItemView();
    public List<ISelectionItem> SKUGroupTypeSelectionItems = new List<ISelectionItem>();
    public List<ISelectionItem> SupplierSelectionItems = new List<ISelectionItem>();
    public List<ISelectionItem> FilterSKUGroupTypeSelectionItems = new List<ISelectionItem>();
    public Winit.Modules.Common.BL.SelectionManager? selectionManagerSKUgroupTypeSM = null;
    public bool IsSKUGroupTypeDDOpen { get; set; }
    public bool IsSupplierPopUpOpen = false;
    private Winit.UIComponents.Web.Filter.Filter? FilterRef;
    public List<FilterModel> FilterColumns = new();
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "SKU Hierarcy Level",
        BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="SKU Hierarcy Level"},
         }
    };

    protected async override Task OnInitializedAsync()
    {
        ShowLoader();
        try
        {
            LoadResources(null, _languageService.SelectedCulture);
            await _SKUGroupViewModel.PopulateViewModel();
            FilterSKUGroupTypeSelectionItems = await _SKUGroupViewModel.GetSKuGroupTypeSelectionItems(0, false, null, true);
            selectionManagerSKUgroupTypeSM = new Winit.Modules.Common.BL.SelectionManager
            (FilterSKUGroupTypeSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
            PrepareFilter();
            StateHasChanged();
            HideLoader();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            HideLoader();
        }
    }
  
   
    public void AddSKUGroupPOPupClosed()
    {
        IsAddSKUGroup = false;
    }
    public async Task GetChildGrid(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        await _SKUGroupViewModel.getChildGrid(sKUGroupItemView);
        StateHasChanged();
    }
    public async Task<bool> CreateSKUGroup(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        bool status = await _SKUGroupViewModel.CreateSKUGroup(sKUGroupItemView);
        base.StateHasChanged();
        return status;
    }
    public async Task<bool> DeleteSKUGroup(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        return await _SKUGroupViewModel.DeleteSKUGroup(sKUGroupItemView);
    }

    public async Task<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView> getClonedISKUGroupItemView(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        return await _SKUGroupViewModel.CreateClone(sKUGroupItemView);
    }
    public async Task<bool> UpdateSKUGroup(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        return await _SKUGroupViewModel.UpdateSKUGroup(sKUGroupItemView);
    }

    public async Task SKUGroupTypeDDClicked(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        var sKuGroupTypeSelectionItems = await _SKUGroupViewModel.GetSKuGroupTypeSelectionItems(sKUGroupItemView.ItemLevel, IsAddSKUGroup, sKUGroupItemView.ParentSKUGroupTypeUID);
        if (sKuGroupTypeSelectionItems != null)
        {
            Context = sKUGroupItemView;
            if (sKUGroupItemView.SKUGroupTypeUID != null) sKuGroupTypeSelectionItems.Where(e => e.UID == sKUGroupItemView.SKUGroupTypeUID).Select(i => i.IsSelected = true);
            SKUGroupTypeSelectionItems.Clear();
            SKUGroupTypeSelectionItems.AddRange(sKuGroupTypeSelectionItems);
            IsSKUGroupTypeDDOpen = true;
        }
    }
    private void PrepareFilter()
    {
        FilterColumns.AddRange(new List<FilterModel>
            {
                new() {FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label =@Localizer["code/name"],
                    ColumnName = "product_hierarchylevel_code_name"},
            });
    }
    public void SupplierDDClicked()
    {
        SupplierSelectionItems = _SKUGroupViewModel.SupplierSelectionItems;
        IsSupplierPopUpOpen = true;
    }
    public async Task SKUGroupTypeSelected(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        IsSKUGroupTypeDDOpen = false;
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedSKugroupType = dropDownEvent.SelectionItems.First();
            Context.SKUGroupTypeUID = selectedSKugroupType.UID;
            Context.SKUGroupTypeName = selectedSKugroupType.Label;
            if (IsAddSKUGroup)
            {
                await _SKUGroupViewModel.SKUGroupTypeSelectedForParent(Context, selectedSKugroupType.UID);
            }
            StateHasChanged();
        }
    }
    public void SupplierSelected(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        IsSupplierPopUpOpen = false;
        if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            var selectedSupplier = dropDownEvent.SelectionItems.First();
            Context.SupplierOrgUID = selectedSupplier.UID;
            Context.SupplierName = selectedSupplier.Label;
            StateHasChanged();
        }
        new Winit.Modules.Common.BL.SelectionManager(SupplierSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single).DeselectAll();
    }

    public async void CreateClickedFromHome()
    {
        bool IsNew = !_SKUGroupViewModel.SKUGroupItemViews.Any(e => e.Code == Context.Code);

        if (IsNew)
        {
            bool status = await CreateSKUGroup(Context);
            if (status)
            {
                _SKUGroupViewModel.SKUGroupItemViews.Add(Context);
                _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["sku_group_added_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                IsAddSKUGroup = false;
            }
            else
            {
                _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["sku_group_adding_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
        }
        else
        {
            _tost.Add(@Localizer["skugroup_hierarchy"], @Localizer["code_should_be_unique"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
        }
        StateHasChanged();
    }

    private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
    {
        await _SKUGroupViewModel.ApplyFilter(keyValuePairs);
    }
}

