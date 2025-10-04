using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Vehicle.Model.Interfaces;

namespace Winit.Modules.Vehicle.Model.Classes
{
    public class VehicleStatus:Vehicle,IVehicleStatus
    {
        public bool IsStarted { get; set; }
        public bool IsRunningRoute { get; set; }
        public string UserJourneyVehicleUID { get; set; }
        public bool IsStopped { get; set; }
        public System.DateTime? CurrentRunningDate { get; set; }
    }
}
