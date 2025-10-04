using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.DL.Interfaces
{
    public interface ISelfRegistrationDL
    {
        Task<Model.Interfaces.ISelfRegistration> SelectSelfRegistrationByUID(string UID);
        Task<int> CreateSelfRegistration(Model.Interfaces.ISelfRegistration selfRegistration);
        Task<int> UpdateSelfRegistration(Model.Interfaces.ISelfRegistration selfRegistration);
        Task<int> DeleteSelfRegistration(string UID);
        Task<bool> CrudSelfRegistration(Winit.Modules.Store.Model.Interfaces.ISelfRegistration selfRegistration);
        Task<ISelfRegistration> SelectSelfRegistrationByMobileNo(string MobileNo);
        Task<bool> VerifyOTP(string UID, string OTP);
        Task<bool> MarkOTPAsVerified(string UID);
    }
}
