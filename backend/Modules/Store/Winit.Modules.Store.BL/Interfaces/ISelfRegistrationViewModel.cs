using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.BL.Interfaces;

public interface ISelfRegistrationViewModel
{
    ISelfRegistration selfRegistration { get; set; }
    Task<bool> HandleSelfRegistration();
    Task<bool> VerifyOTP();
    Task<ILoginResponse?> GetToken();
    Task<IStore> GetStatusFromStore(string UID);
    Task SendSms(string Otp, string MobileNumber);
}
