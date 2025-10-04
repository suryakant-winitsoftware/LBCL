using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Interfaces
{
    public interface IViewTodayJourneyPlanViewModel
    {
        public List<IAssignedJourneyPlan> AssignedJourneyPlanList { get; set; }
        public List<ITodayJourneyPlanInnerGrid> TodayJourneyPlanInnerGridList { get; set; }
        public IAssignedJourneyPlan AssignedJourneyPlan { get; set; }
        public List<FilterCriteria> TodayJournryplanFilterCriterias { get; set; }
        public List<ISelectionItem> EmpSelectionList { get; set; }
        public List<ISelectionItem> RouteSelectionList { get; set; }
        public List<ISelectionItem> VehicleSelectionList { get; set; }
        Task PopulateViewModel(string apiparam = null);
        Task GetInnerGridviewData(IAssignedJourneyPlan assignedJourneyPlan);
        Task ApplyFilter(List<FilterCriteria> filterCriterias,string SelectedTab);
        Task GetSalesman(string OrgUID);
        Task GetRoute(string OrgUID);
        Task GetVehicle(string parentUID);

    }
}
