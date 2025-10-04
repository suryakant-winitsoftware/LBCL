using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Notification.Model.Interfaces.Email;

namespace Winit.Modules.Notification.Model.Classes.Email
{
    public class EmailModelReceiver : IEmailModelReceiver
    {
        public long Id { get; set; }
        public string UID { get; set; }
        public string EmailRequestUID { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string CcEmail { get; set; }
        public string BccEmail { get; set; }
    }
}
