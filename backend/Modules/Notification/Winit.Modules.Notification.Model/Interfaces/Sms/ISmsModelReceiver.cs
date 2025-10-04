using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Notification.Model.Interfaces.Sms
{
    public interface ISmsModelReceiver
    {
        public long Id { get; set; }
        public string UID { get; set; }
        public string SmsRequestUID { get; set; }
        public string Receiver { get; set; }
    }
}
