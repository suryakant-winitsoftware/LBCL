using MassTransit;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.BL.Classes;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;
using WINIT.Shared.Models.Models;

namespace SyncConsumer.Consumers;

public class ReturnOrderSyncConsumer : IConsumer<IAppRequest>
{
    private IReturnOrderBL _returnOrderBL;
    public ReturnOrderSyncConsumer(IReturnOrderBL returnOrderBL)
    {
        _returnOrderBL = returnOrderBL;
    }
    public async Task Consume(ConsumeContext<IAppRequest> context)
    {
        Console.WriteLine("Return order consumer processing...");
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

            ReturnOrderMasterDTO returnOrderMasterDTO = JsonConvert.DeserializeObject<ReturnOrderMasterDTO>(appRequest.RequestBody);

            if (returnOrderMasterDTO == null)
            {
                // Log.Information(ex, "Exception");
                return;
            }
            int TrxStatus = await _returnOrderBL.CreateReturnOrderMaster(returnOrderMasterDTO);
            Console.WriteLine("Return order consumer processing completed");
        }
        catch (Exception ex)
        {
            // Log.Error(ex, "Exception");
            throw;
        }
    }
}
