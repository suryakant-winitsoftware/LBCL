using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreAttributesBL
    {
        Task<IEnumerable<Model.Interfaces.IStoreAttributes>> SelectStoreAttributesByName(string attributeName);
        Task<PagedResponse<Model.Interfaces.IStoreAttributes>> SelectAllStoreAttributes(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Model.Interfaces.IStoreAttributes> SelectStoreAttributesByStoreUID(string storeUID);
        Task<int> CreateStoreAttributes(Model.Interfaces.IStoreAttributes storeAttributes);
        Task<int> UpdateStoreAttributes(Model.Interfaces.IStoreAttributes StoreAttributes);
        Task<int> DeleteStoreAttributes(string storeUID);
        Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesFiltered(string Name, String Email);
        Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesPaged(int pageNumber, int pageSize);
        Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesSorted(List<SortCriteria> sortCriterias);
    }
}
