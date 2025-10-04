using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IProcessedOracleUidsDL
    {
        Task<List<IIsProcessedStatusUids>> GetPendingItemProcessStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetPendingPriceProcessStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetPendingPriceLadderingProcessStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetPendingTaxProcessStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetPendingCustomerPullProcessStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetPendingPOConfirmationStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetPendingInvoiceStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetPendingCreditLimitStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetPendingOutstandingInvoiceStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetTemporaryCreditLimitsStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetWarehouseStocksStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetProvisionsStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetProvisionCreditNotesStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetPayThroughAPMastersStatusAndUids(IIntegrationProcessStatus processDetails);
        Task<List<IIsProcessedStatusUids>> GetCustomerReferenceStatusAndUids(IIntegrationProcessStatus processDetails);
    }
}
