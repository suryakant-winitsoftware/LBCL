using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class SelfRegistration : BaseModel , ISelfRegistration
    {
        public string MobileNo { get; set; }
        public string OTP { get; set; }
        public bool? IsVerified { get; set; }
        public string UserEnteredOtp { get; set; }
      
    }
}
