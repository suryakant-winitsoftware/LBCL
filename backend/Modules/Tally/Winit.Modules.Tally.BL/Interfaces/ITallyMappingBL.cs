using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.BL.Interfaces
{
    public interface ITallyMappingBL
    {
        Task<PagedResponse<ITallySKU>> GetAllTallySKU(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<ITallySKUMapping>> GetAllTallySKUMappingByDistCode(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string Code, string Tab);
        Task<bool> InsertTallySKUMapping(ITallySKUMapping tallySKUMapping);
        Task<bool> UpdateTallySKUMapping(ITallySKUMapping tallySKUMapping);
        Task<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> SelectTallySKUMappingBySKUCode(string OrgUID, string DistCode);
        Task<List<Winit.Modules.SKU.Model.Interfaces.ISKU>> SelectSKUByOrgUID(string OrgUID);

        Task<ITallyConfigurationResponse> GetTallyConfigurationData(string DistCode);
        Task<bool> InsertRetailersFromTally(List<IRetailersFromTally> retailersFromTally);
        Task<bool> InsertInventoryFromTally(List<IInventoryFromTally> inventoryFromTally);
        Task<bool> InsertOrdersFromTally(List<ISalesOrderHeaderFromTally> ordersFromTally);
        Task<List<ITallySKU>> GetDistMappedSKUList(string DistCode);
        Task<List<IRetailersFromDB>> GetRetailersFromDB(string orgUID);
        Task<bool> RetailerStatusFromTally(List<IRetailerTallyStatus> retailerStatusFromTally);
        Task<List<IEmp>> GetAllDistributors();
        Task<List<ISalesOrderHeaderFromDB>> GetSalesOrderFromDB(string orgUID);
        Task<bool> SalesStatusFromTally(List<ISalesTallyStatus> salesStatusFromTally);
    }
}
