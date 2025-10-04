using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface Iint_InvoiceBL
    {
        Task<List<SyncManagerModel.Interfaces.IInt_InvoiceHeader>> GetInvoiceHeaderDetails(string sql);
        Task<List<SyncManagerModel.Interfaces.IInt_InvoiceLine>> GetInvoiceLineDetails(string sql);
        Task<List<SyncManagerModel.Interfaces.IInt_InvoiceSerialNo>> GetInvoiceSerialNoDetails(string sql);
        Task<List<SyncManagerModel.Interfaces.IInt_InvoiceHeader>> GetAllInvoice();
        Task<int> InsertInvoiceHeaderDataIntoMonthTable(List<SyncManagerModel.Interfaces.IInt_InvoiceHeader> invoiceHeaders, IEntityDetails entityDetails);
        Task<int> InsertInvoiceLineDataIntoMonthTable(List<SyncManagerModel.Interfaces.IInt_InvoiceLine> invoiceLines, IEntityDetails entityDetails);
        Task<int> InsertInvoiceSerialNoDataIntoMonthTable(List<SyncManagerModel.Interfaces.IInt_InvoiceSerialNo> invoiceSerialNos, IEntityDetails entityDetails);

    }
}
