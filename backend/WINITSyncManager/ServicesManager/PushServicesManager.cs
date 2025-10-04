using SyncManagerBL.Interfaces;

namespace WINITSyncManager.ServicesManager
{
    public class PushServicesManager : IPushServicesManager
    {
        public Iint_CommonMethodsBL CommonMethods { get; } 
        public ICustomerMasterDetailsBL CustomerPush { get; }
        public IPurchaseOrderDetailsBL PurchaseOrder { get; }
        public ITemporaryCreditLimitBL TemporaryCreditLimit { get; }
        public IProvisionCreditNotePushBL ProvisionCreditNotePush { get; }

        public PushServicesManager(
            Iint_CommonMethodsBL commonMethods, 
            ICustomerMasterDetailsBL customerPush,
            IPurchaseOrderDetailsBL purchaseOrder,
            ITemporaryCreditLimitBL temporaryCreditLimit,
            IProvisionCreditNotePushBL provisionCreditNotePush)
        {
            CommonMethods = commonMethods; 
            CustomerPush = customerPush;
            PurchaseOrder = purchaseOrder;
            TemporaryCreditLimit = temporaryCreditLimit;
            ProvisionCreditNotePush = provisionCreditNotePush;
        }
    }
}
