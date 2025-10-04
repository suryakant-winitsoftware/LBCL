using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Notification.Model.Interfaces;

namespace Winit.Modules.Notification.Model.Classes
{
    public class NotificationRequest: INotificationRequest
    {
        // UniqueUID
        public string UniqueUID { get; set; }
        // Type of the linked item (e.g., Order, Return, or null for independent notifications)
        public string? LinkedItemType { get; set; }

        // Unique identifier for the linked item (e.g., Order ID, Return ID, etc.)
        public string? LinkedItemUID { get; set; }

        // Name of the notification template
        public string TemplateName { get; set; }

        // Routing key for the topic exchange
        public string NotificationRoute { get; set; }

        // Subject of the notification (for general notifications)
        public string? Subject { get; set; }

        // Message content of the notification (for general notifications)
        public string? Message { get; set; }

        // List of recipients for the notification (supports multiple recipients)
        public List<string>? Receiver { get; set; }

        // Custom data for extensibility (e.g., Urgency, CorrelationId, etc.)
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
