using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SMS.Model.Interfaces
{
    public interface ISmsRequestModel
    {
        public string UID { get; set; }
        public string Sender { get; set; }
        public int RetryCount { get; set; }
        public int Priority { get; set; } 
        public string MessageType { get; set; }
        public string Content { get; set; }
        public List<string> Receivers { get; set; }
    }
}
