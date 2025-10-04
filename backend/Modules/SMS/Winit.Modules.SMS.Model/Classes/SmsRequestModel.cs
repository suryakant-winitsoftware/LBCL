using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.SMS.Model.Classes
{
    public class SmsRequestModel : ISmsRequestModel
    {
        public string UID { get; set; }
        public int RetryCount { get; set; } 
        public string Sender { get; set; }  
        public int Priority { get; set; } = 1;
        public string MessageType { get; set; } = "Transactional";
        public string Content { get; set; }
        public List<string> Receivers { get; set; }
    }
}
