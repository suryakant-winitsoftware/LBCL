using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JourneyPlan.Model.Interfaces;

namespace Winit.Modules.JourneyPlan.Model.Classes
{
    public class StoreHistory: BaseModelV2, IStoreHistory
    {
        public string UserJourneyUID { get; set; }
        public int YearMonth { get; set; }
        public string BeatHistoryUID { get; set; }
        public string OrgUID { get; set; }
        public string RouteUID { get; set; }
        public string StoreUID { get; set; }
        public bool IsPlanned { get; set; }
        public int SerialNo { get; set; }
        public string Status { get; set; }
        public int VisitDuration { get; set; }
        public int TravelTime { get; set; }
        public string PlannedLoginTime { get; set; }
        public string PlannedLogoutTime { get; set; }
        public string LoginTime { get; set; }
        public string LogoutTime { get; set; }
        public int NoOfVisits { get; set; }
        public DateTime LastVisitDate { get; set; }
        public bool IsSkipped { get; set; }
        public bool IsProductive { get; set; }
        public bool IsGreen { get; set; }
        public decimal TargetValue { get; set; }
        public decimal TargetVolume { get; set; }
        public decimal TargetLines { get; set; }
        public decimal ActualValue { get; set; }
        public decimal ActualVolume { get; set; }
        public decimal ActualLines { get; set; }
        public int PlannedTimeSpendInMinutes { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Notes { get; set; }
    }
}
