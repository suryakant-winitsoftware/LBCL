using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using System.Resources;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.Vehicle.BL.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.Modules.Common.UIState.Classes;

namespace WinIt.Pages.Maintain_Warehouse;

public partial class MaintainWarehouse
{
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    private bool showFilter = false;
    public string WareHouseCode { get; set; }
    public string WareHouseName { get; set; }
    public bool IsLoaded { get; set; }
    public List<DataGridColumn> DataGridColumns { get; set; }
    public IWarehouseItemView? SelectedWareHouse { get; set; }
    private bool IsDeleteBtnPopUp { get; set; }
    public List<FilterCriteria> WarehouseFilterCriterias { get; set; }
    private bool showFilterComponent = false;
    private Winit.UIComponents.Web.Filter.Filter filterRef;
    public List<FilterModel> ColumnsForFilter;
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Maintain Warehouse",
        BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain Warehouse"},
            }
    };
    protected override async Task OnInitializedAsync()
    {
        LoadResources(null, _languageService.SelectedCulture);
        FilterInitialized();
        _warehouseViewModel.ParentUID = _iAppUser.SelectedJobPosition.OrgUID;
        _warehouseViewModel.PageSize = 20;
        WarehouseFilterCriterias = new List<FilterCriteria>();
        //   _warehouseViewModel.ParentUID = "FR001";
        await _warehouseViewModel.PopulateViewModel();
        await _warehouseViewModel.GetDistributor();
        await GenerateGridColumns();
        IsLoaded = true;
        // await SetHeaderName();
        await StateChageHandler();
    }
    private async Task StateChageHandler()
    {
        _navigationManager.LocationChanged += (sender, args) => SavePageState();
        bool stateRestored = _pageStateHandler.RestoreState("MaintainWarehouse", ref ColumnsForFilter, out PageState pageState);

        ///only work with filters
        await OnFilterApply(_pageStateHandler._currentFilters);

    }
    private void SavePageState()
    {
        _navigationManager.LocationChanged -= (sender, args) => SavePageState();
        _pageStateHandler.SaveCurrentState("MaintainWarehouse");
    }
    public async void ShowFilter()
    {
        showFilterComponent = !showFilterComponent;
        filterRef.ToggleFilter();
    }

    public void FilterInitialized()
    {

        ColumnsForFilter = new List<FilterModel>
        {
            //new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["distributor"], DropDownValues=_warehouseViewModel.DistributorSelectionList,ColumnName = "FranchiseName"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["code/name"],ColumnName = "WarehouseName"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["warehouse_type"],DropDownValues=_warehouseViewModel.WareHouseTypeSelectionItems, ColumnName = "orgtypeuid",SelectionMode=SelectionMode.Multiple},
        };
        if (_iAppUser.SelectedJobPosition.OrgUID == "WINIT")
        {
            ColumnsForFilter.Add(new FilterModel
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                Label = @Localizer["distributor"],
                DropDownValues = _warehouseViewModel.DistributorSelectionList,
                ColumnName = "FranchiseName"
            });
        }
    }
    private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
    {
        _pageStateHandler._currentFilters = filterCriteria;
        List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
        foreach (var keyValue in filterCriteria)
        {
            if (!string.IsNullOrEmpty(keyValue.Value))
            {
                if (keyValue.Key == "FranchiseName")
                {
                    ISelectionItem? selectionItem = _warehouseViewModel.DistributorSelectionList.Find(e => e.UID == keyValue.Value);
                    filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Label, FilterType.Equal));
                }
                else if (keyValue.Key == "orgtypeuid")
                {
                    if (keyValue.Value.Contains(","))
                    {
                        List<string> selectedUids = keyValue.Value.Split(",").ToList();
                        List<string> seletedLabels = _warehouseViewModel.WareHouseTypeSelectionItems.Where(e => selectedUids.Contains(e.UID)).Select(_ => _.Label).ToList();
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", seletedLabels, FilterType.In));
                    }
                    else
                    {
                        ISelectionItem? selectionItem = _warehouseViewModel.WareHouseTypeSelectionItems.Find(e => e.UID == keyValue.Value);
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Label, FilterType.Equal));
                    }
                }
                else
                {
                    filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));

                }
            }
        }
        await _warehouseViewModel.ApplyFilter(filterCriterias);
        StateHasChanged();
    }
    //public async Task SetHeaderName()
    //{
    //    _IDataService.BreadcrumList = new();
    //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_warehouse"], IsClickable = false });
    //    _IDataService.HeaderText = @Localizer["maintain_warehouse"];
    //    await CallbackService.InvokeAsync(_IDataService);
    //}
    private async Task GenerateGridColumns()
    {
        DataGridColumns = new List<DataGridColumn>
        {

            new DataGridColumn { Header = @Localizer["type"], GetValue = s => ((IWarehouseItemView)s)?.OrgTypeUID ?? "N/A",IsSortable = true, SortField = "WarehouseUID" },
            new DataGridColumn { Header = @Localizer["code"], GetValue = s => ((IWarehouseItemView)s)?.WarehouseCode?? "N/A" ,IsSortable = true, SortField = "WarehouseCode"},
            new DataGridColumn {Header = @Localizer["name"], GetValue = s =>((IWarehouseItemView) s) ?.WarehouseName ?? "N/A", IsSortable = true, SortField = "WarehouseName"},
            new DataGridColumn {Header =@Localizer["distributor"], GetValue = s =>((IWarehouseItemView) s) ?.FranchiseName ?? "N/A", IsSortable = true, SortField = "FranchiseName"},

         new DataGridColumn
         {
            Header =@Localizer["actions"],
            IsButtonColumn = true,
            ButtonActions = new List<ButtonAction>
            {
                new ButtonAction
                {
                    ButtonType = ButtonTypes.Image,
                    URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                    Action = item => OnEditClick((IWarehouseItemView)item)
                },
                  new ButtonAction
                  {
                      ButtonType = ButtonTypes.Image,
                      URL = "https://qa-fonterra.winitsoftware.com/assets/Images/view.png",
                    Action = s => OnViewClick((IWarehouseItemView)s)
                  },
                   new ButtonAction
                  {
                    ButtonType = ButtonTypes.Image,
                    URL = "https://qa-fonterra.winitsoftware.com/assets/Images/delete.png",
                    Action = s => OnDeleteClick((IWarehouseItemView)s)
                  }
            }
        }
         };
    }
    private async Task AddNewWareHouse()
    {
        //_navigationManager.NavigateTo($"AddEditWareHouse");
        try
        {
            ShowLoader();
            _navigationManager.NavigateTo("AddEditWareHouse");
        }
        catch (Exception ex)
        {
            await _AlertMessgae.ShowErrorAlert("Error", "Failed to navigate to Add Warehouse page");
        }
        finally
        {
            HideLoader();
        }
    }
    public void OnEditClick(IWarehouseItemView warehouseItemViewforEdit)
    {
        //_navigationManager.NavigateTo($"AddEditWareHouse?WarehouseUID={warehouseItemViewforEdit.WarehouseUID}&IsEditPage=true");
        try
        {
            ShowLoader();
            _navigationManager.NavigateTo($"AddEditWareHouse?WarehouseUID={warehouseItemViewforEdit.WarehouseUID}&IsEditPage=true");
        }
        catch (Exception ex)
        {
            _AlertMessgae.ShowErrorAlert("Error", "Failed to navigate to Edit Warehouse page");
        }
        finally
        {
            HideLoader();
        }
    }
    public async Task OnDeleteClick(IWarehouseItemView warehouseItemViewforDelete)
    {
        SelectedWareHouse = warehouseItemViewforDelete;
        //IsDeleteBtnPopUp = true;
        if (await _AlertMessgae.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_delete_this_item_?"], @Localizer["yes"], @Localizer["no"]))
        {
            try
            {
                string msg = await _warehouseViewModel.DeleteMaintainWareHouse(SelectedWareHouse?.WarehouseUID);
                if (msg.Contains("Failed"))
                {
                    await _AlertMessgae.ShowErrorAlert(@Localizer["failed"], msg);
                }
                else
                {
                    await _AlertMessgae.ShowSuccessAlert(@Localizer["success"], msg);
                    await _warehouseViewModel.PopulateViewModel();
                }

            }
            catch (Exception ex)
            {

                await _AlertMessgae.ShowSuccessAlert(@Localizer["deleted"], ex.Message);
            }
        }
        StateHasChanged();
    }
    private void OnViewClick(IWarehouseItemView warehouseItemView)
    {
        //_navigationManager.NavigateTo($"AddEditWareHouse?WarehouseUID={warehouseItemView.WarehouseUID}&IsViewPage=true");
        try
        {
            ShowLoader();
            _navigationManager.NavigateTo($"AddEditWareHouse?WarehouseUID={warehouseItemView.WarehouseUID}&IsViewPage=true");
        }
        catch (Exception ex)
        {
            _AlertMessgae.ShowErrorAlert("Error", "Failed to navigate to View Warehouse page");
        }
        finally
        {
            HideLoader();
        }
    }
    private async Task OnSortApply(SortCriteria sortCriteria)
    {
        ShowLoader();
        await _warehouseViewModel.ApplySort(sortCriteria);
        StateHasChanged();
        HideLoader();
    }
}
