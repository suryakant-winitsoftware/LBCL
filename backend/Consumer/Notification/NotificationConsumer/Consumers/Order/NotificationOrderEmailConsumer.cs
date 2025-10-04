using MassTransit;
using NotificationConsumer.Models.Constants;
using Serilog;
using System.Text.Json;
using Winit.Modules.Email.Model.Classes;
using Winit.Modules.Email.Model.Interfaces;
using Winit.Modules.Notification.BL.Interfaces.Common;
using Winit.Modules.Notification.BL.Interfaces.DataServices.Order;
using Winit.Modules.Notification.Model.Interfaces;
using ILogger = Serilog.ILogger;

namespace NotificationConsumer.Consumers.Order
{
    public class NotificationOrderEmailConsumer : BaseConsumer, IConsumer<INotificationRequest>
    {
        private readonly INotificationOrderDataServiceBL _notificationDataServiceBL;
        private readonly INotificationEmailServiceBL _notificationEmailServiceBL;
        private readonly ILogger _logger;
        public NotificationOrderEmailConsumer(INotificationOrderDataServiceBL notificationDataServiceBL,
            INotificationEmailServiceBL notificationEmailServiceBL, IConfiguration configuration)
            : base(configuration, Models.Enum.NotificationType.Email, ConsumerSections.Order)
        {
            _notificationDataServiceBL = notificationDataServiceBL;
            _notificationEmailServiceBL = notificationEmailServiceBL;
            _logger = Log.Logger;
        }
        protected async override Task ConsumeNotification(ConsumeContext<INotificationRequest> context)
        {
            INotificationRequest notificationRequest = context.Message;
            //Console.WriteLine(JsonSerializer.Serialize(notificationRequest));
            IMailRequest mailRequest = new MailRequest();
            try
            {
                // Fetch data
                INotificationPOData notificationPOData = await _notificationDataServiceBL.GetDataAsyncForEmail(notificationRequest);

                if (notificationPOData == null)
                {
                    // Log in file [NotificationNoDataLog]
                    string errorMessage = $"No Data Found for request: {JsonSerializer.Serialize(notificationRequest)}";
                    Log.Information(errorMessage, "No Data Found");
                    return;
                }
                // Creating message 
                mailRequest = await _notificationDataServiceBL.GetMessageByTemplateForEmail(notificationPOData, notificationRequest.TemplateName, notificationRequest.UniqueUID);
                if (mailRequest == null)
                {
                    // Log in file [NotificationNoDataLog]
                    string errorMessage = $"No Template Found for request: {JsonSerializer.Serialize(notificationRequest)}";
                    Log.Information(errorMessage, "No Data Found");
                    return;
                }
                if (string.IsNullOrEmpty(mailRequest.Receivers.FirstOrDefault().ToEmail))
                {
                    // Log in file [NotificationNoDataLog]
                    string errorMessage = $"No Email Found for request: {JsonSerializer.Serialize(notificationRequest)}";
                    Log.Information(errorMessage, "No Data Found");
                    return;
                }
                // Insert this in MailRequest table as pending
                // Mahir to do First check with that UniqUID data exitst or if not then insert
                _ = await _notificationDataServiceBL.CreateMailRequest(mailRequest);
                //CreateMailRequest(mailRequest);

                // Send email
                await _notificationEmailServiceBL.SendEmailAsync(mailRequest);

                // update status in MailRequest table as Success
                _ = await _notificationDataServiceBL.UpdateSuccessEmailRequest(mailRequest);
                //UpdateSuccessMailRequestStatus(uniqueUID);
            }
            catch (Exception ex)
            {
                // update status in MailRequest table as failure with error message and increase the count
                mailRequest.RetryCount += 1;
                mailRequest.ErrorDetails = ex.ToString();
                _ = await _notificationDataServiceBL.UpdateFailureEmailRequest(mailRequest);
                // UpdateFailureMailRequestStatus(uniqueUID, errorMessage);
                // Log Error in file [NotificationErrorLog]
                Log.Error(ex, "Exception");
                throw;
            }
        }
    }
}
