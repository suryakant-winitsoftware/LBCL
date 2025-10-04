using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.AuditTrail.Model.Constant
{
    public class ExchangeTypes
    {
        public const string Direct = "direct";
        public const string Fanout = "fanout";
        public const string Topic = "topic";
        public const string Headers = "headers";
    }
}
