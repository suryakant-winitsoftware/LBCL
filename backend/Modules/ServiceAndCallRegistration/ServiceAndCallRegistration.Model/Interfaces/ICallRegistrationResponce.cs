using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ServiceAndCallRegistration.Model.Interfaces
{
    public interface ICallRegistrationResponce
    {
        public int StatusCode { get; set; }
        public string CallID { get; set; }
        public List<string> Errors { get; set; }
    }
}
