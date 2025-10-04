using Newtonsoft.Json;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IPurchaseOrderLineQPS
{
    public string SKUUID { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalQty { get; set; }
}