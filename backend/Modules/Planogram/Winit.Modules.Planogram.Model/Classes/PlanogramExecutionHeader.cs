using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.Model.Classes
{
    public class PlanogramExecutionHeader : BaseModel, IPlanogramExecutionHeader
    {
        public string BeatHistoryUID { get; set; } 

        public string StoreHistoryUID { get; set; } 

        public string StoreUID { get; set; } 

        public string JobPositionUID { get; set; } 

        public string RouteUID { get; set; } 

        public string? Status { get; set; } 
    }
}
