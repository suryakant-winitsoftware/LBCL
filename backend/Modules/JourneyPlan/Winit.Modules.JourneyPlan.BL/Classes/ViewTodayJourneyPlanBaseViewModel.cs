using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public abstract class ViewTodayJourneyPlanBaseViewModel:IViewTodayJourneyPlanViewModel
    {
        public List<IAssignedJourneyPlan> AssignedJourneyPlanList { get; set; }
        public List<ITodayJourneyPlanInnerGrid> TodayJourneyPlanInnerGridList { get; set; }
        public List<FilterCriteria> TodayJournryplanFilterCriterias { get; set; }
        public IAssignedJourneyPlan AssignedJourneyPlan { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> RouteSelectionList { get; set; }
        public List<ISelectionItem> VehicleSelectionList { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public ViewTodayJourneyPlanBaseViewModel(IServiceProvider serviceProvider,
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
            AssignedJourneyPlanList = new List<IAssignedJourneyPlan>();
            TodayJourneyPlanInnerGridList = new List<ITodayJourneyPlanInnerGrid>();
            TodayJournryplanFilterCriterias = new List<FilterCriteria>();
            EmpSelectionList = new List<ISelectionItem>();
            RouteSelectionList = new List<ISelectionItem>();
            VehicleSelectionList = new List<ISelectionItem>();
        }
        public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias, string SelectedTab)
        {
            TodayJournryplanFilterCriterias.Clear();
            TodayJournryplanFilterCriterias.AddRange(filterCriterias);
            await PopulateViewModel(SelectedTab);
        }
        public virtual async Task PopulateViewModel(string selectedTab = null)
        {
            AssignedJourneyPlanList = await GetAssigned_UnAssignedJourneyPlanData(selectedTab);
        }
        public async Task  GetSalesman(string OrgUID)
        {
            EmpSelectionList.Clear();
            EmpSelectionList.AddRange(await GetSalesmanData(OrgUID));
        }
        public async Task GetRoute(string OrgUID)
        {
            RouteSelectionList.Clear();
            RouteSelectionList.AddRange(await GetRouteData(OrgUID));
        }
        public async Task GetVehicle(string parentUID)
        {
            VehicleSelectionList.Clear();
            VehicleSelectionList.AddRange(await GetVehicleData(parentUID));
        }
        #region Business Logic 
        public async Task GetInnerGridviewData(IAssignedJourneyPlan assignedJourneyPlan)
        {
            assignedJourneyPlan.ChildgridJourneyPlans = await GetTodayJourneyPlanInnerGridData(assignedJourneyPlan.BeatHistoryUID);
        }
        #endregion
        #region Database or Services Methods
        public abstract Task<List<IAssignedJourneyPlan>> GetAssigned_UnAssignedJourneyPlanData(string selectedTab);
        public abstract Task<List<ITodayJourneyPlanInnerGrid>> GetTodayJourneyPlanInnerGridData(string BeatHistoryUID);

        public abstract Task<List<ISelectionItem>> GetSalesmanData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetRouteData(string OrgUID);
        public abstract Task<List<ISelectionItem>> GetVehicleData(string parentUID);
        #endregion
    }
}
