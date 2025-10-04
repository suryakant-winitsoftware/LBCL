using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreGroupBL
    {
        Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroup>> SelectAllStoreGroup(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IStoreGroup> SelectStoreGroupByUID(string UID);
        Task<int> CreateStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroup storeGroup);
        Task<int> InsertStoreGroupHierarchy(string type, string UID);
        Task<int> UpdateStoreGroup(Winit.Modules.Store.Model.Interfaces.IStoreGroup storeGroup);
        Task<int> DeleteStoreGroup(string UID);
    }
}
