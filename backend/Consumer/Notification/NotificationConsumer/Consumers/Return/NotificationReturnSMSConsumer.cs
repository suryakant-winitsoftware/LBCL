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
using Winit.Modules.Notification.Model.Interfaces;

namespace NotificationConsumer.Consumers.Return
{
    public class NotificationReturnSMSConsumer : BaseConsumer, IConsumer<INotificationRequest>
    {
        private readonly INotificationGeneralDataServiceBL _notificationDataServiceBL;
        private readonly INotificationSMSServiceBL _notificationSMSServiceBL;
        public NotificationReturnSMSConsumer(INotificationGeneralDataServiceBL notificationDataServiceBL, 
            INotificationSMSServiceBL notificationSMSServiceBL, IConfiguration configuration) 
            : base(configuration, Models.Enum.NotificationType.SMS, ConsumerSections.Return)
        {
            _notificationDataServiceBL = notificationDataServiceBL;
            _notificationSMSServiceBL = notificationSMSServiceBL;
        }
        protected async override Task ConsumeNotification(ConsumeContext<INotificationRequest> context)
        {
            INotificationRequest notificationRequest = context.Message;
            Console.WriteLine($"Processing data by NotificationReturnSMSConsumer");

            // Fetch data
            var data = await _notificationDataServiceBL.GetDataAsync(notificationRequest);

            // Send email
            //await _notificationSMSServiceBL.SendSMSAsync(data);

            await Task.CompletedTask;
        }
    }
}
