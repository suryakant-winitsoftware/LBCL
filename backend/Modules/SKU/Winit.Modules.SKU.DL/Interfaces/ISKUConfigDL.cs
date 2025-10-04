using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Interfaces
{
    public interface ISKUConfigDL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUConfig>> SelectAllSKUConfigDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKU.Model.Interfaces.ISKUConfig> SelectSKUConfigByUID(string UID);

        Task<int> CreateSKUConfig(Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuConfig);
        Task<int> UpdateSKUConfig(Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuConfig);
        Task<int> DeleteSKUConfig(string UID);
    }
}
