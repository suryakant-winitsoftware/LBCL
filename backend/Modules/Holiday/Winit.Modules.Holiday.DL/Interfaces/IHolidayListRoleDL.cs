using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Holiday.DL.Interfaces
{
    public interface IHolidayListRoleDL
    {
        Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole>> GetHolidayListRoleDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole> GetHolidayListRoleByUID(string UID);
        Task<int> CreateHolidayListRole(Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole CreateHolidayListRole);
        Task<int> UpdateHolidayListRole(Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole updateHolidayList);
        Task<int> DeleteHolidayListRole(string UID);
    }
}
