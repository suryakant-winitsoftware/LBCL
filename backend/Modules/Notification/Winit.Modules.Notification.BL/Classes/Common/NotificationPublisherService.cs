using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Notification.BL.Interfaces.Common;

namespace Winit.Modules.Notification.BL.Classes.Common
{
    public class NotificationPublisherService : INotificationPublisherService
    {
        private readonly IBus _bus;

        public NotificationPublisherService(IBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// Publishes a message to a topic exchange with a specific topic.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <param name="topic">The topic pattern (e.g., "sms.transactional").</param>
        public async Task PublishToTopicExchange(object message, string topic)
        {
            await _bus.Publish(message, context =>
            {
                context.SetRoutingKey(topic);
            });
        }
    }
}
