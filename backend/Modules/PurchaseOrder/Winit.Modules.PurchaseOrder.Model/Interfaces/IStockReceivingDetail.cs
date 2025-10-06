using Winit.Modules.Base.Model;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IStockReceivingDetail : IBaseModel
{
    public string PurchaseOrderUID { get; set; }
    public string PurchaseOrderLineUID { get; set; }
    public string? SKUCode { get; set; }
    public string? SKUName { get; set; }
    public decimal OrderedQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public string? AdjustmentReason { get; set; }
    public decimal AdjustmentQty { get; set; }
    public string? ImageURL { get; set; }
}
