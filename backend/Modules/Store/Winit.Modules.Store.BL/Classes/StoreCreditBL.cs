using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreCreditBL : StoreBaseBL, Interfaces.IStoreCreditBL
    {
        protected readonly DL.Interfaces.IStoreCreditDL _storeCreditDL = null;
        public StoreCreditBL(DL.Interfaces.IStoreCreditDL storeRepository) 
        {
            _storeCreditDL = storeRepository;
        }
        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreCredit>> SelectAllStoreCredit(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeCreditDL.SelectAllStoreCredit(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Store.Model.Interfaces.IStoreCredit> SelectStoreCreditByUID(string UID)
        {
            return await _storeCreditDL.SelectStoreCreditByUID(UID);
        }
        public async Task<int> CreateStoreCredit(Winit.Modules.Store.Model.Interfaces.IStoreCredit storeCredit)
        {
            return await _storeCreditDL.CreateStoreCredit(storeCredit);
        }
        public async Task<int> UpdateStoreCredit(Winit.Modules.Store.Model.Interfaces.IStoreCredit storeCredit)
        {
            return await _storeCreditDL.UpdateStoreCredit(storeCredit);
        }
        public async Task<int> UpdateStoreCreditStatus(List<IStoreCredit> storeCredit)
        {
            return await _storeCreditDL.UpdateStoreCreditStatus(storeCredit);
        }
        public async Task<int> DeleteStoreCredit(string UID)
        {
            return await _storeCreditDL.DeleteStoreCredit(UID);
        }

        public async Task<List<IStoreCreditLimit>> GetCurrentLimitByStoreAndDivision(List<string> storeUIDs, string divisionUID)
        {
            return await _storeCreditDL.GetCurrentLimitByStoreAndDivision(storeUIDs, divisionUID);
        }
        public async Task<IStoreCredit> SelectStoreCreditByStoreUID(string StoreUID)
        {
            return await _storeCreditDL.SelectStoreCreditByStoreUID(StoreUID);
        }
        public async Task<List<IPurchaseOrderCreditLimitBufferRange>> GetPurchaseOrderCreditLimitBufferRanges()
        {
            return await _storeCreditDL.GetPurchaseOrderCreditLimitBufferRanges();
        }
    }
}
