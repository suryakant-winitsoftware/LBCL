using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;
using Winit.Modules.RabbitMQQueue.BL;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQService.Interfaces;
using Newtonsoft.Json;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.WHStock.Model.Classes;
using WINIT.Shared.Models.Models;
using Microsoft.Extensions.Logging;
using WINITAPI.Controllers.SalesOrder;
using WINITAPI.HostedServices;

namespace WINITAPI.Controllers.Location
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class RabbitMQQueueController : WINITBaseController
    {
        private readonly Winit.Modules.RabbitMQQueue.BL.Interfaces.IRabbitMQLogBL _rabbitMQLogBL;
        private readonly ILogger<RabbitMQQueueController> _logger;
        private readonly WorkerServices.Classes.SalesOrderWorkerService _salesOrderWorkerService;

        public RabbitMQQueueController(IServiceProvider serviceProvider, 
            Winit.Modules.RabbitMQQueue.BL.Interfaces.IRabbitMQLogBL rabbitMQLogBL, 
            ILogger<RabbitMQQueueController> logger,
            WorkerServices.Classes.SalesOrderWorkerService salesOrderWorkerService) : base(serviceProvider)
        {
            _rabbitMQLogBL = rabbitMQLogBL;
            _logger = logger;
            _salesOrderWorkerService = salesOrderWorkerService;
        }

        [HttpPost]
        [Route("PostToRabbitMQQueue")]
        public async Task<ActionResult> PostToRabbitMQQueue(Winit.Modules.Syncing.Model.Classes.AppRequest appRequest)
        {
            try
            {
                await _rabbitMQLogBL.PostToRabbitMQQueue(new List<Winit.Modules.Syncing.Model.Interfaces.IAppRequest> { appRequest });
                return Ok(new { Message = "Request Submitted Successfully." });
            }

            catch (Exception ex)
            {
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpGet]
        [Route("StartCacheService")]
        public IActionResult StartCacheService()
        {
            _salesOrderWorkerService.StartAsync(default);
            return SendResponse("Service started successfully.");
        }

        /// <summary>
        /// Call this method to stop CacheService
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("StopCacheService")]
        public IActionResult StopCacheService()
        {
            _salesOrderWorkerService.StopAsync(default);
            return SendResponse("Service stopped successfully.");
        }

        private IEnumerable<object> DeserializeRequestBody(string linkedItemType, string requestBody)
        {
            var typeMapping = new Dictionary<string, Type>
            {
                { "SalesOrder", typeof(List<SalesOrderViewModelDCO>) },
                { "ReturnOrder", typeof(List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder>) },
                { "Collections", typeof(List<Collections>) },
                { "WHStock", typeof(List<WHRequestTempleteModel>) }
            };

            if (!typeMapping.TryGetValue(linkedItemType, out Type targetType))
            {
                throw new InvalidOperationException("Unknown LinkedItemType: " + linkedItemType);
            }

            return (IEnumerable<object>)JsonConvert.DeserializeObject(requestBody, targetType);
        }
    }
}
