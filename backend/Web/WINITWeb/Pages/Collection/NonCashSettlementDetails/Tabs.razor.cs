using DocumentFormat.OpenXml.Spreadsheet;
//using Irony.Parsing;
using Microsoft.AspNetCore.Components;
using Nest;
using Practice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Web.SalesManagement.PriceManagement;
using WinIt.Pages.Collection.CashierSettlementDetails;
using static Winit.Modules.Base.BL.ApiService;
using WinIt.Pages.Base;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Shared.Models.Enums;
using System.Globalization;
using System.Resources;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Collection.NonCashSettlementDetails
{
    public partial class Tabs : BaseComponentBase
    {
        List<ISKU> products;
        List<IStore> customers;
        private int _currentPage = 1;
        [Parameter]
        public EventCallback<int> OnStringChanged { get; set; }
        [Parameter] public List<string> tabItems { get; set; }
        [Parameter] public List<string> tabItems1 { get; set; }
        [Parameter] public List<AccElement> pendingDS { get; set; }
        [Parameter] public List<AccCollectionPaymentMode> pendingDataSource { get; set; }
        [Parameter] public List<AccElement> cashierApprovedDS { get; set; }
        [Parameter] public List<AccCollectionPaymentMode> cashierApprovedDataSource { get; set; }
        [Parameter] public List<AccCollectionPaymentMode> bankApprovedDataSource { get; set; }
        [Parameter] public List<AccElement> voidDS { get; set; }
        [Parameter] public List<AccElement> ReversalDS { get; set; }
        [Parameter] public List<AccCollectionPaymentMode> rejectedDataSource { get; set; }
        [Parameter] public List<AccCollectionPaymentMode> reversalDataSource { get; set; }
        [Parameter] public List<AccCollectionPaymentMode> bouncedDataSource { get; set; }
        [Parameter] public List<AccCollectionPaymentMode> commonDataSource { get; set; }
        [Parameter] public List<AccCollectionPaymentMode> emptyDataSource { get; set; }
        [Parameter] public List<AccElement> emptyDS { get; set; }
        [Parameter] public List<DataGridColumn> pendingColumns { get; set; }
        [Parameter] public List<DataGridColumn> _cashierApprovedColumns { get; set; }
        [Parameter] public List<DataGridColumn> _bankApprovedColumns { get; set; }
        [Parameter] public List<DataGridColumn> _rejectedColumns { get; set; }
        [Parameter] public List<DataGridColumn> _bouncedColumns { get; set; }
        [Parameter] public List<DataGridColumn> reversalColumns { get; set; }
        [Parameter] public int Count { get; set; } = 0;
        private List<DataGridColumn> commonColumns { get; set; }
        private List<DataGridColumn> emptyColumn { get; set; }
        [Parameter] public Cashier _cashier { get; set; }
        [Parameter] public Settlement _settlement { get; set; }
        //    private List<TabItem> tabItems = new List<TabItem>
        //{
        //    new TabItem { Name = "Tab 1" },
        //    new TabItem { Name = "Tab 2" },
        //    // Add more tabs as needed
        //};

        //    private TabItem selectedTab;

        //    private void OnTabSelected(TabItem tab)
        //    {
        //        // Handle tab selection, load related tables, etc.
        //        selectedTab = tab;
        //    }

        protected override async Task OnInitializedAsync()
        {
            products = GetSKUs();
            customers = GetStores();
            LoadResources(null, _languageService.SelectedCulture);
        }
       
        private async void Product_OnPageChange(int pageNumber)
        {
            try
            {
                _currentPage = pageNumber;
                await OnStringChanged.InvokeAsync(_currentPage);
            }
            catch (Exception ex)
            {

            }
        }
        List<DataGridColumn> productColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header = "SKU Code", GetValue = s => ((SKU)s).Code, IsSortable = false, SortField = "Code" },
            new DataGridColumn { Header = "SKU Code1", GetValue = s => ((SKU)s).Code, IsSortable = false, SortField = "Code" },
            new DataGridColumn { Header = "SKU Name", GetValue = s => ((SKU)s).Name, IsSortable = true, SortField = "Name" },
            new DataGridColumn { Header = "Is Active", GetValue = s => ((SKU)s).IsActive },

        };
        public List<IStore> GetStores()
        {
            List<IStore> stores = new List<IStore>();
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store1", Name = "Store Name1" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store2", Name = "Store Name2" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store3", Name = "Store Name3" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store4", Name = "Store Name4" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store5", Name = "Store Name5" });
            stores.Add(new Winit.Modules.Store.Model.Classes.Store { Code = "Store6", Name = "Store Name6" });
            return stores;
        }
        public List<ISKU> GetSKUs()
        {
            List<ISKU> skus = new List<ISKU>();
            skus.Add(new SKU { Code = "SKU1", Name = "SKU Name1" });
            skus.Add(new SKU { Code = "SKU2", Name = "SKU Name2" });
            skus.Add(new SKU { Code = "SKU3", Name = "SKU Name3" });
            skus.Add(new SKU { Code = "SKU4", Name = "SKU Name4" });
            skus.Add(new SKU { Code = "SKU5", Name = "SKU Name5" });
            skus.Add(new SKU { Code = "SKU6", Name = "SKU Name6" });
            return skus;
        }
        List<DataGridColumn> customerColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header = "Store Code", GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).Code, IsSortable = true, SortField = "Code" },
            new DataGridColumn { Header = "Store Name", GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).Name, IsSortable = true, SortField = "Name" },
            new DataGridColumn { Header = "Is Active", GetValue = s => ((Winit.Modules.Store.Model.Classes.Store)s).IsActive },

        };
        private List<AccCollectionPaymentMode> GetDataSourceForTab(string tab)
        {
            //Token._tabName = pendingDataSource.Count > 0 ? "Pending" : cashierApprovedDataSource.Count > 0 ? "Cashier Approved" : bankApprovedDataSource.Count > 0 ? "Bank Approved" : rejectedDataSource.Count > 0 ? "Rejected" : bouncedDataSource.Count > 0 ? "Bounced" : "empty";
            return tab == "Pending" ? pendingDataSource : tab == "Cashier Approved" ? cashierApprovedDataSource : tab == "Bank Approved" ? bankApprovedDataSource : tab == "Rejected" ? rejectedDataSource : tab == "Bounced" ? bouncedDataSource : tab == "Reversal" ? reversalDataSource :  emptyDataSource;
        }
        private List<AccElement> GetDataSourceForTab1(string tab)
        {
            //Token._tabName = pendingDS.Count > 0 ? "Pending" : cashierApprovedDS.Count > 0 ? "Settled" : voidDS.Count > 0 ? "Void" : "empty";
            return tab == "Pendings" ? pendingDS : tab == "Settled" ? cashierApprovedDS : tab == "Void" ? voidDS : tab == "Reversal" ? ReversalDS : emptyDS;
        }
        private List<DataGridColumn> GetColumnsForTab(string tab)
        {
            return (tab == "Pending" || tab == "Pendings") ? pendingColumns : (tab == "Cashier Approved" || tab == "Settled") ? _cashierApprovedColumns : (tab == "Bank Approved" || tab == "Void") ? _bankApprovedColumns : tab == "Rejected" ? _rejectedColumns : tab == "Bounced" ? _bouncedColumns : tab == "Reversal" ? reversalColumns : emptyColumn;
        }
        private async void Product_AfterCheckBoxSelection(HashSet<object> hashSet)
        {
            //_cashier.Product_AfterCheckBoxSelection(hashSet);
        }
        public async Task GetInformation()
        {
            await _cashier.GetInformation();
        }
        private async void Product_OnSort(SortCriteria sortCriteria)
        {
            //await _settlement.Product_OnSort(sortCriteria);
        }
    }
}
