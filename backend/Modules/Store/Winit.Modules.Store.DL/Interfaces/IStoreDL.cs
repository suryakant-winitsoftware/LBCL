using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Interfaces
{
    public interface IStoreDL
    {
        Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStore>> SelectAllStore(
            List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);

        Task<Model.Interfaces.IStore> SelectStoreByUID(string UID);
        Task<int> CreateStore(Model.Interfaces.IStore store);
        Task<int> UpdateStore(Model.Interfaces.IStore Store);
        Task<int> UpdateAsmDivisionMapping(List<Model.Interfaces.IAsmDivisionMapping> Store);
        Task<int> DeleteStore(string UID);
        Task<int> DeleteOnBoardingDetails(string UID);

        Task<int> CreateStoreMaster(Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO createStoreMaster);
        Task<Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO> SelectStoreMasterByUID(string UID);
        Task<int> UpdateStoreMaster(Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO updateStoreMaster);

        Task<(List<Winit.Modules.Store.Model.Interfaces.IStore>,
            List<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo>,
            List<Winit.Modules.Store.Model.Interfaces.IStoreCredit>,
            List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes>,
            List<Winit.Modules.Address.Model.Interfaces.IAddress>, List<Winit.Modules.Contact.Model.Interfaces.IContact>
            )> PrepareStoreMaster(List<string> storeUIDs);

        Task<PagedResponse<SelectionItem>> GetAllStoreAsSelectionItems(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID);

        Task<Model.Interfaces.IStore> SelectStoreByOrgUID(string FranchiseeOrgUID);

        Task<List<Model.Interfaces.IStoreItemView>> GetStoreByRouteUID(string routeUID, string BeatHistoryUID,
            bool notInJP);

        Task<List<IStoreItemView>> GetStoreByRouteUID(string routeUID);
        Task<List<IStoreItemView>> GetStoreByRouteUIDWithoutAddress(string routeUID);
        Task<List<IStoreCustomer>> GetStoreCustomersByRouteUID(string routeUID);

        Task<PagedResponse<IStoreCustomer>> GetAllStoreItems(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string orgUID);

        Task<int> CUDOnBoardCustomerInfo(IOnBoardCustomerDTO onBoardCustomerDTO);
        Task<int> CreateAllApprovalRequest(IAllApprovalRequest allApprovalRequest);

        Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview>> SelectAllOnBoardCustomer(
            List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string JobPositionUID, string Role);

        Task<int> UpdateStoreStatus(StoreApprovalDTO storeApprovalDTO);
        Task<List<IAllApprovalRequest>> GetApprovalDetailsByStoreUID(string LinkItemUID);
        Task<List<IAllApprovalRequest>> GetApprovalStatusByStoreUID(string LinkItemUID);
        Task<OnBoardEditCustomerDTO> GetAllOnBoardingDetailsByStoreUID(string UID);
        Task<List<IStore>> GetChannelPartner(string jobPositionUid);
        Task<int> CreateAsmDivisionMapping(Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping asmDivisionMapping);

        Task<List<Model.Interfaces.IAsmDivisionMapping>> GetAsmDivisionMappingByUID(string LinkedItemType,
            string LinkedItemUID, string? asmEmpUID = null);

        Task<Model.Interfaces.IAsmDivisionMapping> CheckAsmDivisionMappingRecordExistsOrNot(string UID);
        Task<int> DeleteAsmDivisionMapping(string UID);
        Task<int> CreateChangeRequest(ChangeRequestDTO changeRequestDTO);
        Task<IEnumerable<string>> GetDivisionsByAsmEmpUID(string asmEmpUID);
        Task GenerateMyTeam(string jobPositionUid);
        Task<bool> IsGstUnique(string GstNumber);
        Task<Dictionary<string, int>> GetTabsCount(List<FilterCriteria> filterCriterias, string JobPositionUid, string Role);
        Task<List<IStore>> GetApplicableToCustomers(List<string> stores, List<string> broadClassifications, List<string> branches)
        {
            throw new NotImplementedException("Not implemented for pg sql");
        }
    }
}
