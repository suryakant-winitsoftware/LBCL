using System;
using System.Collections.Generic;

namespace Winit.Modules.PO.Model.Interfaces
{
    public interface IPOExecution
    {
        int Id { get; set; }
        string UID { get; set; }
        int SS { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedTime { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedTime { get; set; }
        DateTime ServerAddTime { get; set; }
        DateTime ServerModifiedTime { get; set; }
        string BeatHistoryUID { get; set; }
        string StoreHistoryUID { get; set; }
        string RouteUID { get; set; }
        string StoreUID { get; set; }
        string JobPositionUID { get; set; }
        string EmpUID { get; set; }
        DateTime ExecutionTime { get; set; }
        string PONumber { get; set; }
        int LineCount { get; set; }
        decimal QtyCount { get; set; }
        decimal TotalAmount { get; set; }
        List<IPOExecutionLine> Lines { get; set; }
    }
} 