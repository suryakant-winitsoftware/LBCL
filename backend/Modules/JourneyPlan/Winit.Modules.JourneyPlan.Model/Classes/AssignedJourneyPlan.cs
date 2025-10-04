using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JourneyPlan.Model.Interfaces;

namespace Winit.Modules.JourneyPlan.Model.Classes
{
    public class AssignedJourneyPlan : IAssignedJourneyPlan
    {
        public string? BeatHistoryUID { get; set; }
        public string EmpUID { get; set; }

        public DateTime VisitDate { get; set; }
        public string ?SalesmanName { get; set; }
        public string? SalesmanLoginId { get; set; }
        public string? RouteUID { get; set; }
        public long ActualStoreVisits { get; set; }
        public long SkippedStore { get; set; }
        public long ScheduleCall { get; set; }
        public string? RouteName { get; set; }
        public string? VehicleName { get; set; }
        public int PendingVisits { get; set; }
        public bool IsChildgridOpen { get; set; }
        public List<ITodayJourneyPlanInnerGrid> ChildgridJourneyPlans { get; set; }
    }
}
