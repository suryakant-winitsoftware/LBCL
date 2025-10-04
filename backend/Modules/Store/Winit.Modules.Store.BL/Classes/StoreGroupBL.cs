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
    public class StoreGroupBL : StoreBaseBL, Interfaces.IStoreGroupBL
    {
        protected readonly DL.Interfaces.IStoreGroupDL _storeGroupDL = null;
        public StoreGroupBL(DL.Interfaces.IStoreGroupDL storeGroupDL) 
        {
            _storeGroupDL = storeGroupDL;
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreGroup>> SelectAllStoreGroup(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeGroupDL.SelectAllStoreGroup(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Model.Interfaces.IStoreGroup> SelectStoreGroupByUID(string UID)
        {
            return await _storeGroupDL.SelectStoreGroupByUID(UID);
        }
        public async Task<int> CreateStoreGroup(Model.Interfaces.IStoreGroup storeGroup)
        {
            return await _storeGroupDL.CreateStoreGroup(storeGroup);
        }
        public async Task<int> InsertStoreGroupHierarchy(string type, string uid)
        {
            return await _storeGroupDL.InsertStoreGroupHierarchy(type, uid);
        }

        public async Task<int> UpdateStoreGroup(Model.Interfaces.IStoreGroup storeGroup)
        {
            return await _storeGroupDL.UpdateStoreGroup(storeGroup);
        }
        public async Task<int> DeleteStoreGroup(string UID)
        {
            return await _storeGroupDL.DeleteStoreGroup(UID);
        }
    }
}
