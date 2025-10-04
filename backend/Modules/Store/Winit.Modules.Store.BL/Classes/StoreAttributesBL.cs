using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreAttributesBL : StoreBaseBL, Interfaces.IStoreAttributesBL
    {
        protected readonly DL.Interfaces.IStoreAttributesDL _storeAttributesRepository;
        public StoreAttributesBL(DL.Interfaces.IStoreAttributesDL storeAttributesRepository)
        {
            _storeAttributesRepository = storeAttributesRepository;
        }
        public async Task<int> AddStoreAttributes()
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> SelectStoreAttributesByName(string attributeName)
        {
            return await _storeAttributesRepository.SelectStoreAttributesByName(attributeName);
        }
        public async Task<PagedResponse<Model.Interfaces.IStoreAttributes>> SelectAllStoreAttributes(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeAttributesRepository.SelectAllStoreAttributes(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Model.Interfaces.IStoreAttributes> SelectStoreAttributesByStoreUID(string storeUID)
        {
            return await _storeAttributesRepository.SelectStoreAttributesByStoreUID(storeUID);
        }
        public async Task<int> CreateStoreAttributes(Model.Interfaces.IStoreAttributes customer)
        {
            return await _storeAttributesRepository.CreateStoreAttributes(customer);
        }
        public async Task<int> UpdateStoreAttributes(Model.Interfaces.IStoreAttributes StoreAttributes)
        {
            return await _storeAttributesRepository.UpdateStoreAttributes(StoreAttributes);
        }
        public async Task<int> DeleteStoreAttributes(string storeUID)
        {
            return await _storeAttributesRepository.DeleteStoreAttributes(storeUID);
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesFiltered(string Name, string Email)
        {
            return await _storeAttributesRepository.GetStoreAttributesFiltered(Name, Email);
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesPaged(int pageNumber, int pageSize)
        {
            return await _storeAttributesRepository.GetStoreAttributesPaged(pageNumber, pageSize);
        }
        public async Task<IEnumerable<Model.Interfaces.IStoreAttributes>> GetStoreAttributesSorted(List<SortCriteria> sortCriterias)
        {
            return await _storeAttributesRepository.GetStoreAttributesSorted(sortCriterias);
        }
    }
}
