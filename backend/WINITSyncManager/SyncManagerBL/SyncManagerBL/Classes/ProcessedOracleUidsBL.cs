using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class ProcessedOracleUidsBL : IProcessedOracleUidsBL
    {
        private readonly IProcessedOracleUidsDL _processedOracleUids;
        public ProcessedOracleUidsBL(IProcessedOracleUidsDL processedOracleUids)
        {
            _processedOracleUids = processedOracleUids;
        }

        public async Task<List<IIsProcessedStatusUids>> GetPendingItemProcessStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPendingItemProcessStatusAndUids(processDetails);
        }

        public async Task<List<IIsProcessedStatusUids>> GetPendingPriceLadderingProcessStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPendingPriceLadderingProcessStatusAndUids(processDetails);
        }

        public async Task<List<IIsProcessedStatusUids>> GetPendingPriceProcessStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPendingPriceProcessStatusAndUids(processDetails);
        }

        public async Task<List<IIsProcessedStatusUids>> GetPendingTaxProcessStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPendingTaxProcessStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetPendingCustomerPullProcessStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPendingCustomerPullProcessStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetPendingPOConfirmationStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPendingPOConfirmationStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetPendingInvoiceStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPendingInvoiceStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetPendingCreditLimitStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPendingCreditLimitStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetPendingOutstandingInvoiceStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPendingOutstandingInvoiceStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetTemporaryCreditLimitsStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetTemporaryCreditLimitsStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetWarehouseStocksStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetWarehouseStocksStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetProvisionsStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetProvisionsStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetProvisionCreditNotesStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetProvisionCreditNotesStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetPayThroughAPMastersStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetPayThroughAPMastersStatusAndUids(processDetails);
        }
        public async Task<List<IIsProcessedStatusUids>> GetCustomerReferenceStatusAndUids(IIntegrationProcessStatus processDetails)
        {
            return await _processedOracleUids.GetCustomerReferenceStatusAndUids(processDetails);
        }

    }
}
