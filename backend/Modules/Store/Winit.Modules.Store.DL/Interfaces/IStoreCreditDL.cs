using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Interfaces
{
    public interface IStoreCreditDL
    {
        Task<PagedResponse<Model.Interfaces.IStoreCredit>> SelectAllStoreCredit(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IStoreCredit> SelectStoreCreditByUID(string UID);
        Task<int> CreateStoreCredit(Model.Interfaces.IStoreCredit storeCredit);
        Task<int> UpdateStoreCredit(Model.Interfaces.IStoreCredit storeCredit);
        Task<int> UpdateStoreCreditStatus(List<IStoreCredit> storeCredit);
        Task<int> DeleteStoreCredit(string UID);
        Task<IStoreCredit> SelectStoreCreditByStoreUID(string StoreUID);
        Task<List<IStoreCreditLimit>> GetCurrentLimitByStoreAndDivision(List<string> storeUIDs, string divisionUID);
        Task<List<IPurchaseOrderCreditLimitBufferRange>> GetPurchaseOrderCreditLimitBufferRanges();
    }
}
