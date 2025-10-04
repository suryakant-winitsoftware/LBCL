using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
//using static WINITRepository.Classes.Customers.PostgreSQLCustomerRepository;

namespace WINITServices.Interfaces
{
    public interface ISettingsService
    {
        Task<IEnumerable<WINITSharedObjects.Models.Settings>> GetSettingDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

       Task<WINITSharedObjects.Models.Settings> GetSettingById(int Id);

        Task<WINITSharedObjects.Models.Settings> CreateSetting(Settings createSetting);
        Task<int> UpdateSetting(Settings updateSetting);
        Task<int> DeleteSetting(int Id);
    }
}
