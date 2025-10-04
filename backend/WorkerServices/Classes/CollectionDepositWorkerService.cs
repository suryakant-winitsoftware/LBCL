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
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Shared.Models.Constants;
using WINIT.Shared.Models.Models;
using WINITSharedObjects.Models;

namespace WorkerServices.Classes
{
    public class CollectionDepositWorkerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CollectionDepositWorkerService> _logger;
        private readonly string queueName;
        private readonly IServiceProvider _serviceProvider;
        IFirebaseNotificationService _firebaseNotificationService;
        private readonly IDBService _dBService;
        private ICollectionModuleBL _collectionBL;
        private IAppVersionUserBL _appVersionUserBL;
        private string step = "Step3";

        public CollectionDepositWorkerService(IConfiguration configuration, ILogger<CollectionDepositWorkerService> logger,
            IFirebaseNotificationService firebaseNotificationService, IDBService dBService, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            queueName = DbTableGroup.CollectionDeposit + "_queue"+ DbTableGroup.Suffix;
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
                    _logger.LogError(ex, "An error occurred in the CollectionWorkerService background service.");
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
                    _logger.LogInformation("From CollectionWorkerService 1 Queue: {Collection}", obj);
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
            _collectionBL = scope.ServiceProvider.GetService<ICollectionModuleBL>();
            _appVersionUserBL = scope.ServiceProvider.GetService<IAppVersionUserBL>();
        }

        public async Task PrepareCollectionAsyncForQueue(string message)
        {
            InitializeOptionalDependencies();
            TrxHeaderResponse response = new TrxHeaderResponse();
            try
            {
                int index = 0;
                AccCollectionDeposit accCollectionDeposit = null;
                var messageModel = JsonConvert.DeserializeObject<MessageModel>(message);
                //step 3
                step = "Step3";
                try
                {
                    _logger.LogInformation(step + " for " + messageModel.MessageUID);
                    accCollectionDeposit = JsonConvert.DeserializeObject<AccCollectionDeposit>(messageModel.Message.ToString());
                    await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, true, false, null);
                }
                catch (Exception ex)
                {
                    await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, false, true, ex.Message);
                }
                TrxStatusDco TrxStatusList = new TrxStatusDco();
                string gcmKey = string.Empty;
                if (accCollectionDeposit != null)
                {

                    try
                    {
                        //step 4
                        step = "Step4";
                        _logger.LogInformation(step + " for " + messageModel.MessageUID);
                      //  objCollectionViewModelArr[0] = objCollectionViewModel;
                        TrxStatusList = new TrxStatusDco();

                        bool TrxStatus;
                        string StatusMessage = string.Empty;
                        // StatusMessage = await _collectionBL.CreateReceipt(objCollectionViewModelArr);
                        //if (StatusMessage == "Successfully Inserted Data Into tables")
                        //{
                        //    TrxStatus = 1;
                        //} else
                        //{
                        //    TrxStatus = 0;
                        //}
                        TrxStatus = await _collectionBL.CreateCashDepositRequest(accCollectionDeposit);
                        gcmKey = (await _appVersionUserBL.GetAppVersionDetailsByEmpUID(accCollectionDeposit.EmpUID)).GcmKey;
                        await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, step, true, false, TrxStatus + "|" + StatusMessage);
                        if (TrxStatus)
                        {
                            TrxStatusList.Status = 1;
                        }
                        //TrxStatusList.Status = TrxStatus;
                        //TrxStatusList[index].GcmKey = gcmKey;
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
                    await _dBService.LogNotificationSent(messageModel.MessageUID, accCollectionDeposit.UID, "Collection", "Collection", msgbody, DateTime.Now);
                    if (TrxStatusList != null && !string.IsNullOrEmpty(gcmKey))
                        await _firebaseNotificationService.SendNotificationAsync(messageModel.MessageUID, "Collection", msgbody, gcmKey);
                    else
                    {
                        await _dBService.UpdateLogByStepAsync(messageModel.MessageUID, "Step5", false, false, "Not able to send FCM notification");
                        _logger.LogInformation("Not able to send FCM notification. UID: {UID}, Customer Name: {CustomerName}", accCollectionDeposit.UID, accCollectionDeposit.EmpUID);
                    }
                    _logger.LogInformation("CollectionWorkerService 1 Signal R. UID: {UID}, Customer Name: {CustomerName}", accCollectionDeposit.UID, accCollectionDeposit.EmpUID);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a Collection.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a customer by their unique identifier.
        /// </summary>
        /// <param name="Id">The customer's ID</param>
        /// <returns>The customer object if found, otherwise null</returns>
        public async Task<Customer> SelectCustomerByUID(int Id)
        {
            // Implementation of the method
            throw new NotImplementedException();
        }
    }
}
