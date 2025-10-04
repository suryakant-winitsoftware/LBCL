using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Interfaces
{
    public interface IOrgBL
    {
        Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Org.Model.Interfaces.IOrg> GetOrgByUID(string UID);
        Task<int> CreateOrg(Winit.Modules.Org.Model.Interfaces.IOrg createOrg);
        Task<int> CreateOrgBulk(List<Winit.Modules.Org.Model.Interfaces.IOrg> createOrg);
        Task<int> UpdateOrg(Winit.Modules.Org.Model.Interfaces.IOrg updateOrg);
        Task<int> DeleteOrg(string UID);
        Task<int> InsertOrgHierarchy();
        Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> PrepareOrgMaster();
        Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>> ViewFranchiseeWarehouse(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID);
        Task<Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView> ViewFranchiseeWarehouseByUID(string UID);
        Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetWarehouseStockDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string FranchiseeOrgUID, string WarehouseUID, string StockType);
        Task<int> CreateViewFranchiseeWarehouse(Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView createWareHouseItemView);
        Task<int> UpdateViewFranchiseeWarehouse(Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView createWareHouseItemView);
        Task<int> DeleteViewFranchiseeWarehouse(string UID);
        Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IOrgType>> GetOrgTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgByOrgTypeUID(string OrgTypeUID, string? parentUID = null, string? branchUID = null);
        Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrg>> GetOrgByOrgTypeUID(string OrgTypeUID, string? parentUID = null);
        Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IFOCStockItem>> GetFOCStockItemDetails(string StockType);

        // Vanstock
        Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>> GetVanStockItems(string warehouseUID, string orgUID, StockType? stockType);
        Task<List<string>> GetOrgHierarchyParentUIDsByOrgUID(List<string> orgs);
        Task<List<ISelectionItem>> GetProductOrgSelectionItems();
        Task<List<ISelectionItem>> GetProductDivisionSelectionItems();
        Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetDivisions(string storeUID);



        Task<PagedResponse<Winit.Modules.Org.Model.Interfaces.IWareHouseStock>> GetAllWareHouseStock(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<IEnumerable<ISKUGroup>> GetSkuGroupBySkuGroupTypeUID(string skuGroupTypeUid);
        Task<IEnumerable<IOrg>> GetDeliveryDistributorsByOrgUID(string orgUID, string storeUID);
    }
}
