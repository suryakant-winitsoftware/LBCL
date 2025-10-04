using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface ISelfRegistration : IBaseModel
    {
        public string MobileNo { get; set; } 
        public string OTP { get; set; } 
        public bool? IsVerified { get; set; }
        string UserEnteredOtp { get; set; }
    }
}
