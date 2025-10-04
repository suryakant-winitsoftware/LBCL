using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JourneyPlan.Model.Interfaces;

namespace Winit.Modules.JourneyPlan.Model.Classes;

public class UserJourney : BaseModel, IUserJourney
{
    public string JobPositionUID { get; set; }
    public string EmpUID { get; set; }
    public DateTime? JourneyStartTime { get; set; }
    public DateTime? JourneyEndTime { get; set; }
    public int StartOdometerReading { get; set; }
    public int EndOdometerReading { get; set; }
    public string JourneyTime { get; set; }
    public string VehicleUID { get; set; }
    public string EOTStatus { get; set; }
    public string ReOpenedBy { get; set; }
    public bool HasAuditCompleted { get; set; }
    public string BeatHistoryUID { get; set; }
    public string WHStockRequestUID { get; set; }
    //RAMANA ADDED BASED ON THE SelectAlUserJourneyDetails API
    public string LoginId { get; set; }

    public bool IsSynchronizing { get; set; }
    public bool HasInternet { get; set; }
    public string InternetType { get; set; }
    public int DownloadSpeed { get; set; }
    public int UploadSpeed { get; set; }
    public bool HasMobileNetwork { get; set; }
    public bool IsLocationEnabled { get; set; }
    public int BatteryPercentageTarget { get; set; }
    public int BatteryPercentageAvailable { get; set; }
    public string AttendanceStatus { get; set; }
    public string AttendanceLatitude { get; set; }
    public string AttendanceLongitude { get; set; }
    public string AttendanceAddress { get; set; }
}

