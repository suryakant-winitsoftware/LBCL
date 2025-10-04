using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Holiday.BL.Interfaces
{
    public interface IHolidayBL
    {
        Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHoliday>> GetHolidayDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Holiday.Model.Interfaces.IHoliday> GetHolidayByOrgUID(string UID);

        Task<int> CreateHoliday(Winit.Modules.Holiday.Model.Interfaces.IHoliday createHoliday);
        Task<int> UpdateHoliday(Winit.Modules.Holiday.Model.Interfaces.IHoliday updateHoliday);
        Task<int> DeleteHoliday(string UID);
    }
}
