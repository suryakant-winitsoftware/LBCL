using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Classes
{
    public class OracleInt_InvoiceDL : SyncManagerDL.Base.DBManager.OracleServerDBManager, Iint_InvoiceDL
    {
        public OracleInt_InvoiceDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<IInt_InvoiceHeader>> GetInvoiceHeaderDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IInt_InvoiceHeader> invoiceHeaders = await ExecuteQueryAsync<IInt_InvoiceHeader>(sql.ToString(), parameters);
                return invoiceHeaders;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<IInt_InvoiceLine>> GetInvoiceLineDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IInt_InvoiceLine> invoiceLines = await ExecuteQueryAsync<IInt_InvoiceLine>(sql.ToString(), parameters);
                return invoiceLines;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<IInt_InvoiceSerialNo>> GetInvoiceSerialNoDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IInt_InvoiceSerialNo> invoiceLines = await ExecuteQueryAsync<IInt_InvoiceSerialNo>(sql.ToString(), parameters);
                return invoiceLines;
            }
            catch
            {
                throw;
            }
        }
    }
}
