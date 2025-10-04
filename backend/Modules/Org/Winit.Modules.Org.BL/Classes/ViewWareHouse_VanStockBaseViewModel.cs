using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Identity.Client;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Constants;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Classes
{
    public abstract class ViewWareHouse_VanStockBaseViewModel : IViewWareHouse_VanStockViewModel
    {


        public int PageNumberSalableWarehouse_Van { get; set; } = 1;
        public int PageSizeSalableWarehouse_Van { get; set; }
        public int TotalItemsCountSalableWarehouse_Van { get; set; }
        public int PageNumberFocWarehouse_Van { get; set; } = 1;
        public int PageSizeFocWarehouse_Van { get; set; }
        public int TotalItemsCountFocWarehouse_Van { get; set; }

        public List<ISelectionItem> WareHouseSelectionItems { get; set; } = new List<ISelectionItem>();
        public List<ISelectionItem> VanSelectionItems { get; set; } = new List<ISelectionItem>();
        public List<IWarehouseItemView> WareHouseItemViewList = new List<IWarehouseItemView>();
        public List<IOrg> WareHouseItemViewListFrmORG { get; set; }
        public string selectedwarehouseUID { get; set; }
        public string FranchiseeOrgUID { get; set; }
        public List<IOrg> VanItemViewListFrmORG { get; set; }
        public string OrgTypeUID { get; set; }
        public List<IWarehouseStockItemView> Warehouse_VanSalablestockList { get; set; }
        public List<IWarehouseFOCStockItemView> Warehouse_VanFocstockList { get; set; }
        public IWarehouseItemView warehouseItemView { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<ISelectionItem> WareHouseOrganisationalSelectionItems { get; set; }
        public List<ISelectionItem> WareHouseStockSelectionItems { get; set; }
        public List<ISelectionItem> SubWareHouseSelectionItems { get; set; }
        public List<string> OrganisationalDDSelectedUIDs { get; set; }
        public string WareHouseDDSelectedItem { get; set; }
        public List<string> SubWareHouseDDSelectedUIDs { get; set; }
        public List<ISelectionItem> StarRatingDDL { get; set; }
        public List<ISelectionItem> TonnageDDL { get; set; }
        public List<ISelectionItem> ItemSeries { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<Winit.Modules.Org.Model.Interfaces.IWareHouseStock> MaintainWareHouseStockListForGrid { get; set; }
        public List<Winit.Modules.Org.Model.Interfaces.IWareHouseStock> MaintainWareHouseStockListForEXCEl { get; set; }

        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public ViewWareHouse_VanStockBaseViewModel(IServiceProvider serviceProvider,
               IFilterHelper filter,
               ISortHelper sorter,
               IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
             )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            Warehouse_VanSalablestockList = new List<IWarehouseStockItemView>();
            Warehouse_VanFocstockList = new List<IWarehouseFOCStockItemView>();
            FilterCriterias = new List<FilterCriteria>();
            WareHouseSelectionItems = new List<ISelectionItem>();
            SortCriterias = new List<SortCriteria>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
            WareHouseOrganisationalSelectionItems = new List<ISelectionItem>();
            WareHouseStockSelectionItems = new List<ISelectionItem>();
            SubWareHouseSelectionItems = new List<ISelectionItem>();
            OrganisationalDDSelectedUIDs = new List<string>();
            SubWareHouseDDSelectedUIDs = new List<string>();
            StarRatingDDL = new List<ISelectionItem>();
            TonnageDDL = new List<ISelectionItem>();
            ItemSeries = new List<ISelectionItem>();
            MaintainWareHouseStockListForGrid = new List<Model.Interfaces.IWareHouseStock>();
            MaintainWareHouseStockListForEXCEl = new List<Model.Interfaces.IWareHouseStock>();
        }
        /// <summary>
        /// This will seach data from TaxItemViews and store in FilteredTaxItemViews & DisplayedTaxItemViews
        /// </summary>
        /// <param name="filterCriterias"></param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        /// 

        public async Task ApplyFilterWarehouse(List<FilterCriteria> filterCriterias)
        {
            try
            {
                FilterCriterias.Clear();
                FilterCriterias.AddRange(filterCriterias);
                Warehouse_VanSalablestockList.Clear();
                Warehouse_VanSalablestockList.AddRange(await GetWarehouse_VanNormal_SaleableStockData());
                Warehouse_VanFocstockList.Clear();
                Warehouse_VanFocstockList.AddRange(await GetWarehouse_VanFocStockData());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task ApplyFilterVan(List<FilterCriteria> filterCriterias)
        {
            try
            {
                FilterCriterias.Clear();
                FilterCriterias.AddRange(filterCriterias);
                Warehouse_VanSalablestockList.Clear();
                Warehouse_VanSalablestockList.AddRange(await GetWarehouse_VanNormal_SaleableStockData());
                Warehouse_VanFocstockList.Clear();
                Warehouse_VanFocstockList.AddRange(await GetWarehouse_VanFocStockData());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task ResetFilter()
        {
            FilterCriterias.Clear();
        }
        public async Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias)
        {

        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModelForSalableWareHouse();
            // await PopulateViewModelForWareHouseFocTab();
            await PopulateViewModelForSalableVan();
        }

        //public async Task PageIndexChanged_Van(int pageNumber)
        //{
        //    PageNumber = pageNumber;
        //    await PopulateViewModelForVan();
        //}
        public async virtual Task PopulateViewModelForSalableWareHouse()
        {
            Warehouse_VanSalablestockList = await GetWarehouse_VanNormal_SaleableStockData();
        }
        public async virtual Task PopulateViewModelForFocWareHouse()
        {
            Warehouse_VanFocstockList = await GetWarehouse_VanFocStockData();
        }
        public async virtual Task PopulateViewModelForSalableVan()
        {
            Warehouse_VanSalablestockList = await GetWarehouse_VanNormal_SaleableStockData();
        }
        public async virtual Task PopulateViewModelForFocVan()
        {
            Warehouse_VanFocstockList = await GetWarehouse_VanFocStockData();
        }




        public async Task ApplyWarehouseStockSort(SortCriteria sortCriteria, List<string> orgUIDs, string Warehouse, List<string> subWarehouseUids)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            OrganisationalDDSelectedUIDs = orgUIDs;
            WareHouseDDSelectedItem = Warehouse;
            SubWareHouseDDSelectedUIDs = subWarehouseUids;
            await PopulateGridDataForMaintainWareHouseStock();
        }

        public async Task WarehouseStockPageIndexChanged(int pageNumber, List<string> orgUIDs, string Warehouse, List<string> subWarehouseUids)
        {
            PageNumber = pageNumber;
            OrganisationalDDSelectedUIDs = orgUIDs;
            WareHouseDDSelectedItem = Warehouse ;
            SubWareHouseDDSelectedUIDs = subWarehouseUids;
            await PopulateGridDataForMaintainWareHouseStock();
        }
        public async virtual Task PopulateViewModelForWareHouseDD()
        {
            await PopulateDDDataForMaintainWearHouseStock();
            WareHouseItemViewListFrmORG = await GetWarehouseDropdownDataDD(OrgTypeUID);
            if (WareHouseItemViewListFrmORG != null && WareHouseItemViewListFrmORG.Any())
            {
                WareHouseSelectionItems.Clear();
                WareHouseSelectionItems.AddRange(ConvertOrgToSelectionItem(WareHouseItemViewListFrmORG));
            }
        }
        public async virtual Task PopulateViewModelForSubWareHouseDD(string sWH, string wareHouseDDSelectedItem, string branchUID)
        {
            SubWareHouseSelectionItems.Clear();
            var subWareHouseitems = await GetSubWarehouseDropdownDataDD(sWH, wareHouseDDSelectedItem, branchUID);
            SubWareHouseSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems(subWareHouseitems, e => e.UID, e => e.Code, e => e.Name));

        }

        private async Task PopulateDDDataForMaintainWearHouseStock()
        {
            WareHouseOrganisationalSelectionItems.Clear();
            var organisationalItems = await GetWarehouseDropdownDataDD(OrgTypeConst.OU);
            WareHouseOrganisationalSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems(organisationalItems, e => e.UID, e => e.Code, e => e.Name));

            WareHouseStockSelectionItems.Clear();
            var wareHouseItems = await GetWarehouseDropdownDataDD(OrgTypeConst.WH);
            WareHouseStockSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems(wareHouseItems, e => e.UID, e => e.Code, e => e.Name));

            StarRatingDDL.Clear();
            var starRatingItems = await GetWarehouseFilterDropdownDataDD(SKUGroupTypeContants.StarRating);
            StarRatingDDL.AddRange(CommonFunctions.ConvertToSelectionItems(starRatingItems, e => e.UID, e => e.Code, e => e.Name));

            TonnageDDL.Clear();
            var tonnageItems = await GetWarehouseFilterDropdownDataDD(SKUGroupTypeContants.TONAGE);
            TonnageDDL.AddRange(CommonFunctions.ConvertToSelectionItems(tonnageItems, e => e.UID, e => e.Code, e => e.Name));

            ItemSeries.Clear();
            var itemSeriesItems = await GetWarehouseFilterDropdownDataDD(SKUGroupTypeContants.Item_Series);
            ItemSeries.AddRange(CommonFunctions.ConvertToSelectionItems(itemSeriesItems, e => e.UID, e => e.Code, e => e.Name));
        }

       

        public async virtual Task PopulateGridDataForMaintainWareHouseStock()
        {
            MaintainWareHouseStockListForGrid = await GridDataForMaintainWareHouseStock(OrganisationalDDSelectedUIDs, WareHouseDDSelectedItem, SubWareHouseDDSelectedUIDs);
        }
        public async virtual Task ApplyFilterForWareHouseStock(List<FilterCriteria> filterCriterias)
        {
            FilterCriterias.Clear();
            FilterCriterias.AddRange(filterCriterias);
            await PopulateGridDataForMaintainWareHouseStock();
        }
        public async virtual Task PopulateGridDataForEXCEL()
        {
            MaintainWareHouseStockListForEXCEl = await GridDataForMaintainWareHouseStock(OrganisationalDDSelectedUIDs, WareHouseDDSelectedItem, SubWareHouseDDSelectedUIDs);
        }
        public async virtual Task PopulateViewModelForVanDD()
        {
            VanItemViewListFrmORG = await GetVanDropdownDataDD(OrgTypeUID);
            if (VanItemViewListFrmORG != null && VanItemViewListFrmORG.Any())
            {
                VanSelectionItems.Clear();
                VanSelectionItems.AddRange(ConvertOrgToSelectionItem(VanItemViewListFrmORG));
            }
        }
        private List<ISelectionItem> ConvertOrgToSelectionItem(IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg> warehouseItemView)
        {
            List<ISelectionItem> selectionItems = new List<ISelectionItem>();
            foreach (var org in warehouseItemView)
            {
                SelectionItem si = new SelectionItem();
                // si.Code = org.Code;
                si.Label = org.Name;
                si.UID = org.UID;
                selectionItems.Add(si);
            }
            return selectionItems;
        }
        //public async virtual Task PopulateViewModelForWareHouseFocTab()
        //{
        //    Warehouse_VanFocstockList = await GetWarehouse_VanFocStockData();
        //}

        #region Business Logic    
        #endregion
        #region Database or Services Methods
        public abstract Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetWarehouse_VanNormal_SaleableStockData();
        public abstract Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseFOCStockItemView>> GetWarehouse_VanFocStockData();
        public abstract Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetWarehouseDropdownDataDD(string OrgTypeUID);
        public abstract Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetSubWarehouseDropdownDataDD(string OrgTypeUID, string wareHouseDDSelectedItem, string branchUID);

        public abstract Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetVanDropdownDataDD(string OrgTypeUID);
        public abstract Task<List<Winit.Modules.Org.Model.Interfaces.IWareHouseStock>> GridDataForMaintainWareHouseStock(List<string> organisationalDDSelectedItems, string wareHouseDDSelectedItem, List<string> subWareHouseDDSelectedItems);
        public abstract Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> GetWarehouseFilterDropdownDataDD(string starRating);


        #endregion
    }
}
