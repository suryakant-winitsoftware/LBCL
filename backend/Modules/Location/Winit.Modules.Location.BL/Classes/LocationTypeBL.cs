using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Classes
{
    public class LocationTypeBL:ILocationTypeBL
    {
        protected readonly DL.Interfaces.ILocationTypeDL _LocationTypeBL = null;
        public LocationTypeBL(DL.Interfaces.ILocationTypeDL LocationTypeBL)
        {
            _LocationTypeBL = LocationTypeBL;
        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationType>> SelectAllLocationTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _LocationTypeBL.SelectAllLocationTypeDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.ILocationType> GetLocationTypeByUID(string UID)
        {
            return await _LocationTypeBL.GetLocationTypeByUID(UID);
        }
        public async Task<int> CreateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationType createLocationType)
        {
            return await _LocationTypeBL.CreateLocationType(createLocationType);
        }
        public async Task<int> UpdateLocationTypeDetails(Winit.Modules.Location.Model.Interfaces.ILocationType updateLocationType)
        {
            return await _LocationTypeBL.UpdateLocationTypeDetails(updateLocationType);
        }
        public async Task<int> DeleteLocationTypeDetails(string Code)
        {
            return await _LocationTypeBL.DeleteLocationTypeDetails(Code);
        }
    }
}
