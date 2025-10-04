using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Setting.BL.Interfaces
{
    public interface IMaintainSettingViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<ISetting> SettingGridviewList { get; set; }
        public ISetting setting { get; set; }
        public ISetting settingUID { get; set; }
        Task PopulateViewModel();
        Task PopulatetMaintainSettingforEditDetailsData(string Uid);
        Task PageIndexChanged(int pageNumber);
        Task UpdateMaintainSetting(string uid);
        Task OnFilterApply(List<UIModels.Common.Filter.FilterModel> ColumnsForFilter, Dictionary<string, string> filterCriteria);
        Task ApplySort(SortCriteria sortCriteria);

    }
}
