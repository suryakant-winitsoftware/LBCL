using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.BL.Interfaces
{
    public interface ITallyMasterViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public string UID {  get; set; }
        Task PageIndexChanged(int pageNumber, string uID, string TallyModule);
        Task PopulateChannelPartners();
        Task GetDealerMasterGridDataByDist(string uID);
        Task GetInventoryMasterGridDataByDist(string uID);
        Task GetSalesInvoiceMasterGridDataByDist(string uID);
        Task GetSalesInvoiceLineMasterGridDataByUID(string dmsuid);
        Task GetDealerMasterItemDetails(string UID);
        Task GetInventoryMasterItemDetails(string uID);
        Task GetSalesInvoiceMasterItemDetails(string UID);
        Task GetTallySalesInvoiceData(string UID);
        Task ApplyFilterForDealer(List<FilterCriteria> filterCriterias, string selectedDealer);
        Task ApplyFilterForInventory(List<FilterCriteria> filterCriterias, string selectedDealer);
        Task ApplyFilterForSalesInvoice(List<FilterCriteria> filterCriterias, string selectedDealer);
        Task<bool> InsertTallyMaster(List<ITallyDealerMaster> tallyDBDetails);
        Task ApplySort(SortCriteria sortCriteria, string uID, string TallyMpdule);
        Task PopulateGridDataForEXCEL(string uID, string tallyModule);

        public List<ISelectionItem> ChannelPartnerList { get; set; }
        public List<ITallyDealerMaster> DealerMasterDataList { get; set; }
        public List<ITallyDealerMaster> DealerMasterDataListForExcel { get; set; }
        public List<ITallyInventoryMaster> InventoryMasterDataList { get; set; }
        public List<ITallyInventoryMaster> InventoryMasterDataListForExcel { get; set; }
        public List<ITallySalesInvoiceMaster> SalesInvoiceMasterDataList { get; set;}
        public List<ITallySalesInvoiceMaster> SalesInvoiceMasterDataListForExcel { get; set; }
        public List<ITallySalesInvoiceLineMaster> SalesInvoiceLineMasterDataList {  get; set; } 
        public List<ITallySalesInvoiceResult> tallySalesInvoiceResults { get; set; }
        public ITallyDealerMaster tallyDealerMaster { get; set; }
        public ITallyInventoryMaster tallyInventoryMaster { get; set; }
        public ITallySalesInvoiceMaster tallySalesInvoiceMaster { get; set; }
        public ITallySalesInvoiceLineMaster tallySalesInvoiceLineMaster { get; set ; }
    }
}
