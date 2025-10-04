using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.Model.Classes
{
    public class PlanogramExecutionV1 : BaseModel, IPlanogramExecutionV1
    {
        public string BeatHistoryUID { get; set; }
        public string StoreHistoryUID { get; set; }
        public string RouteUID { get; set; }
        public string StoreUID { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public string ScreenName { get; set; }
        public string PlanogramSetupV1UID { get; set; }
        public DateTime ExecutionTime { get; set; }
    }
} 