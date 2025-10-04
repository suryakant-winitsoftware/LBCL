using System;

namespace Winit.Modules.PO.Model.Interfaces
{
    public interface IPOExecutionLine
    {
        int Id { get; set; }
        string UID { get; set; }
        int SS { get; set; }
        int LineNumber { get; set; }
        string SKUUID { get; set; }
        decimal Qty { get; set; }
        decimal Price { get; set; }
        decimal TotalAmount { get; set; }
        string POExecutionUID { get; set; }
    }
} 