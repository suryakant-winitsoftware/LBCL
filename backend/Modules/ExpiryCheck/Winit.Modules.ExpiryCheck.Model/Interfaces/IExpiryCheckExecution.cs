using System;
using System.Collections.Generic;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ExpiryCheck.Model.Interfaces
{
    public interface IExpiryCheckExecution : IBaseModel
    {
        string BeatHistoryUID { get; set; }
        string StoreHistoryUID { get; set; }
        string RouteUID { get; set; }
        string StoreUID { get; set; }
        string JobPositionUID { get; set; }
        string EmpUID { get; set; }
        DateTime ExecutionTime { get; set; }
        List<IExpiryCheckExecutionLine> Lines { get; set; }
    }
} 