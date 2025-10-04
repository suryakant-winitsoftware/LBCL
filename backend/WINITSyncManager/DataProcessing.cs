using Hangfire.Console;
using Hangfire.Server;
using SyncManagerBL.Interfaces;
using SyncManagerModel.Interfaces;
using WINITSyncManager.Constants;

namespace WINITSyncManager
{
    public class DataProcessing
    {
        private readonly Iint_CommonMethodsBL _int_CommonMethods;
        public DataProcessing(Iint_CommonMethodsBL int_CommonMethods)
        {
            _int_CommonMethods = int_CommonMethods;
        }
        public async Task RunMainInsertionProcess(string Entity, PerformContext context)
        {
            try
            {
                context.WriteLine(@$"Step 3:");
                context.WriteLine(@$"Processing Staging Data.");
                List<IIntegrationProcessStatus> pendingProcesses = await _int_CommonMethods.GetAllPendingProcessByEntity(Entity);
                if (pendingProcesses?.Any() == true)
                {
                    foreach (IIntegrationProcessStatus processStatus in pendingProcesses)
                    {
                        await ProcessDataToMainTables(processStatus);
                    }
                    context.WriteLine(@$"Staging Data Processing Completed.");
                }
                else
                    context.WriteLine(@$"There No Staging Data to Process.");
            }
            catch
            {
                throw;
            }
        }
        public async Task ProcessDataToMainTables(IIntegrationProcessStatus integrationProcess)
        {
            try
            {
                var mainTableInsetion = new Dictionary<string, Func<string, string, long, Task<int>>>
                {
                    { EntityNames.ItemMasterPull, _int_CommonMethods.InsertItemDataIntoMainTable },
                    { EntityNames.PricingMaster, _int_CommonMethods.InsertPriceDataIntoMainTable },
                    { EntityNames.PriceLaddering, _int_CommonMethods.InsertPriceLadderingDataIntoMainTable },
                    { EntityNames.TaxMaster, _int_CommonMethods.InsertTaxDataIntoMainTable },
                    { EntityNames.CustomerMasterPull, _int_CommonMethods.InsertCustomerOracleCodeIntoMainTable},
                    { EntityNames.PurchaseOrderConfirmationPull, _int_CommonMethods.InsertPurchaseOrderConfirmationIntoMainTable},
                    { EntityNames.InvoicePull, _int_CommonMethods.InsertInvoicesIntoMainTable},
                    { EntityNames.CustomerCreditLimit, _int_CommonMethods.InsertCreditLimitDataIntoMainTable},
                    { EntityNames.TemporaryCreditLimit, _int_CommonMethods.InsertTemporaryCreditLimitDataIntoMainTable},
                    { EntityNames.OutstandingInvoice, _int_CommonMethods.InsertOutStandingInvoicesIntoMainTable},
                    { EntityNames.TemporaryCreditLimitPull, _int_CommonMethods.InsertTemporaryCreditLimitsIntoMainTable},
                    { EntityNames.WarehouseStock, _int_CommonMethods.InsertWarehouseStocksIntoMainTable},
                    { EntityNames.Provision, _int_CommonMethods.InsertProvisionsIntoMainTable},
                    { EntityNames.ProvisionCreditNotePull, _int_CommonMethods.InsertProvisionCreditNotesIntoMainTable},
                    { EntityNames.PayThroughAPMaster, _int_CommonMethods.InsertPayThroughAPMastersIntoMainTable},
                    { EntityNames.CustomerReference, _int_CommonMethods.InsertCustomerReferenceIntoMainTable},
                };
                if (integrationProcess is not null && mainTableInsetion.TryGetValue(integrationProcess?.InterfaceName, out var InsertionMethod))
                {
                    await InsertionMethod(integrationProcess?.MonthTableName, integrationProcess?.TablePrefix, integrationProcess.SyncLogId);
                }
            }
            catch { throw; }

        }

    }
}
