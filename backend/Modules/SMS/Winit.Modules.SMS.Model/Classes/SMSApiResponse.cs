using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.SMS.Model.Classes
{
    public class SMSApiResponse : ISMSApiResponse
    {
        public SMSApiResponse()
        {
            status = new SMSSubApiResponse(); 
        }
        public string ackid { get; set; }
        public string time { get; set; }
        public SMSSubApiResponse status { get; set; }
        ISMSSubApiResponse ISMSApiResponse.status
        {
            get => (ISMSSubApiResponse)status;  
            set => status = (SMSSubApiResponse)value;  
        }
    }
}
