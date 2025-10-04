using Microsoft.AspNetCore.Components;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Microsoft.Extensions.Logging;

namespace WinIt.Pages.MaintainSKU
{
    /// <summary>
    /// Component for maintaining SKU functionality
    /// </summary>
    public partial class MaintainSKU
    {
        private readonly ILogger<MaintainSKU> _logger;

        /// <summary>
        /// Gets or sets the data grid columns
        /// </summary>
        public List<DataGridColumn> DataGridColumns { get; set; }

        /// <summary>
        /// Gets or sets the data grid columns for static headers
        /// </summary>
        public List<DataGridColumn> DataGridColumns1 { get; set; }

        /// <summary>
        /// Gets or sets the data source
        /// </summary>
        public List<ISKUListView> Datasource { get; set; }

        /// <summary>
        /// Gets or sets the list of SKUs
        /// </summary>
        public List<ISKU> Skus { get; set; } = new List<ISKU>();

        /// <summary>
        /// Gets or sets the list of SKU attributes
        /// </summary>
        public List<ISKUAttributes> SkuAttribute { get; set; } = new List<ISKUAttributes>();

        /// <summary>
        /// Gets or sets the list of SKU grid view items
        /// </summary>
        public List<ISKUListView> SKUGVListViews { get; set; } = new List<ISKUListView>();

        /// <summary>
        /// Gets or sets the list of SKU view items
        /// </summary>
        public List<ISKUListView> SKUListView { get; set; }

        /// <summary>
        /// Gets or sets whether the component is loaded
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Gets or sets the SKU filter criteria
        /// </summary>
        public List<FilterCriteria> SKUFilterCriterials { get; set; }

        private bool showFilter { get; set; }

        /// <summary>
        /// Gets or sets the code filter
        /// </summary>
        public string CodeFilter { get; set; }

        /// <summary>
        /// Gets or sets the name filter
        /// </summary>
        public string NameFilter { get; set; }

        /// <summary>
        /// Gets or sets the organization UID
        /// </summary>
        [Parameter]
        public string OrgUID { get; set; }

        /// <summary>
        /// Gets or sets the selected SKU list view
        /// </summary>
        public ISKUListView? SelectedSKUListView { get; set; }

        /// <summary>
        /// Gets or sets whether the delete button popup is shown
        /// </summary>
        public bool IsDeleteBtnPopUp { get; set; }

        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        private Dictionary<string, string> columnHeaders;

        /// <summary>
        /// Gets or sets the callback service
        /// </summary>
        [CascadingParameter]
        public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

        private readonly IDataService dataService = new DataServiceModel
        {
            HeaderText = "Maintain SKU",
            BreadcrumList = new List<IBreadCrum>
            {
                new BreadCrumModel { SlNo = 1, Text = "Maintain SKU" }
            }
        };

        /// <summary>
        /// Initializes the component
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                LoadResources(null, _languageService.SelectedCulture);
                await GetSKUAttributeType();
                FilterInitialized();
                await _skuViewModel.GetStatus();
                _skuViewModel.PageSize = 10;
                SKUFilterCriterials = new List<FilterCriteria>();
                await _skuViewModel.PopulateViewModel();
                await _skuViewModel.OnDivisionSelectionTypeSelect();
                IsLoaded = true;
                await GenerateGridColumns();
                await GetStaticHeaders();
                await StateChageHandler();
                HideLoader();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during component initialization");
                HideLoader();
                throw;
            }
        }

        private async Task StateChageHandler()
        {
            try
            {
                _navigationManager.LocationChanged += (sender, args) => SavePageState();
                bool stateRestored = _pageStateHandler.RestoreState("MaintainSKU", ref ColumnsForFilter, out PageState pageState);
                await OnFilterApply(_pageStateHandler._currentFilters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in state change handler");
                throw;
            }
        }

        private void SavePageState()
        {
            try
            {
                _navigationManager.LocationChanged -= (sender, args) => SavePageState();
                _pageStateHandler.SaveCurrentState("MaintainSKU");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving page state");
                throw;
            }
        }

        /// <summary>
        /// Gets the static headers for the grid
        /// </summary>
        public async Task GetStaticHeaders()
        {
            try
            {
                DataGridColumns1 = new List<DataGridColumn>
                {
                    new DataGridColumn { Header = @Localizer["code"], GetValue = s => ((ISKUListView)s)?.SKUCode ?? "N/A" },
                    new DataGridColumn { Header = @Localizer["sku_name"], GetValue = s => ((ISKUListView)s)?.SKULongName ?? "N/A" },
                    new DataGridColumn { Header = @Localizer["status"], GetValue = s => ((ISKUListView)s)?.IsActive == true ? "Yes" : "No" },
                    new DataGridColumn { Header = @Localizer["brandownership"], GetValue = s => ((ISKUListView)s)?.BrandOwnershipName ?? "N/A" },
                    new DataGridColumn { Header = @Localizer["category"], GetValue = s => ((ISKUListView)s)?.CategoryName ?? "N/A" },
                    new DataGridColumn { Header = @Localizer["subcategory"], GetValue = s => ((ISKUListView)s)?.SubCategoryName ?? "N/A" },
                    new DataGridColumn { Header = @Localizer["brand"], GetValue = s => ((ISKUListView)s)?.BrandName ?? "N/A" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting static headers");
                throw;
            }
        }

        /// <summary>
        /// Shows or hides the filter
        /// </summary>
        public void ShowFilter()
        {
            try
            {
                showFilterComponent = !showFilterComponent;
                filterRef.ToggleFilter();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling filter");
                throw;
            }
        }

        /// <summary>
        /// Initializes the filter
        /// </summary>
        public void FilterInitialized()
        {
            try
            {
                ColumnsForFilter = new List<FilterModel>
                {
                    new FilterModel 
                    { 
                        FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, 
                        Label = @Localizer["sku_code/name"],
                        ColumnName = "skucodeandname", 
                        IsForSearch = true, 
                        PlaceHolder = "Search By SKU Code / Name", 
                        Width = 1000
                    },
                    new FilterModel 
                    { 
                        FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                        DropDownValues = _skuViewModel.SkuAttributeLevel.SKUGroupTypes,
                        OnDropDownSelect = OnAttributeTypeSelect, 
                        Label = @Localizer["attribute_type"], 
                        ColumnName = "AttributeType",
                        HasChildDependency = true, 
                        IsCodeOnDDLSelect = true
                    },
                    new FilterModel 
                    { 
                        FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                        DropDownValues = _skuViewModel.AttributeNameSelectionItems, 
                        Label = @Localizer["attribute_name"], 
                        ColumnName = "AttributeValue",
                        IsDependent = true,
                        SelectionMode = SelectionMode.Multiple
                    },
                    new FilterModel 
                    { 
                        FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                        DropDownValues = _skuViewModel.ProductDivisionSelectionItems, 
                        Label = "Division", 
                        ColumnName = "DivisionUID", 
                        SelectionMode = SelectionMode.Multiple
                    },
                    new FilterModel 
                    { 
                        FilterType = Winit.Shared.Models.Constants.FilterConst.CheckBox,
                        DropDownValues = _skuViewModel.StatusSelectionItems, 
                        Label = "Show Inactive", 
                        ColumnName = "IsActive"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while initializing filter");
                throw;
            }
        }

        /// <summary>
        /// Gets the SKU attribute type
        /// </summary>
        public async Task<ISKUAttributeLevel> GetSKUAttributeType()
        {
            try
            {
                _skuViewModel.SkuAttributeLevel = await _skuViewModel.GetAttributeType();

                if (_skuViewModel.SkuAttributeLevel == null)
                {
                    _skuViewModel.SkuAttributeLevel = new SKUAttributeLevel
                    {
                        SKUGroupTypes = new List<ISelectionItem>(),
                        SKUGroups = new Dictionary<string, List<ISelectionItem>>()
                    };
                }
                return _skuViewModel.SkuAttributeLevel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting SKU attribute type");
                throw;
            }
        }

        private async Task OnAttributeTypeSelect(DropDownEvent dropDownEvent)
        {
            try
            {
                if (dropDownEvent?.SelectionItems?.Any() == true)
                {
                    await _skuViewModel.OnAttributeTypeSelect(dropDownEvent.SelectionItems.First().Code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling attribute type selection");
                throw;
            }
        }

        private void ToggleFilter()
        {
            try
            {
                showFilter = !showFilter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling filter");
                throw;
            }
        }

        private async Task GenerateGridColumns()
        {
            try
            {
                DataGridColumns = new List<DataGridColumn>
                {
                    new DataGridColumn 
                    { 
                        Header = @Localizer["sku_code"], 
                        GetValue = s => ((ISKUListView)s)?.SKUCode ?? "N/A",
                        IsSortable = true, 
                        SortField = "SKUCode" 
                    },
                    new DataGridColumn 
                    { 
                        Header = @Localizer["sku_name"], 
                        GetValue = s => ((ISKUListView)s)?.SKULongName ?? "N/A",
                        IsSortable = true, 
                        SortField = "SKULongName" 
                    },
                    new DataGridColumn 
                    { 
                        Header = "Product Category Id", 
                        GetValue = s => ((ISKUListView)s)?.ProductCategoryId ?? "N/A",
                        IsSortable = true, 
                        SortField = "ProductCategoryId" 
                    },
                    new DataGridColumn 
                    { 
                        Header = @Localizer["isactive"], 
                        GetValue = s => ((ISKUListView)s)?.IsActive == true ? "Yes" : "No",
                        IsSortable = true, 
                        SortField = "IsActive" 
                    },
                    new DataGridColumn
                    {
                        Header = @Localizer["actions"],
                        IsButtonColumn = true,
                        ButtonActions = new List<ButtonAction>
                        {
                            new ButtonAction
                            {
                                ButtonType = ButtonTypes.Image,
                                URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                                Action = item => OnEditClick((ISKUListView)item)
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating grid columns");
                throw;
            }
        }

        private bool IsAddNewSKUVisiblebtn = false;

        /// <summary>
        /// Handles edit click event
        /// </summary>
        public void OnEditClick(ISKUListView sku)
        {
            try
            {
                _navigationManager.NavigateTo($"AddEditMaintainSKU?SKUUID={sku.SKUUID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling edit click");
                throw;
            }
        }

        /// <summary>
        /// Handles delete click event
        /// </summary>
        public async Task OnDeleteClick(ISKUListView sKUListView)
        {
            try
            {
                SelectedSKUListView = sKUListView;
                if (await _AlertMessgae.ShowConfirmationReturnType(@Localizer["delete"], @Localizer["are_you_sure_you_want_to_delete_this_item_?"]))
                {
                    string msg = await _skuViewModel.DeleteSKUItem(SelectedSKUListView?.SKUUID);
                    if (msg.Contains("Failed"))
                    {
                        await _AlertMessgae.ShowErrorAlert(@Localizer["failed"], msg);
                    }
                    else
                    {
                        await _AlertMessgae.ShowSuccessAlert(@Localizer["success"], msg);
                        await _skuViewModel.PopulateViewModel();
                    }
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling delete click");
                throw;
            }
        }

        private async Task AddNewProduct()
        {
            try
            {
                _navigationManager.NavigateTo("AddEditMaintainSKU", forceLoad: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding new product");
                throw;
            }
        }

        private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            try
            {
                ShowLoader();
                _skuViewModel.PageNumber = 1;
                _pageStateHandler._currentFilters = keyValuePairs;
                await _skuViewModel.OnFilterApply(ColumnsForFilter, keyValuePairs);
                StateHasChanged();
                HideLoader();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while applying filter");
                HideLoader();
                throw;
            }
        }

        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            try
            {
                ShowLoader();
                await _skuViewModel.ApplySort(sortCriteria);
                StateHasChanged();
                HideLoader();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while applying sort");
                HideLoader();
                throw;
            }
        }

        private async Task PageIndexChanged(int pageNumber)
        {
            try
            {
                ShowLoader();
                await _skuViewModel.PageIndexChanged(pageNumber);
                StateHasChanged();
                HideLoader();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing page index");
                HideLoader();
                throw;
            }
        }
    }
}


