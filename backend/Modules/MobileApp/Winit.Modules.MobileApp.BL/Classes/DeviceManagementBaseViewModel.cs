using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Mobile.BL.Interfaces;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mobile.BL.Classes
{
    public abstract class DeviceManagementBaseViewModel:IDeviceManagementViewModel
    {
        public List<IAppVersionUser> DeviceManagementLists { get; set; }
        public IAppVersionUser DeviceManagement { get; set; }
        public string ORGUID { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public string OrgUID { get; set; }
        public List<FilterCriteria> DeviceManagementFilterCriterials { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public DeviceManagementBaseViewModel(IServiceProvider serviceProvider,
             IFilterHelper filter,
             ISortHelper sorter,
             IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            EmpSelectionList = new List<ISelectionItem>();
            DeviceManagementFilterCriterials = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
            // Initialize common properties or perform other common setup
        }
        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            DeviceManagementFilterCriterials.Clear();
            DeviceManagementFilterCriterials.AddRange(filterCriterias);
            await PopulateViewModel();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModel();
        }
        public async Task GetSalesman(string OrgUID)
        {
            EmpSelectionList.Clear();
            EmpSelectionList.AddRange(await GetSalesmanData(OrgUID));
        }
        public async virtual Task PopulateViewModel()
        {
            await PopulateDeviceManagement();
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        protected async Task PopulateDeviceManagement()
        {
            DeviceManagementLists = await GetDeviceManagement(ORGUID);
        }
        public async Task PopulateDeviceManagementforEditDetailsData(string Uid)
        {
            DeviceManagement = await GetDeviceManagementforEditDetailsData(Uid);
        }
        public async Task UpdateDeviceManagement(string deviceid)
        {
            //AppVersion.DeviceId = string.IsNullOrWhiteSpace(deviceid) ? "N/A" : deviceid;
            await UpdateDeviceManagement_data(DeviceManagement);
        }
        #region Business Logic 
        #endregion
        #region Database or Services Methods
        public abstract Task<List<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser>> GetDeviceManagement(string orgUID);
        public abstract Task<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser> GetDeviceManagementforEditDetailsData(string orgUID);
        public abstract Task<bool> UpdateDeviceManagement_data(IAppVersionUser appVersionUser);
        public abstract Task<List<ISelectionItem>> GetSalesmanData(string OrgUID);

        #endregion
    }
}
