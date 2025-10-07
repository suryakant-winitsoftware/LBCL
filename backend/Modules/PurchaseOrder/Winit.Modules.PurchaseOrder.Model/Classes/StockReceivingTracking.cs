using Winit.Modules.Base.Model;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class StockReceivingTracking : BaseModel, IStockReceivingTracking
{
    public string WHStockRequestUID { get; set; } = string.Empty;
    public string? ReceiverName { get; set; }
    public string? ReceiverEmployeeCode { get; set; }
    public string? ForkLiftOperatorUID { get; set; }
    public string? LoadEmptyStockEmployeeUID { get; set; }
    public string? GetpassEmployeeUID { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public DateTime? UnloadingStartTime { get; set; }
    public DateTime? UnloadingEndTime { get; set; }
    public DateTime? LoadEmptyStockTime { get; set; }
    public DateTime? GetpassTime { get; set; }
    public DateTime? PhysicalCountStartTime { get; set; }
    public DateTime? PhysicalCountEndTime { get; set; }
    public string? ReceiverSignature { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
    public bool IsActive { get; set; }

    // Additional fields from JOIN queries
    public string? DeliveryNoteNumber { get; set; }
    public string? request_code { get; set; }
    public DateTime? created_time { get; set; }
}
