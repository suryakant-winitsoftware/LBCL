using DBServices.Interfaces;
using FirebaseNotificationServices.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.Mobile.BL.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Shared.Models.Constants;
using WINIT.Shared.Models.Models;
using WINITSharedObjects.Models;

namespace WorkerServices.Classes
{
    public class MasterWorkerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MasterWorkerService> _logger;
        private readonly string queueName;
        private readonly IServiceProvider _serviceProvider;
        IFirebaseNotificationService _firebaseNotificationService;
        private readonly IDBService _dBService;
        private Winit.Modules.JourneyPlan.BL.Interfaces.IBeatHistoryBL _beatHistoryBL;
        private IAppVersionUserBL _appVersionUserBL;
        private string step = "Step3";

        public MasterWorkerService(IConfiguration configuration, ILogger<MasterWorkerService> logger,
            IFirebaseNotificationService firebaseNotificationService, IDBService dBService, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            queueName = DbTableGroup.Master + "_queue"+ DbTableGroup.Suffix;
            _firebaseNotificationService = firebaseNotificationService;
            _dBService = dBService;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var _rabbitMQService = _serviceProvider.GetService<RabbitMQService.Interfaces.IRabbitMQService>();
            if (_rabbitMQService == null)
            {
                _logger.LogInformation("RabbitMQ service is not available. Skipping message consumption.");
                return;
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _rabbitMQService.SubscribeQueue(queueName, OnMessageReceived);
                    _rabbitMQService.ConsumeMessage(queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the Master WorkerService background service.");
                }

                await Task.Delay(TimeSpan.FromSeconds(35), stoppingToken);
            }
        }

        public async Task OnMessageReceived(string obj)
        {
            try
            {
                await Task.Run(async () =>
                {
                    _logger.LogInformation("From Master Worker Service 1 Queue: {Master}", obj);
                    await PrepareCollectionAsyncForQueue(obj);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process the message when the queue event was triggered");
                throw;
            }
        }

        public void InitializeOptionalDependencies()
        {
            var scope = _serviceProvider.CreateScope();
            _beatHistoryBL = scope.ServiceProvider.GetService<IBeatHistoryBL>();
            _appVersionUserBL = scope.ServiceProvider.GetService<IAppVersionUserBL>();
        }

        public async Task PrepareCollectionAsyncForQueue(string message)
        {
            InitializeOptionalDependencies();
            TrxHeaderResponse response = new TrxHeaderResponse();
            try
            {
                int index = 0;
                MasterDTO masterObject = null;
                var messageModel = JsonConvert.DeserializeObject<MessageModel>(message);
                //step 3
                step = "Step3";
                try
                {
                    _logger.LogInformation(step + " for " + messageModel.MessageUID);
                    masterObject = JsonConvert.DeserializeObject<MasterDTO>(messageModel.Message.ToString());
                    await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, true, false, null);
                }
                catch (Exception ex)
                {
                    await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, ex.Message);
                }
                TrxStatusDco TrxStatusList = new TrxStatusDco();
                string gcmKey = string.Empty;
                if (masterObject != null)
                {

                    try
                    {
                        //step 4
                        step = "Step4";
                        _logger.LogInformation(step + " for " + messageModel.MessageUID);
                        TrxStatusList = new TrxStatusDco();
                        bool TrxStatus;
                        string StatusMessage = string.Empty;
                        TrxStatus = await _beatHistoryBL.InsertMasterRabbitMQueue(masterObject);
                        //empuid sending null
                        gcmKey = (await _appVersionUserBL.GetAppVersionDetailsByEmpUID(null)).GcmKey;
                        await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, true, false, TrxStatus + "|" + StatusMessage);
                        if (TrxStatus)
                        {
                            TrxStatusList.Status = 1;
                        }
                        if (TrxStatus == true)
                        {
                            StatusMessage = "Status as inserted.";
                        }
                        else if (TrxStatus == false)
                        {
                            StatusMessage = "Error Occured.";
                            await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, "");
                            throw new Exception(StatusMessage);

                        }
                        TrxStatusList.Message = StatusMessage;
                        TrxStatusList.ResponseUID = messageModel.MessageUID;
                    }
                    catch (Exception ex)
                    {
                        await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, false, ex.Message);
                        TrxStatusList.Message = "Failure - " + ex.Message.ToString() + ex.StackTrace.ToString();
                        TrxStatusList.Status = -1;
                        _logger.LogError(ex, "An error occurred while inserting a Collection.");
                        throw;
                    }

                    response.TrxStatusList = TrxStatusList;
                    string msgbody = JsonConvert.SerializeObject(TrxStatusList);
                    // empuid sending null
                    await _dBService.LogNotificationSent(messageModel.MessageUID, "Master", "Master", "Master", msgbody, DateTime.Now);
                    if (TrxStatusList != null && !string.IsNullOrEmpty(gcmKey))
                        await _firebaseNotificationService.SendNotificationAsync(messageModel.MessageUID, "Master", msgbody, gcmKey);
                    else
                    {
                        await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, "Step5", false, false, "Not able to send FCM notification");
                        // uid and empuid sending null
                        _logger.LogInformation("Not able to send FCM notification. UID: {UID}, Customer Name: {CustomerName}", null, null);
                    }
                    _logger.LogInformation("CollectionWorkerService 1 Signal R. UID: {UID}, Customer Name: {CustomerName}", null, null);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a Collection.");
                throw;
            }
        }
    }
}
