using Winit.Modules.PurchaseOrder.BL.Interfaces;

namespace Winit.Modules.PurchaseOrder.BL.Events;

public class PurchaseOrderItemEvent
{
    public required IPurchaseOrderItemView PurchaseOrderItemView { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Qty { get; set; }
}
