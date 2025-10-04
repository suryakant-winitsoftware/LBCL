using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Interfaces
{
    public interface IStoreToGroupMappingDL
    {
        Task<int> CreateStoreToGroupMapping(Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping storeGroupAttributes);
        Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping>> SelectAllStoreToGroupMapping(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping> SelectAllStoreToGroupMappingByStoreUID(string UID);
        Task<int> UpdateStoreToGroupMapping(Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping UpdateStoreToGroupMapping);
        Task<int> DeleteStoreToGroupMapping(string UID);
        Task<IList<Winit.Modules.Store.Model.Interfaces.IStoreGroupData>> SelectAllChannelMasterData();
    }
}
