using Winit.Modules.Base.Model;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IStockReceivingTracking : IBaseModel
{
    public string WHStockRequestUID { get; set; }
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
}
