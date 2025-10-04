using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SMS.Model.Interfaces
{
    public interface ISmsResponseModel
    {
        public long RequestId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
