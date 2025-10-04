using MassTransit;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;

namespace SyncConsumer.Consumers
{
    public class SalesOrderSyncConsumer : IConsumer<IAppRequest>
    {
        private ISalesOrderBL _salesOrderBL;
        public SalesOrderSyncConsumer(ISalesOrderBL salesOrderBL)
        {
            _salesOrderBL = salesOrderBL;
        }
        public async Task Consume(ConsumeContext<IAppRequest> context)
        {
            Console.WriteLine("Sales order consumer processing...");
            if (context == null)
            {
                return;
            }
            IAppRequest appRequest = context.Message;
            try
            {
                if (appRequest == null)
                {
                    return;
                }
                SalesOrderViewModelDCO salesOrderResponseModel = Newtonsoft.Json.JsonConvert.DeserializeObject<SalesOrderViewModelDCO>(appRequest.RequestBody);
                if (salesOrderResponseModel == null)
                {
                   // Log.Information(ex, "Exception");
                    return;
                }
                int TrxStatus = await _salesOrderBL.InsertorUpdate_SalesOrders(salesOrderResponseModel);
                Console.WriteLine("Sales order consumer processing completed");
            }
            catch (Exception ex)
            {
                // Log.Error(ex, "Exception");
                throw;
            }
        }
    }
}
