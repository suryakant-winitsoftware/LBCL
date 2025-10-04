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
    public class StoreGroupTypeBL : StoreBaseBL, Interfaces.IStoreGroupTypeBL
    {
        protected readonly DL.Interfaces.IStoreGroupTypeDL _storeGroupTypeDL = null;
        public StoreGroupTypeBL(DL.Interfaces.IStoreGroupTypeDL storeGroupTypeDL) 
        {
            _storeGroupTypeDL = storeGroupTypeDL;
        }
        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroupType>> SelectAllStoreGroupType(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeGroupTypeDL.SelectAllStoreGroupType(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Store.Model.Interfaces.IStoreGroupType> SelectStoreGroupTypeByUID(string UID)
        {
            return await _storeGroupTypeDL.SelectStoreGroupTypeByUID(UID);
        }
        public async Task<int> CreateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupType storeGroupType)
        {
            return await _storeGroupTypeDL.CreateStoreGroupType(storeGroupType);
        }
        public async Task<int> UpdateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupType storeGroupType)
        {
            return await _storeGroupTypeDL.UpdateStoreGroupType(storeGroupType);
        }
        public async Task<int> DeleteStoreGroupType(string UID)
        {
            return await _storeGroupTypeDL.DeleteStoreGroupType( UID);
        }
        
    }
}
