using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models.Store;

namespace WINITServices.Interfaces
{
    public interface IStoreAttributesService
    {
        Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectStoreAttributesByName(string attributeName);
     //   Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectAllStoreAttributes();

        Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> SelectAllStoreAttributes(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias);
       // Task<WINITSharedObjects.Models.Store.StoreAttributes> SelectStoreAttributesByUID(int Id);
        Task<StoreAttributes> SelectStoreAttributesByStoreUID(string storeUID);
        Task<WINITSharedObjects.Models.Store.StoreAttributes> CreateStoreAttributes(WINITSharedObjects.Models.Store.StoreAttributes storeAttributes);
        Task<int> UpdateStoreAttributes( WINITSharedObjects.Models.Store.StoreAttributes StoreAttributes);
        Task<int> DeleteStoreAttributes(string storeUID);
        Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> GetStoreAttributesFiltered(string Name, String Email);
        Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> GetStoreAttributesPaged(int pageNumber, int pageSize);
        Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAttributes>> GetStoreAttributesSorted(List<SortCriteria> sortCriterias);



        //store

        Task<IEnumerable<WINITSharedObjects.Models.Store.Store>> SelectAllStore(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias);
        Task<Store> SelectStoreByUID(string UID);
        Task<WINITSharedObjects.Models.Store.Store> CreateStore(WINITSharedObjects.Models.Store.Store store);
        Task<int> UpdateStore(WINITSharedObjects.Models.Store.Store Store);
        Task<int> DeleteStore(string UID);


        ////StoreAdditionalInfo

        Task<IEnumerable<WINITSharedObjects.Models.Store.StoreAdditionalInfo>> SelectAllStoreAdditionalInfo(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias);
        Task<StoreAdditionalInfo> SelectStoreAdditionalInfoByUID(string storeUID);
        Task<WINITSharedObjects.Models.Store.StoreAdditionalInfo> CreateStoreAdditionalInfo(WINITSharedObjects.Models.Store.StoreAdditionalInfo storeAdditionalInfo);
        Task<int> UpdateStoreAdditionalInfo(WINITSharedObjects.Models.Store.StoreAdditionalInfo storeAdditionalInfo);
        Task<int> DeleteStoreAdditionalInfo(string storeUID);

    }
}
