using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.util;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public abstract class StoreCheckReportBaseViewModel : IStoreCheckReportViewModel
    {
        public List<IStoreCheckReport> StoreCheckReports { get; set; }
        public List<IStoreCheckReportItem> StoreCheckReportsitem { get; set; }
        public IStoreCheckReport StoreCheckReport { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public string OrgUID { get; set; }
        public List<ISelectionItem> RouteSelectionList { get; set; }
        public List<ISelectionItem> SalesmanSelectionList { get; set; }
        public List<ISelectionItem> CustomerSelectionList { get; set; }
        public List<FilterCriteria> StoreCheckReportFilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        private readonly IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private readonly IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;

        public StoreCheckReportBaseViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, 
            IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;

            StoreCheckReports = new List<IStoreCheckReport>();
            StoreCheckReport = new StoreCheckReport();
            RouteSelectionList = new List<ISelectionItem>();
            SalesmanSelectionList = new List<ISelectionItem>();
            CustomerSelectionList = new List<ISelectionItem>();
            StoreCheckReportFilterCriterias = new List<FilterCriteria>();
            SortCriterias = new List<SortCriteria>();
            StoreCheckReportsitem = new List<IStoreCheckReportItem>();
        }

        public async Task ApplyFilter(List<FilterCriteria> filterCriterias)
        {
            StoreCheckReportFilterCriterias.Clear();
            StoreCheckReportFilterCriterias.AddRange(filterCriterias);
            await PopulateViewModel();
        }

        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModel();
        }

        public async Task GetRouteList(string OrgUID)
        {
            RouteSelectionList.Clear();
            RouteSelectionList.AddRange(await GetRouteData(OrgUID));
        }

        public async Task GetSalesmanList(string OrgUID)
        {
            SalesmanSelectionList.Clear();
            SalesmanSelectionList.AddRange(await GetSalesmanData(OrgUID));
        }

        public async Task GetCustomerList(string OrgUID)
        {
            CustomerSelectionList.Clear();
            CustomerSelectionList.AddRange(await GetCustomerData(OrgUID));
        }

        public async virtual Task PopulateViewModel()
        {
            StoreCheckReports = await GetStoreCheckReportData();
        }

        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModel();
        }
       
        public abstract Task<List<IStoreCheckReport>> GetStoreCheckReportData();
        public abstract Task<List<ISelectionItem>> GetRouteData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetSalesmanData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetCustomerData(string OrgUID);
       
    }
}
