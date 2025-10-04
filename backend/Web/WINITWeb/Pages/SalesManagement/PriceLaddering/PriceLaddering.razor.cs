using Winit.Modules.PriceLadder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
namespace WinIt.Pages.SalesManagement.PriceLaddering
{
    public partial class PriceLaddering
    {
        public List<FilterModel>? ColumnsForFilter;
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
        {
            HeaderText = "Price Laddering",
            BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
        {
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Price Laddering" }
        }
        };
        private IPriceLaddering lastExpandedRow = null;
        private bool IsLoading = true;
        private bool IsProductCategoryIdClicked { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                dataService.HeaderText = "Price Laddering";

                if (_viewModel != null)
                {
                    await _viewModel.GetThePriceLadderingList();
                }

                await FilterInitialization();
                HideLoader();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error in OnInitializedAsync: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Code ", GetValue = s => ((ISKU)s)?.Code ?? "N/A"},
                new DataGridColumn {Header = "Name", GetValue = s =>((ISKU) s) ?.Name ?? "N/A"},
            };
        }

        protected async Task OnRowClick(IPriceLaddering selectedItem)
        {
            if (_viewModel.SelectedPriceLaddering == selectedItem)
            {
                _viewModel.SelectedPriceLaddering = null;
                _viewModel.PriceLadderingSubList.Clear();
            }
            else
            {
                _viewModel.SelectedPriceLaddering = selectedItem;
                await _viewModel.GetRelatedRowData();
            }
            StateHasChanged();
        }
        protected async Task OnProductCategoryClick(IPriceLaddering item, IPriceLaddering subItem)
        {
            try
            {
                await GenerateGridColumns();
                await _viewModel.OnProductCategoryClick(item, subItem);
                IsProductCategoryIdClicked = !IsProductCategoryIdClicked;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                StateHasChanged();
            }
        }
        public bool OpenClosePopUp()
        {
            try
            {
                return IsProductCategoryIdClicked = !IsProductCategoryIdClicked;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                StateHasChanged();
            }
        }
        private bool IsRowExpanded(IPriceLaddering item)
        {
            //_viewModel.SelectedPriceLaddering = item;
            return _viewModel.SelectedPriceLaddering == item;
        }
        #region FilterLogic
        public async Task FilterInitialization()
        {
            ColumnsForFilter = new List<FilterModel>
            {
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=await _viewModel.GetOUDetailsFromAPIAsync(),
                    SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                    ColumnName = nameof(IPriceLaddering.OperatingUnit),
                    Label = "Org Unit"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=await _viewModel.GetAllDivisionDDValues(),
                    SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                    ColumnName = nameof(IPriceLaddering.Division),
                    Label = "Division"
                },

                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=await _viewModel.GetAllBranchDDLValues(),
                    SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                    ColumnName = nameof(IPriceLaddering.Branch),
                    Label = "Branch"
                },
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                    DropDownValues=await _viewModel.GetBroadClassificationDDValues(),
                    SelectionMode=Winit.Shared.Models.Enums.SelectionMode.Single,
                    ColumnName = nameof(IPriceLaddering.BroadCustomerClassification),
                    Label = "Broad Classification"
                },

                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    ColumnName = nameof(IPriceLaddering.SkuCode),
                    Label = "Sku Code"
                },


            };

        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            _loadingService.ShowLoading();
            //_ApprovalEngine.PageNumber = 1;
            await _viewModel.OnFilterApply(filterCriteria: filterCriteria);
            _loadingService.HideLoading();
            StateHasChanged();
        }
        #endregion

        public bool isAscending = true;
        public string currentSortField { get; set; }
        private void SortColumn(string columnName)
        {

            if (currentSortField == columnName)
            {
                isAscending = !isAscending;
            }
            else
            {
                currentSortField = columnName;
                isAscending = true;
            }
            SortCriteria sortCriteria = new SortCriteria(currentSortField, isAscending ? SortDirection.Asc : SortDirection.Desc);
            _viewModel.ApplySort(sortCriteria);
        }
    }
}