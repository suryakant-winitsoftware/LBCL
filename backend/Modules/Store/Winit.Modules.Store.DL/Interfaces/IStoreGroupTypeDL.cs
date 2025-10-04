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
    public interface IStoreGroupTypeDL
    {
        Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroupType>> SelectAllStoreGroupType(List<SortCriteria> sortCriterias, int pageNumber,int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> CreateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupType storeGroupType);
        Task<Winit.Modules.Store.Model.Interfaces.IStoreGroupType?> SelectStoreGroupTypeByUID(string UID);
        Task<int> UpdateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupType StoreGroupType);
        Task<int> DeleteStoreGroupType(string UID);
    }
}
