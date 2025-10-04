using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IOutstandingInvoiceStagingDL
    {
        Task<int> InsertOutstandingInvoiceDataIntoMonthTable(List<IOutstandingInvoice> outstandings, IEntityDetails entityDetails);
    }
}
