using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class SKUToGroupMappingBL : ISKUToGroupMappingBL
    {
        protected readonly DL.Interfaces.ISKUToGroupMappingDL _skuToGroupMappingDL = null;
        public SKUToGroupMappingBL(DL.Interfaces.ISKUToGroupMappingDL skuToGroupMappingDL)
        {
            _skuToGroupMappingDL = skuToGroupMappingDL;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping>> SelectAllSKUToGroupMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _skuToGroupMappingDL.SelectAllSKUToGroupMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping> SelectSKUToGroupMappingByUID(string UID)
        {
            return await _skuToGroupMappingDL.SelectSKUToGroupMappingByUID(UID);
        }
        public async Task<int> CreateSKUToGroupMapping(Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping sKUToGroupMapping)
        {
            return await _skuToGroupMappingDL.CreateSKUToGroupMapping(sKUToGroupMapping);
        }

        public async Task<int> UpdateSKUToGroupMapping(Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping sKUToGroupMapping)
        {
            return await _skuToGroupMappingDL.UpdateSKUToGroupMapping(sKUToGroupMapping);
        }

        public async Task<int> DeleteSKUToGroupMappingByUID(string UID)
        {
            return await _skuToGroupMappingDL.DeleteSKUToGroupMappingByUID(UID);
        }
    }
}
