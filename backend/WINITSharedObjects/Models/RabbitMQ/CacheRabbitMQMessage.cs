using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RabbitMQ
{
    public class CacheRabbitMQMessage
    {
        public string MessageType { get; set; }
        public string MessageText { get; set; }
    }
}
