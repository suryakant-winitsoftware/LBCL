using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ServiceAndCallRegistration.Model.Interfaces
{
    public interface IServiceRequestStatus
    {
        public string CallId { get; set; }
        public string DeviceId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
