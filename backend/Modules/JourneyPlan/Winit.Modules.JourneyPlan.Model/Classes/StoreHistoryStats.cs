using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.JourneyPlan.Model.Interfaces;

namespace Winit.Modules.JourneyPlan.Model.Classes
{
    public class StoreHistoryStats : BaseModel, IStoreHistoryStats
    {
        public string StoreHistoryUID { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? TotalTimeInMin { get; set; }
        public bool? IsForceCheckIn { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

    }
}
