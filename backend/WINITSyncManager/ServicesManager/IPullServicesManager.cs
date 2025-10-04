using SyncManagerBL.Interfaces;
using WINITSyncManager.Common;

namespace WINITSyncManager.ServicesManager
{
    public interface IPullServicesManager
    {

        IItemMasterBL ItemMasterBL { get; }
        SyncManagerHelperMethod SyncManagerHelperMethod { get; }
        IPriceLadderingBL PriceLaddering { get; }
        IPricingMasterBL PricingMaster { get; }
        ICustomerCreditLimitBL CustomerCreditLimit { get; }
        ITaxMasterBL TaxMaster { get; }
        IOutstandingInvoiceBL OutstandingInvoice { get; }
        Iint_CommonMethodsBL IntCommonMethods { get; }
        ICustomerMasterDetailsBL CustomerPushDetails { get; }
        IPurchaseOrderDetailsBL PurchaseOrderDetails { get; }
        Iint_InvoiceBL InvoicePullBL { get; }
        Iint_PurchaseOrderConfirmationBL PurchaseOrderConfirmationPull { get; }
        ICustomerMasterPullBL CustomerMasterPull { get; }
        IStatementAndBalanceConfiBL StatementAndBalanceConfi { get; }
        ITemporaryCreditLimitBL TemporaryCreditLimitBL { get; }
        IWarehouseStockBL WarehouseStockBL { get; }
        IProvisionBL ProvisionBL { get; }
        IProvisionCreditNotePullBL ProvisionCreditNotePull { get; }
        IPayThroughAPMasterBL PayThroughAPMasterBL { get; }
        ICustomer_RefBL CustomerRefBL { get; }
    }
}
