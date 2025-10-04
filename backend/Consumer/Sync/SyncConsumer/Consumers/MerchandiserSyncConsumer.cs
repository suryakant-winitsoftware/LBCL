using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CaptureCompetitor.BL.Interfaces;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;
using Winit.Modules.Syncing.Model.Interfaces;

namespace SyncConsumer.Consumers
{
    public class MerchandiserSyncConsumer : IConsumer<IAppRequest>
    {
        private ICaptureCompetitorBL _captureCompetitorBL;
        public MerchandiserSyncConsumer(ICaptureCompetitorBL captureCompetitorBL)
        {
            _captureCompetitorBL = captureCompetitorBL;
        }
        public async Task Consume(ConsumeContext<IAppRequest> context)
        {
            Console.WriteLine("Merchandiser consumer processing...");
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
                MerchandiserDTO merchandiserResponseModel = Newtonsoft.Json.JsonConvert.DeserializeObject<MerchandiserDTO>(appRequest.RequestBody);
                if (merchandiserResponseModel == null)
                {
                    // Log.Information(ex, "Exception");
                    return;
                }
                int TrxStatus = await _captureCompetitorBL.InsertorUpdate_Merchandiser(merchandiserResponseModel);
                Console.WriteLine("Merchandiser consumer processing completed");
            }
            catch (Exception ex)
            {
                // Log.Error(ex, "Exception");
                throw;
            }
        }
    }
}
