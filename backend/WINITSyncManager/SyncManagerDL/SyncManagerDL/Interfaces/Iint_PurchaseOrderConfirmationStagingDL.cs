using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface Iint_PurchaseOrderConfirmationStagingDL
    {
        Task<int> InsertPurchaseOrderStatusDataIntoMonthTable(List<SyncManagerModel.Interfaces.Iint_PurchaseOrderStatus> purchaseOrderStatuses, IEntityDetails entityDetails);
        Task<int> InsertPurchaseOrderCancellationDataIntoMonthTable(List<SyncManagerModel.Interfaces.Iint_PurchaseOrderCancellation> purchaseOrderCancellations, IEntityDetails entityDetails);
    }
}
