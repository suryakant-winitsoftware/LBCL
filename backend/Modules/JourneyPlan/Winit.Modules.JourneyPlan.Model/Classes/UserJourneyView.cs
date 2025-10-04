using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JourneyPlan.Model.Interfaces;

namespace Winit.Modules.JourneyPlan.Model.Classes;

public class UserJourneyView :BaseModel, IUserJourneyView
{
    public string User { get; set; }
    public DateTime JourneyDate { get; set; }
    public string? EOTStatus { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string JourneyTime { get; set; }
    public int StartOdometerReading { get; set; }
    public int EndOdometerReading { get; set; }
    public int TotalReading { get; set; }
    public bool is_synchronizing { get; set; }
    public bool has_internet { get; set; }
    public string internet_type { get; set; }
    public string BatteryStatus { get; set; }
    public bool is_location_enabled { get; set; }
    public int battery_percentage_target { get; set; }
    public bool has_mobile_network { get; set; }
    public int download_speed { get; set; }
    public int upload_speed { get; set; }
    public int battery_percentage_available { get; set; }
    public string attendance_status { get; set; }
    public decimal attendance_latitude { get; set; }
    public decimal attendance_longitude { get; set; }
    public string ImagePath { get; set; }
}

