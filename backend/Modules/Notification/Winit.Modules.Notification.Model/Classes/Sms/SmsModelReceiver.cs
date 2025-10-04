using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Notification.Model.Interfaces.Sms;

namespace Winit.Modules.Notification.Model.Classes.Sms
{
    public class SmsModelReceiver : ISmsModelReceiver
    {
        public long Id { get; set; }
        public string UID { get; set; }
        public string SmsRequestUID { get; set; }
        public string Receiver { get; set; }
    }
}
