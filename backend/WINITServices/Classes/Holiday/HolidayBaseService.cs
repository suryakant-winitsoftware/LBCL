using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Bank.PostgreSQLBankRepository;

namespace WINITServices.Classes.Holiday
{
    public abstract class HolidayBaseService : Interfaces.IHolidayService
    {
        protected readonly WINITRepository.Interfaces.Holiday.IHolidayRepository _holidayRepository;
        public HolidayBaseService(WINITRepository.Interfaces.Holiday.IHolidayRepository holidayRepository)
        {
            _holidayRepository = holidayRepository;
        }

        public abstract Task<IEnumerable<WINITSharedObjects.Models.HolidayDetails>> HolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias);
        //public abstract Task<DataSet> HolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        //     int pageSize, List<FilterCriteria> filterCriterias);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Holiday>> GetHolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
               int pageSize, List<FilterCriteria> filterCriterias);

        //public abstract Task<DataSet> GetHolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        // int pageSize, List<FilterCriteria> filterCriterias);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.HolidayListRole>> GetHolidayListRoleDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

        public abstract Task<WINITSharedObjects.Models.Holiday> GetHolidayByOrgUID(string holidayListUID);

        public abstract Task<WINITSharedObjects.Models.Holiday> CreateHoliday(WINITSharedObjects.Models.Holiday createHoliday);
        public abstract Task<int> UpdateHoliday(WINITSharedObjects.Models.Holiday updateHoliday);
        public abstract Task<int> DeleteHoliday(string holidayListUID);

        //HOLIDAYLISTROLE
    public abstract  Task<WINITSharedObjects.Models.HolidayListRole> GetHolidayListRoleByHolidayListUID(string holidayListUID);
        public abstract Task<WINITSharedObjects.Models.HolidayListRole> CreateHolidayListRole(WINITSharedObjects.Models.HolidayListRole CreateHolidayListRole);
       public abstract Task<int> UpdateHolidayListRole(WINITSharedObjects.Models.HolidayListRole updateHolidayListRole);
      public abstract  Task<int> DeleteHolidayListRole(string holidayListUID);

        //HolidayList


       public abstract Task<WINITSharedObjects.Models.HolidayList> GetHolidayListByHolidayListUID(string uID);

       public abstract Task<WINITSharedObjects.Models.HolidayList> CreateHolidayList(WINITSharedObjects.Models.HolidayList createHolidayList);
       public abstract Task<int> UpdateHolidayList(WINITSharedObjects.Models.HolidayList updateHolidayList);
       public abstract Task<int> DeleteHolidayList(string uID);

    }
}
