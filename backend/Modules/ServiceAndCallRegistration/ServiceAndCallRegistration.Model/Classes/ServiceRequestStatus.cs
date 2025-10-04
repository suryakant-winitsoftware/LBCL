using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;

namespace Winit.Modules.ServiceAndCallRegistration.Model.Classes
{
    public class ServiceRequestStatus : IServiceRequestStatus
    {
        public string CallId { get; set; }
        public string DeviceId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
