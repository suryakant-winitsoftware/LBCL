using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Classes
{
    public class LocationMappingBL:ILocationMappingBL
    {
        protected readonly DL.Interfaces.ILocationMappingDL _LocationMappingBL = null;
        public LocationMappingBL(DL.Interfaces.ILocationMappingDL LocationMappingBL)
        {
            _LocationMappingBL = LocationMappingBL;
        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationMapping>> SelectAllLocationMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _LocationMappingBL.SelectAllLocationMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.ILocationMapping> GetLocationMappingByUID(string UID)
        {
            return await _LocationMappingBL.GetLocationMappingByUID(UID);
        }
        public async Task<int> CreateLocationMapping(Winit.Modules.Location.Model.Interfaces.ILocationMapping createLocationMapping)
        {
            return await _LocationMappingBL.CreateLocationMapping(createLocationMapping);
        }
        public async Task<int> InsertLocationHierarchy(string type, string uid)
        {
            return await _LocationMappingBL.InsertLocationHierarchy(type, uid);
        }
        public async Task<int> UpdateLocationMappingDetails(Winit.Modules.Location.Model.Interfaces.ILocationMapping updateLocationMapping)
        {
            return await _LocationMappingBL.UpdateLocationMappingDetails(updateLocationMapping);
        }
        public async Task<int> DeleteLocationMappingDetails(string Code)
        {
            return await _LocationMappingBL.DeleteLocationMappingDetails(Code);
        }
        public async Task<List<ILocationData>> GetLocationMaster()
        {
            return await _LocationMappingBL.GetLocationMaster();
        }
       
    }
}
