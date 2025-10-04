using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.JourneyPlan.Model.Interfaces
{
    public interface IJPBeatHistory : IBeatHistory
    {
        public string Status { get; set; }
        public bool IsLastRoute { get; set; }
        public string ActionButtonTextLabel { get; set; }
        public string ActionButtonText { get; set; }
        public string ExecutionStatus { get; set; }
        public DateTime JourneyStartDate { get; set; }
        void UpdatedMyStatus();
    }
}
