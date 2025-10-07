using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Interfaces;

public interface IDeliveryLoadingTrackingDL
{
    Task<List<IDeliveryLoadingTracking>> GetByStatusAsync(string status);
    Task<IDeliveryLoadingTracking?> GetByWHStockRequestUIDAsync(string whStockRequestUID);
    Task<bool> SaveDeliveryLoadingTrackingAsync(IDeliveryLoadingTracking deliveryLoadingTracking);
    Task<bool> UpdateDeliveryLoadingTrackingAsync(IDeliveryLoadingTracking deliveryLoadingTracking);
    Task<bool> UpdateWHStockRequestStatusAsync(string whStockRequestUID, string status);
}
