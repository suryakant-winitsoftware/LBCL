using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Setting.BL.Classes;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Setting.BL.Classes
{
    public class SettingBL : SettingBaseBL, ISettingBL 
    { 
        protected readonly DL.Interfaces.ISettingDL _SettingDL = null;
        public SettingBL(Winit.Modules.Setting.DL.Interfaces.ISettingDL settingDL)
        {
            _SettingDL = settingDL;
        }
        public async  Task<PagedResponse<Winit.Modules.Setting.Model.Interfaces.ISetting>> SelectAllSettingDetails(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _SettingDL.SelectAllSettingDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async  Task<Winit.Modules.Setting.Model.Interfaces.ISetting> GetSettingByUID(string UID)
        {
            return await _SettingDL.GetSettingByUID(UID);
        }
        public async  Task<int> CreateSetting(Winit.Modules.Setting.Model.Interfaces.ISetting createSetting)
        {
            return await _SettingDL.CreateSetting(createSetting);
        }

        public async  Task<int> UpdateSetting(Winit.Modules.Setting.Model.Interfaces.ISetting updateSetting)
        {
            return await _SettingDL.UpdateSetting(updateSetting);
        }

        public async  Task<int> DeleteSetting(string UID)
        {
            return await _SettingDL.DeleteSetting(UID);
        }
    }
}
