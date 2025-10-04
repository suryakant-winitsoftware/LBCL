using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Holiday.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Holiday.BL.Classes
{
    public class HolidayBL:HolidayBaseBL,IHolidayBL
    {
        protected readonly DL.Interfaces.IHolidayDL _HolidayDL = null;
        public HolidayBL(DL.Interfaces.IHolidayDL holidayDL)
        {
            _HolidayDL = holidayDL;
        }
        public async  Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHoliday>> GetHolidayDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _HolidayDL.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async  Task<Winit.Modules.Holiday.Model.Interfaces.IHoliday> GetHolidayByOrgUID(string UID)
        {
            return await _HolidayDL.GetHolidayByOrgUID(UID);
        }
        public async  Task<int> CreateHoliday(Winit.Modules.Holiday.Model.Interfaces.IHoliday createHoliday)
        {
            return await _HolidayDL.CreateHoliday(createHoliday);
        }

        public async  Task<int> UpdateHoliday(Winit.Modules.Holiday.Model.Interfaces.IHoliday updateHoliday)
        {
            return await _HolidayDL.UpdateHoliday(updateHoliday);
        }

        public async  Task<int> DeleteHoliday(string UID)
        {
            return await _HolidayDL.DeleteHoliday(UID);
        }
    }
}
