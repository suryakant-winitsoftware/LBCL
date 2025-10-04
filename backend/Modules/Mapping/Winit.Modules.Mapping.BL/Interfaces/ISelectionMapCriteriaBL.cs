using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.BL.Interfaces
{
    public interface ISelectionMapCriteriaBL
    {
        Task<PagedResponse<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria>> SelectAllSelectionMapCriteria(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria> GetSelectionMapCriteriaByUID(string UID);
        Task<int> CreateSelectionMapCriteria(List<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria?> createSelectionMapCriterias);
        Task<int> UpdateSelectionMapCriteria(Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria updateSelectionMapCriteria);
        Task<int> DeleteSelectionMapCriteria(string UID);
        Task<bool> CUDSelectionMapMaster(ISelectionMapMaster selectionMapMaster);
        Task<ISelectionMapMaster> GetSelectionMapMasterByLinkedItemUID(string linkedItemUID);
        Task<ISelectionMapMaster> GetSelectionMapMasterByCriteriaUID(string criteriaUID);
        Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs);
    }
}
