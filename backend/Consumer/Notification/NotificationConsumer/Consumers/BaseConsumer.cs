using MassTransit;
using Microsoft.AspNetCore.Components.Sections;
using NotificationConsumer.Models.Constants;
using NotificationConsumer.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Notification.Model.Interfaces;

namespace NotificationConsumer.Consumers
{
    public abstract class BaseConsumer
    {
        protected readonly List<string> SMSEnabledForSections = new List<string>();
        protected readonly List<string> EmailEnabledForSections = new List<string>();
        private Models.Enum.NotificationType _notificationType { get; set; }
        private string _consumerSections { get; set; }
        public BaseConsumer(IConfiguration configuration, 
            Models.Enum.NotificationType notificationType, string consumerSections)
        {
            // Initialize setting
            SMSEnabledForSections = (configuration["AppSettings:SMSEnabledForSections"] ?? "").Split(",").ToList();
            EmailEnabledForSections = (configuration["AppSettings:EmailEnabledForSections"] ?? "").Split(",").ToList();

            _notificationType = notificationType;
            _consumerSections = consumerSections;
        }
        public async Task Consume(ConsumeContext<INotificationRequest> context)
        {
            if (!CheckIfSectionEnabled())
            {
                return;
            }

            // Call child class implementation
            await ConsumeNotification(context);
        }

        // Child classes must implement this method
        protected abstract Task ConsumeNotification(ConsumeContext<INotificationRequest> context);
        public bool CheckIfSectionEnabled()
        {
            switch (_notificationType)
            {
                case NotificationType.Email:
                    return EmailEnabledForSections.Exists(e=> e == _consumerSections);
                case NotificationType.SMS:
                    return SMSEnabledForSections.Exists(e => e == _consumerSections);
                default:
                    return false;
            }
        }
    }
}
