using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Territory.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Territory.BL.Classes
{
    public class TerritoryBL : ITerritoryBL
    {
        protected readonly DL.Interfaces.ITerritoryDL _TerritoryDL = null;

        public TerritoryBL(DL.Interfaces.ITerritoryDL TerritoryDL)
        {
            _TerritoryDL = TerritoryDL;
        }

        public async Task<PagedResponse<Winit.Modules.Territory.Model.Interfaces.ITerritory>> SelectAllTerritories(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _TerritoryDL.SelectAllTerritories(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<Winit.Modules.Territory.Model.Interfaces.ITerritory> GetTerritoryByUID(string UID)
        {
            return await _TerritoryDL.GetTerritoryByUID(UID);
        }

        public async Task<Winit.Modules.Territory.Model.Interfaces.ITerritory> GetTerritoryByCode(string territoryCode, string orgUID)
        {
            return await _TerritoryDL.GetTerritoryByCode(territoryCode, orgUID);
        }

        public async Task<List<Winit.Modules.Territory.Model.Interfaces.ITerritory>> GetTerritoriesByOrg(string orgUID)
        {
            return await _TerritoryDL.GetTerritoriesByOrg(orgUID);
        }

        public async Task<List<Winit.Modules.Territory.Model.Interfaces.ITerritory>> GetTerritoriesByManager(string managerEmpUID)
        {
            return await _TerritoryDL.GetTerritoriesByManager(managerEmpUID);
        }

        public async Task<List<Winit.Modules.Territory.Model.Interfaces.ITerritory>> GetTerritoriesByCluster(string clusterCode)
        {
            return await _TerritoryDL.GetTerritoriesByCluster(clusterCode);
        }

        public async Task<int> CreateTerritory(Winit.Modules.Territory.Model.Interfaces.ITerritory createTerritory)
        {
            return await _TerritoryDL.CreateTerritory(createTerritory);
        }

        public async Task<int> UpdateTerritory(Winit.Modules.Territory.Model.Interfaces.ITerritory updateTerritory)
        {
            return await _TerritoryDL.UpdateTerritory(updateTerritory);
        }

        public async Task<int> DeleteTerritory(string UID)
        {
            return await _TerritoryDL.DeleteTerritory(UID);
        }
    }
}
