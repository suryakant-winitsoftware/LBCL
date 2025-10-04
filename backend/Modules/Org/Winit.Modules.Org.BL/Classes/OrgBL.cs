using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Org.DL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Classes
{
    public class OrgBL : IOrgBL
    {
        protected readonly DL.Interfaces.IOrgDL _orgRepository = null;
        public OrgBL(DL.Interfaces.IOrgDL orgRepository)
        {
            _orgRepository = orgRepository;
        }
        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _orgRepository.GetOrgDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Org.Model.Interfaces.IOrg> GetOrgByUID(string UID)
        {
            return await _orgRepository.GetOrgByUID(UID);
        }
        public async Task<int> CreateOrg(Winit.Modules.Org.Model.Interfaces.IOrg createOrg)
        {
            return await _orgRepository.CreateOrg(createOrg);
        }
        public async Task<int> CreateOrgBulk(List<Winit.Modules.Org.Model.Interfaces.IOrg> createOrg)
        {
            return await _orgRepository.CreateOrgBulk(createOrg);
        }
        public async Task<int> UpdateOrg(Winit.Modules.Org.Model.Interfaces.IOrg updateOrg)
        {
            return await _orgRepository.UpdateOrg(updateOrg);
        }
        public async Task<int> DeleteOrg(string Code)
        {
            return await _orgRepository.DeleteOrg(Code);
        }
        public async Task<int> InsertOrgHierarchy()
        {
            return await _orgRepository.InsertOrgHierarchy();
        }
        public async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> PrepareOrgMaster()
        {
            return await _orgRepository.PrepareOrgMaster();

        }
        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>> ViewFranchiseeWarehouse(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID)
        {
            return await _orgRepository.ViewFranchiseeWarehouse(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, FranchiseeOrgUID);
        }
        public async Task<Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView> ViewFranchiseeWarehouseByUID(string UID)
        {
            return await _orgRepository.ViewFranchiseeWarehouseByUID(UID);
        }
        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetWarehouseStockDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID, string WarehouseUID, string StockType)
        {
            return await _orgRepository.GetWarehouseStockDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, FranchiseeOrgUID, WarehouseUID, StockType);
        }
        public async Task<int> CreateViewFranchiseeWarehouse(IEditWareHouseItemView createWareHouseItemView)
        {
            return await _orgRepository.CreateViewFranchiseeWarehouse(createWareHouseItemView);
        }
        public async Task<int> UpdateViewFranchiseeWarehouse(Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView updateWareHouseItemView)
        {
            return await _orgRepository.UpdateViewFranchiseeWarehouse(updateWareHouseItemView);
        }
        public async Task<int> DeleteViewFranchiseeWarehouse(string UID)
        {
            return await _orgRepository.DeleteViewFranchiseeWarehouse(UID);
        }
        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrgType>> GetOrgTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _orgRepository.GetOrgTypeDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgByOrgTypeUID(string OrgTypeUID, string? parentUID = null, string? branchUID = null)
        {
            return await _orgRepository.GetOrgByOrgTypeUID(OrgTypeUID, parentUID, branchUID);
        }
        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgByOrgTypeUID(string OrgTypeUID, string? parentUID = null)
        {
            return await _orgRepository.GetOrgByOrgTypeUID(OrgTypeUID, parentUID);
        }
        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IFOCStockItem>> GetFOCStockItemDetails(string StockType)
        {
            return await _orgRepository.GetFOCStockItemDetails(StockType);
        }

        public async Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetVanStockItems(string warehouseUID, string orgUID,
            StockType? stockType)
        {
            return await _orgRepository.GetVanStockItems(warehouseUID, orgUID, stockType);
        }
        public async Task<IEnumerable<ISKUGroup>> GetSkuGroupBySkuGroupTypeUID(string skuGroupTypeUid)
        {
            return await _orgRepository.GetSkuGroupBySkuGroupTypeUID(skuGroupTypeUid);
        }
        public async Task<List<string>> GetOrgHierarchyParentUIDsByOrgUID(List<string> orgs)
        {
            return await _orgRepository.GetOrgHierarchyParentUIDsByOrgUID(orgs);
        }
        public async Task<List<ISelectionItem>> GetProductOrgSelectionItems()
        {
            return await _orgRepository.GetProductOrgSelectionItems();
        }
        public async Task<List<ISelectionItem>> GetProductDivisionSelectionItems()
        {
            return await _orgRepository.GetProductDivisionSelectionItems();
        }

        public async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetDivisions(string storeUID)
        {
            return await _orgRepository.GetDivisions(storeUID);
        }
        public async Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IWareHouseStock>> GetAllWareHouseStock(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _orgRepository.GetAllWareHouseStock(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<IEnumerable<IOrg>> GetDeliveryDistributorsByOrgUID(string orgUID, string storeUID)
        {
            return await _orgRepository.GetDeliveryDistributorsByOrgUID(orgUID, storeUID);
        }
    }
}
