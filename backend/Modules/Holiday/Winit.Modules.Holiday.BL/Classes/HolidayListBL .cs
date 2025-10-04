using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Holiday.BL.Interfaces;
using Winit.Modules.Holiday.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Holiday.BL.Classes
{
    public class HolidayListBL:HolidayBaseBL,IHolidayListBL
    {
        protected readonly DL.Interfaces.IHolidayListDL _IHolidayListDL = null;
        public HolidayListBL(DL.Interfaces.IHolidayListDL holidayListDL)
        {
            _IHolidayListDL = holidayListDL;
        }
        public async Task<PagedResponse<IHolidayList>> SelectAllHolidayListDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
           return await _IHolidayListDL.SelectAllHolidayList(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Holiday.Model.Interfaces.IHolidayList> GetHolidayListByHolidayListUID(string uID)
        {
            return await _IHolidayListDL.GetHolidayListByHolidayListUID(uID);
        }
        public async Task<int> CreateHolidayList(Winit.Modules.Holiday.Model.Interfaces.IHolidayList createHolidayList)
        {
            return await _IHolidayListDL.CreateHolidayList(createHolidayList);
        }
        public async Task<int> UpdateHolidayList(Winit.Modules.Holiday.Model.Interfaces.IHolidayList updateHolidayList)
        {
            return await _IHolidayListDL.UpdateHolidayList(updateHolidayList);
        }
        public async Task<int> DeleteHolidayList(string uID)
        {
            return await _IHolidayListDL.DeleteHolidayList(uID);
        }

    }
}
