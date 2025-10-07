using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Interfaces;

public interface IStockReceivingTrackingDL
{
    Task<IStockReceivingTracking?> GetByWHStockRequestUIDAsync(string whStockRequestUID);
    Task<IStockReceivingTracking?> GetByPurchaseOrderUIDAsync(string purchaseOrderUID);
    Task<IEnumerable<IStockReceivingTracking>> GetAllAsync();
    Task<bool> SaveStockReceivingTrackingAsync(IStockReceivingTracking stockReceivingTracking);
    Task<bool> UpdateStockReceivingTrackingAsync(IStockReceivingTracking stockReceivingTracking);
    Task<bool> UpdateWHStockRequestStatusAsync(string whStockRequestUID, string status);
    Task<bool> UpdatePurchaseOrderStatusAsync(string purchaseOrderUID, string status);
}
