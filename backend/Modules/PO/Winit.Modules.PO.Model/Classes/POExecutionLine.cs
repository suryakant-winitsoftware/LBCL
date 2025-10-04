using System;
using Winit.Modules.PO.Model.Interfaces;

namespace Winit.Modules.PO.Model.Classes
{
    public class POExecutionLine : IPOExecutionLine
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public int SS { get; set; }
        public int LineNumber { get; set; }
        public string SKUUID { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public string POExecutionUID { get; set; }
    }
} 