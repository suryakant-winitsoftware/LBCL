using System;
using System.Collections.Generic;
using Winit.Modules.Base.Model;
using Winit.Modules.ExpiryCheck.Model.Interfaces;

namespace Winit.Modules.ExpiryCheck.Model.Classes
{
    public class ExpiryCheckExecution : BaseModel, IExpiryCheckExecution
    {
        public string BeatHistoryUID { get; set; }
        public string StoreHistoryUID { get; set; }
        public string RouteUID { get; set; }
        public string StoreUID { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public DateTime ExecutionTime { get; set; }
        public List<IExpiryCheckExecutionLine> Lines { get; set; } = new List<IExpiryCheckExecutionLine>();
    }
} 