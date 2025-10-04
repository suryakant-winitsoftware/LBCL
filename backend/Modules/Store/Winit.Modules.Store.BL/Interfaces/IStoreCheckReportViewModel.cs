using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreCheckReportViewModel
    {
        List<IStoreCheckReport> StoreCheckReports { get; set; }
        IStoreCheckReport StoreCheckReport { get; set; }
        int PageNumber { get; set; }
        int PageSize { get; set; }
        int TotalItemsCount { get; set; }
        string OrgUID { get; set; }
        List<ISelectionItem> RouteSelectionList { get; set; }
        List<ISelectionItem> SalesmanSelectionList { get; set; }
        List<ISelectionItem> CustomerSelectionList { get; set; }
        List<FilterCriteria> StoreCheckReportFilterCriterias { get; set; }
        List<SortCriteria> SortCriterias { get; set; }
        public List<IStoreCheckReportItem> StoreCheckReportsitem { get; set; }

        Task ApplyFilter(List<FilterCriteria> filterCriterias);
        Task ApplySort(SortCriteria sortCriteria);
        Task GetRouteList(string OrgUID);
        Task GetSalesmanList(string OrgUID);
        Task GetCustomerList(string OrgUID);
        Task PopulateViewModel();
        Task PageIndexChanged(int pageNumber);
    }
}
