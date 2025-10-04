using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Interfaces
{
    public interface ILocationMappingBL
    {
        Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationMapping>> SelectAllLocationMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Location.Model.Interfaces.ILocationMapping> GetLocationMappingByUID(string UID);
        Task<int> CreateLocationMapping(Winit.Modules.Location.Model.Interfaces.ILocationMapping createLocationMapping);
        Task<int> InsertLocationHierarchy(string type, string uid);
        Task<int> UpdateLocationMappingDetails(Winit.Modules.Location.Model.Interfaces.ILocationMapping updateLocationMapping);
        Task<int> DeleteLocationMappingDetails(string UID);
        Task<List<ILocationData>> GetLocationMaster();

        
    }
}
