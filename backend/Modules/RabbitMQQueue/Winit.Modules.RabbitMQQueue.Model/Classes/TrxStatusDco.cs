using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.RabbitMQQueue.Model.Classes
{
   public class TrxStatusDco
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string ResponseUID { get; set; }
    }
}
