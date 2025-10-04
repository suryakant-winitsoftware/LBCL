using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common.Filter;
using WINITMobile.Pages.Base;
using SelectionMode = Winit.Shared.Models.Enums.SelectionMode;

namespace WINITMobile.Pages.Maintain_WareHouse
{
    public partial class MaintainWareHouseStock : BaseComponentBase
    {
        public bool IsInitialized { get; set; } = false;
        public bool IsGridInitialised { get; set; } = false;
        public List<string> WarehouseStockOrganisationalSelectedUIDs { get; set; } = new List<string>();
        public string WareHouseDDSelectedItemCode { get; set; } = "";
        public List<string> SubWarehouseStockSelectedUIDs { get; set; } = new List<string>();
        List<FilterModel> ColumnsForFilter = [];
        public List<Winit.Modules.Org.Model.Interfaces.IWareHouseStock> DataSource = new List<IWareHouseStock>();
        public List<DataGridColumn> DataGridColumns { get; set; }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                _loadingService.ShowLoading();
                _warehousestockViewModel.PageSize = 10;
                await _warehousestockViewModel.PopulateViewModelForWareHouseDD();
                SetFilters();
                IsInitialized = true;
                _loadingService.HideLoading();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task HandleWarehouseOrganisationalSelection(DropDownEvent eventArgs)
        {
            _loadingService.ShowLoading();
            WarehouseStockOrganisationalSelectedUIDs = _warehousestockViewModel.WareHouseOrganisationalSelectionItems.Where(item => item.IsSelected == true).Select(item => item.Code).ToList();
            _warehousestockViewModel.OrganisationalDDSelectedUIDs = WarehouseStockOrganisationalSelectedUIDs;

            _loadingService.HideLoading();
        }
        private async Task HandleWarehouseSelection(DropDownEvent eventArgs)
        {
            _loadingService.ShowLoading();
            WareHouseDDSelectedItemCode = _warehousestockViewModel.WareHouseStockSelectionItems.Where(item => item.IsSelected == true).Select(item => item.Code).FirstOrDefault();
            _warehousestockViewModel.WareHouseDDSelectedItem = WareHouseDDSelectedItemCode;
            string uID = _warehousestockViewModel.WareHouseStockSelectionItems.Where(item => item.IsSelected == true).Select(item => item.UID).FirstOrDefault();
            await _warehousestockViewModel.PopulateViewModelForSubWareHouseDD(OrgTypeConst.SWH, uID, _appUser.SelectedJobPosition.BranchUID);
            _loadingService.HideLoading();

        }
        private async Task HandleSubWarehouseSelection(DropDownEvent eventArgs)
        {
            _loadingService.ShowLoading();
            SubWarehouseStockSelectedUIDs = _warehousestockViewModel.SubWareHouseSelectionItems.Where(item => item.IsSelected == true).Select(item => item.Code).ToList();
            _warehousestockViewModel.SubWareHouseDDSelectedUIDs = SubWarehouseStockSelectedUIDs;
            _loadingService.HideLoading();
        }
        private async Task PopulateDataBasedOnSelections()
        {
            _loadingService.ShowLoading();
            if (SelectionsValidated())
            {
                _warehousestockViewModel.PageNumber = 1;
                await _warehousestockViewModel.PopulateGridDataForMaintainWareHouseStock();
                GenerateGridColumns();
                IsGridInitialised = true;
            }
            else
            {
                IsGridInitialised = false;
            }
            _loadingService.HideLoading();
        }

        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

                new DataGridColumn { Header = "Organisation Unit" , GetValue = item => ((IWareHouseStock)item).OUCode ?? "N/A",IsSortable=true ,SortField="OUCode" },
                new DataGridColumn { Header = "Warehouse" , GetValue = item => $"[{((IWareHouseStock)item).WarehouseCode}] {((IWareHouseStock)item).WarehouseName}" ?? "N/A",IsSortable=true ,SortField="WarehouseCode" },
                new DataGridColumn { Header = "Sub Warehouse" ,  GetValue = item => $"[{((IWareHouseStock)item).SubWarehouseCode}] {((IWareHouseStock)item).SubWarehouseName}" ?? "N/A",IsSortable=true ,SortField="SubWarehouseCode" },
                new DataGridColumn { Header = "SKU Description" ,  GetValue = item => $"[{((IWareHouseStock)item).SKUCode}] {((IWareHouseStock)item).SKUName}" ?? "N/A",IsSortable=true ,SortField="SKUName" },
                new DataGridColumn { Header = "Qty" , GetValue = item => ((IWareHouseStock)item).Quantity.ToString() ?? "N/A",IsSortable=true ,SortField="Quantity" },
            };
        }

        private bool SelectionsValidated()
        {
            if (string.IsNullOrWhiteSpace(WareHouseDDSelectedItemCode))
            {
                ShowAlert("Error", "Warehouse cannot be null or empty.");
                return false;
            }
            else if (_warehousestockViewModel.WareHouseOrganisationalSelectionItems.Where(item => item.IsSelected == true).Select(item => item.Code).ToList().Count() == 0)
            {
                ShowAlert("Error", "Please select atleast one organisation unit.");
                return false;
            }
            else
            {
                return true;
            }
        }
        private async Task PageIndexChanged(int pageNumber)
        {
            ShowLoader();
            WarehouseStockOrganisationalSelectedUIDs = _warehousestockViewModel.WareHouseOrganisationalSelectionItems.Where(item => item.IsSelected == true).Select(item => item.Code).ToList();
            SubWarehouseStockSelectedUIDs = _warehousestockViewModel.SubWareHouseSelectionItems.Where(item => item.IsSelected == true).Select(item => item.Code).ToList();
            await _warehousestockViewModel.WarehouseStockPageIndexChanged(pageNumber, WarehouseStockOrganisationalSelectedUIDs, WareHouseDDSelectedItemCode, SubWarehouseStockSelectedUIDs);
            HideLoader();
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            WarehouseStockOrganisationalSelectedUIDs = _warehousestockViewModel.WareHouseOrganisationalSelectionItems.Where(item => item.IsSelected == true).Select(item => item.Code).ToList();
            SubWarehouseStockSelectedUIDs = _warehousestockViewModel.SubWareHouseSelectionItems.Where(item => item.IsSelected == true).Select(item => item.Code).ToList();
            await _warehousestockViewModel.ApplyWarehouseStockSort(sortCriteria, WarehouseStockOrganisationalSelectedUIDs, WareHouseDDSelectedItemCode, SubWarehouseStockSelectedUIDs);
            StateHasChanged();
            HideLoader();
        }
        protected void SetFilters()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = "SKU Code/Name",
                    ColumnName = "SKUName"

                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues = _warehousestockViewModel.ItemSeries,
                    SelectionMode = SelectionMode.Multiple,
                    Label = "Item Series",
                    ColumnName = "item_series"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues = _warehousestockViewModel.TonnageDDL,
                    SelectionMode = SelectionMode.Multiple,
                    Label = "Tonage",
                    ColumnName = "tonage"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues = _warehousestockViewModel.StarRatingDDL,
                    SelectionMode = SelectionMode.Multiple,
                    Label = "Star Rating",
                    ColumnName = "star_rating"
                }
             };
        }
        private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            _loadingService.ShowLoading();
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if (keyValue.Key == "SKUName")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                    }
                    else if (keyValue.Key == "item_series")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.In));
                    }
                    else if (keyValue.Key == "tonage")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.In));
                    }
                    else if (keyValue.Key == "star_rating")
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.In));
                    }
                }
            }
            await _warehousestockViewModel.ApplyFilterForWareHouseStock(filterCriterias);
            StateHasChanged();
            _loadingService.HideLoading();
        }
        private async Task OnExcelDownloadClick()
        {
            try
            {
                _warehousestockViewModel.PageSize = 0;
                _warehousestockViewModel.PageNumber = 0;
                await _warehousestockViewModel.PopulateGridDataForEXCEL();
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
