using Hangfire.Console;
using Hangfire.Server;
using SyncManagerBL.Interfaces;
using SyncManagerModel.Interfaces;
using WINITSyncManager.Constants;

namespace WINITSyncManager
{
    public class OracleStatusUpdateService
    {
        private readonly Iint_CommonMethodsBL _int_CommonMethods;
        private readonly IProcessedOracleUidsBL _processedOracleUids;
        private readonly IIsProcessedStatusUpdateBL _isProcessedStatusUpdate;
        public OracleStatusUpdateService(Iint_CommonMethodsBL iint_CommonMethods, IProcessedOracleUidsBL processedOracleUids, IIsProcessedStatusUpdateBL isProcessedStatusUpdate)
        {
            _int_CommonMethods = iint_CommonMethods;
            _processedOracleUids = processedOracleUids;
            _isProcessedStatusUpdate = isProcessedStatusUpdate;
        }
        public async Task RunOracleStatusUpdationProcess(string Entity, PerformContext context)
        {
            try
            {
                context.WriteLine(@$"Step 4:");
                context.WriteLine(@$"Updating Oracle Status.");
                List<IIntegrationProcessStatus> oraclePendingProcesses = await _int_CommonMethods.GetAllOraclePendingProcessesByEntity(Entity);
                var entityProcessMethods = new Dictionary<string, Func<IIntegrationProcessStatus, Task<List<IIsProcessedStatusUids>>>>
            {
                { EntityNames.ItemMasterPull, _processedOracleUids.GetPendingItemProcessStatusAndUids},
                { EntityNames.PricingMaster, _processedOracleUids.GetPendingPriceProcessStatusAndUids},
                { EntityNames.PriceLaddering, _processedOracleUids.GetPendingPriceLadderingProcessStatusAndUids},
                { EntityNames.TaxMaster, _processedOracleUids.GetPendingTaxProcessStatusAndUids },
                { EntityNames.CustomerMasterPull, _processedOracleUids.GetPendingCustomerPullProcessStatusAndUids },
                { EntityNames.PurchaseOrderConfirmationPull, _processedOracleUids.GetPendingPOConfirmationStatusAndUids },
                { EntityNames.InvoicePull, _processedOracleUids.GetPendingInvoiceStatusAndUids },
                { EntityNames.CustomerCreditLimit, _processedOracleUids.GetPendingCreditLimitStatusAndUids },
                { EntityNames.OutstandingInvoice, _processedOracleUids.GetPendingOutstandingInvoiceStatusAndUids},
                { EntityNames.TemporaryCreditLimitPull, _processedOracleUids.GetTemporaryCreditLimitsStatusAndUids},
                { EntityNames.WarehouseStock, _processedOracleUids.GetWarehouseStocksStatusAndUids},
                { EntityNames.Provision, _processedOracleUids.GetProvisionsStatusAndUids},
                { EntityNames.ProvisionCreditNotePull, _processedOracleUids.GetProvisionCreditNotesStatusAndUids},
                { EntityNames.PayThroughAPMaster, _processedOracleUids.GetPayThroughAPMastersStatusAndUids},
                { EntityNames.CustomerReference, _processedOracleUids.GetCustomerReferenceStatusAndUids},
            };
                int oracleStatusValue = 0;
                if (oraclePendingProcesses.Count > 0 && entityProcessMethods.TryGetValue(Entity, out var processMethod))
                {
                    foreach (var item in oraclePendingProcesses)
                    {
                        List<IIsProcessedStatusUids> pendingProcess = await processMethod(item);
                        if (pendingProcess.Count > 0)
                        {
                            oracleStatusValue = await UpdateOracleStatus(item.InterfaceName ?? "", pendingProcess);
                            if (oracleStatusValue > 0)
                                await _int_CommonMethods.UpdateIntegrationProcessStatusByProcessId(item.ProcessId ?? 0, 1);
                        }
                    }
                }
                context.WriteLine(@$"Oracle Status Updated.");
            }
            catch { throw; }
        }
        public async Task<int> UpdateOracleStatus(string Entity, List<IIsProcessedStatusUids> isProcessedStatuses)
        {
            try
            {
                var statusUpdateMethod = new Dictionary<string, Func<List<IIsProcessedStatusUids>, Task<int>>>
                {
                    { EntityNames.ItemMasterPull, _isProcessedStatusUpdate.UpdateOracleItemIsProcessedStatus},
                    { EntityNames.PricingMaster, _isProcessedStatusUpdate.UpdateOraclePriceIsProcessedStatus},
                    { EntityNames.PriceLaddering, _isProcessedStatusUpdate.UpdateOraclePriceLadderingIsProcessedStatus},
                    { EntityNames.TaxMaster, _isProcessedStatusUpdate.UpdateOracleTaxIsProcessedStatus},
                    { EntityNames.CustomerMasterPull, _isProcessedStatusUpdate.UpdateOracleCustomerReadFromOracleStatus},
                    { EntityNames.PurchaseOrderConfirmationPull, _isProcessedStatusUpdate.UpdateOraclePOConfirmationIsProcessedStatus},
                    { EntityNames.InvoicePull, _isProcessedStatusUpdate.UpdateOracleInvoiceIsProcessedStatus},
                    { EntityNames.CustomerCreditLimit, _isProcessedStatusUpdate.UpdateOracleCreditLimitIsProcessedStatus},
                    { EntityNames.OutstandingInvoice, _isProcessedStatusUpdate.UpdateOracleOutstandingInvoiceIsProcessedStatus},
                    { EntityNames.TemporaryCreditLimitPull, _isProcessedStatusUpdate.UpdateOracleTemporaryCreditLimitsIsProcessedStatus},
                    { EntityNames.WarehouseStock, _isProcessedStatusUpdate.UpdateOracleWarehouseStocksIsProcessedStatus},
                    { EntityNames.Provision, _isProcessedStatusUpdate.UpdateOracleProvisionsIsProcessedStatus},
                    { EntityNames.ProvisionCreditNotePull, _isProcessedStatusUpdate.UpdateOracleProvisionCreditNotesIsProcessedStatus},
                    { EntityNames.PayThroughAPMaster, _isProcessedStatusUpdate.UpdateOraclePayThroughAPMastersIsProcessedStatus},
                    { EntityNames.CustomerReference, _isProcessedStatusUpdate.UpdateOracleCustomerReferenceIsProcessedStatus},
                };
                if (statusUpdateMethod.TryGetValue(Entity, out var processMethod))
                {
                    return await processMethod(isProcessedStatuses);
                }
                else return 0;
            }
            catch { throw; }
        }

    }
}
