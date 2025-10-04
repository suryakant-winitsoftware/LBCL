using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WINITRepository.Interfaces.Products;
using WINITServices.Classes.Currency;
using WINITServices.Classes.Holiday;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace WINITServices.Classes.Holiday
{
    public class HolidayService : HolidayBaseService
    {
        public HolidayService(WINITRepository.Interfaces.Holiday.IHolidayRepository holidayRepository) : base(holidayRepository)
        {

        }
        public async override Task<IEnumerable<WINITSharedObjects.Models.HolidayDetails>> HolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _holidayRepository.HolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        }


        //public async override Task<DataSet> HolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        // int pageSize, List<FilterCriteria> filterCriterias)
        //{
        //    return await _holidayRepository.HolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        //}

        public async override Task<IEnumerable<WINITSharedObjects.Models.Holiday>> GetHolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _holidayRepository.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        }

        //public async override Task<DataSet> GetHolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        //    int pageSize, List<FilterCriteria> filterCriterias)
        //{
        //    return await _holidayRepository.GetHolidayDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        //}


        public async override Task<IEnumerable<WINITSharedObjects.Models.HolidayListRole>> GetHolidayListRoleDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _holidayRepository.GetHolidayListRoleDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        }

        public async override Task<WINITSharedObjects.Models.Holiday> GetHolidayByOrgUID(string holidayListUID)
        {
            return await _holidayRepository.GetHolidayByOrgUID(holidayListUID);
        }
        public async override Task<WINITSharedObjects.Models.Holiday> CreateHoliday(WINITSharedObjects.Models.Holiday createHoliday)
        {
            return await _holidayRepository.CreateHoliday(createHoliday);
        }

        public async override Task<int> UpdateHoliday(WINITSharedObjects.Models.Holiday updateHoliday)
        {
            return await _holidayRepository.UpdateHoliday(updateHoliday);
        }

        public async override Task<int> DeleteHoliday(string holidayListUID)
        {
            return await _holidayRepository.DeleteHoliday(holidayListUID);
        }



        //HOLIDAYLISTROLE
        public async override Task<WINITSharedObjects.Models.HolidayListRole> GetHolidayListRoleByHolidayListUID(string holidayListUID)
        {
            return await _holidayRepository.GetHolidayListRoleByHolidayListUID(holidayListUID);
        }
        public async override Task<WINITSharedObjects.Models.HolidayListRole> CreateHolidayListRole(WINITSharedObjects.Models.HolidayListRole CreateHolidayListRole)
        {
            return await _holidayRepository.CreateHolidayListRole(CreateHolidayListRole);
        }

        public async override Task<int> UpdateHolidayListRole(WINITSharedObjects.Models.HolidayListRole updateHolidayListRole)
        {
            return await _holidayRepository.UpdateHolidayListRole(updateHolidayListRole);
        }

        public async override Task<int> DeleteHolidayListRole(string holidayListUID)
        {
            return await _holidayRepository.DeleteHolidayListRole(holidayListUID);
        }


        //HolidayList
        public async override Task<WINITSharedObjects.Models.HolidayList> GetHolidayListByHolidayListUID(string uID)
        {
            return await _holidayRepository.GetHolidayListByHolidayListUID(uID);
        }
        public async override Task<WINITSharedObjects.Models.HolidayList> CreateHolidayList(WINITSharedObjects.Models.HolidayList createHolidayList)
        {
            return await _holidayRepository.CreateHolidayList(createHolidayList);
        }

        public async override Task<int> UpdateHolidayList(WINITSharedObjects.Models.HolidayList updateHolidayList)
        {
            return await _holidayRepository.UpdateHolidayList(updateHolidayList);
        }

        public async override Task<int> DeleteHolidayList(string uID)
        {
            return await _holidayRepository.DeleteHolidayList(uID);
        }
    }
}
