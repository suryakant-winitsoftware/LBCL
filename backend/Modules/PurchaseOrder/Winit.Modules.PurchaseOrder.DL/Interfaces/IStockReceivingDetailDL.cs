using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Interfaces;

public interface IStockReceivingDetailDL
{
    Task<IEnumerable<IStockReceivingDetail>> GetByPurchaseOrderUIDAsync(string purchaseOrderUID);
    Task<bool> SaveStockReceivingDetailsAsync(IEnumerable<IStockReceivingDetail> details);
    Task<bool> DeleteByPurchaseOrderUIDAsync(string purchaseOrderUID);
}
