using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
//using static WINITRepository.Classes.Customers.PostgreSQLCustomerRepository;

namespace WINITServices.Interfaces
{
    public interface IHolidayService
    {
        Task<IEnumerable<WINITSharedObjects.Models.HolidayDetails>> HolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias);

        //Task<DataSet> HolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        //int pageSize, List<FilterCriteria> filterCriterias);


        Task<IEnumerable<WINITSharedObjects.Models.Holiday>> GetHolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

        //Task<DataSet> GetHolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        //  int pageSize, List<FilterCriteria> filterCriterias);


        Task<IEnumerable<WINITSharedObjects.Models.HolidayListRole>> GetHolidayListRoleDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias);

        Task<WINITSharedObjects.Models.Holiday> GetHolidayByOrgUID(string holidayListUID);

        Task<WINITSharedObjects.Models.Holiday> CreateHoliday(Holiday createHoliday);
        Task<int> UpdateHoliday(WINITSharedObjects.Models.Holiday updateHoliday);
        Task<int> DeleteHoliday(string holidayListUID);

        //HOLIDAYLISTROLE

        Task<WINITSharedObjects.Models.HolidayListRole> GetHolidayListRoleByHolidayListUID(string holidayListUID);
      
        Task<WINITSharedObjects.Models.HolidayListRole> CreateHolidayListRole(WINITSharedObjects.Models.HolidayListRole CreateHolidayListRole);
        Task<int> UpdateHolidayListRole(WINITSharedObjects.Models.HolidayListRole updateHolidayListRole);
        Task<int> DeleteHolidayListRole(string holidayListUID);


        //HolidayList


        Task<WINITSharedObjects.Models.HolidayList> GetHolidayListByHolidayListUID(string uID);

        Task<WINITSharedObjects.Models.HolidayList> CreateHolidayList(WINITSharedObjects.Models.HolidayList createHolidayList);
        Task<int> UpdateHolidayList(WINITSharedObjects.Models.HolidayList updateHolidayList);
        Task<int> DeleteHolidayList(string uID);
    }
}
