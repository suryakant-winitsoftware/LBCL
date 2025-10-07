using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Interfaces;

public interface IStockReceivingDetailDL
{
    Task<IEnumerable<IStockReceivingDetail>> GetByWHStockRequestUIDAsync(string whStockRequestUID);
    Task<bool> SaveStockReceivingDetailsAsync(IEnumerable<IStockReceivingDetail> details);
    Task<bool> DeleteByWHStockRequestUIDAsync(string whStockRequestUID);
}
