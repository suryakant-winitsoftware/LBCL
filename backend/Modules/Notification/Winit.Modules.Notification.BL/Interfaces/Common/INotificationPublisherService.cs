using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Notification.BL.Interfaces.Common
{
    public interface INotificationPublisherService
    {
        Task PublishToTopicExchange(object message, string topic);
    }
}
