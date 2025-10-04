namespace Winit.Modules.Store.Model.Interfaces;

public interface IPurchaseOrderCreditLimitBufferRange
{
    public decimal RangeFrom { get; set; }
    public decimal RangeTo { get; set; }
    public decimal PercentageBuffer { get; set; }
}
