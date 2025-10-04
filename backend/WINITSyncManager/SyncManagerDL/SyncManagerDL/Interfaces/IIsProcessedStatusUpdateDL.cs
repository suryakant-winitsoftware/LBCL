using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IIsProcessedStatusUpdateDL
    {
        Task<int> UpdateOracleItemIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOraclePriceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOraclePriceLadderingIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleTaxIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleCustomerReadFromOracleStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOraclePOConfirmationIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleInvoiceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleCreditLimitIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleOutstandingInvoiceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleTemporaryCreditLimitsIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleWarehouseStocksIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleProvisionsIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleProvisionCreditNotesIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOraclePayThroughAPMastersIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
        Task<int> UpdateOracleCustomerReferenceIsProcessedStatus(List<IIsProcessedStatusUids> isProcessedstatus);
    }
}
