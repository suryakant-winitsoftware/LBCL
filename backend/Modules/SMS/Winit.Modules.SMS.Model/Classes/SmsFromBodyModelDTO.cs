using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.Model.Constants.Notification;

namespace Winit.Modules.SMS.Model.Classes
{
    public class SmsFromBodyModelDTO
    {
        public string TemplateName { get; set; }
        public List<string> OrderUIDs { get; set; }
        public List<NotificationTemplateNames> SmsTemplateNames { get; set; }
    }
}
