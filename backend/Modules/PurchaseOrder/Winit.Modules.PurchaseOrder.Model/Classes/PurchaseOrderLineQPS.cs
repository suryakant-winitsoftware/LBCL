using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderLineQPS : IPurchaseOrderLineQPS
{
    public required string SKUUID { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalQty { get; set; }
}