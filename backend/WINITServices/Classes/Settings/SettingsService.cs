using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WINITRepository.Interfaces.Products;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Settings.PostgreSQLSettingsRepository;

namespace WINITServices.Classes.Settings
{
    public class SettingsService : SettingsBaseService
    {
        public SettingsService(WINITRepository.Interfaces.Settings.ISettingsRepository settingsRepository) : base(settingsRepository)
        {

        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.Settings>> GetSettingDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _settingsRepository.GetSettingDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        }

        public async override Task<WINITSharedObjects.Models.Settings> GetSettingById(int Id)
        {
            return await _settingsRepository.GetSettingById(Id);
        }
        public async override Task<WINITSharedObjects.Models.Settings> CreateSetting(WINITSharedObjects.Models.Settings createSetting)
        {
            return await _settingsRepository.CreateSetting(createSetting);
        }

        public async override Task<int> UpdateSetting(WINITSharedObjects.Models.Settings updateSetting)
        {
            return await _settingsRepository.UpdateSetting(updateSetting);
        }

        public async override Task<int> DeleteSetting(int Id)
        {
            return await _settingsRepository.DeleteSetting(Id);
        }
    }
}
