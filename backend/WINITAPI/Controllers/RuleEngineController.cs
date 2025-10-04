using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WINITServices.Classes.RabbitMQ;
using WINITServices.Classes.RuleEngine;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Models.RuleEngine;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RuleEngineController : WINITBaseController
    {
        protected readonly WINITServices.Interfaces.RuleEngine.IRuleEngineService _ruleEngineService;
        private readonly PublisherMQService _publisher;
        private readonly SubscriberMQService _subscriber;
        // protected const string qname = "RuleEngineQ";
        public RuleEngineController(IServiceProvider serviceProvider, 
            WINITServices.Interfaces.RuleEngine.IRuleEngineService ruleEngineService,
            SubscriberMQService subscriber, PublisherMQService publisher) : base(serviceProvider)
        {
            _ruleEngineService = ruleEngineService;
            _subscriber = subscriber;
            _publisher = publisher;

        }
        [HttpGet("{ruleId}")]
        public async Task<IActionResult> GetRule(int ruleId)
        {
            try
            {
                var rule = await _ruleEngineService.GetRule(ruleId);
                Log.Information("Successfully retrieved data", rule);
                return SendResponse(rule);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve data with ID: {@ID}", ruleId);
                return NotFound();
            }
        }
        [HttpPost("ExecuteRule/{ruleId}")]
        public async Task<IActionResult> ExecuteRule(int ruleId, Dictionary<string, object> parameters)
        {
            try
            {
                var r = await _ruleEngineService.CreateRequest(ruleId, parameters);
                if (r > 0)
                {
                    var data = new MessageData
                    {
                        requestId = r,
                        status = RuleEngineStatus.InProgress.ToString()
                    };
                    var serializedData = JsonConvert.SerializeObject(data);
                    // Publish the message and trigger the consuming process asynchronously
                    await Task.WhenAll(Task.Run(() => _subscriber.StartConsuming(_ruleEngineService.HandleMessageReceivedAsync)), Task.Run(() => _publisher.PublishMessage(serializedData)));
                    return Ok(new { success = true, message = "Request submitted successfully", requetsid = r });
                }
                else
                    return Ok(new { success = false, message = "" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to complete operation with ID: {@ID}", ruleId);
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("doaction/{requestid}")]
        public async Task<IActionResult> doaction(int requestid, WINITSharedObjects.Constants.requestStatus req)
        {
            try
            {
                var r = await _ruleEngineService.ApproveRejectRequest(requestid, req.status);
                if (r > 0)
                {
                    var data = new MessageData
                    {
                        requestId = requestid,
                        status = req.status.ToString()
                    };
                    var serializedData = JsonConvert.SerializeObject(data);
                    // Publish the message and trigger the consuming process asynchronously
                    await Task.WhenAll(Task.Run(() => _subscriber.StartConsuming(_ruleEngineService.HandleMessageReceivedAsync)), Task.Run(() => _publisher.PublishMessage(serializedData)));
                    return Ok(new { success = true, message = "Request " + req.status + " successfully" });
                }
                else
                    return Ok(new { success = false, message = "" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to complete operation with ID: {@ID}", requestid);
                return StatusCode(500, ex.Message);
            }
        }
    }

}
