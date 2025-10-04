using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IOutstandingInvoiceBL
    {
        Task<List<SyncManagerModel.Interfaces.IOutstandingInvoice>> GetOutstandingInvoiceDetails(string sql);
        Task<int> InsertOutstandingInvoiceDataIntoMonthTable(List<SyncManagerModel.Interfaces.IOutstandingInvoice> outstandings, IEntityDetails entityDetails);

    }
}
