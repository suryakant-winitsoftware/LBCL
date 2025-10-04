using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using System.Resources;
using System.Text;
using Winit.Modules.Common.BL;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.User.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.ManageUnallocatedOrders
{
    public partial class ManageUnallocatedOrders
    {

        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsLoaded { get; set; }
        bool isFirstColumnCheckbox;
        public string SelectedTab = SalesOrderStatus.DRAFT;
        public List<DataGridColumn> DataGridColumnsDeliveredlStock { get; set; }
        //public List<ISelectionItem> TabSelectionItems = new List<ISelectionItem>
        //{
        //     new SelectionItem{ Label="Draft", Code=SalesOrderStatus.DRAFT, UID="1"},
        // new SelectionItem{ Label="Pending", Code=SalesOrderStatus.PENDING, UID="2"},
        //new SelectionItem{ Label="Assigned", Code=SalesOrderStatus.ASSIGNED, UID="3"},
        //new SelectionItem{ Label="Allocated", Code=SalesOrderStatus.ALLOCATED, UID="4"},
        //new SelectionItem{ Label="Delivered", Code=SalesOrderStatus.DELIVERED, UID="5"},
        //new SelectionItem{ Label="Deleted", Code=SalesOrderStatus.DELETED, UID="6"},
        //};
        private string selectedOption = "Route";
        public string SelectedRoute { get; set; }
        public DateTime SelectedDeliveryDate { get; set; } = DateTime.Today;
        public bool IsRouteSelected => selectedOption == "Route";
        public bool IsDeliveryDateSelected => selectedOption == "DeliveryDate";
        public bool IsBothSelected => selectedOption == "Both";
        private List<ISelectionItem> _tabSelectionItems;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Manage Unallocated Order",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Manage Unallocated Order"},
            }
        };
        public List<ISelectionItem> TabSelectionItems
		{
			get
			{
				if (_tabSelectionItems == null)
				{
					_tabSelectionItems = new List<ISelectionItem>
		{
			new SelectionItem { Label = @Localizer["draft"], Code = SalesOrderStatus.DRAFT, UID = "1" },
			new SelectionItem { Label = @Localizer["pending"], Code = SalesOrderStatus.PENDING, UID = "2" },
			new SelectionItem { Label = @Localizer["assigned"], Code = SalesOrderStatus.ASSIGNED, UID = "3" },
			new SelectionItem { Label = @Localizer["allocated"], Code = SalesOrderStatus.ALLOCATED, UID = "4" },
			new SelectionItem { Label = @Localizer["delivered"], Code = SalesOrderStatus.DELIVERED, UID = "5" },
			new SelectionItem { Label = @Localizer["deleted"], Code = SalesOrderStatus.DELETED, UID = "6" },
		};
				}
				return _tabSelectionItems;
			}
		}
        private SelectionManager TabSM;
        public string PageName = "ManageUnallocatedOrders";
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        public List<ISelectionItem> OrderTypeSelectionItems = new List<ISelectionItem>
        {
         new SelectionItem{ Label="Cashsales", Code=OrderType.Cashsales, UID="Cashsales"},
         new SelectionItem{ Label="Vansales", Code=OrderType.Vansales, UID="Vansales"},
        new SelectionItem{ Label="Presales", Code=OrderType.Presales, UID="Presales"},
        };
        
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            FilterInitialized();
            _manageSalesOrdersViewModel.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
            //_manageSalesOrdersViewModel.OrgUID = "FR001";
           await _manageSalesOrdersViewModel.GetDistributor();
           await _manageSalesOrdersViewModel.GetSalesman(_manageSalesOrdersViewModel.OrgUID);
           await _manageSalesOrdersViewModel.GetRoute(_manageSalesOrdersViewModel.OrgUID);
            await _manageSalesOrdersViewModel.PopulateViewModel();
            _manageSalesOrdersViewModel.salesOrderFilterCriterials.Clear();
            _manageSalesOrdersViewModel.salesOrderFilterCriterials.Add(new FilterCriteria("OrderType", OrderType.CashVansales, FilterType.In));
            await GetDataLoadAsync();
            await GenerateGridColumnsForNormalStock();
            TabSM = new SelectionManager(TabSelectionItems, SelectionMode.Single);
            TabSelectionItems[0].IsSelected = SelectedTab == SalesOrderStatus.DRAFT;
            IsLoaded = true;
            //await SetHeaderName();
            await CheckBoxSelections();
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
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["distributor"], DropDownValues=_manageSalesOrdersViewModel.DistributorSelectionList,ColumnName = "OrgUID"},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["sales_order_no"],ColumnName = "SalesOrderNumber"},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["order_number"],ColumnName = "DraftOrderNumber"},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["salesman"], DropDownValues=_manageSalesOrdersViewModel.EmpSelectionList,  ColumnName = "EmpUID",SelectionMode=SelectionMode.Multiple},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["route"], DropDownValues=_manageSalesOrdersViewModel.RouteSelectionItems, OnDropDownSelect=OnRouteSelect, ColumnName = "RouteName",SelectionMode=SelectionMode.Multiple},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["customer"], DropDownValues=_manageSalesOrdersViewModel.CustomerSelectionList, ColumnName = "StoreName",SelectionMode=SelectionMode.Multiple},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["order_type"],ColumnName = "OrderType", DropDownValues = OrderTypeSelectionItems,SelectionMode=SelectionMode.Multiple},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = @Localizer["delivery_date"],ColumnName = "DeliveryDate"},
            };
        }
        private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            await _manageSalesOrdersViewModel.OnFilterApply(ColumnsForFilter, keyValuePairs,SelectedTab);
        }
       
        private async Task OnRouteSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                await _manageSalesOrdersViewModel.OnRouteSelect(dropDownEvent.SelectionItems.First().UID);
            }
        }
        private async Task CheckBoxSelections()
        {
            if (TabSelectionItems[0].IsSelected || TabSelectionItems[1].IsSelected)
            {
                isFirstColumnCheckbox = true;
            }
            else
            {
                isFirstColumnCheckbox = false;
            }
        }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_unallocated_orders"], IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["manage_unallocated_orders"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        private async Task GenerateGridColumnsForNormalStock()
        {
            DataGridColumnsDeliveredlStock = new List<DataGridColumn>
            {
                new DataGridColumn { Header =@Localizer["sales_order_no"], GetValue = s => ((IDeliveredPreSales)s)?.SalesOrderNumber?? "N/A",IsSortable=true,SortField="SalesOrderNumber" },
                new DataGridColumn { Header =@Localizer["order_number"] , GetValue = s => ((IDeliveredPreSales)s)?.DraftOrderNumber ?? "N/A",IsSortable=true,SortField="DraftOrderNumber" },
                new DataGridColumn { Header =@Localizer["route"] , GetValue = s => ((IDeliveredPreSales)s)?.RouteName ?? "N/A",IsSortable=true,SortField="RouteName" },
                new DataGridColumn { Header =@Localizer["customer_number"] , GetValue = s => ((IDeliveredPreSales)s)?.StoreNumber?? "N/A",IsSortable=true,SortField="StoreNumber" },
                new DataGridColumn { Header =@Localizer["customer_name"] , GetValue = s => ((IDeliveredPreSales)s)?.StoreName ?? "N/A",IsSortable=true,SortField="StoreName" },
                new DataGridColumn { Header =@Localizer["order_type"] , GetValue = s => ((IDeliveredPreSales)s)?.OrderType ?? "N/A",IsSortable=true,SortField="OrderType" },
                new DataGridColumn { Header =@Localizer["delivery_date"] , GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((IDeliveredPreSales)s)?.DeliveryDate) ,IsSortable=true,SortField="DeliveryDate" },
                new DataGridColumn { Header =@Localizer["sku"] , GetValue = s => ((IDeliveredPreSales)s)?.SKUCount ?? 0 },
            new DataGridColumn
             {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                      new ButtonAction
                      {
                           ButtonType = ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/view.png",
                        Action = s => OnViewClick((IDeliveredPreSales)s),
                        IsVisible = SelectedTab == SalesOrderStatus.DRAFT   ? false:true
                      },
                        new ButtonAction
                      {
                           ButtonType = ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                        Action = s => OnEditClick((IDeliveredPreSales)s),
                        IsVisible = SelectedTab == SalesOrderStatus.DRAFT   ? true:false
                      }
                }
            }
             };
        }
        private void OnViewClick(IDeliveredPreSales deliveredPreSales)
        {
            _navigationManager.NavigateTo($"ViewSalesOrdersDetails?SalesOrderUID={deliveredPreSales.SalesOrderUID}&PageName={PageName}&Status={deliveredPreSales.Status}");
        }
        private void OnEditClick(IDeliveredPreSales deliveredPreSales)
        {
            string SalesType = Winit.Shared.Models.Constants.OrderType.Cashsales;
            switch (deliveredPreSales.OrderType)
            {
                case Winit.Shared.Models.Constants.OrderType.Cashsales:
                    SalesType = "CS";
                    break;
                case Winit.Shared.Models.Constants.OrderType.Vansales:
                    SalesType = "VS";
                    break;
            }
            _navigationManager.NavigateTo($"SalesOrder?SalesOrderUID={deliveredPreSales.SalesOrderUID}&SalesType={SalesType}");
        }
        private async void Product_AfterCheckBoxSelection(HashSet<object> hashSet)
        {

        }
        public async void OnTabSelect(ISelectionItem selectionItem)
        {
            if (!selectionItem.IsSelected)
            {
                TabSM.Select(selectionItem);
                SelectedTab = selectionItem.Code;
                InvokeAsync(async () =>
                {
                    await GetDataLoadAsync();
                    StateHasChanged();
                });
            }
            await CheckBoxSelections();
            await GenerateGridColumnsForNormalStock();
        }
        private async Task GetDataLoadAsync()
        {
            await _manageSalesOrdersViewModel.PopulateViewModel(SelectedTab);
        }
        private async Task HandleRouteSelection(DropDownEvent eventArgs)
        {
            if (eventArgs != null && eventArgs.SelectionItems != null && eventArgs.SelectionItems.Count > 0)
            {
                var item = eventArgs.SelectionItems.FirstOrDefault();
                _manageSalesOrdersViewModel.OrgUID = item.UID;
                //await _deliveredPreSalesViewModel.PopulateViewModel();
            }
        }
      
       
      
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _manageSalesOrdersViewModel.ApplySort(sortCriteria,SelectedTab);
            StateHasChanged();
            HideLoader();
        }
       

        private void HandleRadioChange(ChangeEventArgs e)
        {
            selectedOption = e.Value.ToString();
        }
    }
}
