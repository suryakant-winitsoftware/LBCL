using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreWeekOffBL : StoreBaseBL, Interfaces.IStoreWeekOffBL
    {
        protected readonly DL.Interfaces.IStoreWeekOffDL _storeWeekOffDL = null;
        public StoreWeekOffBL(DL.Interfaces.IStoreWeekOffDL storeWeekOffDL) 
        {
            _storeWeekOffDL = storeWeekOffDL;
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreWeekOff>> SelectAllStoreWeekOff(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeWeekOffDL.SelectAllStoreWeekOff(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Model.Interfaces.IStoreWeekOff> SelectStoreWeekOffByUID(string UID)
        {
            return await _storeWeekOffDL.SelectStoreWeekOffByUID(UID);
        }
        public async Task<int> CreateStoreWeekOff(Model.Interfaces.IStoreWeekOff storeWeekOff)
        {
            return await _storeWeekOffDL.CreateStoreWeekOff(storeWeekOff);
        }
        public async Task<int> UpdateStoreWeekOff(Model.Interfaces.IStoreWeekOff storeWeekOff)
        {
            return await _storeWeekOffDL.UpdateStoreWeekOff(storeWeekOff);
        }
        public async Task<int> DeleteStoreWeekOff(string UID)
        {
            return await _storeWeekOffDL.DeleteStoreWeekOff(UID);
        }
    }
}
