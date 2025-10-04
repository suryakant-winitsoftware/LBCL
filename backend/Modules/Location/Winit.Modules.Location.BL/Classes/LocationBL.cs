using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Classes
{
    public class LocationBL:ILocationBL
    {
        protected readonly DL.Interfaces.ILocationDL _LocationBL = null;
        public LocationBL(DL.Interfaces.ILocationDL LocationBL)
        {
            _LocationBL = LocationBL;
        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocation>> SelectAllLocationDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _LocationBL.SelectAllLocationDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.ILocation> GetLocationByUID(string UID)
        {
            return await _LocationBL.GetLocationByUID(UID);
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.ILocation> GetCountryAndRegionByState(string UID, string Type)
        {
            return await _LocationBL.GetCountryAndRegionByState(UID,Type);
        }

        public async Task<List<Winit.Modules.Location.Model.Interfaces.ILocation>> GetLocationByType(List<string> locationTypes)
        {
            return await _LocationBL.GetLocationByType(locationTypes);
        }
        public async Task<int> CreateLocation(Winit.Modules.Location.Model.Interfaces.ILocation createLocation)
        {
            return await _LocationBL.CreateLocation(createLocation);
        }
        public async Task<int> UpdateLocationDetails(Winit.Modules.Location.Model.Interfaces.ILocation updateLocation)
        {
            return await _LocationBL.UpdateLocationDetails(updateLocation);
        }
        public async Task<int> DeleteLocationDetails(string Code)
        {
            return await _LocationBL.DeleteLocationDetails(Code);
        }
        public async Task<List<Winit.Modules.Location.Model.Interfaces.ILocation>> GetCityandLoaclityByUIDs(List<string> UIDs)
        {
            return await _LocationBL.GetCityandLoaclityByUIDs(UIDs);
        }
    }
}
