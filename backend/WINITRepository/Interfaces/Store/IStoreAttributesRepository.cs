using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using static WINITRepository.Classes.StoreAttributess.PostgreSQLStoreAttributesRepository;
using WINITSharedObjects.Models.Store;

namespace WINITRepository.Interfaces.StoreAttributess
{
    public interface IStoreAttributesRepository
    {
        Task<IEnumerable<StoreAttributes>> SelectStoreAttributesByName(string attributeName);
        //Task<IEnumerable<StoreAttributes>> SelectAllStoreAttributes();
        Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectAllStoreAttributes(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias);
        Task<StoreAttributes> SelectStoreAttributesByStoreUID(string storeUID);
        Task<StoreAttributes> CreateStoreAttributes(StoreAttributes storeAttributes);
      //  Task<int> UpdateStoreAttributes(int Id, StoreAttributes StoreAttributes);
        Task<int> UpdateStoreAttributes( StoreAttributes StoreAttributes);
        Task<int> DeleteStoreAttributes(string storeUID);
        Task<IEnumerable<StoreAttributes>> GetStoreAttributesFiltered(string Name, String Email);
        Task<IEnumerable<StoreAttributes>> GetStoreAttributesPaged(int pageNumber, int pageSize);
        Task<IEnumerable<StoreAttributes>> GetStoreAttributesSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias);

        //store
        Task<IEnumerable<WINITSharedObjects.Models.Store.Store>> SelectAllStore(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias);
        Task<Store> SelectStoreByUID(string UID);
        Task<WINITSharedObjects.Models.Store.Store> CreateStore(WINITSharedObjects.Models.Store.Store store);
        Task<int> UpdateStore(WINITSharedObjects.Models.Store.Store Store);
        Task<int> DeleteStore(string UID);


        //StoreAdditionalInfo

        Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAdditionalInfo>> SelectAllStoreAdditionalInfo(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias);
        Task<StoreAdditionalInfo> SelectStoreAdditionalInfoByUID(string storeUID);
        Task<WINITSharedObjects.Models.Store.StoreAdditionalInfo> CreateStoreAdditionalInfo(WINITSharedObjects.Models.Store.StoreAdditionalInfo storeAdditionalInfo);
        Task<int> UpdateStoreAdditionalInfo(WINITSharedObjects.Models.Store.StoreAdditionalInfo storeAdditionalInfo);
        Task<int> DeleteStoreAdditionalInfo(string storeUID);

    }
}
