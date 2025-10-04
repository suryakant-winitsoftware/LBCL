using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.SMS.Model.Classes
{
    public class SmsResponseModel : ISmsResponseModel
    {
        public long RequestId { get; set; }
        public string RequestID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
