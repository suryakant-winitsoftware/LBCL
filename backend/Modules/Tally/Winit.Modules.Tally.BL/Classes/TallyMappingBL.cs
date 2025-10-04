using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.Tally.BL.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.BL.Classes
{
    public class TallyMappingBL : ITallyMappingBL
    {
        protected readonly Winit.Modules.Tally.DL.Interfaces.ITallyMappingDL _tallyMappingDL;
        private readonly IServiceProvider? _serviceProvider = null;
        public TallyMappingBL(Winit.Modules.Tally.DL.Interfaces.ITallyMappingDL tallyMappingDL, IServiceProvider serviceProvider)
        {
            _tallyMappingDL = tallyMappingDL;
            _serviceProvider = serviceProvider;
        }
        public async Task<PagedResponse<ITallySKU>> GetAllTallySKU(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _tallyMappingDL.GetAllTallySKU(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        
        public async Task<PagedResponse<ITallySKUMapping>> GetAllTallySKUMappingByDistCode(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string Code, string Tab)
        {
            return await _tallyMappingDL.GetAllTallySKUMappingByDistCode(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, Code, Tab);
        }

        public async Task<bool> InsertTallySKUMapping(ITallySKUMapping tallySKUMapping)
        {
            return await _tallyMappingDL.InsertTallySKUMapping(tallySKUMapping);
        }
        public async Task<bool> UpdateTallySKUMapping(ITallySKUMapping tallySKUMapping)
        {
            return await _tallyMappingDL.UpdateTallySKUMapping(tallySKUMapping);
        }
        public async Task<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping> SelectTallySKUMappingBySKUCode(string OrgUID, string DistCode)
        {
            return await _tallyMappingDL.SelectTallySKUMappingBySKUCode(OrgUID, DistCode);
        }

        public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKU>> SelectSKUByOrgUID(string OrgUID)
        {
            return await _tallyMappingDL.SelectSKUByOrgUID(OrgUID);
        }
        public async Task<ITallyConfigurationResponse> GetTallyConfigurationData(string DistCode)
        {
            return await _tallyMappingDL.GetTallyConfigurationData(DistCode);
        }
        public async Task<bool> InsertRetailersFromTally(List<IRetailersFromTally> retailersFromTally)
        {
            return await _tallyMappingDL.InsertRetailersFromTally(retailersFromTally);
        }
        public async Task<bool> InsertInventoryFromTally(List<IInventoryFromTally> inventoryFromTally)
        {
            return await _tallyMappingDL.InsertInventoryFromTally(inventoryFromTally);
        }
        public async Task<bool> InsertOrdersFromTally(List<ISalesOrderHeaderFromTally> ordersFromTally)
        {
            return await _tallyMappingDL.InsertOrdersFromTally(ordersFromTally);
        }
        public async Task<List<ITallySKU>> GetDistMappedSKUList(string DistCode)
        {
            return await _tallyMappingDL.GetDistMappedSKUList(DistCode);
        }
        public async Task<List<IRetailersFromDB>> GetRetailersFromDB(string orgUID)
        {
            return await _tallyMappingDL.GetRetailersFromDB(orgUID);
        }
        public async Task<bool> RetailerStatusFromTally(List<IRetailerTallyStatus> retailerStatusFromTally)
        {
            return await _tallyMappingDL.RetailerStatusFromTally(retailerStatusFromTally);
        }
        public async Task<List<IEmp>> GetAllDistributors()
        {
            return await _tallyMappingDL.GetAllDistributors();
        }
        public async Task<List<ISalesOrderHeaderFromDB>> GetSalesOrderFromDB(string orgUID)
        {
            return await _tallyMappingDL.GetSalesOrderFromDB(orgUID);
        }
        public async Task<bool> SalesStatusFromTally(List<ISalesTallyStatus> salesStatusFromTally)
        {
            return await _tallyMappingDL.SalesStatusFromTally(salesStatusFromTally);
        }
    }
}
