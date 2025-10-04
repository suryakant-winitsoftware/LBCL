using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.DL.Interfaces
{
    public interface ITallyMasterDL
    {
        Task<ITallyDealerMaster> GetTallyDealerMasterItem(string uID);
        Task<ITallyInventoryMaster> GetInventoryMasterItem(string uID);
        Task<ITallySalesInvoiceMaster> GetSalesInvoiceMasterItem(string uID);
        Task<PagedResponse<ITallyDealerMaster>> GetTallyDealerMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uid);
        Task<PagedResponse<ITallyInventoryMaster>> GetTallyInventoryMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uid);
        Task<PagedResponse<ITallySalesInvoiceLineMaster>> GetTallySalesInvoiceLineMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uID);
        Task<PagedResponse<ITallySalesInvoiceMaster>> GetTallySalesInvoiceMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uID);
        Task<PagedResponse<ITallySalesInvoiceResult>> GetTallySalesInvoiceData(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string UID);
        Task<bool> UpdateTallyMasterData(ITallyDealerMaster data);
    }
}
