using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Bank.PostgreSQLBankRepository;

namespace WINITServices.Classes.Settings
{
    public abstract class SettingsBaseService : Interfaces.ISettingsService
    {
        protected readonly WINITRepository.Interfaces.Settings.ISettingsRepository _settingsRepository;
        public SettingsBaseService(WINITRepository.Interfaces.Settings.ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }
      public abstract  Task<IEnumerable<WINITSharedObjects.Models.Settings>> GetSettingDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias);

        public abstract Task<WINITSharedObjects.Models.Settings> GetSettingById(int Id);

        public abstract Task<WINITSharedObjects.Models.Settings> CreateSetting(WINITSharedObjects.Models.Settings createSetting);
        public abstract Task<int> UpdateSetting(WINITSharedObjects.Models.Settings updateSetting);
        public abstract Task<int> DeleteSetting(int Id);

    }
}
