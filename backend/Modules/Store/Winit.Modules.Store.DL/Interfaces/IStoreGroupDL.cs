using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Store.DL.Interfaces
{
    public interface IStoreGroupDL
    {
        Task<PagedResponse<Model.Interfaces.IStoreGroup>> SelectAllStoreGroup(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IStoreGroup?> SelectStoreGroupByUID(string UID);
        Task<int> CreateStoreGroup(Model.Interfaces.IStoreGroup storeGroup);
        Task<int> InsertStoreGroupHierarchy(string type, string uid);
        Task<int> UpdateStoreGroup(Model.Interfaces.IStoreGroup storeGroup);
        Task<int> DeleteStoreGroup(string UID);
    }
}
