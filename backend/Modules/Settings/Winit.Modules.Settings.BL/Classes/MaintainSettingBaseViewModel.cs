using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Setting.BL.Classes
{

    public abstract class MaintainSettingBaseViewModel:IMaintainSettingViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<ISetting> SettingGridviewList { get; set; }
        public List<FilterCriteria> SettingFilterCriterials { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        public ISetting setting { get; set; }
        public ISetting settingUID { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private List<string> _propertiesToSearch = new List<string>();
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public MaintainSettingBaseViewModel(IServiceProvider serviceProvider,
             IFilterHelper filter,
             ISortHelper sorter,
             IListHelper listHelper,
             IAppUser appUser,
             IAppSetting appSetting,
             IDataManager dataManager,
             IAppConfig appConfigs,
             Base.BL.ApiService apiService
         ) 
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            _appConfigs = appConfigs;
            _apiService = apiService;
            SettingGridviewList = new List<ISetting>();
            SettingFilterCriterials = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
        }
        public async Task OnFilterApply(List<UIModels.Common.Filter.FilterModel> ColumnsForFilter, Dictionary<string, string> filterCriteria)
        {
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                   filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                }
            }
            await ApplyFilter(filterCriterias);
        }
        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            SettingFilterCriterials.Clear();
            SettingFilterCriterials.AddRange(filterCriterias);
            await PopulateViewModel();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModel();
        }
        public async virtual Task PopulateViewModel()
        {
            await PopulateMaintainSetting();
        }        
        protected async Task PopulateMaintainSetting()
        {
            SettingGridviewList = await GetMaintainSetting();
        }
        public async Task PopulatetMaintainSettingforEditDetailsData(string Uid)
        {
            setting = await GetMaintainSettingforEditDetailsData(Uid);
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        public async Task UpdateMaintainSetting(string uid)
        {
            //AppVersion.DeviceId = string.IsNullOrWhiteSpace(deviceid) ? "N/A" : deviceid;
            await UpdateMaintainSetting_data(setting);
        }
        #region Business Logic 
        #endregion
        #region Database or Services Methods
        public abstract Task<List<Winit.Modules.Setting.Model.Interfaces.ISetting>> GetMaintainSetting();
        public abstract Task<Winit.Modules.Setting.Model.Interfaces.ISetting> GetMaintainSettingforEditDetailsData(string orguid);
        public abstract Task<bool> UpdateMaintainSetting_data(ISetting settingUID);
        #endregion
    }
}
