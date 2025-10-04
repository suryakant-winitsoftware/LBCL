using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using DBServices.Interfaces;
using FirebaseNotificationServices.Interfaces;
using Google.Apis.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQService.Interfaces;
using Winit.Modules.Mobile.BL.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Newtonsoft.Json;
using Winit.Modules.SalesOrder.Model.Classes;
using WINITSharedObjects.Models;
using WINIT.Shared.Models.Models;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Shared.Models.Constants;

namespace WorkerServices.Classes
{
    public class ReturnOrderWorkerService : BackgroundService
    {
        private readonly IConnection _rabbitMQConnection;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<ReturnOrderWorkerService> _logger;
        //private readonly IRabbitMQService _rabbitMQService;
        private readonly string queueName;
        private readonly IModel _salesOrderChannel;
        private readonly string excelFileDirectory = "D:\\Logs"; 
        private string _fcmToken;
        private readonly IServiceProvider _serviceProvider;
        IFirebaseNotificationService _firebaseNotificationService;
        private readonly IDBService _dBService;
        private IReturnOrderBL _returnOrderBL;
        private IAppVersionUserBL _appVersionUserBL;
        private string step = "Step3";

        public ReturnOrderWorkerService(IConfiguration configuration, ILogger<ReturnOrderWorkerService> logger,
            /*RabbitMQService.Interfaces.IRabbitMQService rabbitMQService,*/ IFirebaseNotificationService firebaseNotificationService, 
            IDBService dBService, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            //_rabbitMQService = rabbitMQService;
            queueName = $"{DbTableGroup.Return}_queue{DbTableGroup.Suffix}";
            _firebaseNotificationService = firebaseNotificationService;
            /*try
            {
                _rabbitMQService.SubscribeQueue(queueName, OnMessageReceived);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the SalesOrderWorkerService background service.");
            }*/
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
                    _logger.LogError(ex, "An error occurred in the ReturnOrderWorkerService background service.");
                }

                await Task.Delay(TimeSpan.FromSeconds(35), stoppingToken);
            }
        }

        public async Task OnMessageReceived(string obj)
        {
            try
            {
                // throw new Exception {  };
                await Task.Run(async () =>
                {
                    _logger.LogInformation("From ReturnOrderWorkerService 2 Queue: {ReturnOrder}", obj);
                    await PrepareReturnOrderAsyncWithJSONMessage(obj);
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
            _returnOrderBL = scope.ServiceProvider.GetService<IReturnOrderBL>();
            _appVersionUserBL = scope.ServiceProvider.GetService<IAppVersionUserBL>();
        }

        public async Task PrepareReturnOrderAsyncWithJSONMessage(string message)
        {
            InitializeOptionalDependencies();
            TrxHeaderResponse response = new TrxHeaderResponse();
            try
            {
                int index = 0;
                ReturnOrderMasterDTO returnOrderMasterDTO = null;
                var messageModel = JsonConvert.DeserializeObject<MessageModel>(message);
                //step 3
                step = "Step3";
                try
                {
                    _logger.LogInformation(step + " for " + messageModel.MessageUID);
                    returnOrderMasterDTO = JsonConvert.DeserializeObject<ReturnOrderMasterDTO>(messageModel.Message.ToString());
                    await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, true, false, null);
                }
                catch (Exception ex)
                {
                    await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, ex.Message);
                }
                TrxStatusDco TrxStatusList = new TrxStatusDco();
                string gcmKey = string.Empty;
                if (returnOrderMasterDTO != null)
                {

                    try
                    {
                        //step 4
                        step = "Step4";
                        _logger.LogInformation(step + " for " + messageModel.MessageUID);
                        //objSalesOrderViewModelForJSON = new SalesOrderViewModelForJSON();

                        //Map Transaction object.
                        //objSalesOrderViewModelForJSON = Mapper.ToDataContractObjects(objSalesOrderViewModelDCO);

                        //if (objSalesOrderViewModelForJSON != null)
                        //{
                        TrxStatusList = new TrxStatusDco();
                        //string strJSONTransction = Mapper.SerializeObjectToJson(returnOrderMasterDTO);

                        int TrxStatus;
                        string StatusMessage = string.Empty;

                        TrxStatus = 1;  //1 mean Success

                        //if (strJSONTransction != null)
                        //{

                        //(ReturnValue, TrxStatus, StatusMessage) = await _salesOrderDL.InsertTransactionFromJson(strJSONTransction);
                        TrxStatus = await _returnOrderBL.CreateReturnOrderMaster(returnOrderMasterDTO);
                        gcmKey = (await _appVersionUserBL.GetAppVersionDetailsByEmpUID(returnOrderMasterDTO?.ReturnOrder?.EmpUID)).GcmKey;
                        await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, true, false, TrxStatus + "|" + StatusMessage);

                        //}
                        //else
                        //{
                        //    _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, "Something is wrong with request body");
                        //    TrxStatus = -100;
                        //}
                        //Preparing reponse object.
                        /*TrxStatusList[index].TrxCode = objSalesOrderViewModelDCO.TrxCode;
                        TrxStatusList[index].AppTrxId = objSalesOrderViewModelDCO.AppTrxId;*/
                        TrxStatusList.Status = TrxStatus;
                        //if (StatusMessage.Contains("|"))
                        //{
                        //TrxStatusList[index].GcmKey = gcmKey;
                        //StatusMessage = (StatusMessage ?? "").Split('|').FirstOrDefault();
                        //}
                        if (TrxStatus == 1)
                        {
                            StatusMessage = "Status as inserted.";
                        }
                        else if (TrxStatus == -1)
                        {
                            StatusMessage = "Error Occured.";
                            await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, "");
                            throw new Exception(StatusMessage);

                        }
                        TrxStatusList.Message = StatusMessage;
                        TrxStatusList.ResponseUID = messageModel.MessageUID;
                        //}
                        //else
                        //{
                        //    _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, "Something is wrong with request body");
                        //}
                    }
                    catch (Exception ex)
                    {
                        await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, false, ex.Message);
                        TrxStatusList.Message = "Failure - " + ex.Message.ToString() + ex.StackTrace.ToString();
                        TrxStatusList.Status = -1;
                        _logger.LogError(ex, "An error occurred while inserting a sales order.");
                        throw;
                    }

                    response.TrxStatusList = TrxStatusList;
                    //response.ServerDateTime = DateTime.Now.ToString();
                    string msgbody = JsonConvert.SerializeObject(TrxStatusList);
                    await _dBService.LogNotificationSent(messageModel.MessageUID, returnOrderMasterDTO?.ReturnOrder?.UID, "ReturnOrder", "ReturnOrder", msgbody, DateTime.Now);
                    if (TrxStatusList != null && !string.IsNullOrEmpty(gcmKey))
                        await _firebaseNotificationService.SendNotificationAsync(messageModel.MessageUID, "SalesOrder", msgbody, gcmKey);
                    else
                    {
                        await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, "Step5", false, false, "Not able to send FCM notification");
                        _logger.LogInformation("Not able to send FCM notification. UID: {UID}, Customer Name: {CustomerName}", returnOrderMasterDTO.ReturnOrder.ReturnOrderNumber, returnOrderMasterDTO.ReturnOrder.EmpUID);
                    }
                    _logger.LogInformation("ReturnOrderWorkerService 1 Signal R. UID: {UID}, Customer Name: {CustomerName}", returnOrderMasterDTO.ReturnOrder.UID, returnOrderMasterDTO?.ReturnOrder?.EmpUID);
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
