using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SMS.Model.Interfaces
{
    public interface ISMSApiResponse
    {
        public string ackid { get; set; }
        public string time { get; set; }
        public ISMSSubApiResponse status { get; set; }
    }
}
