using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JourneyPlan.Model.Interfaces;

namespace Winit.Modules.JourneyPlan.Model.Classes
{
    public class ExceptionLog : BaseModel, IExceptionLog
    {

        public string? StoreHistoryUID { get; set; }
        public string? StoreHistoryStatsUID { get; set; }
        public string? ExceptionType { get; set; }
        public string? ExceptionDetails { get; set; }
    }
}
