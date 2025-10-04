using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mapping.BL.Interfaces;
using Winit.Modules.Mapping.DL.Interfaces;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.BL.Classes
{
    public class SelectionMapCriteriaBL : ISelectionMapCriteriaBL
    {
        private readonly ISelectionMapCriteriaDL _selectionMapCriteriaDL;
        public SelectionMapCriteriaBL(ISelectionMapCriteriaDL selectionMapCriteriaDL)
        {
            _selectionMapCriteriaDL = selectionMapCriteriaDL;
        }
        public async Task<bool> CUDSelectionMapMaster(ISelectionMapMaster selectionMapMaster)
        {
            return await _selectionMapCriteriaDL.CUDSelectionMapMaster(selectionMapMaster);
        }

        public Task<int> CreateSelectionMapCriteria(List<ISelectionMapCriteria?> createSelectionMapCriterias)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteSelectionMapCriteria(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<ISelectionMapCriteria> GetSelectionMapCriteriaByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public async Task<ISelectionMapMaster> GetSelectionMapMasterByLinkedItemUID(string linkedItemUID)
        {
            return await _selectionMapCriteriaDL.GetSelectionMapMasterByLinkedItemUID(linkedItemUID);
        }

        public async Task<ISelectionMapMaster> GetSelectionMapMasterByCriteriaUID(string criteriaUID)
        {
            return await _selectionMapCriteriaDL.GetSelectionMapMasterByCriteriaUID(criteriaUID);
        }

        public async Task<PagedResponse<ISelectionMapCriteria>> SelectAllSelectionMapCriteria(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _selectionMapCriteriaDL.SelectAllSelectionMapCriteria(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public Task<int> UpdateSelectionMapCriteria(ISelectionMapCriteria updateSelectionMapCriteria)
        {
            throw new NotImplementedException();
        }
        public async Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs)
        {
            return await _selectionMapCriteriaDL.GetLinkedItemUIDByStore(linkedItemType, storeUIDs);
        }

    }
}
