using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IPurchaseOrderDetailsDL
    {
        Task<List<Iint_PurchaseOrderHeader>> GetPurchaseOrderHeaderDetails(IEntityDetails entityDetails)
        {
            throw new NotImplementedException();
        } 
        Task<List<Iint_PurchaseOrderLine>> GetPurchaseOrderLineDetails(IEntityDetails entityDetails)
        {
            throw new NotImplementedException();
        }
        Task<string> InsertPurchaseOrderIntoOraceleStaging(List<Iint_PurchaseOrderHeader> purchaseOrderHeaders, List<Iint_PurchaseOrderLine> purchaseOrderLines)
        {
            throw new NotImplementedException();
        }
    }
}
