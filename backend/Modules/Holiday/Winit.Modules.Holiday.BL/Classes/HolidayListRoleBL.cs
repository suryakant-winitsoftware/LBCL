using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Holiday.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Holiday.BL.Classes
{
    public class HolidayListRoleBL:HolidayBaseBL,IHolidayListRoleBL
    {
        protected readonly DL.Interfaces.IHolidayListRoleDL _IHolidayListRoleDL = null;
        public HolidayListRoleBL(DL.Interfaces.IHolidayListRoleDL holidayListRoleDL)
        {
            _IHolidayListRoleDL = holidayListRoleDL;
        }
        public async  Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole>> GetHolidayListRoleDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _IHolidayListRoleDL.GetHolidayListRoleDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async  Task<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole> GetHolidayListRoleByUID(string UID)
        {
            return await _IHolidayListRoleDL.GetHolidayListRoleByUID(UID);
        }
        public async  Task<int> CreateHolidayListRole(Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole CreateHolidayListRole)
        {
            return await _IHolidayListRoleDL.CreateHolidayListRole(CreateHolidayListRole);
        }

        public async  Task<int> UpdateHolidayListRole(Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole updateHolidayListRole)
        {
            return await _IHolidayListRoleDL.UpdateHolidayListRole(updateHolidayListRole);
        }

        public async  Task<int> DeleteHolidayListRole(string UID)
        {
            return await _IHolidayListRoleDL.DeleteHolidayListRole(UID);
        }
    }
}
