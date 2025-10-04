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
    public class StoreToGroupMappingBL : StoreBaseBL, Interfaces.IStoreToGroupMappingBL
    {
        protected readonly DL.Interfaces.IStoreToGroupMappingDL _storeToGroupMappingDL = null;
        public StoreToGroupMappingBL(DL.Interfaces.IStoreToGroupMappingDL storeToGroupMappingDL) 
        {
            _storeToGroupMappingDL = storeToGroupMappingDL;
        }
        public async  Task<int> CreateStoreToGroupMapping(Model.Interfaces.IStoreToGroupMapping storeGroupAttributes)
        {
            return await _storeToGroupMappingDL.CreateStoreToGroupMapping(storeGroupAttributes);
        }
        public async  Task<PagedResponse<Model.Interfaces.IStoreToGroupMapping>> SelectAllStoreToGroupMapping(List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeToGroupMappingDL.SelectAllStoreToGroupMapping(sortCriterias, pageNumber, pageSize, filterCriterias,isCountRequired);
        }
        public async  Task<Model.Interfaces.IStoreToGroupMapping> SelectAllStoreToGroupMappingByStoreUID(string UID)
        {
            return await _storeToGroupMappingDL.SelectAllStoreToGroupMappingByStoreUID(UID);
        }
        public async  Task<int> UpdateStoreToGroupMapping(Model.Interfaces.IStoreToGroupMapping StoreToGroupMapping)
        {
            return await _storeToGroupMappingDL.UpdateStoreToGroupMapping(StoreToGroupMapping);
        }
        public async Task<int> DeleteStoreToGroupMapping(string UID)
        {
            return await _storeToGroupMappingDL.DeleteStoreToGroupMapping(UID);
        }
        public async Task<IList<Winit.Modules.Store.Model.Interfaces.IStoreGroupData>> SelectAllChannelMasterData()
        {
            return await _storeToGroupMappingDL.SelectAllChannelMasterData();
        }
    }
}
