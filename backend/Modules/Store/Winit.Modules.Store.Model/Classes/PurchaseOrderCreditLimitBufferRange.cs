using Winit.Modules.Store.Model.Interfaces;
namespace Winit.Modules.Store.Model.Classes;

public class PurchaseOrderCreditLimitBufferRange : IPurchaseOrderCreditLimitBufferRange
{
    public decimal RangeFrom { get; set; }
    public decimal RangeTo { get; set; }
    public decimal PercentageBuffer { get; set; }
}
