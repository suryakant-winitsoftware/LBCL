using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WINITServices.Classes.RabbitMQ
{
    public class RabbitMQQueueName
    {
        public const string CACHE_INVALIDATE = "CACHE_INVALIDATE";
        public static string RuleEngineQ = "RuleEngineQ";
    }
}
