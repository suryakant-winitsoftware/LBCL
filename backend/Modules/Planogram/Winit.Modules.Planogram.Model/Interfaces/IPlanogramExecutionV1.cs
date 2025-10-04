using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Planogram.Model.Interfaces
{
    public interface IPlanogramExecutionV1 : IBaseModel
    {
        string BeatHistoryUID { get; set; }
        string StoreHistoryUID { get; set; }
        string RouteUID { get; set; }
        string StoreUID { get; set; }
        string JobPositionUID { get; set; }
        string EmpUID { get; set; }
        string ScreenName { get; set; }
        string PlanogramSetupV1UID { get; set; }
        DateTime ExecutionTime { get; set; }
    }
} 