using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SMS.Model.Interfaces
{
    public interface ISmsModelReceiver
    {
        public long Id { get; set; }
        public string UID { get; set; }
        public string SmsRequestUID { get; set; }
        public string Receiver { get; set; }
    }
}
