using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class IsProcessedStatusUpdateBL : IIsProcessedStatusUpdateBL
    {
        private readonly IIsProcessedStatusUpdateDL _isProcessedStatusUpdateDL;
        public IsProcessedStatusUpdateBL(IIsProcessedStatusUpdateDL isProcessedStatusUpdateDL)
        {
            _isProcessedStatusUpdateDL = isProcessedStatusUpdateDL;
        }
        public async Task<int> UpdateOracleItemIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleItemIsProcessedStatus(isProcessedstatus);
        }

        public async Task<int> UpdateOraclePriceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOraclePriceIsProcessedStatus(isProcessedstatus);
        }

        public async Task<int> UpdateOraclePriceLadderingIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOraclePriceLadderingIsProcessedStatus(isProcessedstatus);
        }

        public async Task<int> UpdateOracleTaxIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleTaxIsProcessedStatus(isProcessedstatus);
        }

        public async Task<int> UpdateOracleCustomerReadFromOracleStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleCustomerReadFromOracleStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOraclePOConfirmationIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOraclePOConfirmationIsProcessedStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOracleInvoiceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleInvoiceIsProcessedStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOracleCreditLimitIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleCreditLimitIsProcessedStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOracleOutstandingInvoiceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleOutstandingInvoiceIsProcessedStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOracleTemporaryCreditLimitsIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleTemporaryCreditLimitsIsProcessedStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOracleWarehouseStocksIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleWarehouseStocksIsProcessedStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOracleProvisionsIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleProvisionsIsProcessedStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOracleProvisionCreditNotesIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleProvisionCreditNotesIsProcessedStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOraclePayThroughAPMastersIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOraclePayThroughAPMastersIsProcessedStatus(isProcessedstatus);
        }
        public async Task<int> UpdateOracleCustomerReferenceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus)
        {
            return await _isProcessedStatusUpdateDL.UpdateOracleCustomerReferenceIsProcessedStatus(isProcessedstatus);
        }
    }
}
