using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreBL : StoreBaseBL, Interfaces.IStoreBL
    {
        protected readonly DL.Interfaces.IStoreDL _storeDL = null;
        IServiceProvider _serviceProvider = null;
        public StoreBL(DL.Interfaces.IStoreDL storeDL, IServiceProvider serviceProvider)
        {
            _storeDL = storeDL;
            _serviceProvider = serviceProvider;
        }
        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>> SelectAllStore(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeDL.SelectAllStore(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Model.Interfaces.IStore> SelectStoreByUID(string UID)
        {
            return await _storeDL.SelectStoreByUID(UID);
        }
        public async Task<int> CreateStore(Model.Interfaces.IStore store)
        {
            return await _storeDL.CreateStore(store);
        }

        public async Task<int> UpdateStore(Model.Interfaces.IStore Store)
        {
            return await _storeDL.UpdateStore(Store);
        }
        public async Task<int> UpdateAsmDivisionMapping(List<Model.Interfaces.IAsmDivisionMapping> AsmDivisionMapping)
        {
            return await _storeDL.UpdateAsmDivisionMapping(AsmDivisionMapping);
        }

        public async Task<int> DeleteStore(string UID)
        {
            return await _storeDL.DeleteStore(UID);
        }


        public async Task<int> DeleteOnBoardingDetails(string UID)
        {
            return await _storeDL.DeleteOnBoardingDetails(UID);
        }
        public async Task<List<IStore>> GetChannelPartner(string jobPositionUid)
        {
            return await _storeDL.GetChannelPartner(jobPositionUid);
        }

        public async Task<int> CreateStoreMaster(Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO createStoreMaster)
        {
            return await _storeDL.CreateStoreMaster(createStoreMaster);
        }

        public async Task<Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO> SelectStoreMasterByUID(string UID)
        {
            return await _storeDL.SelectStoreMasterByUID(UID);
        }

        public async Task<int> UpdateStoreMaster(Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO updateStoreMaster)
        {
            return await _storeDL.UpdateStoreMaster(updateStoreMaster);
        }

        public async Task<List<Winit.Modules.Store.Model.Interfaces.IStoreMaster>> PrepareStoreMaster(List<string> storeUIDs)
        {
            List<Winit.Modules.Store.Model.Interfaces.IStoreMaster>? storeMastersList = null;
            var (storeList, storeAdditionalInfoList, storeCreditList, storeAttributesList, addressList, contactList) = await _storeDL.PrepareStoreMaster(storeUIDs);
            if (storeList != null && storeList.Count > 0)
            {
                storeMastersList = new List<Model.Interfaces.IStoreMaster>();

                var additionalInfoLookup = storeAdditionalInfoList?.ToLookup(e => e.StoreUID);
                var creditLookup = storeCreditList?.ToLookup(e => e.StoreUID);
                var attributesLookup = storeAttributesList?.ToLookup(e => e.StoreUID);
                var addressLookup = addressList?.ToLookup(e => e.LinkedItemUID);
                var contactLookup = contactList?.ToLookup(e => e.LinkedItemUID);

                foreach (var store in storeList)
                {
                    var storeMaster = _serviceProvider.CreateInstance<IStoreMaster>();

                    storeMaster.Store = store;

                    if (additionalInfoLookup != null)
                        storeMaster.StoreAdditionalInfo = additionalInfoLookup[store.UID].FirstOrDefault();

                    if (creditLookup != null)
                        storeMaster.storeCredits = creditLookup[store.UID].ToList();

                    if (attributesLookup != null)
                        storeMaster.storeAttributes = attributesLookup[store.UID].ToList();

                    if (addressLookup != null)
                        storeMaster.Addresses = addressLookup[store.UID].ToList();

                    if (contactLookup != null)
                        storeMaster.Contacts = contactLookup[store.UID].ToList();

                    storeMastersList.Add(storeMaster);
                }
            }
            return storeMastersList;
        }
        public async Task<PagedResponse<SelectionItem>> GetAllStoreAsSelectionItems(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID)
        {
            return await _storeDL.GetAllStoreAsSelectionItems(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, OrgUID);
        }

        public async Task<PagedResponse<IStoreCustomer>> GetAllStoreItems(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID)
        {
            return await _storeDL.GetAllStoreItems(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, OrgUID);
        }


        public async Task<Model.Interfaces.IStore> SelectStoreByOrgUID(string FranchiseeOrgUID)
        {
            return await _storeDL.SelectStoreByOrgUID(FranchiseeOrgUID);
        }



        public async Task<List<Model.Interfaces.IStoreItemView>> GetStoreByRouteUID(string routeUID, string BeatHistoryUID, bool notInJP)
        {
            return await _storeDL.GetStoreByRouteUID(routeUID, BeatHistoryUID, notInJP);
        }
        public async Task<List<IStoreCustomer>> GetStoreCustomersByRouteUID(string routeUID)
        {
            return await _storeDL.GetStoreCustomersByRouteUID(routeUID);
        }

        public async Task<List<IStoreItemView>> GetStoreByRouteUID(string routeUID)
        {
            return await _storeDL.GetStoreByRouteUID(routeUID);
        }
        
        public async Task<List<IStoreItemView>> GetStoreByRouteUIDWithoutAddress(string routeUID)
        {
            return await _storeDL.GetStoreByRouteUIDWithoutAddress(routeUID);
        }
        public async Task<int> CUDOnBoardCustomerInfo(IOnBoardCustomerDTO onBoardCustomerDTO)
        {
            return await _storeDL.CUDOnBoardCustomerInfo(onBoardCustomerDTO);
        }
        public async Task<int> CreateAllApprovalRequest(IAllApprovalRequest allApprovalRequest)
        {
            return await _storeDL.CreateAllApprovalRequest(allApprovalRequest);
        }

        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview>> SelectAllOnBoardCustomer(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string JobPositionUID, string Role)
        {
            return await _storeDL.SelectAllOnBoardCustomer(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, JobPositionUID, Role);
        }
        public async Task<int> UpdateStoreStatus(StoreApprovalDTO storeApprovalDTO)
        {
            return await _storeDL.UpdateStoreStatus(storeApprovalDTO);
        }
        public async Task<List<IAllApprovalRequest>> GetApprovalDetailsByStoreUID(string LinkItemUID)
        {
            return await _storeDL.GetApprovalDetailsByStoreUID(LinkItemUID);
        }
        public async Task<List<IAllApprovalRequest>> GetApprovalStatusByStoreUID(string LinkItemUID)
        {
            return await _storeDL.GetApprovalStatusByStoreUID(LinkItemUID);
        }
        public async Task<OnBoardEditCustomerDTO> GetAllOnBoardingDetailsByStoreUID(string UID)
        {
            return await _storeDL.GetAllOnBoardingDetailsByStoreUID(UID);
        }
        public async Task<int> CreateAsmDivisionMapping(Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping asmDivisionMapping)
        {
            return await _storeDL.CreateAsmDivisionMapping(asmDivisionMapping);
        }
        public async Task<List<Model.Interfaces.IAsmDivisionMapping>> GetAsmDivisionMappingByUID(string LinkedItemType, string LinkedItemUID, string? asmEmpUID = null)
        {
            return await _storeDL.GetAsmDivisionMappingByUID(LinkedItemType, LinkedItemUID, asmEmpUID);
        }
        public async Task<Model.Interfaces.IAsmDivisionMapping> CheckAsmDivisionMappingRecordExistsOrNot(string UID)
        {
            return await _storeDL.CheckAsmDivisionMappingRecordExistsOrNot(UID);
        }
        public async Task<int> DeleteAsmDivisionMapping(string UID)
        {
            return await _storeDL.DeleteAsmDivisionMapping(UID);
        }
        public async Task<int> CreateChangeRequest(ChangeRequestDTO changeRequestDTO)
        {
            return await _storeDL.CreateChangeRequest(changeRequestDTO);
        }
        public async Task<IEnumerable<string>> GetDivisionsByAsmEmpUID(string asmEmpUID)
        {
            return await _storeDL.GetDivisionsByAsmEmpUID(asmEmpUID);
        }
        public async Task GenerateMyTeam(string jobPositionUid)
        {
            await _storeDL.GenerateMyTeam(jobPositionUid);
        }
        public async Task<bool> IsGstUnique(string GstNumber)
        {
            return await _storeDL.IsGstUnique(GstNumber);
        }
        public async Task<Dictionary<string, int>> GetTabsCount(List<FilterCriteria> filterCriterias, string JobPositionUID, string Role)
        {
            return await _storeDL.GetTabsCount(filterCriterias, JobPositionUID, Role);
        }
        public async Task<List<IStore>> GetApplicableToCustomers(List<string> stores, List<string> broadClassifications, List<string> branches)
        {
            return await _storeDL.GetApplicableToCustomers(stores, broadClassifications, branches);
        }
    }
}
