using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Bank.PostgreSQLBankRepository;

namespace WINITRepository.Interfaces.Settings
{
    public interface ISettingsRepository
    {
        Task<IEnumerable<WINITSharedObjects.Models.Settings>> GetSettingDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias);

        Task<WINITSharedObjects.Models.Settings> GetSettingById(int Id);

        Task<WINITSharedObjects.Models.Settings> CreateSetting(WINITSharedObjects.Models.Settings createSetting);
        Task<int> UpdateSetting(WINITSharedObjects.Models.Settings updateSetting);
        Task<int> DeleteSetting(int Id);
    }
}
