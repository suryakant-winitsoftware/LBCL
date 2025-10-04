using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Holiday.BL.Interfaces
{
    public interface IHolidayListBL
    {
        Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>> SelectAllHolidayListDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Holiday.Model.Interfaces.IHolidayList> GetHolidayListByHolidayListUID(string uID);
        Task<int> CreateHolidayList(Winit.Modules.Holiday.Model.Interfaces.IHolidayList createHolidayList);
        Task<int> UpdateHolidayList(Winit.Modules.Holiday.Model.Interfaces.IHolidayList updateHolidayList);
        Task<int> DeleteHolidayList(string uID);
    }
}
