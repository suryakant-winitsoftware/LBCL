using Winit.Modules.Base.Model;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class StockReceivingDetail : BaseModel, IStockReceivingDetail
{
    public string WHStockRequestUID { get; set; } = string.Empty;
    public string WHStockRequestLineUID { get; set; } = string.Empty;
    public string? SKUCode { get; set; }
    public string? SKUName { get; set; }
    public decimal OrderedQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public string? AdjustmentReason { get; set; }
    public decimal AdjustmentQty { get; set; }
    public string? ImageURL { get; set; }
}
