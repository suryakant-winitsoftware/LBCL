using System.Data;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IPurchaseOrderLineBL
{
    Task<int> DeletePurchaseOrderLinesByUIDs(List<string> purchaseOrderLineUIDs);
}
