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
    public class StoreSpecialDayBL : StoreBaseBL, Interfaces.IStoreSpecialDayBL
    {
        protected readonly DL.Interfaces.IStoreSpecialDayDL _storespeciadaylDL = null;
        public StoreSpecialDayBL(DL.Interfaces.IStoreSpecialDayDL storespeciadaylDL) 
        {
            _storespeciadaylDL = storespeciadaylDL;
        }
        public async  Task<int> CreateStoreSpecialDay(Model.Interfaces.IStoreSpecialDay storeSpecialDay)
        {
            return await _storespeciadaylDL.CreateStoreSpecialDay(storeSpecialDay);
        }
        public async  Task<PagedResponse<Model.Interfaces.IStoreSpecialDay>> SelectAllStoreSpecialDay(List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storespeciadaylDL.SelectAllStoreSpecialDay(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async  Task<Model.Interfaces.IStoreSpecialDay> SelectAllStoreSpecialDayByUID(string UID)
        {
            return await _storespeciadaylDL.SelectAllStoreSpecialDayByUID(UID);
        }
        public async  Task<int> UpdateStoreSpecialDay(Model.Interfaces.IStoreSpecialDay storeSpecialDay)
        {
            return await _storespeciadaylDL.UpdateStoreSpecialDay( storeSpecialDay);
        }
        public async Task<int> DeleteStoreSpecialDay(string UID)
        {
            return await _storespeciadaylDL.DeleteStoreSpecialDay(UID);
        }
    }
}
