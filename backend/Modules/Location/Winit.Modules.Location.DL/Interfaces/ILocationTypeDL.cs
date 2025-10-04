using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Interfaces
{
    public interface ILocationTypeDL
    {
        Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationType>> SelectAllLocationTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Location.Model.Interfaces.ILocationType?> GetLocationTypeByUID(string UID);
        Task<int> CreateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationType createLocationType);
        Task<int> UpdateLocationTypeDetails(Winit.Modules.Location.Model.Interfaces.ILocationType updateLocationType);
        Task<int> DeleteLocationTypeDetails(string UID);
    }
}
