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
    public class OracleOutstandingInvoiceDL : SyncManagerDL.Base.DBManager.OracleServerDBManager, IOutstandingInvoiceDL
    {
        public OracleOutstandingInvoiceDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<IOutstandingInvoice>> GetOutstandingInvoiceDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IOutstandingInvoice> outstandingInvoice = await ExecuteQueryAsync<IOutstandingInvoice>(sql.ToString(), parameters);
                return outstandingInvoice;
            }
            catch
            {
                throw;
            }
        }
    }
}
