using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Notification.Model.Classes
{
    public class NotificationPublishResult
    {
        public string? LinkedItemType { get; set; } // Unique identifier for the notification (if available)
        public string? LinkedItemUID { get; set; } // Unique identifier for the notification (if available)
        public string Status { get; set; } // "Success" or "Failed"
        public string? ErrorMessage { get; set; } // Error details if failed
    }
}
