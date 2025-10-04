using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using Nest;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using SixLabors.Fonts;
using System.Globalization;
using System.Resources;
using Winit.Modules.Common.BL;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
namespace WinIt.Pages.ViewWarehouseStock
{
    public partial class ViewWarehouseStock
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        [Parameter]
        public bool IsInitialized { get; set; }
        private bool showFilter = false;
        public string SKUCode { get; set; }
        public string SKUName { get; set; }
        public decimal TotalOuter = 0;
        public decimal TotalEach = 0;
        public decimal AllocatedOuter = 0;
        public decimal AllocatedEach = 0;
        public decimal AvailableOuter = 0;
        public decimal AvailableEach = 0;
        public List<FilterCriteria> WareHouseStockFilterCriterias { get; set; }
        public List<ISelectionItem> TabSelectionItems = new List<ISelectionItem>();
        private SelectionManager TabSM;
        public List<DataGridColumn> DataGridColumnsNormalStock { get; set; }
        public List<DataGridColumn> DataGridColumnsFocStock { get; set; }
        public List<IWarehouseStockItemView> Datasource { get; set; }
        public IWarehouseStockItemView? SelectedWarehouseStock { get; set; }
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        private string SortColumn { get; set; }
        private SortDirection SortDirection1 { get; set; }
        private string SelectedTabLabel;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Warehouse Stock",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="View Warehouse Stock"},
            }
        };
        public enum SortDirection
        {
            Ascending,
            Descending
        }
        private void SortByColumn(string columnName)
        {
            if (SortColumn == columnName)
                SortDirection1 = SortDirection1 == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
            else
            {
                SortColumn = columnName;
                SortDirection1 = SortDirection.Ascending;
            }
            SortData();
        }
        private void SortData()
        {
            _loadingService.ShowLoading();
            // Implement sorting logic based on SortColumn and SortDirection
            // You can use LINQ to sort your data
            if (_warehousestockViewModel.Warehouse_VanFocstockList != null)
            {
                var sortedList = _warehousestockViewModel.Warehouse_VanFocstockList;

                switch (SortColumn)
                {
                    case "SKUCode":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.SKUCode).ToList() :
                            sortedList.OrderByDescending(item => item.SKUCode).ToList();
                        break;
                    case "SKUName":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.SKUName).ToList() :
                            sortedList.OrderByDescending(item => item.SKUName).ToList();
                        break;
                    case "OuterQty":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.OuterQty).ToList() :
                            sortedList.OrderByDescending(item => item.OuterQty).ToList();
                        break;
                    case "OuterQty2":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.OuterQty2).ToList() :
                            sortedList.OrderByDescending(item => item.OuterQty2).ToList();
                        break;
                    case "OuterQty3":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.OuterQty3).ToList() :
                            sortedList.OrderByDescending(item => item.OuterQty3).ToList();
                        break;
                    case "EAQty":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.EAQty).ToList() :
                            sortedList.OrderByDescending(item => item.EAQty).ToList();
                        break;
                    case "EAQty2":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.EAQty2).ToList() :
                            sortedList.OrderByDescending(item => item.EAQty2).ToList();
                        break;
                    case "EAQty3":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.EAQty3).ToList() :
                            sortedList.OrderByDescending(item => item.EAQty3).ToList();
                        break;
                }
                _loadingService.HideLoading();
                _warehousestockViewModel.Warehouse_VanFocstockList = sortedList;
            }
        }

        protected override async Task OnInitializedAsync()
        {

            try
            {
                _loadingService.ShowLoading();
				LoadResources(null, _languageService.SelectedCulture);
				_warehousestockViewModel.PageSizeSalableWarehouse_Van = 50;
                _warehousestockViewModel.PageSizeFocWarehouse_Van = 50;
                TabSelectionItems = new List<ISelectionItem>
                {
                    new SelectionItem { Label = @Localizer["salable"], Code = "Normal", UID = "1" },
                    new SelectionItem { Label = @Localizer["foc"], Code = "FOC", UID = "2" },
                 };
                FilterInitializedForWarehouse_Van();
                TabSM = new SelectionManager(TabSelectionItems, SelectionMode.Single);
                TabSelectionItems[0].IsSelected = true;
                WareHouseStockFilterCriterias = new List<FilterCriteria>();
                _warehousestockViewModel.OrgTypeUID = "FRWH";
                await _warehousestockViewModel.PopulateViewModelForWareHouseDD();
                _warehousestockViewModel.selectedwarehouseUID = _warehousestockViewModel.WareHouseItemViewListFrmORG.First().UID;
                _warehousestockViewModel.FranchiseeOrgUID = _iAppUser.SelectedJobPosition.OrgUID;
                await _warehousestockViewModel.PopulateViewModelForSalableWareHouse();
                _warehousestockViewModel.WareHouseSelectionItems[0].IsSelected = true;
                StateHasChanged();
                await GenerateGridColumnsForNormalStock();
                IsInitialized = true;
               // await SetHeaderName();
                _loadingService.HideLoading();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in OnInitializedAsync: {ex.Message}");
            }
        }
        public async void ShowFilter()
        {
            showFilterComponent = !showFilterComponent;
            filterRef.ToggleFilter();
        }
        public void FilterInitializedForWarehouse_Van()
        {
            ColumnsForFilter = new List<FilterModel>
        {
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["sku_code/name"],ColumnName = "SKUCode"},
        };
        }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_ware_house_stock"], IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["view_ware_house_stock"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        public async void ApplyFilter(Dictionary<string, string> filterCriterias)
        {
            _loadingService.ShowLoading();
            if (filterCriterias == null)
                return;
            List<FilterCriteria> criteriaList = filterCriterias.Select(pair =>
                new FilterCriteria(pair.Key, pair.Value, FilterType.Like)).ToList();
            _warehousestockViewModel.PageNumberSalableWarehouse_Van = 1;
            _warehousestockViewModel.PageNumberFocWarehouse_Van = 1;
            await _warehousestockViewModel.ApplyFilterWarehouse(criteriaList);
            _loadingService.HideLoading();
            StateHasChanged();
        }
        public async void ResetFilter()
        {
            _loadingService.ShowLoading();
            // Clear the filter criteria
            SKUCode = null;
            SKUName = null;
            await _warehousestockViewModel.ResetFilter();
            _loadingService.HideLoading();
            StateHasChanged();
        }
        private async Task GenerateGridColumnsForNormalStock()
        {
            DataGridColumnsNormalStock = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["sku_code"], GetValue = s => ((IWarehouseStockItemView)s)?.SKUCode ?? "N/A",IsSortable = true, SortField = "SKUCode" },
                new DataGridColumn { Header = @Localizer["sku_name"], GetValue = s => ((IWarehouseStockItemView)s)?.SKUName?? "N/A",IsSortable = true, SortField = "SKUName" },
                new DataGridColumn { Header = @Localizer["ou_qty"], GetValue = s => ((IWarehouseStockItemView)s)?.OuterQty.ToString("N0") ?? "N/A",IsSortable = true, SortField = "OuterQty" },
                new DataGridColumn { Header = @Localizer["ea_qty"], GetValue = s => ((IWarehouseStockItemView)s)?.EAQty.ToString("N0") ?? "N/A",IsSortable = true, SortField = "EAQty" },
                new DataGridColumn { Header = @Localizer["total_ea_qty"], GetValue = s => ((IWarehouseStockItemView)s)?.TotalEAQty.ToString("N0") ?? "N/A",IsSortable = true, SortField = "TotalEAQty" },
                new DataGridColumn { Header = @Localizer["cost_price"], GetValue = s => ((IWarehouseStockItemView)s)?.CostPrice.ToString("N2") ?? "N/A",IsSortable = false, SortField = "CostPrice" },
                new DataGridColumn { Header = @Localizer["net"], GetValue = s => ((IWarehouseStockItemView)s)?.Net.ToString("N2") ?? "N/A" ,IsSortable = false, SortField = "Net"}

            };
        }
        private async Task HandleWarehouseSelection(DropDownEvent eventArgs)
        {
            _loadingService.ShowLoading();
            if (eventArgs != null && eventArgs.SelectionItems != null && eventArgs.SelectionItems.Count > 0)
            {
                // Assuming the UID property in SelectionItem corresponds to the UID of the Warehouse
                var item = eventArgs.SelectionItems.FirstOrDefault();
                _warehousestockViewModel.selectedwarehouseUID = item.UID;
                await _warehousestockViewModel.PopulateViewModelForSalableWareHouse();
                await _warehousestockViewModel.PopulateViewModelForFocWareHouse();
            }
            _loadingService.HideLoading();

        }
        // selva
        //public async void OnWareHouseSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        //{
        //    if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        //    {

        //        var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
        //        // sku.Code = selecetedValue?.Code;
        //        _warehousestockViewModel.warehouseItemView.OrgTypeUID = selecetedValue?.UID;

        //        _warehousestockViewModel.selectedwarehouseUID = /*selecetedValue.UID*/ "hiohif";  
        //        _warehousestockViewModel.PopulateViewModel();
        //    }
        //}

        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            _loadingService.ShowLoading();
            if (!selectionItem.IsSelected)
            {
                TabSM.Select(selectionItem);
            }
            SelectedTabLabel = selectionItem.Label;
            if (selectionItem.Label == "Salable")
            {
                await _warehousestockViewModel.PopulateViewModelForSalableWareHouse();
            }
            else
            {
                await _warehousestockViewModel.PopulateViewModelForFocWareHouse();
            }
            _loadingService.HideLoading();
        }
        public async Task PageIndexChangedSalable(int pageNumber)
        {
            _loadingService.ShowLoading();
            _warehousestockViewModel.PageNumberSalableWarehouse_Van = pageNumber;
            await _warehousestockViewModel.PopulateViewModelForSalableWareHouse();
            _loadingService.HideLoading();
        }
        public async Task PageIndexChangedFoc(int pageNumber)
        {
            _loadingService.ShowLoading();
            _warehousestockViewModel.PageNumberFocWarehouse_Van = pageNumber;
            await _warehousestockViewModel.PopulateViewModelForFocWareHouse();
            _loadingService.HideLoading();
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            _loadingService.ShowLoading();
            await Task.Delay(TimeSpan.FromSeconds(2));
            _loadingService.HideLoading();
            await _warehousestockViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            _loadingService.HideLoading();
        }
        
        public string ViewingMessageFoc
        {
            get
            {
                int startRecord = 0;
                int endRecord = 0;
                if (_warehousestockViewModel.TotalItemsCountFocWarehouse_Van == 0)
                {
                    return $"You are viewing {startRecord}-{endRecord} out of {_warehousestockViewModel.TotalItemsCountFocWarehouse_Van}";
                }
                else
                {
                    startRecord = ((_warehousestockViewModel.PageNumberFocWarehouse_Van - 1) * _warehousestockViewModel.PageSizeFocWarehouse_Van) + 1;
                    endRecord = Math.Min(_warehousestockViewModel.PageNumberFocWarehouse_Van * _warehousestockViewModel.PageSizeFocWarehouse_Van, _warehousestockViewModel.TotalItemsCountFocWarehouse_Van);
                    return $"You are viewing {startRecord}-{endRecord} out of {_warehousestockViewModel.TotalItemsCountFocWarehouse_Van}";
                }
            }
        }
    }
}
