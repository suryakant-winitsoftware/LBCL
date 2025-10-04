using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using NPOI.OpenXmlFormats.Spreadsheet;
using OfficeOpenXml.Drawing.Slicer.Style;
using System.Linq.Expressions;
using Winit.Modules.Common.BL;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Tax.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
namespace WinIt.Pages.ViewSalesOrders
{
    public partial class ViewSalesOrdersDetails
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        [Parameter]
        public bool IsViewPage { get; set; }
        public bool IsIntialized { get; set; }
        [Parameter]
        public string? SalesOrderUID { get; set; }
        public string ReferenceUID { get; set; }
        public string PageNameForForwardLink = "ViewsalesOrder";
        public string PageName { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Sales Order Details",
            BreadcrumList = new List<IBreadCrum>()
      {
        
      }
        };
        public string Status { get; set; }

        private DateTime selectedDate = DateTime.Now;
        protected override async Task OnInitializedAsync()
        {
           
            LoadResources(null, _languageService.SelectedCulture);
            var uri = new Uri(_navigationManager.Uri);
            var query = uri.Query;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                PageName = queryParams.Get("PageName");
                Status = queryParams.Get("Status");
            }
            //await SetHeaderName();
            await BreadCrum();
            SalesOrderUID = _commonFunctions.GetParameterValueFromURL("SalesOrderUID");
            if (SalesOrderUID != null)
            {
                IsViewPage = true;
                await _manageSalesOrdersViewModel.PopulateManageSalesOrdersDataforView(SalesOrderUID);
                await _manageSalesOrdersViewModel.SetEditForviewpresales(_manageSalesOrdersViewModel.SKUViewPreSalesList);
                _manageSalesOrdersViewModel.OrgUID = "FR001";
                await _manageSalesOrdersViewModel.PopulateViewModel();
            }
           
            StateHasChanged();
           // await GenerateGridColumns();

            IsIntialized = true;
        }
        //private async Task GenerateGridColumns()
        //{
        //    DataGridColumns= new List<DataGridColumn>
        //    {
        //        new DataGridColumn { Header = @Localizer["sku_code"], GetValue = s => ((ISKUViewPreSales)s)?.SKUCode ?? "N/A" },
        //        new DataGridColumn { Header = @Localizer["sku_name"], GetValue = s => ((ISKUViewPreSales)s)?.SKUName?? "N/A" },
        //        new DataGridColumn { Header = @Localizer["item_type"], GetValue = s => ((ISKUViewPreSales)s)?.ItemType ?? "N/A"},
        //        new DataGridColumn { Header = @Localizer["uom"], GetValue = s => ((ISKUViewPreSales)s)?.UoM ?? "N/A" },
        //        new DataGridColumn {Header = @Localizer["req_qty"], GetValue = s =>((ISKUViewPreSales) s) ?.RecoQty ?? 0},
        //        new DataGridColumn { Header = @Localizer["app_qty"], GetValue = s => ((ISKUViewPreSales)s)?.ApprovedQty ?? 0},
        //        new DataGridColumn {Header = @Localizer["price"], GetValue = s =>((ISKUViewPreSales) s) ?.Price ?? 0},
        //        new DataGridColumn {Header = @Localizer["discount"], GetValue = s =>((ISKUViewPreSales) s) ?.Discount ?? 0},
        //        new DataGridColumn {Header = @Localizer["amount_exc_tax"], GetValue = s =>((ISKUViewPreSales) s) ?.AmountExcTax ?? 0},
        //        new DataGridColumn {Header = @Localizer["tax"], GetValue = s =>((ISKUViewPreSales) s) ?.Tax ?? 0},
        //        new DataGridColumn {Header = @Localizer["amount_inc_tax"], GetValue = s =>((ISKUViewPreSales) s) ?.AmountIncTax ?? 0}

        //    };
        //}
        //public async Task SetHeaderName()
        //{
        //    // string pageText = PageName == "ManageUnallocatedOrders" ? "Manage Unallocated Orders" : "View Sales Orders";

        //    _IDataService.BreadcrumList = new();
        //    if (PageName == "ViewsalesOrder")
        //    {
        //        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_sales_orders"], IsClickable = true, URL = PageName });
        //        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["view_sales_orders_details"], IsClickable = false });
        //        _IDataService.HeaderText = @Localizer["view_sales_orders_details"];
        //        await CallbackService.InvokeAsync(_IDataService);
        //    }
        //    else if (PageName == "ManageUnallocatedOrders")
        //    {
        //        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_unallocated_orders"], IsClickable = true, URL = PageName });
        //        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["view_sales_orders_details"], IsClickable = false });
        //        _IDataService.HeaderText = @Localizer["view_sales_orders_details"];
        //        await CallbackService.InvokeAsync(_IDataService);
        //    }
        //    else if (PageName == "ManagePresalesOrders")
        //    {
        //        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_presales_orders"], IsClickable = true, URL = PageName });
        //        _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["view_presales_order"], IsClickable = false });
        //        _IDataService.HeaderText = @Localizer["view_presales_order"];
        //        await CallbackService.InvokeAsync(_IDataService);
        //    }
        //}
        public async Task BreadCrum()
        {
            if (PageName == "ViewsalesOrder")
            {
                dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = "View Sales Orders", URL = PageName, IsClickable = true });
                dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = "View Sales Order" });
            }
            else if (PageName == "ManageUnallocatedOrders")
            {
                dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = "Manage Unallocated Order", URL = PageName, IsClickable = true });
                dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = "View Sales Order" });
            }
            else if (PageName == "ManagePresalesOrders")
            {
                dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = "Manage PreSales Order", URL = PageName, IsClickable = true });
                dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = "View Sales Order" });
            }
        }
        private async Task BackBtnClicked()
        {
            string pageUrl = "";

            if (PageName == "ManageUnallocatedOrders")
            {
                pageUrl = "ManageUnallocatedOrders";
            }
            else if (PageName == "ManagePresalesOrders")
            {
                pageUrl = "ManagePresalesOrders";
            }
            else if (PageName == "ViewsalesOrder")
            {
                pageUrl = "ViewsalesOrder";
            }
            else
            {
                pageUrl = "ViewsalesOrder";
            }
            _navigationManager.NavigateTo(pageUrl);
        }

        private async Task HandleRouteSelection(DropDownEvent eventArgs)
        {
            if (eventArgs != null && eventArgs.SelectionItems != null && eventArgs.SelectionItems.Count > 0)
            {
                var item = eventArgs.SelectionItems.FirstOrDefault();
                _manageSalesOrdersViewModel.OrgUID = item.UID;
                //await _manageSalesOrdersViewModel.PopulateViewModel();
            }
        }
        private async Task OnForwardOrderClick(IViewPreSales viewPreSales)
        {
            await _manageSalesOrdersViewModel.PopulateManageSalesOrdersDataforView(viewPreSales?.ReferenceUID);
            _navigationManager.NavigateTo($"ViewSalesOrdersDetails?SalesOrderUID={viewPreSales.ReferenceUID}&PageName={PageNameForForwardLink}",forceLoad:true);
            
        }
    }
}
