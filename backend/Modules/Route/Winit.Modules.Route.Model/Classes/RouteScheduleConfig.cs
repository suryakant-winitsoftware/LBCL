using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Route.Model.Classes
{
    public class RouteScheduleConfig : BaseModel, IRouteScheduleConfig
    {
        public string ScheduleType { get; set; }
        public string WeekNumber { get; set; }
        public int DayNumber { get; set; }
        public bool IsDeleted { get; set; }
    }
}