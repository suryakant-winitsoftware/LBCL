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
    public class OracleInt_PurchaseOrderConfirmationDL : SyncManagerDL.Base.DBManager.OracleServerDBManager, Iint_PurchaseOrderConfirmationDL
    {
        public OracleInt_PurchaseOrderConfirmationDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<Iint_PurchaseOrderCancellation>> GetPurchaseOrderCancellationDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<Iint_PurchaseOrderCancellation>purchaseOrderCancellation = await ExecuteQueryAsync<Iint_PurchaseOrderCancellation>(sql.ToString(), parameters);
                return purchaseOrderCancellation;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Iint_PurchaseOrderStatus>> GetPurchaseOrderStatusDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<Iint_PurchaseOrderStatus> purchaseOrderStatus = await ExecuteQueryAsync<Iint_PurchaseOrderStatus>(sql.ToString(), parameters);
                return purchaseOrderStatus;
            }
            catch
            {
                throw;
            }
        }
    }
}
