using Microsoft.AspNetCore.Components.Web.Virtualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Tally.BL.Interfaces;
using Winit.Modules.Tally.Model.Classes;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.BL.Classes
{
    public abstract class TallyMasterBaseViewModel : ITallyMasterViewModel
    {
        public List<ISelectionItem> ChannelPartnerList { get; set; }
        public List<ITallyDealerMaster> DealerMasterDataList { get; set; }
        public List<ITallyInventoryMaster> InventoryMasterDataList { get; set; }
        public List<ITallySalesInvoiceMaster> SalesInvoiceMasterDataList { get; set; }
        public List<ITallySalesInvoiceLineMaster> SalesInvoiceLineMasterDataList { get; set; }
        public List<ITallySalesInvoiceResult> tallySalesInvoiceResults { get; set; }
        public ITallyDealerMaster tallyDealerMaster { get; set; }
        public ITallyInventoryMaster tallyInventoryMaster { get; set; }
        public ITallySalesInvoiceMaster tallySalesInvoiceMaster { get; set; }
        public ITallySalesInvoiceLineMaster tallySalesInvoiceLineMaster { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        public List<FilterCriteria> DealerMasterFilterCriteria { get; set; }
        public List<FilterCriteria> InventoryMasterFilterCriteria { get; set; }
        public List<FilterCriteria> SalesInvoiceMasterFilterCriteria { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public string UID { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<ITallyDealerMaster> DealerMasterDataListForExcel { get ; set ; }
        public List<ITallyInventoryMaster> InventoryMasterDataListForExcel { get ; set ; }
        public List<ITallySalesInvoiceMaster> SalesInvoiceMasterDataListForExcel { get ; set ; }

        public TallyMasterBaseViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
        ISortHelper sorter,
            IAppUser appUser,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
         )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
            ChannelPartnerList = new List<ISelectionItem>();
            DealerMasterDataList = new List<ITallyDealerMaster>();
            InventoryMasterDataList = new List<ITallyInventoryMaster>();
            SalesInvoiceMasterDataList = new List<ITallySalesInvoiceMaster>();
            SalesInvoiceLineMasterDataList = new List<ITallySalesInvoiceLineMaster>();
            tallyDealerMaster = new TallyDealerMaster();
            tallyInventoryMaster = new TallyInventoryMaster();
            tallySalesInvoiceMaster = new TallySalesInvoiceMaster();
            tallySalesInvoiceLineMaster = new TallySalesInvoiceLineMaster();
            tallySalesInvoiceResults = new List<ITallySalesInvoiceResult>();
            DealerMasterFilterCriteria = new List<FilterCriteria>();
            InventoryMasterFilterCriteria = new List<FilterCriteria>();
            SalesInvoiceMasterFilterCriteria = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
            DealerMasterDataListForExcel = new List<ITallyDealerMaster>();
            InventoryMasterDataListForExcel = new List<ITallyInventoryMaster>();
            SalesInvoiceMasterDataListForExcel = new List<ITallySalesInvoiceMaster>();
        }
        public async Task PageIndexChanged(int pageNumber, string uID, string tallyModule)
        {
            if (UID != null)
            {
                PageNumber = pageNumber;
                if (tallyModule == "DealerMaster")
                {
                    await GetDealerMasterGridDataByDist(uID);
                }
                else if (tallyModule == "Inventory")
                {
                    await GetInventoryMasterGridDataByDist(uID);
                }
                else
                {
                    await GetSalesInvoiceMasterGridDataByDist(uID);
                }
            }
           
        }
        public async Task ApplySort(SortCriteria sortCriteria, string uID, string tallyModule)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            if (tallyModule == "DealerMaster")
            {
                await GetDealerMasterGridDataByDist(uID);
            }
            else if (tallyModule == "Inventory")
            {
                await GetInventoryMasterGridDataByDist(uID);
            }
            else
            {
                await GetSalesInvoiceMasterGridDataByDist(uID);
            }
        }
        public async virtual Task PopulateChannelPartners()
        {
            await GetChannelPartnersList();
        }
        public async virtual Task GetDealerMasterGridDataByDist(string UID)
        {
            DealerMasterDataList = await GetDealerMasterGridDataByDistUID(UID);
            //TotalItemsCount = DealerMasterDataList.Count();
        }
        public async virtual Task PopulateGridDataForEXCEL(string uID ,string tallyModule )
        {
            if (tallyModule == "DealerMaster")
            {
                DealerMasterDataListForExcel = await GetDealerMasterGridDataByDistUID(UID);
            }
            else if (tallyModule == "Inventory")
            {
                InventoryMasterDataListForExcel = await GetInventoryMasterGridDataByDistUID(UID);
            }
            else
            {
                SortCriterias.Clear();
                List<SortCriteria> sorts = new List<SortCriteria>();
                sorts.Add(new SortCriteria("date", SortDirection.Desc));
                SortCriterias.AddRange(sorts);
                SalesInvoiceMasterDataListForExcel = await GetSalesInvoiceMasterGridDataByDistUID(UID);
            }
        }
        public async virtual Task GetInventoryMasterGridDataByDist(string UID)
        {
            InventoryMasterDataList = await GetInventoryMasterGridDataByDistUID(UID);
        }
        public async virtual Task GetSalesInvoiceMasterGridDataByDist(string UID)
        {
            SortCriterias.Clear();
            List<SortCriteria> sorts = new List<SortCriteria>();
            sorts.Add(new SortCriteria("date", SortDirection.Desc));
            SortCriterias.AddRange(sorts);
            SalesInvoiceMasterDataList = await GetSalesInvoiceMasterGridDataByDistUID(UID);
        }
        public async virtual Task GetSalesInvoiceLineMasterGridDataByUID(string dmsuid)
        {
            SalesInvoiceLineMasterDataList = await GetSalesInvoiceLineMasterGridData(dmsuid);
        }
        public async virtual Task GetDealerMasterItemDetails(string uid)
        {
            tallyDealerMaster = await GetDealerMasterItemDetailsByUID(uid);
        }
        public async virtual Task GetInventoryMasterItemDetails(string uid)
        {
            tallyInventoryMaster = await GetInventoryMasterItemDetailsByUID(uid);
        }


        public async virtual Task GetSalesInvoiceMasterItemDetails(string uid)
        {
            tallySalesInvoiceMaster = await GetSalesInvoiceMasterItemDetailsUID(uid);
        }
        public async virtual Task GetTallySalesInvoiceData(string UID)
        {
            tallySalesInvoiceResults = await GetTallySalesInvoiceDataByUID(UID);
        }
        public async Task ApplyFilterForDealer(List<FilterCriteria> filterCriterias, string selectedDealer)
        {
            DealerMasterFilterCriteria.Clear();
            DealerMasterFilterCriteria.AddRange(filterCriterias);
            DealerMasterDataList = await GetDealerMasterGridDataByDistUID(selectedDealer);
           
        }
        public async Task ApplyFilterForInventory(List<FilterCriteria> filterCriterias, string selectedDealer)
        {
            InventoryMasterFilterCriteria.Clear();
            InventoryMasterFilterCriteria.AddRange(filterCriterias);
            InventoryMasterDataList = await GetInventoryMasterGridDataByDistUID(selectedDealer);
        }
        public async Task ApplyFilterForSalesInvoice(List<FilterCriteria> filterCriterias, string selectedDealer)
        {
            SalesInvoiceMasterFilterCriteria.Clear();
            SalesInvoiceMasterFilterCriteria.AddRange(filterCriterias);
            SalesInvoiceMasterDataList = await GetSalesInvoiceMasterGridDataByDistUID(selectedDealer);
        }
        private async Task GetChannelPartnersList()
        {
            ChannelPartnerList.Clear();
            var newChannelpartners = await GetChannelPartnersListForMaster();
            if (newChannelpartners != null && newChannelpartners.Any())
            {
                ChannelPartnerList.AddRange(CommonFunctions.ConvertToSelectionItems(newChannelpartners, e => e.UID, e => e.Code, e => e.Name, e => $"[{e.Code}] {e.Name}"));
            }
        }
        public abstract Task<List<IStore>> GetChannelPartnersListForMaster();
        public abstract Task<List<ITallyDealerMaster>> GetDealerMasterGridDataByDistUID(string UID);
        public abstract Task<List<ITallyInventoryMaster>> GetInventoryMasterGridDataByDistUID(string UID);
        public abstract Task<List<ITallySalesInvoiceMaster>> GetSalesInvoiceMasterGridDataByDistUID(string UID);
        public abstract Task<List<ITallySalesInvoiceLineMaster>> GetSalesInvoiceLineMasterGridData(string UID);
        public abstract Task<ITallyDealerMaster> GetDealerMasterItemDetailsByUID(string uid);
        public abstract Task<ITallyInventoryMaster> GetInventoryMasterItemDetailsByUID(string uid);
        public abstract Task<ITallySalesInvoiceMaster> GetSalesInvoiceMasterItemDetailsUID(string uid);
        public abstract Task<List<ITallySalesInvoiceResult>> GetTallySalesInvoiceDataByUID(string uid);
        public abstract Task<bool> InsertTallyMaster(List<ITallyDealerMaster> tallyDBDetails);

        
    }
}
