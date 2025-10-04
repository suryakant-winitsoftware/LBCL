using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;

namespace SyncConsumer.Consumers
{
    public class FileSysProcessConsumer : IConsumer<IAppRequest>
    {
        private readonly IFileSysBL _fileSysBL;
        public FileSysProcessConsumer(IFileSysBL fileSysBL)
        {
            _fileSysBL = fileSysBL;
        }
        public async Task Consume(ConsumeContext<IAppRequest> context)
        {
            Console.WriteLine("FileSys Consumer called");
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
                FileSys fileSys = Newtonsoft.Json.JsonConvert.DeserializeObject<FileSys>(appRequest.RequestBody);
                if (fileSys == null)
                {
                    return;
                }
                int TrxStatus = await _fileSysBL.CUDFileSys(fileSys);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
