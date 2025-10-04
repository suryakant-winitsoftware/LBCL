using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Interfaces
{
    public interface ILocationDL
    {
        Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocation>> SelectAllLocationDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Location.Model.Interfaces.ILocation?> GetLocationByUID(string UID);
        Task<Winit.Modules.Location.Model.Interfaces.ILocation?> GetCountryAndRegionByState(string UID, string Type);
        Task<List<Winit.Modules.Location.Model.Interfaces.ILocation>> GetLocationByType(List<string> locationTypes);
        Task<int> CreateLocation(Winit.Modules.Location.Model.Interfaces.ILocation createLocation);
        Task<int> UpdateLocationDetails(Winit.Modules.Location.Model.Interfaces.ILocation updateLocation);
        Task<int> DeleteLocationDetails(string UID);
        Task<List<Winit.Modules.Location.Model.Interfaces.ILocation>> GetCityandLoaclityByUIDs(List<string> UIDs);
    }
}
