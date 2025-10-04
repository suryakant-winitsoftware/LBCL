using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Territory.DL.Interfaces
{
    public interface ITerritoryDL
    {
        Task<PagedResponse<Winit.Modules.Territory.Model.Interfaces.ITerritory>> SelectAllTerritories(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Territory.Model.Interfaces.ITerritory?> GetTerritoryByUID(string UID);
        Task<Winit.Modules.Territory.Model.Interfaces.ITerritory?> GetTerritoryByCode(string territoryCode, string orgUID);
        Task<List<Winit.Modules.Territory.Model.Interfaces.ITerritory>> GetTerritoriesByOrg(string orgUID);
        Task<List<Winit.Modules.Territory.Model.Interfaces.ITerritory>> GetTerritoriesByManager(string managerEmpUID);
        Task<List<Winit.Modules.Territory.Model.Interfaces.ITerritory>> GetTerritoriesByCluster(string clusterCode);
        Task<int> CreateTerritory(Winit.Modules.Territory.Model.Interfaces.ITerritory createTerritory);
        Task<int> UpdateTerritory(Winit.Modules.Territory.Model.Interfaces.ITerritory updateTerritory);
        Task<int> DeleteTerritory(string UID);
    }
}
