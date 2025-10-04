using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Winit.Modules.Common.BL;
using Winit.Modules.SalesOrder.BL.UIClasses;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using WINITMobile.Pages.Base;
//@inject Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderViewModel _salesOrderViewModel;

namespace WINITMobile.Pages.Demo
{
    public partial class SalesTest : BaseComponentBase
    {
        protected override async Task OnInitializedAsync()
        {
            string s = Configuration["CustomSetting"];
            string s1 = Configuration["AppSettings:ApiBaseUrl"];
            var s2 = Configuration.GetSection("ApiUrls");
            _loadingService.ShowLoading("hiii");
            await Task.Delay(2000); // Simulate a delay
            _loadingService.HideLoading();
            Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel1 = CreateDummyStoreData();
            base._dataManager.SetData("SelectedreturnViewModel", storeViewModel1);
            Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel = (Winit.Modules.Store.Model.Interfaces.IStoreItemView)base._dataManager.GetData("SelectedreturnViewModel");
            if (storeViewModel == null)
            {
                throw new Exception("storeViewModel is null");
            }
            //_returnOrderViewModel.PopulateViewModel(storeViewModel, true, "");
            //_returnOrderViewModel.SaveOrder();

        }
        private Winit.Modules.Store.Model.Interfaces.IStoreItemView CreateDummyStoreData()
        {
            // Later this will be coming from calling activity
            Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel = _serviceProvider.CreateInstance<Winit.Modules.Store.Model.Interfaces.IStoreItemView>();
            storeViewModel.UID = "Store1";
            storeViewModel.Code = "Code1";
            storeViewModel.StoreNumber = "1";
            storeViewModel.Name = "Store 1";
            storeViewModel.Latitude = "0";
            storeViewModel.Longitude = "0";
            storeViewModel.Address = "Address1";
            storeViewModel.IsPlanned = false;
            storeViewModel.IsStopDelivery = false;
            storeViewModel.IsActive = true;
            storeViewModel.IsBlocked = false;
            storeViewModel.BlockedReasonDescription = string.Empty;
            storeViewModel.IsAwayPeriod = false;
            storeViewModel.IsPromotionsBlock = true;
            storeViewModel.SelectedOrgUID = "WINIT";
            storeViewModel.IsTaxApplicable = true;
            storeViewModel.TaxDocNumber = string.Empty;
            storeViewModel.TotalCreditLimit = 100000;
            storeViewModel.UsedCreditLimit = 10000;
            return storeViewModel;
        }
        private async void TestSalesOrderViewModel()
        {
            Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderViewModel vk = _salesOrderViewModel;
            Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView vk1 = _salesOrderItemView;
            Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView vk3 = _serviceProvider.CreateInstance<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView>();
            vk3.UsedUOMCodes = new HashSet<string> { "PACK", "CASE" ,"PCS"};
            Winit.Modules.SKU.Model.UIInterfaces.ISKUUOMView newUOM = new Winit.Modules.SKU.Model.UIClasses.SKUUOMView();
            newUOM.SKUUID = "SKU1";
            newUOM.Code = "SKU1Code";
            newUOM.Name = "SKU1Name";

            // Filter Test
            for (int i = 1; i <= 10; i++)
            {
                int currentInex = i;
                Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView = _serviceProvider.CreateInstance<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView>();
                if (i == 5)
                {
                    salesOrderItemView.SKUName = "SKUName" + (currentInex - 1);
                }
                else
                {
                    salesOrderItemView.SKUName = "SKUName" + currentInex;

                }
                salesOrderItemView.UID = "SKU" + currentInex;
                salesOrderItemView.SKUCode = "SKUCode" + currentInex;
                //UOM
                for (int j = 1; j <= 2; j++)
                {
                    Winit.Modules.SKU.Model.UIInterfaces.ISKUUOMView uom = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.UIInterfaces.ISKUUOMView>();
                    if (uom != null)
                    {
                        uom.Code = "UOMCode_" + salesOrderItemView.SKUCode + "_" + j;
                        uom.Name = "UOMName_" + salesOrderItemView.SKUCode + "_" + j;
                        salesOrderItemView.AllowedUOMs.Add(uom);
                    }
                }

                //Attributes
                for (int j = 1; j <= 2; j++)
                {
                    string key = "Brand";
                    if (j == 1)
                    {
                        key = "Brand";
                    }
                    else if (j == 2)
                    {
                        key = "Category";
                    }
                    Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView attribute = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView>();
                    if (attribute != null)
                    {
                        attribute.Code = "AttrCode_" + salesOrderItemView.SKUCode + "_" + j;
                        attribute.Name = key;
                        salesOrderItemView.Attributes.Add(key, attribute);
                    }
                }
                _salesOrderViewModel.SalesOrderItemViews.Add(salesOrderItemView);
            }

            var salesOrderFilter = new SalesOrderFilter();
            List<FilterCriteria> filterCriteriaList = new List<FilterCriteria>();
            filterCriteriaList.Add(new FilterCriteria("SKUCode", "Code2", FilterType.Like));
            filterCriteriaList.Add(new FilterCriteria("SKUCode", "3", FilterType.Like));
            // Or
            _salesOrderViewModel.FilteredSalesOrderItemViews = await salesOrderFilter
                .ApplyFilter<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView>(
                _salesOrderViewModel.SalesOrderItemViews, filterCriteriaList, Winit.Shared.Models.Enums.FilterMode.Or);
            // And
            _salesOrderViewModel.FilteredSalesOrderItemViews = await salesOrderFilter
                .ApplyFilter<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView>(_salesOrderViewModel.SalesOrderItemViews,
                filterCriteriaList, Winit.Shared.Models.Enums.FilterMode.And);

            // List searching From Dictionary
            var filterCriteriaBrandTemp = new List<(Func<Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView, string> fieldSelector, string fieldSelectorValue)>();


            // First set of data
            filterCriteriaBrandTemp.Add((value => value.Name, "Brand"));
            filterCriteriaBrandTemp.Add((value => value.Code, "AttrCode_SKUCode1_1"));

            // Second set of data (similar structure)
            filterCriteriaBrandTemp.Add((value => value.Name, "Brand"));
            filterCriteriaBrandTemp.Add((value => value.Code, "AttrCode_SKUCode1_2"));

            var filterCriteriaBrand = new List<(Func<Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView, string> fieldSelector, string fieldSelectorValue)>
            {
                (value => value.Name, "Brand"),
                (value => value.Code, "AttrCode_SKUCode1_1")
            };
            var filteredByAttributeWithoutKey = salesOrderFilter.FilterFromDictionary<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView, Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView>
                (
                    _salesOrderViewModel.SalesOrderItemViews,
                    item => item.Attributes,
                    filterCriteriaBrand,
                    null
                );

            var filterCriteriaCategory = new List<(Func<Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView, string> fieldSelector, string fieldSelectorValue)>
            {
                (value => value.Name, "Category"),
                (value => value.Code, "AttrCode_SKUCode1_2")
            };
            var filteredByAttributeWithoutKeyCategory = salesOrderFilter.FilterFromDictionary<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView, Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView>
                (
                    _salesOrderViewModel.SalesOrderItemViews,
                    item => item.Attributes,
                    filterCriteriaCategory,
                    null
                );

            // List searching From List with multiple criteria
            var filterCriteriaUOM = new List<(Func<Winit.Modules.SKU.Model.UIInterfaces.ISKUUOMView, string> fieldSelector, string fieldSelectorValue)>
            {
                (value => value.Name, "UOMName_SKUCode1_1"),
                (value => value.Code, "UOMCode_SKUCode2_2"),
            };
            var filteredByUOM1 = salesOrderFilter.FilterFromList<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView, Winit.Modules.SKU.Model.UIInterfaces.ISKUUOMView>
                (
                    _salesOrderViewModel.SalesOrderItemViews,
                    item => item.AllowedUOMs,
                    filterCriteriaUOM
                );

            // Search Test
            List<string> propertiesToSearch = new List<string>();
            propertiesToSearch.Add("SKUCode");
            propertiesToSearch.Add("SKUName");

            string searchString = "Name1";
            _salesOrderViewModel.DisplayedSalesOrderItemViews = await salesOrderFilter
                .ApplySearch<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView>(
                _salesOrderViewModel.SalesOrderItemViews,
                searchString, propertiesToSearch);

            // Sorting
            List<SortCriteria> sortCriteriaList = new List<SortCriteria>();
            sortCriteriaList.Add(new SortCriteria("SKUName", SortDirection.Desc));
            sortCriteriaList.Add(new SortCriteria("SKUCode", SortDirection.Desc));

            var salesOrderSortHelper = new SalesOrderSort();
            _salesOrderViewModel.DisplayedSalesOrderItemViews = await salesOrderSortHelper
                .Sort<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView>(
                _salesOrderViewModel.SalesOrderItemViews,
                sortCriteriaList);

            // Clone Testing
            Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView vk4 = vk3.Clone(newUOM, Winit.Shared.Models.Enums.ItemState.Cloned, "vk");
            vk4.UsedUOMCodes.Add("Box");
            Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView vk5 = vk3.Clone(newUOM, Winit.Shared.Models.Enums.ItemState.FOC, "vk1");
        }
        private void TestExtension()
        {
            //_salesOrderViewModel.ConvertToISalesOrderLine(_salesOrderViewModel);
        }
        private async void ReadConfiguration()
        {
            string restSvcApiUrl = Configuration["RestSvc:ApiUrl"];
            string vk = Configuration["AppSettings:ApiBaseUrl"];
        }
        private async void DownloadSqlite()
        {
            // Get the base directory of your application
            string appBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string folderPath = Path.Combine(appBaseDirectory, @"Data\DB");
            string fileName = "WINITSQLite.db";


            //string sqlitePath = await _commonFunctions.DownloadFileAsync("https://qa-fonterra.winitsoftware.com/MobileServices/data/vishaltest/WINITSQLite.zip", folderPath, fileName);
            string sqlitePath = await _commonFunctions.DownloadAndExtractZipAsync("https://qa-fonterra.winitsoftware.com/MobileServices/data/vishaltest/WINITSQLite.zip", folderPath, fileName);
            string connectionString = $"Data Source={sqlitePath};";
            await _alertService.ShowSuccessAlert("Download","SqlLite Successfully Downloaded");
        }
        private async void LoadSettingMaster()
        {
            var data = await _settingBL.SelectAllSettingDetails(null, 1, 10000, null, false);
            _appSetting.PopulateSettings(data.PagedData); // Call only once at the time of successful login
        }
        private void ReadSettingData()
        {
            int RoundOffDecimal = _appSetting.RoundOffDecimal;
        }
        private async void ReadDataFromSqlite()
        {
            //IStore store = await _storeBL.SelectStoreByUID("0078545a-b806-4a7b-a10e-5bb3d4df0d38");
            //PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore> pagedResponse = await _storeBL.SelectAllStore(null, 1, 100, null, true);
        }
        private async void ReadSKUPriceDataDataFromSqlite()
        {
            //PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore> pagedResponse = await _storeBL.SelectAllStore(null, 1, 100, null, true);
            var data = await _SKUPriceBL.SelectAllSKUPriceDetails(null, 1, 100, null, true);
        }
        private void NavigateToPage1()
        {
            _navigationManager.NavigateTo("page1", true);
        }


    }
}
