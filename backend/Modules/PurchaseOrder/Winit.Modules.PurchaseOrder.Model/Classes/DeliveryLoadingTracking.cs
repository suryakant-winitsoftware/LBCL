using Winit.Modules.Base.Model;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class DeliveryLoadingTracking : BaseModel, IDeliveryLoadingTracking
{
    public string WHStockRequestUID { get; set; } = string.Empty;
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
    public string? Status { get; set; }
    public bool IsActive { get; set; }

    // WH Stock Request fields (for joined queries)
    public string? request_code { get; set; }
    public DateTime? created_time { get; set; }
    public string? warehouse_uid { get; set; }
    public string? status { get; set; }
    public string? OrgName { get; set; }
}
