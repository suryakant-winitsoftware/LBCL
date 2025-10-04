using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class OutstandingInvoiceBL : IOutstandingInvoiceBL
    {
        private readonly IOutstandingInvoiceDL _outstandingInvoiceDL;
        private readonly IOutstandingInvoiceStagingDL _outstandingInvoiceStagingDL;
        public OutstandingInvoiceBL(IOutstandingInvoiceDL outstandingInvoiceDL, IOutstandingInvoiceStagingDL outstandingInvoiceStagingDL)
        {
            _outstandingInvoiceDL = outstandingInvoiceDL;
            _outstandingInvoiceStagingDL = outstandingInvoiceStagingDL;
        }

        public async Task<List<IOutstandingInvoice>> GetOutstandingInvoiceDetails(string sql)
        {
            return await _outstandingInvoiceDL.GetOutstandingInvoiceDetails(sql);
        }

        public async Task<int> InsertOutstandingInvoiceDataIntoMonthTable(List<IOutstandingInvoice> outstandings, IEntityDetails entityDetails)
        {
           return await _outstandingInvoiceStagingDL.InsertOutstandingInvoiceDataIntoMonthTable(outstandings, entityDetails);
        }
    }
}
