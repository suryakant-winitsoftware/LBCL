using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces;

public interface IStoreHistory : IBaseModelV2
{
    string UserJourneyUID { get; set; }
    int YearMonth { get; set; }
    string BeatHistoryUID { get; set; }
    string OrgUID { get; set; }
    string RouteUID { get; set; }
    string StoreUID { get; set; }
    bool IsPlanned { get; set; }
    int SerialNo { get; set; }
    string Status { get; set; }
    int VisitDuration { get; set; }
    int TravelTime { get; set; }
    string PlannedLoginTime { get; set; }
    string PlannedLogoutTime { get; set; }
    string LoginTime { get; set; }
    string LogoutTime { get; set; }
    int NoOfVisits { get; set; }
    DateTime LastVisitDate { get; set; }
    bool IsSkipped { get; set; }
    bool IsProductive { get; set; }
    bool IsGreen { get; set; }
    decimal TargetValue { get; set; }
    decimal TargetVolume { get; set; }
    decimal TargetLines { get; set; }
    decimal ActualValue { get; set; }
    decimal ActualVolume { get; set; }
    decimal ActualLines { get; set; }
    int PlannedTimeSpendInMinutes { get; set; }
    string Latitude { get; set; }
    string Longitude { get; set; }
    string Notes { get; set; }
}
