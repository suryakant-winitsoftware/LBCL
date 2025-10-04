namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IPurchaseOrderLevelCalculator
{
    List<IPurchaseOrderItemView> PurchaseOrderItemViews { get; set; }
    void SetOrderViewModel(IPurchaseOrderViewModel salesOrderViewModel);
    Task ComputeOrderLevelTaxesAndOrderSummary();
}
