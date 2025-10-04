using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.BL.Interfaces;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.Classes
{
    public class SKUClassGroupBL:ISKUClassGroupBL
    {
        protected readonly DL.Interfaces.ISKUClassGroupDL _skuClassGroupBL = null;
        public SKUClassGroupBL(DL.Interfaces.ISKUClassGroupDL skuClassGroupBL)
        {
            _skuClassGroupBL = skuClassGroupBL;
        }
        public async Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup>> SelectAllSKUClassGroupDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _skuClassGroupBL.SelectAllSKUClassGroupDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup> GetSKUClassGroupByUID(string UID)
        {
            return await _skuClassGroupBL.GetSKUClassGroupByUID(UID);
        }
        public async Task<int> CreateSKUClassGroup(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup createSKUClassGroup)
        {
            return await _skuClassGroupBL.CreateSKUClassGroup(createSKUClassGroup);
        }
        public async Task<int> UpdateSKUClassGroup(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup updateSKUClassGroup)
        {
            return await _skuClassGroupBL.UpdateSKUClassGroup(updateSKUClassGroup);
        }
        public async Task<int> DeleteSKUClassGroup(string UID)
        {
            return await _skuClassGroupBL.DeleteSKUClassGroup(UID);
        }
        public async Task<bool> CUD_SKUClassGroupMaster(ISKUClassGroupMaster sKUClassGroupMaster)
        {
            return await _skuClassGroupBL.CUD_SKUClassGroupMaster(sKUClassGroupMaster);
        }
        public async Task<ISKUClassGroupMaster> GetSKUClassGroupMaster(string sKUClassGroupUID)
        {
            return await _skuClassGroupBL.GetSKUClassGroupMaster(sKUClassGroupUID);
        }

        public async Task<int> DeleteSKUClassGroupMaster(string skuClassGroupUId)
        {
            return await _skuClassGroupBL.DeleteSKUClassGroupMaster(skuClassGroupUId);
        }
    }
}
