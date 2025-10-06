using Winit.Modules.Base.Model;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IDeliveryLoadingTracking : IBaseModel
{
    public string PurchaseOrderUID { get; set; }
    public string? VehicleUID { get; set; }
    public string? DriverEmployeeUID { get; set; }
    public string? ForkLiftOperatorUID { get; set; }
    public string? SecurityOfficerUID { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public DateTime? LoadingStartTime { get; set; }
    public DateTime? LoadingEndTime { get; set; }
    public DateTime? DepartureTime { get; set; }
    public string? LogisticsSignature { get; set; }
    public string? DriverSignature { get; set; }
    public string? Notes { get; set; }
    public string? DeliveryNoteFilePath { get; set; }
    public string? DeliveryNoteNumber { get; set; }
    public bool IsActive { get; set; }
}
