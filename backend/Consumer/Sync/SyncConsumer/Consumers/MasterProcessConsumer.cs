using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;

namespace SyncConsumer.Consumers
{
    public class MasterProcessConsumer : IConsumer<IAppRequest>
    {
        private readonly IBeatHistoryBL _beatHistoryBL;
        public MasterProcessConsumer(IBeatHistoryBL beatHistoryBL)
        {
            _beatHistoryBL = beatHistoryBL;
        }

        public async Task Consume(ConsumeContext<IAppRequest> context)
        {
            Console.WriteLine("Master consumer processing...");
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
                MasterDTO masterDTO = Newtonsoft.Json.JsonConvert.DeserializeObject<MasterDTO>(appRequest.RequestBody);
                if (masterDTO == null)
                {
                    return;
                }
                bool TrxStatus = await _beatHistoryBL.InsertMasterRabbitMQueue(masterDTO);
                Console.WriteLine("Master consumer processing completed");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
