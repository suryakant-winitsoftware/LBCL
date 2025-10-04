using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.SMS.Model.Classes
{
    public class SMSSubApiResponse : ISMSSubApiResponse
    {
        public string code { get; set; }
        public string desc { get; set; }
    }
}
