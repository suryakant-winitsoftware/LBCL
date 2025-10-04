using Dapper;
using SyncManagerDL.Classes;
using WINITSyncManager.Common;
using WINITSyncManager.ServicesManager;

namespace WINITSyncManager
{
    public static class ServiceConfigurator
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            // Register your services and dependencies here

            DefaultTypeMap.MatchNamesWithUnderscores = true;

            // Register interface with implementation
            _ = services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://netcoretest.winitsoftware.com/api/") });
            _ = services.AddScoped<Int_ApiService>();

            _ = services.AddTransient<SyncManagerModel.Interfaces.IItemMaster, SyncManagerModel.Classes.ItemMaster>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IItemMasterDL, SyncManagerDL.Classes.OracleItemMasterDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IItemMasterStagingDL, SyncManagerDL.Classes.MSSQLItemMasterStagingDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IItemMasterBL, SyncManagerBL.Classes.ItemMasterBL>();

            _ = services.AddTransient<SyncManagerModel.Interfaces.IPriceLaddering, SyncManagerModel.Classes.PriceLaddering>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IPriceLadderingDL, SyncManagerDL.Classes.OraclePriceLadderingDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IPriceLadderingStagingDL, SyncManagerDL.Classes.MSSQLPriceLadderingStagingDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IPriceLadderingBL, SyncManagerBL.Classes.PriceLadderingBL>();

            _ = services.AddTransient<SyncManagerModel.Interfaces.IPricingMaster, SyncManagerModel.Classes.PricingMaster>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IPricingMasterDL, SyncManagerDL.Classes.OraclePricingMasterDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IPricingMasterStagingDL, SyncManagerDL.Classes.MSSQLPricingMasterStagingDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IPricingMasterBL, SyncManagerBL.Classes.PricingMasterBL>();

            _ = services.AddTransient<SyncManagerModel.Interfaces.ICustomerCreditLimit, SyncManagerModel.Classes.CustomerCreditLimit>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.ICustomerCreditLimitDL, SyncManagerDL.Classes.OracleCustomerCreditLimitDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.ICustomerCreditLimitStagingDL, SyncManagerDL.Classes.MSSQLCustomerCreditLimitDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.ICustomerCreditLimitBL, SyncManagerBL.Classes.CustomerCreditLimitBL>();

            _ = services.AddTransient<SyncManagerModel.Interfaces.ITemporaryCreditLimit, SyncManagerModel.Classes.TemporaryCreditLimit>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.ITemporaryCreditLimitBL, SyncManagerBL.Classes.TemporaryCreditLimitBL>();
            _ = services.AddTransient<SyncManagerDL.Classes.OracleTemporaryCreditLimitDL>();
            _ = services.AddTransient<SyncManagerDL.Classes.MSSQLTemporaryCreditLimitDL>();
            _ = services.AddTransient<Func<string, SyncManagerDL.Interfaces.ITemporaryCreditLimitDL?>>(serviceProvider => key =>
            {
                return key switch
                {
                    Winit.Shared.Models.Constants.ConnectionStringName.OracleServer => serviceProvider.GetService<SyncManagerDL.Classes.OracleTemporaryCreditLimitDL>(),
                    Winit.Shared.Models.Constants.ConnectionStringName.SqlServer => serviceProvider.GetService<SyncManagerDL.Classes.MSSQLTemporaryCreditLimitDL>(),
                    _ => throw new NotImplementedException()
                };
            });
            _ = services.AddTransient<SyncManagerDL.Classes.OracleTemporaryCreditLimitPullDL>();
            _ = services.AddTransient<SyncManagerDL.Classes.MSSQLTemporaryCreditLimitPullDL>();
            _ = services.AddTransient<Func<string, SyncManagerDL.Interfaces.ITemporaryCreditLimitPullDL?>>(serviceProvider => key =>
            {
                return key switch
                {
                    Winit.Shared.Models.Constants.ConnectionStringName.OracleServer => serviceProvider.GetService<SyncManagerDL.Classes.OracleTemporaryCreditLimitPullDL>(),
                    Winit.Shared.Models.Constants.ConnectionStringName.SqlServer => serviceProvider.GetService<SyncManagerDL.Classes.MSSQLTemporaryCreditLimitPullDL>(),
                    _ => throw new NotImplementedException()
                };
            });

            _ = services.AddTransient<SyncManagerModel.Interfaces.IOutstandingInvoice, SyncManagerModel.Classes.OutstandingInvoice>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IOutstandingInvoiceDL, SyncManagerDL.Classes.OracleOutstandingInvoiceDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IOutstandingInvoiceStagingDL, SyncManagerDL.Classes.MSSQLOutstandingInvoiceStagingDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IOutstandingInvoiceBL, SyncManagerBL.Classes.OutstandingInvoiceBL>();

            _ = services.AddTransient<SyncManagerModel.Interfaces.ITaxMaster, SyncManagerModel.Classes.TaxMaster>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.ITaxMasterDL, SyncManagerDL.Classes.OracleTaxMasterDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.ITaxMasterStagingDL, SyncManagerDL.Classes.MSSQLTaxMasterStagingDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.ITaxMasterBL, SyncManagerBL.Classes.TaxMasterBL>();


            _ = services.AddTransient<SyncManagerModel.Interfaces.IEntityScript, SyncManagerModel.Classes.EntityScript>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IEntityScriptDL, SyncManagerDL.Classes.MSSQLEntityScriptDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IEntityScriptBL, SyncManagerBL.Classes.EntityScriptBL>();

            _ = services.AddTransient<SyncManagerModel.Interfaces.IApiRequest, SyncManagerModel.Classes.ApiRequest>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.IEntityScript, SyncManagerModel.Classes.EntityScript>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IEntityScriptDL, SyncManagerDL.Classes.MSSQLEntityScriptDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IEntityScriptBL, SyncManagerBL.Classes.EntityScriptBL>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.IPushDataStatus, SyncManagerModel.Classes.PushDataStatus>();

            _ = services.AddTransient<SyncManagerBL.Interfaces.Iint_CommonMethodsBL, SyncManagerBL.Classes.int_CommonMethodsBL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IInt_CommonMethodsDL, SyncManagerDL.Classes.MSSQLint_CommonMethodsDL>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.IPrepareDB, SyncManagerModel.Classes.PrepareDB>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.IEntityDetails, SyncManagerModel.Classes.EntityDetails>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.IEntityData, SyncManagerModel.Classes.EntityData>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.IIntegrationMessageProcess, SyncManagerModel.Classes.IntegrationMessageProcess>();

            _ = services.AddTransient<SyncManagerModel.Interfaces.ICustomerMasterDetails, SyncManagerModel.Classes.CustomerMasterDetails>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.ICustomerMasterDetailsDL, SyncManagerDL.Classes.MSSQLCustomerMasterDetailsDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.ICustomerMasterDetailsBL, SyncManagerBL.Classes.CustomerMasterDetailsBL>();

            //Customer Pull
            _ = services.AddTransient<SyncManagerBL.Interfaces.ICustomerMasterDetailsBL, SyncManagerBL.Classes.CustomerMasterDetailsBL>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.ICustomerMasterDetails, SyncManagerModel.Classes.CustomerMasterDetails>();
            _ = services.AddTransient<SyncManagerDL.Classes.MSSQLCustomerMasterDetailsDL>();
            _ = services.AddTransient<SyncManagerDL.Classes.OracleCustomerMasterDetailsDL>();
            _ = services.AddTransient<Func<string, SyncManagerDL.Interfaces.ICustomerMasterDetailsDL?>>(serviceProvider => key =>
            {
                return key switch
                {
                    Winit.Shared.Models.Constants.ConnectionStringName.OracleServer => serviceProvider.GetService<SyncManagerDL.Classes.OracleCustomerMasterDetailsDL>(),
                    Winit.Shared.Models.Constants.ConnectionStringName.SqlServer => serviceProvider.GetService<SyncManagerDL.Classes.MSSQLCustomerMasterDetailsDL>(),
                    _ => serviceProvider.GetService<SyncManagerDL.Classes.MSSQLCustomerMasterDetailsDL>()
                };
            });
            //Purchase Order Pull
            _ = services.AddTransient<SyncManagerBL.Interfaces.IPurchaseOrderDetailsBL, SyncManagerBL.Classes.PurchaseOrderDetailsBL>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.Iint_PurchaseOrderHeader, SyncManagerModel.Classes.Int_PurchaseOrderHeader>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.Iint_PurchaseOrderLine, SyncManagerModel.Classes.Int_PurchaseOrderLine>();
            _ = services.AddTransient<SyncManagerDL.Classes.MSSQLPurchaseOrderDetailsDL>();
            _ = services.AddTransient<SyncManagerDL.Classes.OraclePurchaseOrderDetailsDL>();
            _ = services.AddTransient<Func<string, SyncManagerDL.Interfaces.IPurchaseOrderDetailsDL?>>(serviceProvider => key =>
            {
                return key switch
                {
                    Winit.Shared.Models.Constants.ConnectionStringName.OracleServer => serviceProvider.GetService<SyncManagerDL.Classes.OraclePurchaseOrderDetailsDL>(),
                    Winit.Shared.Models.Constants.ConnectionStringName.SqlServer => serviceProvider.GetService<SyncManagerDL.Classes.MSSQLPurchaseOrderDetailsDL>(),
                    _ => serviceProvider.GetService<SyncManagerDL.Classes.MSSQLPurchaseOrderDetailsDL>()
                };

            });
            //CMI Invoice pull
            _ = services.AddTransient<SyncManagerModel.Interfaces.IInt_InvoiceHeader, SyncManagerModel.Classes.Int_InvoiceHeader>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.IInt_InvoiceLine, SyncManagerModel.Classes.Int_InvoiceLine>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.IInt_InvoiceSerialNo, SyncManagerModel.Classes.Int_InvoiceSerialNo>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.Iint_InvoiceDL, SyncManagerDL.Classes.OracleInt_InvoiceDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.Iint_InvoiceStagingDL, SyncManagerDL.Classes.MSSQLInt_InvoiceStagingDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.Iint_InvoiceBL, SyncManagerBL.Classes.Int_InvoiceBL>();
            //CMI Purchase Order Conformation
            _ = services.AddTransient<SyncManagerModel.Interfaces.Iint_PurchaseOrderCancellation, SyncManagerModel.Classes.Int_PurchaseOrderCancellation>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.Iint_PurchaseOrderStatus, SyncManagerModel.Classes.Int_PurchaseOrderStatus>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.Iint_PurchaseOrderConfirmationDL, SyncManagerDL.Classes.OracleInt_PurchaseOrderConfirmationDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.Iint_PurchaseOrderConfirmationStagingDL, SyncManagerDL.Classes.MSSQLPurchaseOrderConfirmationStagingDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.Iint_PurchaseOrderConfirmationBL, SyncManagerBL.Classes.Int_PurchaseOrderConfirmationBL>();

            _ = services.AddTransient<SyncManagerModel.Interfaces.IIntegrationProcessStatus, SyncManagerModel.Classes.IntegrationProcessStatus>();
            _ = services.AddTransient<SyncManagerModel.Interfaces.IIsProcessedStatusUids, SyncManagerModel.Classes.IsProcessedStatusUids>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IIsProcessedStatusUpdateDL, SyncManagerDL.Classes.OracleIsProcessedStatusUpdateDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IIsProcessedStatusUpdateBL, SyncManagerBL.Classes.IsProcessedStatusUpdateBL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IProcessedOracleUidsDL, SyncManagerDL.Classes.MSSQLProcessedOracleUidsDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IProcessedOracleUidsBL, SyncManagerBL.Classes.ProcessedOracleUidsBL>();
            /* Customer Master Pull */
            _ = services.AddTransient<SyncManagerModel.Interfaces.ICustomerMasterPull, SyncManagerModel.Classes.CustomerMasterPull>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.ICustomerMasterPullDL, SyncManagerDL.Classes.OracleCustomerMasterPullDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.ICustomerMasterPullStagingDL, SyncManagerDL.Classes.MSSQLCustomerMasterPullStagingDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.ICustomerMasterPullBL, SyncManagerBL.Classes.CustomerMasterPullBL>();
            /* Statement And Balance Confi Pull */
            _ = services.AddTransient<SyncManagerModel.Interfaces.IStatementAndBalanceConfi, SyncManagerModel.Classes.StatementAndBalanceConfi>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IStatementAndBalanceConfiDL, SyncManagerDL.Classes.OracleStatementAndBalanceConfiDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IStatementAndBalanceConfiStagingDL, SyncManagerDL.Classes.MSSQLStatementAndBalanceConfiStagingDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IStatementAndBalanceConfiBL, SyncManagerBL.Classes.StatementAndBalanceConfiBL>();
            /* Warehouse Stock */
            _ = services.AddTransient<SyncManagerModel.Interfaces.IWarehouseStock, SyncManagerModel.Classes.WarehouseStock>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IWarehouseStockDL, SyncManagerDL.Classes.OracleWarehouseStockDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IWarehouseStockStagingDL, SyncManagerDL.Classes.MSSQLWarehouseStockDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IWarehouseStockBL, SyncManagerBL.Classes.WarehouseStockBL>();
            // Provision Pull
            _ = services.AddTransient<SyncManagerModel.Interfaces.IProvision, SyncManagerModel.Classes.Provision>();
            _ = services.AddTransient<SyncManagerDL.Classes.MSSQLProvisionDL>();
            _ = services.AddTransient<SyncManagerDL.Classes.OracleProvisionDL>();
            _ = services.AddTransient<Func<string, SyncManagerDL.Interfaces.IProvisionDL?>>(serviceProvider => key =>
            {
                return key switch
                {
                    Winit.Shared.Models.Constants.ConnectionStringName.OracleServer => serviceProvider.GetService<SyncManagerDL.Classes.OracleProvisionDL>(),
                    Winit.Shared.Models.Constants.ConnectionStringName.SqlServer => serviceProvider.GetService<SyncManagerDL.Classes.MSSQLProvisionDL>(),
                    _ => null
                };

            });
            _ = services.AddTransient<SyncManagerBL.Interfaces.IProvisionBL, SyncManagerBL.Classes.ProvisionBL>();

            // Provision CreditNote Push
            _ = services.AddTransient<SyncManagerModel.Interfaces.IProvisionCreditNote, SyncManagerModel.Classes.ProvisionCreditNote>();
            _ = services.AddTransient<SyncManagerDL.Classes.MSSQLProvisionCreditNotePushDL>();
            _ = services.AddTransient<SyncManagerDL.Classes.OracleProvisionCreditNotePushDL>();
            _ = services.AddTransient<Func<string, SyncManagerDL.Interfaces.IProvisionCreditNotePushDL?>>(serviceProvider => key =>
            {
                return key switch
                {
                    Winit.Shared.Models.Constants.ConnectionStringName.OracleServer => serviceProvider.GetService<SyncManagerDL.Classes.OracleProvisionCreditNotePushDL>(),
                    Winit.Shared.Models.Constants.ConnectionStringName.SqlServer => serviceProvider.GetService<SyncManagerDL.Classes.MSSQLProvisionCreditNotePushDL>(),
                    _ => null
                };

            });
            _ = services.AddTransient<SyncManagerBL.Interfaces.IProvisionCreditNotePushBL, SyncManagerBL.Classes.ProvisionCreditNotePushBL>();

            // Provision CreditNote Pull
            _ = services.AddTransient<SyncManagerModel.Interfaces.IProvisionCreditNote, SyncManagerModel.Classes.ProvisionCreditNote>();
            _ = services.AddTransient<SyncManagerDL.Classes.MSSQLProvisionCreditNotePullDL>();
            _ = services.AddTransient<SyncManagerDL.Classes.OracleProvisionCreditNotePullDL>();
            _ = services.AddTransient<Func<string, SyncManagerDL.Interfaces.IProvisionCreditNotePullDL?>>(serviceProvider => key =>
            {
                return key switch
                {
                    Winit.Shared.Models.Constants.ConnectionStringName.OracleServer => serviceProvider.GetService<SyncManagerDL.Classes.OracleProvisionCreditNotePullDL>(),
                    Winit.Shared.Models.Constants.ConnectionStringName.SqlServer => serviceProvider.GetService<SyncManagerDL.Classes.MSSQLProvisionCreditNotePullDL>(),
                    _ => null
                };

            });
            _ = services.AddTransient<SyncManagerBL.Interfaces.IProvisionCreditNotePullBL, SyncManagerBL.Classes.ProvisionCreditNotePullBL>();

            // PayThroughAPMaster pull
            _ = services.AddTransient<SyncManagerModel.Interfaces.IPayThroughAPMaster, SyncManagerModel.Classes.PayThroughAPMaster>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IPayThroughAPMasterStaginigDL, SyncManagerDL.Classes.MSSQLPayThroughAPMasterStaginigDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.IPayThroughAPMasterDL, SyncManagerDL.Classes.OraclePayThroughAPMasterDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.IPayThroughAPMasterBL, SyncManagerBL.Classes.PayThroughAPMasterBL>();
            // CustomerReference pull
            _ = services.AddTransient<SyncManagerModel.Interfaces.ICustomer_Ref, SyncManagerModel.Classes.Customer_Ref>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.ICustomer_RefStagingDL, SyncManagerDL.Classes.Customer_RefStagingDL>();
            _ = services.AddTransient<SyncManagerDL.Interfaces.ICustomer_RefDL, SyncManagerDL.Classes.OracleCustomer_RefDL>();
            _ = services.AddTransient<SyncManagerBL.Interfaces.ICustomer_RefBL, SyncManagerBL.Classes.Customer_RefBL>();

            _ = services.AddScoped<IPullServicesManager, PullServicesManager>();
            _ = services.AddScoped<IPushServicesManager, PushServicesManager>();
            _ = services.AddTransient<PushIntegration>();
            _ = services.AddTransient<PullIntegration>();
            _ = services.AddTransient<StagingInsertion>();
            _ = services.AddTransient<DataProcessing>();
            _ = services.AddTransient<OracleStatusUpdateService>();
            _ = services.AddTransient<SyncManagerHelperMethod>();
            _ = services.AddTransient<CommonFunctions>();
            _ = services.AddTransient<ProcessJobs>();

            services.AddTransient<App>();// Register main App class
        }

    }
}
