using MassTransit;
using NotificationConsumer.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Winit.Modules.Notification.BL.Interfaces.Common;
using Winit.Modules.Notification.BL.Interfaces.DataServices.Order;
using Serilog;
using ILogger = Serilog.ILogger;
using Winit.Modules.Notification.Model.Interfaces;
using Winit.Modules.SMS.Model.Interfaces;
using Winit.Modules.SMS.Model.Classes;

namespace NotificationConsumer.Consumers.Order
{
    public class NotificationOrderSMSConsumer : BaseConsumer, IConsumer<INotificationRequest>
    {
        private readonly INotificationOrderDataServiceBL _notificationDataServiceBL;
        private readonly INotificationSMSServiceBL _notificationSMSServiceBL;
        public NotificationOrderSMSConsumer(INotificationOrderDataServiceBL notificationDataServiceBL,
            INotificationSMSServiceBL notificationSMSServiceBL, IConfiguration configuration) 
            : base(configuration, Models.Enum.NotificationType.SMS, ConsumerSections.Order)
        {
            _notificationDataServiceBL = notificationDataServiceBL;
            _notificationSMSServiceBL = notificationSMSServiceBL;
        }
        protected async override Task ConsumeNotification(ConsumeContext<INotificationRequest> context)
        {
            INotificationRequest notificationRequest = context.Message;
            //Console.WriteLine(JsonSerializer.Serialize(notificationRequest));

            ISms smsRequest = new Sms();

            try
            {
                // Fetch data
                INotificationPOData notificationPOData = await _notificationDataServiceBL.GetDataAsyncForSms(notificationRequest);

                if (notificationPOData == null)
                {
                    // Log in file [NotificationNoDataLog]
                    string errorMessage = $"No Data Found for request: {JsonSerializer.Serialize(notificationRequest)}";
                    Log.Information(errorMessage, "No Data Found");
                    return;
                }
                //if (string.IsNullOrEmpty(notificationPOData.ToEmail))
                //{
                //    // Log in file [NotificationNoDataLog]
                //    string errorMessage = $"No Email Found for request: {JsonSerializer.Serialize(notificationRequest)}";
                //    return;
                //}
                // Creating message 
                smsRequest = await _notificationDataServiceBL.GetMessageByTemplateForSms(notificationPOData, notificationRequest.TemplateName, notificationRequest.UniqueUID);
                if (smsRequest == null)
                {
                    // Log in file [NotificationNoDataLog]
                    string errorMessage = $"No Template Found for request: {JsonSerializer.Serialize(notificationRequest)}";
                    return;
                }

                // Insert this in MailRequest table as pending
                // Mahir to do First check with that UniqUID data exitst or if not then insert
                _ = await _notificationDataServiceBL.CreateSmsRequest(smsRequest);
                //CreateMailRequest(mailRequest);

                // Send email
                await _notificationSMSServiceBL.SendSMSAsync(smsRequest);

                // update status in MailRequest table as Success
                smsRequest.RequestStatus = "Success";
                _ = await _notificationDataServiceBL.UpdateSmsRequest(smsRequest);
                //UpdateSuccessMailRequestStatus(uniqueUID);
            }
            catch (Exception ex)
            {
                // update status in MailRequest table as failure with error message and increase the count
                smsRequest.RequestStatus = "Failure";
                smsRequest.RetryCount += 1;
                smsRequest.ErrorDetails = ex.ToString();
                _ = await _notificationDataServiceBL.UpdateSmsRequest(smsRequest);
                // UpdateFailureMailRequestStatus(uniqueUID, errorMessage);
                // Log Error in file [NotificationErrorLog]
                Log.Error(ex, "Exception");
                throw;
            }
        }
    }
}
