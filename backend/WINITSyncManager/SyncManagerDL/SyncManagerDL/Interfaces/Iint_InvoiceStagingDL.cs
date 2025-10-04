using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface Iint_InvoiceStagingDL
    {
        Task<int> InsertInvoiceHeaderDataIntoMonthTable(List<IInt_InvoiceHeader> invoiceHeaders, IEntityDetails entityDetails);
        Task<int> InsertInvoiceLineDataIntoMonthTable(List<IInt_InvoiceLine> invoiceLines, IEntityDetails entityDetails);
        Task<int> InsertInvoiceSerialNoDataIntoMonthTable(List<IInt_InvoiceSerialNo> invoiceSerialNos, IEntityDetails entityDetails);
    }
}
