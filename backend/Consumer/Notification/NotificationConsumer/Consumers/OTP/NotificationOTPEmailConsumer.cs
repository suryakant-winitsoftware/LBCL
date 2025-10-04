using MassTransit;
using NotificationConsumer.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.Notification.BL.Interfaces.Common;
using Winit.Modules.Notification.BL.Interfaces.DataServices.General;
using Winit.Modules.Notification.Model.Interfaces;

namespace NotificationConsumer.Consumers.OTP
{
    public class NotificationOTPEmailConsumer : BaseConsumer, IConsumer<INotificationRequest>
    {
        private readonly INotificationGeneralDataServiceBL _notificationDataServiceBL;
        private readonly INotificationEmailServiceBL _notificationEmailServiceBL;
        public NotificationOTPEmailConsumer(INotificationGeneralDataServiceBL notificationDataServiceBL, 
            INotificationEmailServiceBL notificationEmailServiceBL, IConfiguration configuration) 
            : base(configuration, Models.Enum.NotificationType.Email, ConsumerSections.Otp)
        {
            _notificationDataServiceBL = notificationDataServiceBL;
            _notificationEmailServiceBL = notificationEmailServiceBL;
        }
        protected async override Task ConsumeNotification(ConsumeContext<INotificationRequest> context)
        {
            INotificationRequest notificationRequest = context.Message;
            Console.WriteLine($"Processing data by NotificationOTPEmailConsumer");

            // Fetch data
            var data = await _notificationDataServiceBL.GetDataAsync(notificationRequest);

            // Send email
            await _notificationEmailServiceBL.SendEmailAsync(null);

            await Task.CompletedTask;
        }
    }
}
