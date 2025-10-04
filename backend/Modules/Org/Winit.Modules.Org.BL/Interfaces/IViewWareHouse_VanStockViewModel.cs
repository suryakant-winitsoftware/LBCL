using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Interfaces
{
    public interface IViewWareHouse_VanStockViewModel
    {

        public List<IWarehouseStockItemView> Warehouse_VanSalablestockList { get; set; }
        public List<IWarehouseFOCStockItemView> Warehouse_VanFocstockList { get; set; }
        public List<ISelectionItem> WareHouseSelectionItems { get; set; }
        public List<ISelectionItem> VanSelectionItems { get; set; }
        public List<IOrg> WareHouseItemViewListFrmORG { get; set; }
        public List<IOrg> VanItemViewListFrmORG { get; set; }
        public IWarehouseItemView warehouseItemView { get; set; }
        public string OrgTypeUID { get; set; }
        public string selectedwarehouseUID { get; set; }
        public string FranchiseeOrgUID { get; set; }

        public int PageNumberSalableWarehouse_Van { get; set; } 
        public int PageSizeSalableWarehouse_Van { get; set; }
        public int TotalItemsCountSalableWarehouse_Van { get; set; }
        public int PageNumberFocWarehouse_Van { get; set; } 
        public int PageSizeFocWarehouse_Van { get; set; }
        public int TotalItemsCountFocWarehouse_Van { get; set; }
       
        /// <summary>
        /// Apply Filter
        /// </summary>
        /// <param name="filterCriterias"></param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        Task ApplyFilterWarehouse(List<FilterCriteria> filterCriterias);
        Task ApplyFilterVan(List<FilterCriteria> filterCriterias);
        /// <summary>
        /// Apply Search
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        Task ResetFilter();
        Task ApplySort(SortCriteria sortCriteria);
        
        Task PopulateViewModelForVanDD();
        Task PopulateViewModelForSalableWareHouse();
        Task PopulateViewModelForFocWareHouse();

        //Task PopulateViewModelForWareHouseFocTab();
        Task PopulateViewModelForSalableVan();
        Task PopulateViewModelForFocVan();

        //Task PageIndexChanged(int pageNumber);
        //Task PageIndexChanged_Van(int pageNumber);




        ///WareHouseStock Done By Ravichandra <summary>
        /// WareHouseStock Done By Ravichandra
        /// </summary>
        /// 
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount {  get; set; }
        Task PopulateViewModelForWareHouseDD();
        Task PopulateViewModelForSubWareHouseDD(string sWH, string wareHouseDDSelectedItem, string branchUID);
        public List<ISelectionItem> WareHouseOrganisationalSelectionItems { get; set; }
        public List<ISelectionItem> WareHouseStockSelectionItems { get; set; }
        public List<ISelectionItem> SubWareHouseSelectionItems { get; set; }
        public List<string> OrganisationalDDSelectedUIDs {  get; set; }
        public string WareHouseDDSelectedItem {  get; set; }
        public List<string> SubWareHouseDDSelectedUIDs { get; set; }
        public List<Winit.Modules.Org.Model.Interfaces.IWareHouseStock> MaintainWareHouseStockListForGrid {  get; set; }
        public List<Winit.Modules.Org.Model.Interfaces.IWareHouseStock> MaintainWareHouseStockListForEXCEl { get; set; }
        public List<ISelectionItem> StarRatingDDL { get; set; }
        public List<ISelectionItem> TonnageDDL { get; set; }
        public List <ISelectionItem> ItemSeries { get; set; }
        Task PopulateGridDataForMaintainWareHouseStock();
        Task ApplyWarehouseStockSort(SortCriteria sortCriteria, List<string> orgUIDs, string Warehouse, List<string> subWarehouseUids);
        Task WarehouseStockPageIndexChanged(int pageNumber, List<string> orgUIDs, string Warehouse, List<string> subWarehouseUids);
        Task PopulateGridDataForEXCEL();
        Task ApplyFilterForWareHouseStock(List<FilterCriteria> filterCriterias);
    }
}
