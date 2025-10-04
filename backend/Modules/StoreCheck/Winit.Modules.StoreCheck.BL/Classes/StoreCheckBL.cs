using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreCheck.BL.Interfaces;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.BL.Classes
{
    public class StoreCheckBL: IStoreCheckBL
    {
        protected readonly Winit.Modules.StoreCheck.DL.Interfaces.IStoreCheckDL _storeCheckDL = null;
        IServiceProvider _serviceProvider = null;
        public StoreCheckBL(DL.Interfaces.IStoreCheckDL whStockAuditBL, IServiceProvider serviceProvider)
        {
            _storeCheckDL = whStockAuditBL;
            _serviceProvider = serviceProvider;
        }
        public async Task<int> CUDStoreCheck(/*Winit.Modules.StoreCheck.Model.Classes.WHStockAuditRequestTemplateModel wHStockAuditRequestTemplateModel*/)
        {
            return await _storeCheckDL.CUDStoreCheck(/*wHStockAuditRequestTemplateModel*/);
        }

        public async Task<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView> SelectStoreCheckHistoryData(string beatHistoryUID, string storeHistoryUID)
        {
            return await _storeCheckDL.SelectStoreCheckHistoryData(beatHistoryUID, storeHistoryUID);

        }
        public async Task<IStoreCheckItemUomQty> SelectStoreCheckItemUomQty(string storeCheckItemHistoryUID, string uom)
        {
            return await _storeCheckDL.SelectStoreCheckItemUomQty(storeCheckItemHistoryUID,uom);

        }
        public async Task<IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>> SelectStoreCheckItemExpiryDREHistory(string storeCheckItemHistoryUID)
        {
            return await _storeCheckDL.SelectStoreCheckItemExpiryDREHistory(storeCheckItemHistoryUID);
        }
        public async Task<int> CreateUpdateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckModel storeCheckModel)
        {
            return await _storeCheckDL.CreateUpdateStoreCheckHistory(storeCheckModel);
        }
        public async Task<int> CreateUpdateStoreCheckUomQty(Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemUomQty storeCheckItemUomQty)
        {
            //return await _storeCheckDL.CreateUpdateStoreCheckUomQty(storeCheckItemUomQty);
            return 1;
        }
        public async Task<int> CreateUpdateStoreCheckExpireDreHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemExpiryDREHistory storeCheckItemExpiryDREHistory)
        {
            //return await _storeCheckDL.CreateUpdateStoreCheckExpireDreHistory(storeCheckItemExpiryDREHistory);
            return 1;
        }

        public async Task<PagedResponse<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>> SelectStoreCheckItemHistoryData(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string storeCheckHistoryUID)
        {
            return await _storeCheckDL.SelectStoreCheckItemHistoryData(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, storeCheckHistoryUID);
        }
        public async Task<int> InsertorUpdate_StoreCheck(StoreCheckMaster storeCheckMaster)
        {
            return await _storeCheckDL.InsertorUpdate_StoreCheck(storeCheckMaster);
        }
    }
}




