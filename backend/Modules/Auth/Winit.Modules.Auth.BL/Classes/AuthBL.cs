using Winit.Modules.Auth.DL.Interfaces;
using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;

namespace Winit.Modules.Auth.BL.Classes
{
    public class AuthBL : IAuthBL
    {
        protected readonly IAuthDL _authDL = null;
        public AuthBL(DL.Interfaces.IAuthDL authDL)
        {
            _authDL = authDL;
        }
        public async Task<ILoginResponse> ValidateUser(string userId, string password)
        {
            return await _authDL.ValidateUser(userId, password);
        }
        public async Task<string> GetEmpUIDByUserId(string userId)
        {
            return await _authDL.GetEmpUIDByUserId(userId);
        }

        public async Task<bool> UpdateUserPassword(string encryptedPassword, string empUID)
        {
            return await _authDL.UpdateUserPassword(encryptedPassword, empUID);
        }

        public async Task<bool> VerifyUserIdAndSendRandomPassword(string randomSixDigitPassCode, string encryptedPassword, IEmpInfo empinfo)
        {
            return await _authDL.VerifyUserIdAndSendRandomPassword(randomSixDigitPassCode, encryptedPassword, empinfo);
        }
    }
}