using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Interfaces;

namespace Winit.Modules.SMS.Model.Classes
{
    public class SmsRequestDTO 
    {
        public string UID { get; set; }
        public string Content { get; set; }
        public int RetryCount { get; set; }
        public string Receivers { get; set; }
    }
}
