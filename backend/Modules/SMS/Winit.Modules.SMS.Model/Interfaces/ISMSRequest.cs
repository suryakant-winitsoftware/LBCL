using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Classes;

namespace Winit.Modules.SMS.Model.Interfaces
{
    public interface ISMSRequest
    {
        public string ver { get; set; }
        public string key { get; set; }
        public List<ISMSSubRequest> messages { get; set; }
       
    }
}
