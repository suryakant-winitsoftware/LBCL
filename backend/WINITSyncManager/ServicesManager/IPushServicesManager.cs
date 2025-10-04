using SyncManagerBL.Interfaces;

namespace WINITSyncManager.ServicesManager
{
    public interface IPushServicesManager
    {
        Iint_CommonMethodsBL CommonMethods { get; } 
        ICustomerMasterDetailsBL CustomerPush { get; }
        IPurchaseOrderDetailsBL PurchaseOrder { get; }
        ITemporaryCreditLimitBL TemporaryCreditLimit { get; }
        IProvisionCreditNotePushBL ProvisionCreditNotePush { get; }
    }
}
