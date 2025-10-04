using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Interfaces
{
    public interface ISKUToGroupMappingDL
    {
        Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping>> SelectAllSKUToGroupMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping> SelectSKUToGroupMappingByUID(string UID);

        Task<int> CreateSKUToGroupMapping(Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping sKU);
        Task<int> UpdateSKUToGroupMapping(Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping sKU);
        Task<int> DeleteSKUToGroupMappingByUID(string UID);
    }
}
