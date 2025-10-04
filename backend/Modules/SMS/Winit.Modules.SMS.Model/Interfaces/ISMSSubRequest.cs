using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SMS.Model.Interfaces
{
    public interface ISMSSubRequest
    {
        public List<string> dest { get; set; }
        public string text { get; set; }
        public string send { get; set; }
        public string type { get; set; }
    }
}
