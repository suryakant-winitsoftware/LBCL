using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Classes;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Org.Model.Classes;
using Newtonsoft.Json;
using Winit.Shared.Models.Enums;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.SalesOrder.BL.Classes
{
    public abstract class ManageSalesOrdersBaseViewModel : IManageSalesOrdersViewModel
    {
        public List<IDeliveredPreSales> ManageSalesOrderList { get; set; }
        public IDeliveredPreSales ManageSalesOrder { get; set; }
        public List<FilterCriteria> salesOrderFilterCriterials { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        public bool IsPreSalesPage { get; set; }
        public bool IsCashVanSalesPage { get; set; }
        public List<ISelectionItem> RouteSelectionItems { get; set; }
        public List<ISelectionItem> DistributorSelectionList { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> CustomerSelectionList { get; set; }
        public string OrgUID { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<IViewPreSales> SKUViewPreSalesList { get; set; }
        public IEnumerable<IRoute> RoutesListDD { get; set; }
        public IViewPreSales ReferenceUID { get; set; }
        public IViewPreSales ViewPresales { get; set; }
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
        public ManageSalesOrdersBaseViewModel(IServiceProvider serviceProvider,
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
            ManageSalesOrderList = new List<Model.Interfaces.IDeliveredPreSales>();
            salesOrderFilterCriterials = new List<FilterCriteria>();
            SKUViewPreSalesList = new List<IViewPreSales>();
            RouteSelectionItems = new List<ISelectionItem>();
            DistributorSelectionList = new List<ISelectionItem>();
            EmpSelectionList = new List<ISelectionItem>();
            CustomerSelectionList = new List<ISelectionItem>();
            SortCriterias = new List<SortCriteria>();
        }
        public async Task OnFilterApply(List<UIModels.Common.Filter.FilterModel> ColumnsForFilter, Dictionary<string, string> filterCriteria, string selectectab)
        {
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if (keyValue.Key == nameof(IDeliveredPreSales.OrgUID))
                    {
                        ISelectionItem? selectionItem = DistributorSelectionList.Find(e => e.UID == keyValue.Value);
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.UID, FilterType.Equal));
                    }
                    else if (keyValue.Key == nameof(IDeliveredPreSales.EmpUID))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            List<string> selectedUids = keyValue.Value.Split(",").ToList();
                            List<string> seletedLabels = EmpSelectionList.Where(e => selectedUids.Contains(e.UID)).Select(_ => _.UID).ToList();
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", seletedLabels, FilterType.In));
                        }
                        else
                        {
                            ISelectionItem? selectionItem = EmpSelectionList.Find(e => e.UID == keyValue.Value);
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.UID, FilterType.Equal));
                        }
                    }
                    else if (keyValue.Key == nameof(IDeliveredPreSales.RouteName))
                    {

                        if (keyValue.Value.Contains(","))
                        {
                            List<string> selectedUids = keyValue.Value.Split(",").ToList();
                            List<string> seletedLabels = RouteSelectionItems.Where(e => selectedUids.Contains(e.UID)).Select(_ => _.Label).ToList();
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", seletedLabels, FilterType.In));
                        }
                        else
                        {
                            ISelectionItem? selectionItem = RouteSelectionItems.Find(e => e.UID == keyValue.Value);
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Label, FilterType.Equal));
                        }
                    }
                    else if (keyValue.Key == nameof(IDeliveredPreSales.OrderType))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", values, FilterType.In));
                        }
                        else
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));

                        }
                    }
                    else if (keyValue.Key == nameof(IDeliveredPreSales.StoreName))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            List<string> selectedUids = keyValue.Value.Split(",").ToList();
                            List<string> seletedLabels = CustomerSelectionList.Where(e => selectedUids.Contains(e.UID)).Select(_ => _.Label).ToList();
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", seletedLabels, FilterType.In));
                        }
                        else
                        {
                            ISelectionItem? selectionItem = CustomerSelectionList.Find(e => e.UID == keyValue.Value);
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Label, FilterType.Equal));
                        }

                    }
                    else if (keyValue.Key == nameof(IDeliveredPreSales.DeliveryDate))
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                    }
                    else
                    {
                        filterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                    }
                }
            }
            await ApplyFilter(filterCriterias, selectectab);
        }
        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias, string selectectab)
        {
            salesOrderFilterCriterials.Clear();
            salesOrderFilterCriterials.AddRange(filterCriterias);
            await PopulateViewModel(selectectab);
        }
        public async Task ApplySort(SortCriteria sortCriteria, string selectedtab)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateViewModel(selectedtab);
        }
        public async Task GetDistributor()
        {
            DistributorSelectionList.Clear();
            DistributorSelectionList.AddRange(await GetDistributorData());
        }
        public async Task GetSalesman(string OrgUID)
        {
            EmpSelectionList.Clear();
            EmpSelectionList.AddRange(await GetSalesmanData(OrgUID));
        }
        public async Task GetRoute(string OrgUID)
        {
            RouteSelectionItems.Clear();
            RouteSelectionItems.AddRange(await GetRouteData(OrgUID));
        }
        public async Task OnRouteSelect(string UID)
        {
            CustomerSelectionList.Clear();
            CustomerSelectionList.AddRange(await GetCustomerData(UID));
        }
        public virtual async Task PopulateViewModel(string selectedTab = null)
        {
            await PopulateViewModelForGrid(selectedTab);
            //DeliveredPreSalesList = await GetSalesOrderDataFromAPIAsync();
        }
        public async Task PopulateViewModelForGrid(string selectedTab = null)
        {
            ManageSalesOrderList = await GetManageSalesOrdersData(selectedTab);
        }
        public async Task PopulateManageSalesOrdersDataforView(string salesorderuid)
        {
            ViewPresales = await GetManageSalesOrdersDataforView(salesorderuid);
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateViewModelForGrid();
        }
        #region Business Logic  
        public async Task SetEditForviewpresales(List<IViewPreSales> sKUViewPreSales)
        {
            SKUViewPreSalesList.Clear();
            SKUViewPreSalesList.AddRange(sKUViewPreSales);
        }
        #endregion
        #region Database or Services Methods
        public abstract Task<List<Winit.Modules.SalesOrder.Model.Interfaces.IDeliveredPreSales>> GetManageSalesOrdersData(string selectedtab);
        public abstract Task<Winit.Modules.SalesOrder.Model.Interfaces.IViewPreSales> GetManageSalesOrdersDataforView(string salesorderUID);
        public abstract Task<List<ISelectionItem>> GetDistributorData();
        public abstract Task<List<ISelectionItem>> GetSalesmanData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetRouteData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetCustomerData(string UID);

        #endregion
    }
}
