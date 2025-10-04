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
using Winit.Modules.Mobile.BL.Interfaces;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Shared.Models.Constants;
using WINIT.Shared.Models.Models;
using WINITSharedObjects.Models;

namespace WorkerServices.Classes
{
    public class WHStockWorkerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<WHStockWorkerService> _logger;
        private readonly string queueName;
        private readonly IServiceProvider _serviceProvider;
        IFirebaseNotificationService _firebaseNotificationService;
        private readonly IDBService _dBService;
        private IWHStockBL _whStockBL;
        private IAppVersionUserBL _appVersionUserBL;
        private string step = "Step3";

        public WHStockWorkerService(IConfiguration configuration, ILogger<WHStockWorkerService> logger,
            IFirebaseNotificationService firebaseNotificationService, IDBService dBService, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            queueName = $"{DbTableGroup.StockRequest}_queue{DbTableGroup.Suffix}";
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
                    _logger.LogError(ex, "An error occurred in the WHStockWorkerService background service.");
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
                    _logger.LogInformation("From StoreCheckWorkerService 2 Queue: {StoreCheck}", obj);
                    await PrepareWHStockAsyncForQueue(obj);
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
            _whStockBL = scope.ServiceProvider.GetService<IWHStockBL>();
            _appVersionUserBL = scope.ServiceProvider.GetService<IAppVersionUserBL>();
        }

        public async Task PrepareWHStockAsyncForQueue(string message)
        {
            InitializeOptionalDependencies();
            TrxHeaderResponse response = new TrxHeaderResponse();
            try
            {
                int index = 0;
                WHRequestTempleteModel objWHRequestTempleteModel = null;
                var messageModel = JsonConvert.DeserializeObject<MessageModel>(message);
                //step 3
                step = "Step3";
                try
                {
                    _logger.LogInformation(step + " for " + messageModel.MessageUID);
                    objWHRequestTempleteModel = JsonConvert.DeserializeObject<WHRequestTempleteModel>(messageModel.Message.ToString());
                    _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, true, false, null);
                }
                catch (Exception ex)
                {
                    _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, ex.Message);
                }
                TrxStatusDco TrxStatusList = new TrxStatusDco();
                string gcmKey = string.Empty;
                if (objWHRequestTempleteModel != null)
                {

                    try
                    {
                        //step 4
                        step = "Step4";
                        _logger.LogInformation(step + " for " + messageModel.MessageUID);
                        TrxStatusList = new TrxStatusDco();

                        int TrxStatus;
                        string StatusMessage = string.Empty;
                        

                        TrxStatus = 1;  //1 mean Success

                        TrxStatus = await _whStockBL.CUDWHStock(objWHRequestTempleteModel);
                        
                        gcmKey = (await _appVersionUserBL.GetAppVersionDetailsByEmpUID(objWHRequestTempleteModel.WHStockRequest.RequestByEmpUID)).GcmKey;
                        _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, true, false, TrxStatus + "|" + StatusMessage);

                        TrxStatusList.Status = TrxStatus;
                        //TrxStatusList[index].GcmKey = gcmKey;
                        if (TrxStatus == 1)
                        {
                            StatusMessage = "Status as inserted.";
                        }
                        else if (TrxStatus == -1)
                        {
                            StatusMessage = "Error Occured.";
                            _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, "");
                            throw new Exception(StatusMessage);

                        }
                        TrxStatusList.Message = StatusMessage;
                        TrxStatusList.ResponseUID = messageModel.MessageUID;
                    }
                    catch (Exception ex)
                    {
                        _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, false, ex.Message);
                        TrxStatusList.Message = "Failure - " + ex.Message.ToString() + ex.StackTrace.ToString();
                        TrxStatusList.Status = -1;
                        _logger.LogError(ex, "An error occurred while inserting a Load Request.");
                        throw;
                    }

                    response.TrxStatusList = TrxStatusList;
                    string msgbody = JsonConvert.SerializeObject(TrxStatusList);
                    await _dBService.LogNotificationSent(messageModel.MessageUID, objWHRequestTempleteModel.WHStockRequest.UID, "WHStockRequest", "WHStockRequest", msgbody, DateTime.Now);
                    if (TrxStatusList != null && !string.IsNullOrEmpty(gcmKey))
                        await _firebaseNotificationService.SendNotificationAsync(messageModel.MessageUID, "WHStockRequest", msgbody, gcmKey);
                    else
                    {
                        _dBService.UpdateLogByStepAsync(messageModel.MessageUID, "Step5", false, false, "Not able to send FCM notification");
                        _logger.LogInformation("Not able to send FCM notification. UID: {UID}, Customer Name: {CustomerName}", objWHRequestTempleteModel.WHStockRequest.WareHouseUID, objWHRequestTempleteModel.WHStockRequest.RequestByEmpUID);
                    }
                    _logger.LogInformation("WHStockWorkerService 1 Signal R. UID: {UID}, Customer Name: {CustomerName}", objWHRequestTempleteModel.WHStockRequest.WareHouseUID, objWHRequestTempleteModel.WHStockRequest.RequestByEmpUID);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a sales order.");
                throw;
            }
        }
    }
}
