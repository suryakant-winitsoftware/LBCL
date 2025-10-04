using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IPurchaseOrderTaxCalculator
{
    void SetOrderViewModel(IPurchaseOrderViewModel purchaseOrderViewModel);
    Task CalculateItemTaxes(IPurchaseOrderItemView purchaseOrderItemView);
    Task CalculateInvoiceTaxes(IPurchaseOrderHeader? purchaseOrderHeader = null, List<IPurchaseOrderItemView>? purchaseOrderItemViews = null);
    List<string> GetApplicableTaxesByApplicableAt(Dictionary<string, ITax> taxDictionary, string applicableAt);
}
