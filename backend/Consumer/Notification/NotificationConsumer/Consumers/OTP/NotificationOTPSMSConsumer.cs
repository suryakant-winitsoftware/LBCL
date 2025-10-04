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
using Winit.Modules.Notification.BL.Interfaces.DataServices.General;
using Winit.Modules.Notification.Model.Classes.Sms;
using Winit.Modules.Notification.Model.Interfaces;
using Winit.Modules.Notification.Model.Interfaces.Sms;
using Winit.Modules.SMS.Model.Classes;

namespace NotificationConsumer.Consumers.OTP
{
    public class NotificationOTPSMSConsumer : BaseConsumer, IConsumer<INotificationRequest>
    {
        private readonly INotificationGeneralDataServiceBL _notificationDataServiceBL;
        private readonly INotificationSMSServiceBL _notificationSMSServiceBL;
        public NotificationOTPSMSConsumer(INotificationGeneralDataServiceBL notificationDataServiceBL, 
            INotificationSMSServiceBL notificationSMSServiceBL, IConfiguration configuration) 
            : base(configuration, Models.Enum.NotificationType.SMS, ConsumerSections.Otp)
        {
            _notificationDataServiceBL = notificationDataServiceBL;
            _notificationSMSServiceBL = notificationSMSServiceBL;
        }
        protected async override Task ConsumeNotification(ConsumeContext<INotificationRequest> context)
        {
            INotificationRequest notificationRequest = context.Message;
            Console.WriteLine($"Processing data by NotificationOTPSMSConsumer");

            // Send SMS
            var data = await _notificationDataServiceBL.GetDataAsync(notificationRequest);

            await Task.CompletedTask;
        }
    }
}
