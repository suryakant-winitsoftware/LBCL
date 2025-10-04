using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class SKUConfigBL : ISKUConfigBL
    {
        protected readonly DL.Interfaces.ISKUConfigDL _skuConfigDL = null;
        public SKUConfigBL(DL.Interfaces.ISKUConfigDL skuConfigDL)
        {
            _skuConfigDL = skuConfigDL;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUConfig>> SelectAllSKUConfigDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _skuConfigDL.SelectAllSKUConfigDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUConfig> SelectSKUConfigByUID(string UID)
        {
            return await _skuConfigDL.SelectSKUConfigByUID(UID);
        }
        public async Task<int> CreateSKUConfig(Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuConfig)
        {
            return await _skuConfigDL.CreateSKUConfig(skuConfig);
        }

        public async Task<int> UpdateSKUConfig(Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuConfig)
        {
            return await _skuConfigDL.UpdateSKUConfig(skuConfig);
        }

        public async Task<int> DeleteSKUConfig(string UID)
        {
            return await _skuConfigDL.DeleteSKUConfig(UID);
        }
    }
}
