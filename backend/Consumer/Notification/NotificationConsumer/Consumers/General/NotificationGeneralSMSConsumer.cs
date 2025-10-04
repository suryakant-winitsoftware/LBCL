using MassTransit;
using NotificationConsumer.Models.Constants;
using Serilog;
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

namespace NotificationConsumer.Consumers.General
{
    public class NotificationGeneralSMSConsumer : BaseConsumer, IConsumer<INotificationRequest>
    {
        private readonly INotificationGeneralDataServiceBL _notificationDataServiceBL;
        private readonly INotificationSMSServiceBL _notificationSMSServiceBL;
        public NotificationGeneralSMSConsumer(INotificationGeneralDataServiceBL notificationDataServiceBL, 
            INotificationSMSServiceBL notificationSMSServiceBL, IConfiguration configuration) 
            : base(configuration, Models.Enum.NotificationType.SMS, ConsumerSections.General)
        {
            _notificationDataServiceBL = notificationDataServiceBL;
            _notificationSMSServiceBL = notificationSMSServiceBL;
        }
        protected async override Task ConsumeNotification(ConsumeContext<INotificationRequest> context)
        {
            INotificationRequest notificationRequest = context.Message;
            Console.WriteLine($"Processing data by NotificationGeneralSMSConsumer");
            //Log.Information("This should go to DataLog.txt only.");
            //Log.Error("This should go to ErrorLog.txt only.");
            // Fetch data
            var data = await _notificationDataServiceBL.SendSms(notificationRequest);

            // Send email
            //await _notificationSMSServiceBL.SendSMSAsync(data);
        }
    }
}
