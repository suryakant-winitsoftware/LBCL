using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.Syncing.BL.Classes;
using Winit.Modules.Syncing.Model.Classes;
using Winit.Modules.Syncing.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.Syncing
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class AppRequestController : WINITBaseController
    {
        private readonly Winit.Modules.Syncing.BL.Interfaces.IAppRequestBL _appRequestBL;
        private readonly Winit.Modules.RabbitMQQueue.BL.Interfaces.IRabbitMQLogBL _rabbitMQLogBL;
        public AppRequestController(IServiceProvider serviceProvider, 
            Winit.Modules.Syncing.BL.Interfaces.IAppRequestBL  appRequestBL,
            Winit.Modules.RabbitMQQueue.BL.Interfaces.IRabbitMQLogBL rabbitMQLogBL) : base(serviceProvider)
        {
            _appRequestBL = appRequestBL;
            _rabbitMQLogBL = rabbitMQLogBL;
        }
        [HttpPost]
        [Route("PostAppRequest")]
        public async Task<ActionResult> PostAppRequest(List<AppRequest> appRequests)
        {
            try
            {
                List<IAppRequest> appRequestList = appRequests.ToList<IAppRequest>();
                bool retVal = await _appRequestBL.PostAppRequest(appRequestList);

                await PostToRabbitMQQueue(appRequestList);

                return (retVal == true) ? CreateOkApiResponse(retVal) : throw new Exception("Error while saving data");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create PostAppRequest details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        /// <summary>
        /// Post to Rabbit MQ in separate thread
        /// </summary>
        /// <param name="appRequests"></param>
        /// <returns></returns>
        private async Task PostToRabbitMQQueue(List<IAppRequest> appRequests)
        {
            await Task.Run(async () =>
            {
                try
                {
                    // Post to Queue
                    await _rabbitMQLogBL.PostToRabbitMQQueue(appRequests);
                }
                catch (Exception ex) 
                {
                    Log.Error(ex, "Failed to post to the Queue.");
                }
            });
        }
    }
}
