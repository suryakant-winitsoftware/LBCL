using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRouteSchedule : Winit.Modules.Base.Model.IBaseModel
    {
            public string CompanyUID { get; set; }
            public string RouteUID { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public DateTime StartDate { get; set; }
            public int Status { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int VisitDurationInMinutes { get; set; }
            public int TravelTimeInMinutes { get; set; }
            public DateTime NextBeatDate { get; set; }
            public DateTime LastBeatDate { get; set; }
            public bool AllowMultipleBeatsPerDay { get; set; }
            public string PlannedDays { get; set; }
    }
}
