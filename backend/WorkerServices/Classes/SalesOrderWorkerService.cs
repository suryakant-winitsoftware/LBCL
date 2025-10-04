using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Google.Cloud.Firestore;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using System.ServiceModel;
using System.Data;
using System.Data.SqlClient;
using RabbitMQService.Interfaces;
//using RabbitMQService.Classes;
using DBServices.Interfaces;
using WINIT.Shared.Models.Models;
using FirebaseNotificationServices.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using WINITSharedObjects.Models;
using WorkerServices.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.Mobile.BL.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Winit.Shared.Models.Constants;

namespace WorkerServices.Classes
{
    public class SalesOrderWorkerService : BackgroundService/*, ISalesOrderWorkerService*/
    {
        private readonly IConnection _rabbitMQConnection;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<SalesOrderWorkerService> _logger;
        //private readonly IRabbitMQService _rabbitMQService;
        private string queueName;
        private readonly IModel _salesOrderChannel;
        private readonly string excelFileDirectory = "D:\\Logs";
        //private readonly FirebaseNotificationService _firebaseNotificationService;
        private string _fcmToken;
        private readonly IServiceProvider _serviceProvider;
        IFirebaseNotificationService _firebaseNotificationService;
        private readonly IDBService _dBService;
        private ISalesOrderBL _salesOrderBL;
        private IAppVersionUserBL _appVersionUserBL;
        private string step = "Step3";
        private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(4);
        private readonly int _maxRetryAttempts = 1;
        public SalesOrderWorkerService(IConfiguration configuration, ILogger<SalesOrderWorkerService> logger,
            /*RabbitMQService.Interfaces.IRabbitMQService rabbitMQService,*/ IFirebaseNotificationService firebaseNotificationService, IDBService dBService,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            //_rabbitMQService = rabbitMQService;
            queueName = $"{DbTableGroup.Sales}_queue{DbTableGroup.Suffix}";
            _firebaseNotificationService = firebaseNotificationService;
            /*try
            {
                if (_rabbitMQService != null)
                {
                    _rabbitMQService.SubscribeQueue(queueName, OnMessageReceived);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while subscribing to the RabbitMQ queue.");
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
            try
            {
                _rabbitMQService.SubscribeQueue(queueName, OnMessageReceived);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the SalesOrderWorkerService background service.");
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _rabbitMQService.ConsumeMessage(queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the SalesOrderWorkerService background service.");
                }

                await Task.Delay(TimeSpan.FromSeconds(35), stoppingToken);
            }
            /*int retryCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _rabbitMQService.SubscribeQueue(queueName, OnMessageReceived);
                    _rabbitMQService.ConsumeMessage(queueName);
                    retryCount = 0; // Reset retry count if successful
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the SalesOrderWorkerService background service.");

                    if (retryCount >= _maxRetryAttempts)
                    {
                        _logger.LogError("Maximum retry attempts reached. Exiting background service.");
                        break;
                    }

                    retryCount++;
                    _logger.LogInformation($"Retrying connection to RabbitMQ in {_retryInterval.TotalSeconds} seconds...");

                    await Task.Delay(_retryInterval, stoppingToken);
                }
            }*/

        }

        public void InitializeOptionalDependencies()
        {
            var scope = _serviceProvider.CreateScope();
            _salesOrderBL = scope.ServiceProvider.GetService<ISalesOrderBL>();
            _appVersionUserBL = scope.ServiceProvider.GetService<IAppVersionUserBL>();
        }
        public async Task OnMessageReceived(string obj)
        {
            try
            {
                await Task.Run(async () =>
                {
                    _logger.LogInformation("From SalesOrderWorkerService 1 Queue: {SalesOrder}", obj);
                    await PrepareSalesOrderAsyncWithJSONMessageNew(obj);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process the message when the queue event was triggered");
            }
        }


        public async Task PrepareSalesOrderAsyncWithJSONMessageNew(string message)
        {
            InitializeOptionalDependencies();
            TrxHeaderResponse response = new TrxHeaderResponse();
            try
            {
                int index = 0;
                //SalesOrderViewModelForJSON objSalesOrderViewModelForJSON = null;
                SalesOrderViewModelDCO objSalesOrderViewModelDCO = null;
                var messageModel = JsonConvert.DeserializeObject<MessageModel>(message);
                //step 3
                step = "Step3";
                try
                {
                    _logger.LogInformation(step + " for " + messageModel.MessageUID);
                    objSalesOrderViewModelDCO = JsonConvert.DeserializeObject<SalesOrderViewModelDCO>(messageModel.Message.ToString());
                    await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, true, false, null);
                }
                catch (Exception ex)
                {
                    await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, ex.Message);
                }
                string gcmKey = string.Empty;
                TrxStatusDco TrxStatusList = new TrxStatusDco();
                if (objSalesOrderViewModelDCO != null)
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
                        //string strJSONTransction = Mapper.SerializeObjectToJson(objSalesOrderViewModelDCO);

                        int TrxStatus;
                        string StatusMessage = string.Empty;

                        TrxStatus = 1;  //1 mean Success

                        //if (strJSONTransction != null)
                        //{

                        //(ReturnValue, TrxStatus, StatusMessage) = await _salesOrderDL.InsertTransactionFromJson(strJSONTransction);
                        TrxStatus = await _salesOrderBL.InsertorUpdate_SalesOrders(objSalesOrderViewModelDCO);
                        gcmKey = (await _appVersionUserBL.GetAppVersionDetailsByEmpUID(objSalesOrderViewModelDCO.SalesOrder.EmpUID))?.GcmKey;
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
                        
                        //if (StatusMessage.Contains("|"))
                        //{
                        //TrxStatusList[index].GcmKey = gcmKey;
                        //StatusMessage = (StatusMessage ?? "").Split('|').FirstOrDefault();
                        //}
                        if (TrxStatus > 0)
                        {
                            TrxStatusList.Status = 1;
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
                    await _dBService.LogNotificationSent(messageModel.MessageUID, objSalesOrderViewModelDCO.SalesOrder.UID, "SalesOrder", "SalesOrder", msgbody, DateTime.Now);
                    if (TrxStatusList != null && !string.IsNullOrEmpty(gcmKey))
                        await _firebaseNotificationService.SendNotificationAsync(messageModel.MessageUID, "SalesOrder", msgbody, gcmKey);
                    else
                    {
                        await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, "Step5", false, false, "Not able to send FCM notification");
                        _logger.LogInformation("Not able to send FCM notification. UID: {UID}, Customer Name: {CustomerName}", objSalesOrderViewModelDCO.SalesOrder.UID, objSalesOrderViewModelDCO.SalesOrder.EmpUID);
                    }
                    _logger.LogInformation("SalesOrderWorkerService 1 Signal R. UID: {UID}, Customer Name: {CustomerName}", objSalesOrderViewModelDCO.SalesOrder.UID, objSalesOrderViewModelDCO.SalesOrder.EmpUID);
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

