using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Setting.BL.Interfaces
{
    public interface ISettingBL
    {
        Task<PagedResponse<Winit.Modules.Setting.Model.Interfaces.ISetting>> SelectAllSettingDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Setting.Model.Interfaces.ISetting> GetSettingByUID(string UID);
        Task<int> CreateSetting(Winit.Modules.Setting.Model.Interfaces.ISetting createSetting);
        Task<int> UpdateSetting(Winit.Modules.Setting.Model.Interfaces.ISetting updateSetting);
        Task<int> DeleteSetting(string UID);
    }
}
