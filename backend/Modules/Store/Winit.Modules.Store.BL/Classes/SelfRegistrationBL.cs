using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.BL.Classes
{
    public class SelfRegistrationBL : ISelfRegistrationBL
    {
        Winit.Modules.Store.DL.Interfaces.ISelfRegistrationDL _selfRegistrationDL;
        public SelfRegistrationBL(Winit.Modules.Store.DL.Interfaces.ISelfRegistrationDL selfRegistrationDL) 
        {
            _selfRegistrationDL = selfRegistrationDL;
        }
        public async Task<int> CreateSelfRegistration(ISelfRegistration selfRegistration)
        {
          return  await _selfRegistrationDL.CreateSelfRegistration(selfRegistration);
        }

        public async Task<bool> CrudSelfRegistration(ISelfRegistration selfRegistration)
        {
            return await _selfRegistrationDL.CrudSelfRegistration(selfRegistration);
        }

        public async Task<int> DeleteSelfRegistration(string UID)
        {
            return await _selfRegistrationDL.DeleteSelfRegistration(UID);
        }

        public async Task<bool> MarkOTPAsVerified(string UID)
        {
            return await _selfRegistrationDL.MarkOTPAsVerified(UID);
        }

        public async  Task<ISelfRegistration> SelectSelfRegistrationByMobileNo(string MobileNo)
        {
            return await _selfRegistrationDL.SelectSelfRegistrationByMobileNo(MobileNo);
        }

        public async Task<ISelfRegistration> SelectSelfRegistrationByUID(string UID)
        {
            return await _selfRegistrationDL.SelectSelfRegistrationByUID(UID);
        }

        public async Task<int> UpdateSelfRegistration(ISelfRegistration selfRegistration)
        {
            return await _selfRegistrationDL.UpdateSelfRegistration(selfRegistration);
        }

        public async Task<bool> VerifyOTP(string UID, string OTP)
        {
           return await _selfRegistrationDL.VerifyOTP(UID, OTP);
        }
    }
}
