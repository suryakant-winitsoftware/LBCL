using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models.Store;
using static WINITRepository.Classes.StoreAttributess.PostgreSQLStoreAttributesRepository;

namespace WINITServices.Classes.StoreAttributes
{
    public class StoreAttributesService : StoreAttributesBaseService
    {
        public StoreAttributesService(WINITRepository.Interfaces.StoreAttributess.IStoreAttributesRepository storeAttributesRepository) : base(storeAttributesRepository)
        {

        }
        public async override Task<int> AddStoreAttributes()
        {
            throw new NotImplementedException();
        }
        public async override Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectStoreAttributesByName(string attributeName)
        {
            return await _storeAttributesRepository.SelectStoreAttributesByName(attributeName);
        }
        //public async override Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectAllStoreAttributes()
        //{
        //    return await _storeAttributesRepository.SelectAllStoreAttributes();
        //}

        public async override Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectAllStoreAttributes(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _storeAttributesRepository.SelectAllStoreAttributes(sortCriterias, pageNumber, pageSize,filterCriterias);
        }
        //public async override Task<WINITSharedObjects.Models.Store.StoreAttributes> SelectStoreAttributesByUID(int Id)
        //{
        //    return await _storeAttributesRepository.SelectStoreAttributesByUID(Id);
        //}

        public async override Task<WINITSharedObjects.Models.Store.StoreAttributes> SelectStoreAttributesByStoreUID(string storeUID)
        {
            return await _storeAttributesRepository.SelectStoreAttributesByStoreUID(storeUID);
        }
        public async override Task<WINITSharedObjects.Models.Store.StoreAttributes> CreateStoreAttributes(WINITSharedObjects.Models.Store.StoreAttributes customer)
        {
            return await _storeAttributesRepository.CreateStoreAttributes(customer);
        }
        public async override Task<int> UpdateStoreAttributes( WINITSharedObjects.Models.Store.StoreAttributes StoreAttributes)
        {
            return await _storeAttributesRepository.UpdateStoreAttributes(StoreAttributes);
        }
        public async override Task<int> DeleteStoreAttributes(string storeUID)
        {
            return await _storeAttributesRepository.DeleteStoreAttributes(storeUID);
        }
        public async override Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> GetStoreAttributesFiltered(string Name, String Email)
        {
            return await _storeAttributesRepository.GetStoreAttributesFiltered(Name, Email);
        }
        public async override Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> GetStoreAttributesPaged(int pageNumber, int pageSize)
        {
            return await _storeAttributesRepository.GetStoreAttributesPaged(pageNumber,pageSize);
        }
        public async override Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> GetStoreAttributesSorted(List<SortCriteria> sortCriterias)
        {
            return await _storeAttributesRepository.GetStoreAttributesSorted(sortCriterias);
        }

        //store


        public async override Task<IEnumerable<WINITSharedObjects.Models.Store.Store>> SelectAllStore(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _storeAttributesRepository.SelectAllStore(sortCriterias, pageNumber, pageSize, filterCriterias);
        }

        public async override Task<WINITSharedObjects.Models.Store.Store> SelectStoreByUID(string UID)
        {
            return await _storeAttributesRepository.SelectStoreByUID(UID);
        }
        public async override Task<WINITSharedObjects.Models.Store.Store> CreateStore(WINITSharedObjects.Models.Store.Store store)
        {
            return await _storeAttributesRepository.CreateStore(store);
        }
        public async override Task<int> UpdateStore(WINITSharedObjects.Models.Store.Store Store)
        {
            return await _storeAttributesRepository.UpdateStore(Store);
        }
        public async override Task<int> DeleteStore(string UID)
        {
            return await _storeAttributesRepository.DeleteStore(UID);
        }

        //StoreAdditionalInfo

        public async override Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAdditionalInfo>> SelectAllStoreAdditionalInfo(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _storeAttributesRepository.SelectAllStoreAdditionalInfo(sortCriterias, pageNumber, pageSize, filterCriterias);
        }
        public async override Task<StoreAdditionalInfo> SelectStoreAdditionalInfoByUID(string storeUID)
        {
            return await _storeAttributesRepository.SelectStoreAdditionalInfoByUID(storeUID);
        }
        public async override Task<WINITSharedObjects.Models.Store.StoreAdditionalInfo> CreateStoreAdditionalInfo(WINITSharedObjects.Models.Store.StoreAdditionalInfo storeAdditionalInfo)
        {
            return await _storeAttributesRepository.CreateStoreAdditionalInfo(storeAdditionalInfo);
        }
        public async override Task<int> UpdateStoreAdditionalInfo(WINITSharedObjects.Models.Store.StoreAdditionalInfo storeAdditionalInfo)
        {
            return await _storeAttributesRepository.UpdateStoreAdditionalInfo(storeAdditionalInfo);
        }
        public async override Task<int> DeleteStoreAdditionalInfo(string storeUID)
        {
            return await _storeAttributesRepository.DeleteStoreAdditionalInfo(storeUID);

        }



    }
}
