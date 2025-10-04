using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.DL.Interfaces
{
    public interface IStoreCheckDL
    {
        Task<int> CUDStoreCheck(/*Winit.Modules.StockAudit.Model.Classes.WHStockAuditRequestTemplateModel wHRequestTempleteModel*/);

        Task<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView> SelectStoreCheckHistoryData(string beatHistoryUID, string storeHistoryUID);
        Task<IStoreCheckItemUomQty> SelectStoreCheckItemUomQty(string storeCheckItemHistoryUID, string uom);

        Task<IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>> SelectStoreCheckItemExpiryDREHistory(string storeCheckItemHistoryUID);

        Task<PagedResponse<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>> SelectStoreCheckItemHistoryData(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string storeCheckHistoryUID);

        Task<int> CreateUpdateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckModel storeCheckModel);

        //Task<int> CreateUpdateStoreCheckUomQty(Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemUomQty storeCheckItemUomQty);

        //Task<int> CreateUpdateStoreCheckExpireDreHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemExpiryDREHistory storeCheckItemExpiryDREHistory);
        Task<int> InsertorUpdate_StoreCheck(StoreCheckMaster storeCheckMaster);
    }
}
