using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models.Store;
using static WINITRepository.Classes.StoreAttributess.PostgreSQLStoreAttributesRepository;

namespace WINITServices.Classes.StoreAttributes
{
    public abstract class StoreAttributesBaseService : Interfaces.IStoreAttributesService
    {
        protected readonly WINITRepository.Interfaces.StoreAttributess.IStoreAttributesRepository _storeAttributesRepository;
        public StoreAttributesBaseService(WINITRepository.Interfaces.StoreAttributess.IStoreAttributesRepository storeAttributesRepository)
        {
            _storeAttributesRepository = storeAttributesRepository;
        }
        public abstract Task<int> AddStoreAttributes();
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectStoreAttributesByName(string attributeName);
        //public abstract Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectAllStoreAttributes();

    public abstract Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectAllStoreAttributes(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias);
        //    public abstract Task<WINITSharedObjects.Models.Store.StoreAttributes> SelectStoreAttributesByUID(int Id);
       public abstract  Task<WINITSharedObjects.Models.Store.StoreAttributes> SelectStoreAttributesByStoreUID(string storeUID);
        public abstract Task<WINITSharedObjects.Models.Store.StoreAttributes> CreateStoreAttributes(WINITSharedObjects.Models.Store.StoreAttributes storeAttributes);
        public abstract Task<int> UpdateStoreAttributes( WINITSharedObjects.Models.Store.StoreAttributes StoreAttributes);
        public abstract Task<int> DeleteStoreAttributes(string storeUID);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> GetStoreAttributesFiltered(string Name, String Email);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> GetStoreAttributesPaged(int pageNumber, int pageSize);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> GetStoreAttributesSorted(List<SortCriteria> sortCriterias);


        //store

     public abstract Task<IEnumerable<WINITSharedObjects.Models.Store.Store>> SelectAllStore(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
               int pageSize, List<FilterCriteria> filterCriterias);
     public abstract   Task<Store> SelectStoreByUID(string UID);
      public abstract  Task<WINITSharedObjects.Models.Store.Store> CreateStore(WINITSharedObjects.Models.Store.Store store);
      public abstract  Task<int> UpdateStore(WINITSharedObjects.Models.Store.Store Store);
     public abstract   Task<int> DeleteStore(string UID);


        //StoreAdditionalInfo

        public abstract Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAdditionalInfo>> SelectAllStoreAdditionalInfo(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);
        public abstract Task<StoreAdditionalInfo> SelectStoreAdditionalInfoByUID(string storeUID);
        public abstract Task<WINITSharedObjects.Models.Store.StoreAdditionalInfo> CreateStoreAdditionalInfo(WINITSharedObjects.Models.Store.StoreAdditionalInfo storeAdditionalInfo);
        public abstract Task<int> UpdateStoreAdditionalInfo(WINITSharedObjects.Models.Store.StoreAdditionalInfo storeAdditionalInfo);
        public abstract Task<int> DeleteStoreAdditionalInfo(string storeUID);

    }
}
