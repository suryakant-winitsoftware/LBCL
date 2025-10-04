using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.Interfaces
{
    public interface IManageSalesOrdersViewModel
    {
        public List<IDeliveredPreSales> ManageSalesOrderList { get; set; }
        public IDeliveredPreSales ManageSalesOrder { get; set; }
        public IViewPreSales ViewPresales { get; set; }
        public List<IViewPreSales> SKUViewPreSalesList { get; set; }
        public IViewPreSales ReferenceUID { get; set; }
        public List<FilterCriteria> salesOrderFilterCriterials { get; set; }
        public List<ISelectionItem>RouteSelectionItems { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> DistributorSelectionList { get; set; }
        public List<ISelectionItem> CustomerSelectionList { get; set; }

        public bool IsPreSalesPage { get; set; }
        public bool IsCashVanSalesPage { get; set; }
        public int TotalItemsCount { get; set; }
        public int PageNumber { get; set; } 
        public int PageSize { get; set; }
        public string OrgUID { get; set; }
        Task ApplySort(SortCriteria sortCriteria,string selectectab);
        Task SetEditForviewpresales(List<IViewPreSales> sKUViewPreSales);
        Task PageIndexChanged(int pageNumber);
        Task PopulateViewModel(string apiparam=null);
        Task PopulateManageSalesOrdersDataforView(string salesorderuid);
        Task OnFilterApply(List<UIModels.Common.Filter.FilterModel> ColumnsForFilter, Dictionary<string, string> filterCriteria, string selectectab);
        Task GetDistributor();
        Task GetSalesman(string OrgUID);
        Task GetRoute(string OrgUID);
        Task OnRouteSelect(string UID);

    }
}
