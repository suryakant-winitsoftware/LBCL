using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface Iint_PurchaseOrderConfirmationBL
    {
        Task<List<SyncManagerModel.Interfaces.Iint_PurchaseOrderCancellation>> GetPurchaseOrderCancellationDetails(string sql);
        Task<List<SyncManagerModel.Interfaces.Iint_PurchaseOrderStatus>> GetPurchaseOrderStatusDetails(string sql);
        Task<int> InsertPurchaseOrderStatusDataIntoMonthTable(List<SyncManagerModel.Interfaces.Iint_PurchaseOrderStatus> purchaseOrderStatuses, IEntityDetails entityDetails);
        Task<int> InsertPurchaseOrderCancellationDataIntoMonthTable(List<SyncManagerModel.Interfaces.Iint_PurchaseOrderCancellation> purchaseOrderCancellations, IEntityDetails entityDetails);
        Task<List<SyncManagerModel.Interfaces.Iint_PurchaseOrderStatus>> GetPurchaseOrderConfirmationDetails();  
    }
}
