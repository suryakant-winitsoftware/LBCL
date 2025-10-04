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
    public class TaxSkuMapBL : ITaxSkuMapBL
    {
        protected readonly DL.Interfaces.ITaxSkuMapDL _taxSkuMapDL = null;
        public TaxSkuMapBL(DL.Interfaces.ITaxSkuMapDL taxSkuMapDL)
        {
            _taxSkuMapDL = taxSkuMapDL;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap>> SelectAllTaxSkuMapDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _taxSkuMapDL.SelectAllTaxSkuMapDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap> SelectTaxSkuMapByUID(string UID)
        {
            return await _taxSkuMapDL.SelectTaxSkuMapByUID(UID);
        }
        public async Task<int> CreateTaxSkuMap(Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap taxSkuMap)
        {
            return await _taxSkuMapDL.CreateTaxSkuMap(taxSkuMap);
        }

        public async Task<int> UpdateTaxSkuMap(Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap taxSkuMap)
        {
            return await _taxSkuMapDL.UpdateTaxSkuMap(taxSkuMap);
        }

        public async Task<int> DeleteTaxSkuMapByUID(string UID)
        {
            return await _taxSkuMapDL.DeleteTaxSkuMapByUID(UID);
        }
    }
}
