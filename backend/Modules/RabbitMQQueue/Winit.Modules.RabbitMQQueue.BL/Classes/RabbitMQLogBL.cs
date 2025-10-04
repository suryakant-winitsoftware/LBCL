using Winit.Modules.RabbitMQQueue.DL;
using Winit.Modules.RabbitMQQueue.BL.Interfaces;
using RabbitMQService.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Microsoft.Extensions.Logging;
using WINIT.Shared.Models.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.RabbitMQQueue.BL.Classes
{
    public class RabbitMQLogBL : IRabbitMQLogBL
    {
        protected readonly DL.Interfaces.IRabbitMQLogDL _rabbitMQLogDL = null;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQLogBL> _logger;
        public RabbitMQLogBL(DL.Interfaces.IRabbitMQLogDL rabbitMQLogDL, IServiceProvider serviceProvider, ILogger<RabbitMQLogBL> logger)
        {
            _rabbitMQLogDL = rabbitMQLogDL;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task<int> InsertAppRequestInfo(Winit.Modules.Syncing.Model.Interfaces.IAppRequest appRequest)
        {
            return await _rabbitMQLogDL.InsertAppRequestInfo(appRequest);
        }

        public async Task<int> UpdateLogByStepAsync(string UID, string Step, bool StepResult, bool IsFailed, string comments)
        {
            return await _rabbitMQLogDL.UpdateLogByStepAsync(UID, Step, StepResult, IsFailed, comments);

        }
        public async Task PostToRabbitMQQueue(List<Winit.Modules.Syncing.Model.Interfaces.IAppRequest> appRequests)
        {
            if(appRequests == null || appRequests.Count == 0)
            {
                return;
            }
            foreach(Winit.Modules.Syncing.Model.Interfaces.IAppRequest appRequest in appRequests)
            {
                await PostToRabbitMQQueue(appRequest);
            }
        }
        private async Task PostToRabbitMQQueue(Winit.Modules.Syncing.Model.Interfaces.IAppRequest appRequest)
        {
            string step = "";
            try
            {
                IRabbitMQService? _rabbitMQService = _serviceProvider.GetService<RabbitMQService.Interfaces.IRabbitMQService>();
                if(_rabbitMQService == null)
                {
                    return;
                }
                //IRabbitMQService _rabbitMQService = _serviceProvider.CreateInstance<IRabbitMQService>();
                _logger.LogInformation($"MessageUID: {appRequest.UID}");
                // Create app_request_info record if not exits
                await InsertAppRequestInfo(appRequest);
                _logger.LogInformation($"app_request_info created. MessageUID: {appRequest.UID}");
                try
                {
                    step = "Step2";
                    MessageModel messageModel = new() { MessageUID = appRequest.UID, Message = appRequest.RequestBody };
                    string messageBody = JsonConvert.SerializeObject(messageModel);
                    _rabbitMQService?.SendMessage($"{appRequest.LinkedItemType}_queue{DbTableGroup.Suffix}", messageBody);

                    // Step 2
                    _ = await UpdateLogByStepAsync(appRequest.UID, step, true, false, null);
                    _logger.LogInformation($"Posted to Queue. MessageUID: {appRequest.UID}, {step} completed.");
                }
                catch (Exception ex)
                {
                    _ = await UpdateLogByStepAsync(appRequest.UID, step, false, true, ex.Message);
                    _logger.LogError(ex, $"Error occurred while sending message to RabbitMQ. MessageUID: {appRequest.UID}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to post to the Queue. MessageUID: {appRequest.UID}");
                throw;
            }
        }
    }
}
