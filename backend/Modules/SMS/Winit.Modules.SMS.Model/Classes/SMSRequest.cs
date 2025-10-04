using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.SMS.Model.Classes
{
    public class SMSRequest : ISMSRequest
    {
        public SMSRequest()
        {
            messages = new List<SMSSubRequest>();
        }
        public string ver { get; set; }
        public string key { get; set; }
        public List<SMSSubRequest> messages { get; set; }
        List<ISMSSubRequest> ISMSRequest.messages
        {
            get => messages.Cast<ISMSSubRequest>().ToList();
            set => messages = value.Cast<SMSSubRequest>().ToList();
        }
    }
}
