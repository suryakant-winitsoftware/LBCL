using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tally.DL.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.BL.Classes
{
    public class TallyMasterBL : Interfaces.ITallyMasterBL
    {
        protected readonly DL.Interfaces.ITallyMasterDL _tallyMasterDL;
        public TallyMasterBL(DL.Interfaces.ITallyMasterDL tallyMasterViewDL)
        {
            _tallyMasterDL = tallyMasterViewDL;
        }

        public Task<ITallyInventoryMaster> GetInventoryMasterItem(string uID)
        {
            return _tallyMasterDL.GetInventoryMasterItem(uID);
        }

        public Task<ITallySalesInvoiceMaster> GetSalesInvoiceMasterItem(string uID)
        {
            return _tallyMasterDL.GetSalesInvoiceMasterItem(uID);
        }
        public Task<ITallyDealerMaster> GetTallyDealerMasterItem(string uID)
        {
            return _tallyMasterDL.GetTallyDealerMasterItem(uID);
        }

        public Task<PagedResponse<ITallyDealerMaster>> GetTallyDealerMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uID)
        {
            return _tallyMasterDL.GetTallyDealerMasterDataByUID(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, uID);
        }

        public Task<PagedResponse<ITallyInventoryMaster>> GetTallyInventoryMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uID)
        {
            return _tallyMasterDL.GetTallyInventoryMasterDataByUID(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, uID);
        }

        public Task<PagedResponse<ITallySalesInvoiceLineMaster>> GetTallySalesInvoiceLineMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uID)
        {
            return _tallyMasterDL.GetTallySalesInvoiceLineMasterDataByUID(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, uID);
        }

        public Task<PagedResponse<ITallySalesInvoiceMaster>> GetTallySalesInvoiceMasterDataByUID(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uID)
        {
            return _tallyMasterDL.GetTallySalesInvoiceMasterDataByUID(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, uID);
        }

        public Task<PagedResponse<ITallySalesInvoiceResult>> GetTallySalesInvoiceData(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string UID)
        {
            return _tallyMasterDL.GetTallySalesInvoiceData(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, UID);
        }

        public Task<bool> UpdateTallyMasterData(ITallyDealerMaster data)
        {
            return _tallyMasterDL.UpdateTallyMasterData(data);
        }


    }
}
