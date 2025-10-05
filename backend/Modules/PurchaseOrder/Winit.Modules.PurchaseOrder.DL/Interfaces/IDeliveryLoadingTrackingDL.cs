using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Interfaces;

public interface IDeliveryLoadingTrackingDL
{
    Task<IDeliveryLoadingTracking?> GetByPurchaseOrderUIDAsync(string purchaseOrderUID);
    Task<bool> SaveDeliveryLoadingTrackingAsync(IDeliveryLoadingTracking deliveryLoadingTracking);
    Task<bool> UpdateDeliveryLoadingTrackingAsync(IDeliveryLoadingTracking deliveryLoadingTracking);
    Task<bool> UpdatePurchaseOrderStatusAsync(string purchaseOrderUID, string status);
}
