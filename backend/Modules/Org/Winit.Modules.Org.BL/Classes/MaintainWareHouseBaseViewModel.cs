using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Org.BL.Classes
{
    public abstract class MaintainWareHouseBaseViewModel : IMaintainWareHouseViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public string ParentUID { get; set; }
        public List<IWarehouseItemView> MaintainWarehouseGridList { get; set; }
        public List<FilterCriteria> MaintainWarehouseFilterCriterias { get; set; }
        public List<ISelectionItem> DistributorSelectionList { get; set; }
        public List<ISelectionItem> WareHouseTypeSelectionItems { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        public List<ISelectionItem> MaintainWarehouseTypeSelectionItems { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private List<string> _propertiesToSearch = new List<string>();
        public MaintainWareHouseBaseViewModel(IServiceProvider serviceProvider,
               IFilterHelper filter,
               ISortHelper sorter,
                IAppUser appUser,
               IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
             )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _appUser = appUser;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            //WareHouseItemViewList = new List<IOrgType>();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
            MaintainWarehouseFilterCriterias = new List<FilterCriteria>();
            DistributorSelectionList = new List<ISelectionItem>();
            SortCriterias = new List<SortCriteria>();
            WareHouseTypeSelectionItems = new List<ISelectionItem>();
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
            MaintainWarehouseFilterCriterias.Clear();
            MaintainWarehouseFilterCriterias.AddRange(filterCriterias);
            await PopulateMaintainWarehouse();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateMaintainWarehouse();
        }
        public async Task GetDistributor()
        {
            DistributorSelectionList.Clear();
            DistributorSelectionList.AddRange(await GetDistributorData());
        }
        public async Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias)
        {

        }
        public async virtual Task PopulateViewModel()
        {
            await PopulateMaintainWarehouse();
            var data = await GetWarehouseTypeDropdownDataFromAPIAsync();
            if (data != null && data.Any())
            {
                WareHouseTypeSelectionItems.Clear();
                WareHouseTypeSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IOrgType>(data, new List<string>{ "UID"
                ,"Name","WarehouseType"}));
            }
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
        protected async Task PopulateMaintainWarehouse()
        {
            MaintainWarehouseGridList = await GetMaintainWarteHouseData(ParentUID);
        }
        public async Task<string> DeleteMaintainWareHouse(string UID)
        {
            return await DeleteMaintainWareHouseFromGrid(UID);
        }
        #region Business Logic 
        #endregion
        #region Database or Services Methods
        public abstract Task<List<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView>> GetMaintainWarteHouseData(string orgUID);
        public abstract Task<string> DeleteMaintainWareHouseFromGrid(string uid);
        public abstract Task<List<ISelectionItem>> GetDistributorData();
        protected abstract Task<List<Winit.Modules.Org.Model.Interfaces.IOrgType>> GetWarehouseTypeDropdownDataFromAPIAsync();
        #endregion
    }
}
