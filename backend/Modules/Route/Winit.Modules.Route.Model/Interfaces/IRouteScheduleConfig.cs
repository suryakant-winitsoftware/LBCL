using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Route.Model.Interfaces
{
    public interface IRouteScheduleConfig : IBaseModel
    {
        string ScheduleType { get; set; }
        string WeekNumber { get; set; }
        int DayNumber { get; set; }
        bool IsDeleted { get; set; }
    }
}