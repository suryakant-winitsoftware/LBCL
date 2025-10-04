using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SMS.Model.Interfaces
{
    public interface ISMSSubApiResponse
    {
        public string code { get; set; }
        public string desc { get; set; }
    }
}
