using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.Interfaces
{
    public interface ISKUClassGroupBL
    {
        Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup>> SelectAllSKUClassGroupDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup> GetSKUClassGroupByUID(string UID);
        Task<int> CreateSKUClassGroup(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup createSKUClassGroup);
        Task<int> UpdateSKUClassGroup(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup updateSKUClassGroup);
        Task<int> DeleteSKUClassGroup(string UID);
        Task<bool> CUD_SKUClassGroupMaster(ISKUClassGroupMaster sKUClassGroupMaster);
        Task<ISKUClassGroupMaster> GetSKUClassGroupMaster(string sKUClassGroupUID);
        Task<int> DeleteSKUClassGroupMaster(string skuClassGroupUId);
    }
}
