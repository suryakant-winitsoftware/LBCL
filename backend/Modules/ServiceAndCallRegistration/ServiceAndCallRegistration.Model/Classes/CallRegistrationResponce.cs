using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;

namespace Winit.Modules.ServiceAndCallRegistration.Model.Classes
{
    public class CallRegistrationResponce : ICallRegistrationResponce
    {
        public int StatusCode {  get; set; }
        public string CallID {  get; set; }
        public List<string> Errors { get; set; }
    }
}
