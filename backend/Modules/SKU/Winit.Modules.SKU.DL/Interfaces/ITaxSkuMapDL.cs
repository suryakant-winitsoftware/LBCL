using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Interfaces
{
    public interface ITaxSkuMapDL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap>> SelectAllTaxSkuMapDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap> SelectTaxSkuMapByUID(string UID);

        Task<int> CreateTaxSkuMap(Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap taxSkuMap);
        Task<int> UpdateTaxSkuMap(Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap taxSkuMap);
        Task<int> DeleteTaxSkuMapByUID(string UID);
    }
}
