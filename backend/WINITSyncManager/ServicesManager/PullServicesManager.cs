using SyncManagerBL.Interfaces;
using WINITSyncManager.Common;

namespace WINITSyncManager.ServicesManager
{
    public class PullServicesManager : IPullServicesManager
    {
        public IItemMasterBL ItemMasterBL { get; }
        public SyncManagerHelperMethod SyncManagerHelperMethod { get; }
        public IPriceLadderingBL PriceLaddering { get; }
        public IPricingMasterBL PricingMaster { get; }
        public ICustomerCreditLimitBL CustomerCreditLimit { get; }
        public ITaxMasterBL TaxMaster { get; }
        public IOutstandingInvoiceBL OutstandingInvoice { get; }
        public Iint_CommonMethodsBL IntCommonMethods { get; }
        public ICustomerMasterDetailsBL CustomerPushDetails { get; }
        public IPurchaseOrderDetailsBL PurchaseOrderDetails { get; }
        public Iint_InvoiceBL InvoicePullBL { get; }
        public Iint_PurchaseOrderConfirmationBL PurchaseOrderConfirmationPull { get; }
        public ICustomerMasterPullBL CustomerMasterPull { get; }
        public IStatementAndBalanceConfiBL StatementAndBalanceConfi { get; }
        public ITemporaryCreditLimitBL TemporaryCreditLimitBL { get; }
        public IWarehouseStockBL WarehouseStockBL { get; }
        public IProvisionBL ProvisionBL { get; }
        public IProvisionCreditNotePullBL ProvisionCreditNotePull { get; }
        public IPayThroughAPMasterBL PayThroughAPMasterBL { get; }
        public ICustomer_RefBL CustomerRefBL { get; }
        public PullServicesManager(
        IItemMasterBL itemMasterBL,
        SyncManagerHelperMethod syncManagerHelperMethod,
        IPriceLadderingBL priceLaddering,
        IPricingMasterBL pricingMaster,
        ICustomerCreditLimitBL customerCreditLimit,
        ITaxMasterBL taxMaster,
        IOutstandingInvoiceBL outstandingInvoice,
        Iint_CommonMethodsBL intCommonMethods,
        ICustomerMasterDetailsBL customerPushDetails,
        IPurchaseOrderDetailsBL purchaseOrderDetails,
        Iint_InvoiceBL invoicePullBL,
        Iint_PurchaseOrderConfirmationBL purchaseOrderConfirmationPull,
        ICustomerMasterPullBL customerMasterPull,
        IStatementAndBalanceConfiBL statementAndBalanceConfi,
        ITemporaryCreditLimitBL temporaryCreditLimitBL,
        IWarehouseStockBL warehouseStockBL,
        IProvisionBL provisionBL,
        IProvisionCreditNotePullBL provisionCreditNotePull,
        IPayThroughAPMasterBL payThroughAPMasterBL,
        ICustomer_RefBL customer_RefBL)
        {
            ItemMasterBL = itemMasterBL;
            SyncManagerHelperMethod = syncManagerHelperMethod;
            PriceLaddering = priceLaddering;
            PricingMaster = pricingMaster;
            CustomerCreditLimit = customerCreditLimit;
            TaxMaster = taxMaster;
            OutstandingInvoice = outstandingInvoice;
            IntCommonMethods = intCommonMethods;
            CustomerPushDetails = customerPushDetails;
            PurchaseOrderDetails = purchaseOrderDetails;
            InvoicePullBL = invoicePullBL;
            PurchaseOrderConfirmationPull = purchaseOrderConfirmationPull;
            CustomerMasterPull = customerMasterPull;
            StatementAndBalanceConfi = statementAndBalanceConfi;
            TemporaryCreditLimitBL = temporaryCreditLimitBL;
            WarehouseStockBL = warehouseStockBL;
            ProvisionBL = provisionBL;
            ProvisionCreditNotePull = provisionCreditNotePull;
            PayThroughAPMasterBL = payThroughAPMasterBL;
            CustomerRefBL = customer_RefBL;
        }
    }
}
