using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using Winit.Shared.Models.Constants;

namespace SyncManagerBL.Classes
{
    public class Int_InvoiceBL : Iint_InvoiceBL
    {
        private readonly Iint_InvoiceDL _invoiceDL;
        private readonly Iint_InvoiceStagingDL _invoiceStagingDL;
        private readonly IEntityScriptBL _entityScriptBL;
        public Int_InvoiceBL(Iint_InvoiceDL invoiceDL, Iint_InvoiceStagingDL invoiceStagingDL, IEntityScriptBL entityScriptBL)
        {
            _invoiceDL = invoiceDL;
            _invoiceStagingDL = invoiceStagingDL;
            _entityScriptBL = entityScriptBL;
        }

        public async Task<List<IInt_InvoiceHeader>> GetAllInvoice()
        {
            try
            {
                IEntityScript headerEntityScript = await _entityScriptBL.GetEntityScriptDetailsByEntity(Int_EntityNames.InvoiceHeaderPull);
                IEntityScript lineEntityScript = await _entityScriptBL.GetEntityScriptDetailsByEntity(Int_EntityNames.InvoiceLinePull);
                IEntityScript serialNoEntityScript = await _entityScriptBL.GetEntityScriptDetailsByEntity(Int_EntityNames.InvoiceSerialNoPull);

                bool areAllScriptsEmpty = headerEntityScript == null || lineEntityScript == null || serialNoEntityScript == null;
                if (areAllScriptsEmpty)
                    throw new Exception(@$"select query is empty for {Int_EntityNames.InvoiceHeaderPull} or {Int_EntityNames.InvoiceLinePull} or {Int_EntityNames.InvoiceSerialNoPull}");
                List<IInt_InvoiceHeader> invoiceHeaders = await GetInvoiceHeaderDetails(headerEntityScript.SelectQuery + $" FETCH FIRST {(headerEntityScript.MaxCount == 0 ? 10 : headerEntityScript.MaxCount)} ROWS ONLY ");
                List<IInt_InvoiceLine> invoiceLines = await GetInvoiceLineDetails(lineEntityScript.SelectQuery + $" FETCH FIRST {(lineEntityScript.MaxCount == 0 ? 10 : lineEntityScript.MaxCount)} ROWS ONLY ");
                List<IInt_InvoiceSerialNo> invoiceSerialNos = await GetInvoiceSerialNoDetails(serialNoEntityScript.SelectQuery + $" FETCH FIRST {(serialNoEntityScript.MaxCount == 0 ? 10 : serialNoEntityScript.MaxCount)} ROWS ONLY ");

                var invoiceLinesByDeliveryId = invoiceLines.GroupBy(l => l.DeliveryId).ToDictionary(g => g.Key, g => g.ToList());
                var invoiceSerialNosByDeliveryId = invoiceSerialNos.GroupBy(s => s.DeliveryId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var invoice in invoiceHeaders)
                {  if (invoiceLinesByDeliveryId.TryGetValue(invoice.DeliveryId, out var lines))
                    {
                        invoice.InvoiceLines = lines;
                    }
                   if (invoiceSerialNosByDeliveryId.TryGetValue(invoice.DeliveryId, out var serialNos))
                    {
                        invoice.InvoiceSerialNos = serialNos;
                    }
                }
                return invoiceHeaders;
            }
            catch { throw; }
        }

        public async Task<List<IInt_InvoiceHeader>> GetInvoiceHeaderDetails(string sql)
        {
            return await _invoiceDL.GetInvoiceHeaderDetails(sql);
        }
        public async Task<List<IInt_InvoiceLine>> GetInvoiceLineDetails(string sql)
        {
            return await _invoiceDL.GetInvoiceLineDetails(sql);
        }
        public async Task<List<IInt_InvoiceSerialNo>> GetInvoiceSerialNoDetails(string sql)
        {
            return await _invoiceDL.GetInvoiceSerialNoDetails(sql);
        }

        public async Task<int> InsertInvoiceHeaderDataIntoMonthTable(List<IInt_InvoiceHeader> invoiceHeaders, IEntityDetails entityDetails)
        {
            return await _invoiceStagingDL.InsertInvoiceHeaderDataIntoMonthTable(invoiceHeaders, entityDetails);
        }

        public async Task<int> InsertInvoiceLineDataIntoMonthTable(List<IInt_InvoiceLine> invoiceLines, IEntityDetails entityDetails)
        {
            return await _invoiceStagingDL.InsertInvoiceLineDataIntoMonthTable(invoiceLines, entityDetails);
        }

        public async Task<int> InsertInvoiceSerialNoDataIntoMonthTable(List<IInt_InvoiceSerialNo> invoiceSerialNos, IEntityDetails entityDetails)
        {
            return await _invoiceStagingDL.InsertInvoiceSerialNoDataIntoMonthTable(invoiceSerialNos, entityDetails);

        }
    }
}
