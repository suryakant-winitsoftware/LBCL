using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Components;
using NPOI.OpenXmlFormats.Spreadsheet;
using System.Globalization;
using System.Resources;
using Winit.Modules.Common.BL;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Tax.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.ViewSalesOrders
{
    public partial class ViewSalesOrders
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public List<DataGridColumn> DataGridColumnsDeliveredlStock { get; set; }
        public bool IsLoaded { get; set; }
        public string SelectedTab = SalesOrderStatus.DELIVERED;
        public string PageName = "ViewsalesOrder";
        public bool IsManageUnallocated { get; set; }
        private List<ButtonAction> buttonActions;
        public ISelectionItem SelectionItem { get; set; }
        //public List<ISelectionItem> TabSelectionItems = new List<ISelectionItem>
        //{
        // new SelectionItem{ Label="Delivered", Code=SalesOrderStatus.DELIVERED, UID="1"},
        //new SelectionItem{ Label="Sent", Code=SalesOrderStatus.SENT, UID="2"},
        //new SelectionItem{ Label="Received", Code=SalesOrderStatus.RECEIVED, UID="3"},
        //new SelectionItem{ Label="Error", Code=SalesOrderStatus.ERROR, UID="4"},
        //};
        private List<ISelectionItem> _tabSelectionItems;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Sales Order",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="View Sales Order"},
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
            new SelectionItem { Label = @Localizer["delivered"], Code = SalesOrderStatus.DRAFT, UID = "1" },
            new SelectionItem { Label = @Localizer["sent"], Code = SalesOrderStatus.SENT, UID = "2" },
            new SelectionItem { Label = @Localizer["received"], Code = SalesOrderStatus.RECEIVED, UID = "3" },
            new SelectionItem { Label = @Localizer["error"], Code = SalesOrderStatus.ERROR, UID = "4" },

        };
                }
                return _tabSelectionItems;
            }
        }
        private SelectionManager TabSM;
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        public List<ISelectionItem> OrderTypeSelectionItems = new List<ISelectionItem>
        {
         new SelectionItem{ Label="Cashsales", Code=OrderType.Cashsales, UID="Cashsales"},
         new SelectionItem{ Label="Vansales", Code=OrderType.Vansales, UID="Vansales"},
         new SelectionItem{ Label="Presales", Code=OrderType.Presales, UID="Presales"},
        };
        public async void ShowFilter()
        {
            showFilterComponent = !showFilterComponent;
            filterRef.ToggleFilter();
        }
        public void FilterInitialized()
        {

            ColumnsForFilter = new List<FilterModel>
        {
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["distributor"],DropDownValues=_manageSalesOrdersViewModel.DistributorSelectionList,ColumnName = "OrgUID"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label =@Localizer["sales_order_no"],ColumnName = "SalesOrderNumber"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["orderno"],ColumnName = "DraftOrderNumber"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["salesman"], DropDownValues=_manageSalesOrdersViewModel.EmpSelectionList,  ColumnName = "EmpUID",SelectionMode=SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label =@Localizer["route"], DropDownValues=_manageSalesOrdersViewModel.RouteSelectionItems, OnDropDownSelect=OnRouteSelect, ColumnName = "RouteName",SelectionMode=SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["customer"], DropDownValues=_manageSalesOrdersViewModel.CustomerSelectionList, ColumnName = "StoreName",SelectionMode=SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label =@Localizer["ordertype"],ColumnName = "OrderType",DropDownValues=OrderTypeSelectionItems,SelectionMode=SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = @Localizer["delivery_date"],ColumnName = "DeliveryDate"},
        };
        }
        private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            await _manageSalesOrdersViewModel.OnFilterApply(ColumnsForFilter, keyValuePairs, SelectedTab);
        }
        private async Task OnRouteSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                    await _manageSalesOrdersViewModel.OnRouteSelect(dropDownEvent.SelectionItems.First().UID);
            }
        }
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            FilterInitialized();
            _manageSalesOrdersViewModel.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
            //_manageSalesOrdersViewModel.OrgUID = "FR001";
            await _manageSalesOrdersViewModel.GetDistributor();
            await _manageSalesOrdersViewModel.GetSalesman(_manageSalesOrdersViewModel.OrgUID);
            await _manageSalesOrdersViewModel.GetRoute(_manageSalesOrdersViewModel.OrgUID);
            //_manageSalesOrdersViewModel.PageSize = 20;
            _manageSalesOrdersViewModel.IsCashVanSalesPage = true;
            _manageSalesOrdersViewModel.salesOrderFilterCriterials.Clear();
            _manageSalesOrdersViewModel.salesOrderFilterCriterials.Add(new FilterCriteria("OrderType", OrderType.CashVansales, FilterType.In));
            //_deliveredPreSalesViewModel.salesOrderFilterCriterials.Add(new FilterCriteria("OrderType", OrderType.Vansales, FilterType.Equal));
            TabSM = new SelectionManager(TabSelectionItems, SelectionMode.Single);
            TabSelectionItems[0].IsSelected = SelectedTab == SalesOrderStatus.DELIVERED;
            //OnTabSelect(TabSelectionItems[0]);
            await GetDataLoadAsync();
            IsLoaded = true;
            await GenerateGridColumnsForNormalStock();
           // await SetHeaderName();
        }
        
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_sales_orders"], IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["view_sales_orders"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        private async Task GenerateGridColumnsForNormalStock()
        {
            DataGridColumnsDeliveredlStock = new List<DataGridColumn>
            {
                new DataGridColumn { Header =@Localizer["franchisee"], GetValue = s => ((IDeliveredPreSales)s)?.FranchiseCode ?? "N/A" ,IsSortable=true,SortField="FranchiseCode"},
                new DataGridColumn { Header = @Localizer["sales_order_no"], GetValue = s => ((IDeliveredPreSales)s)?.SalesOrderNumber?? "N/A",IsSortable=true,SortField="SalesOrderNumber" },
                new DataGridColumn {Header = @Localizer["sales_rep"], GetValue = s =>((IDeliveredPreSales) s) ?.EmpName ?? "N/A", IsSortable = true, SortField = "EmpName"},
                new DataGridColumn {Header = @Localizer["route"], GetValue = s =>((IDeliveredPreSales) s) ?.RouteName ?? "N/A", IsSortable = true, SortField = "RouteName"},
                new DataGridColumn {Header =  @Localizer["customer_number"], GetValue = s =>((IDeliveredPreSales) s) ?.StoreNumber ?? "N/A", IsSortable = true, SortField = "StoreNumber"},
                new DataGridColumn {Header = @Localizer["customer_code"], GetValue = s =>((IDeliveredPreSales) s) ?.StoreCode ?? "N/A", IsSortable = true, SortField = "StoreCode"},
                new DataGridColumn {Header = @Localizer["customer_name"], GetValue = s =>((IDeliveredPreSales) s) ?.StoreName ?? "N/A", IsSortable = true, SortField = "StoreName"},
                new DataGridColumn {Header = @Localizer["order_type"], GetValue = s =>((IDeliveredPreSales) s) ?.OrderType ?? "N/A", IsSortable = true, SortField = "OrderType"},
                new DataGridColumn { Header = @Localizer["order_date"], GetValue = s => ((IDeliveredPreSales)s)?.OrderDate,IsSortable=true,SortField="OrderDate" },
                new DataGridColumn { Header = @Localizer["delivery_date"], GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((IDeliveredPreSales) s) ?.DeliveryDate),IsSortable=true,SortField="DeliveryDate" },
                new DataGridColumn { Header = @Localizer["total_sku_count"], GetValue = s => ((IDeliveredPreSales)s)?.SKUCount ?? 0,IsSortable=true,SortField="SKUCount" },
                new DataGridColumn { Header = @Localizer["amount_inc_tax_$"], GetValue = s => ((IDeliveredPreSales)s)?.NetAmount ?? 0,IsSortable=true,SortField="NetAmount" },

            new DataGridColumn
             {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                //ButtonActions = this.buttonActions
                ButtonActions = new List<ButtonAction>
                    {
                     new ButtonAction
                    {
                             ButtonType = ButtonTypes.Image,
                             URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                            Action = item => OnEditClick((IDeliveredPreSales)item),
                            IsVisible = SelectedTab == SalesOrderStatus.DRAFT   ? true:false
                        },
                        new ButtonAction
                        {
                             ButtonType = ButtonTypes.Image,
                             URL = "https://qa-fonterra.winitsoftware.com/assets/Images/view.png",
                            Action = item => OnViewClick((IDeliveredPreSales)item),
                             IsVisible = SelectedTab ==SalesOrderStatus.DELIVERED || SelectedTab == SalesOrderStatus.SENT ||  SelectedTab == SalesOrderStatus.DRAFT || SelectedTab == SalesOrderStatus.ERROR?true:false
                        }
                     }
            }
             };
        }
        private void OnEditClick(IDeliveredPreSales deliveredPreSales)
        {
            string SalesType = Winit.Shared.Models.Constants.OrderType.Cashsales;
            switch (deliveredPreSales.OrderType)
            {
                case Winit.Shared.Models.Constants.OrderType.Presales:
                    SalesType = "PS";
                    break;
                case Winit.Shared.Models.Constants.OrderType.Cashsales:
                    SalesType = "CS";
                    break;
                case Winit.Shared.Models.Constants.OrderType.Vansales:
                    SalesType = "VS";
                    break;
            }
            _navigationManager.NavigateTo($"SalesOrder?SalesOrderUID={deliveredPreSales.SalesOrderUID}&SalesType={SalesType}");
        }
        private void OnViewClick(IDeliveredPreSales deliveredPreSales)
        {

            _navigationManager.NavigateTo($"ViewSalesOrdersDetails?SalesOrderUID={deliveredPreSales.SalesOrderUID}&PageName={PageName}");

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
           await GenerateGridColumnsForNormalStock();
        }
        private async Task GetDataLoadAsync()
        {
             await _manageSalesOrdersViewModel.PopulateViewModel(SelectedTab);
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _manageSalesOrdersViewModel.ApplySort(sortCriteria, SelectedTab);
            StateHasChanged();
            HideLoader();
        }
    }
}
