using System;
using System.Collections.Generic;
using Winit.Modules.PO.Model.Interfaces;

namespace Winit.Modules.PO.Model.Classes
{
    public class POExecution : IPOExecution
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public int SS { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public string BeatHistoryUID { get; set; }
        public string StoreHistoryUID { get; set; }
        public string RouteUID { get; set; }
        public string StoreUID { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string PONumber { get; set; }
        public int LineCount { get; set; }
        public decimal QtyCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<IPOExecutionLine> Lines { get; set; }
    }
} 