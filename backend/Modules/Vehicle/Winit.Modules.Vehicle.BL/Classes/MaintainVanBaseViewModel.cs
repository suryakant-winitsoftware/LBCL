using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Vehicle.BL.Interfaces;
using Winit.Modules.Vehicle.Model.Classes;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Vehicle.BL.Classes
{
    public abstract class MaintainVanBaseViewModel:IMaintainVanViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public string OrgUID { get; set; }
        public List<IVehicle> MaintainVanList { get; set; }
        public List<FilterCriteria> VanFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        // Injection
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
        public MaintainVanBaseViewModel(IServiceProvider serviceProvider,
              IFilterHelper filter,
              ISortHelper sorter,
              IListHelper listHelper,
              IAppUser appUser,
              IAppSetting appSetting,
              IDataManager dataManager,
              IAppConfig appConfigs,
              Base.BL.ApiService apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            // Initialize common properties or perform other common setup
            MaintainVanList = new List<IVehicle>();
            VanFilterCriterias = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
            _appConfigs = appConfigs;
            _apiService = apiService;
        }
        /// <summary>
        /// This will seach data from TaxItemViews and store in FilteredTaxItemViews & DisplayedTaxItemViews
        /// </summary>
        /// <param name="filterCriterias"></param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        /// 
        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias)
        {
            VanFilterCriterias.Clear();
            VanFilterCriterias.AddRange(filterCriterias);
            await PopulateMaintainVan();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModel();
        }
        public async Task ResetFilter()
        {
            VanFilterCriterias.Clear();
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }       
        public async virtual Task PopulateViewModel()
        {
            await PopulateMaintainVan();
        }
        protected async Task PopulateMaintainVan()
        {
            MaintainVanList = await GetMaintainVanData(OrgUID);
        }
        #region Business Logics  
        public async Task<string> DeleteVehicle(string UID)
        {
            return await DeleteVanFromGrid(UID);
        }
        #endregion
        #region Database or Services Methods
        public abstract Task<List<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>> GetMaintainVanData(string orgUID);
        public abstract Task<string> DeleteVanFromGrid(string uid);
        #endregion
    }
}
