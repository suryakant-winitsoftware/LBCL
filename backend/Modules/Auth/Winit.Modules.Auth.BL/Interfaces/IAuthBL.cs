using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;

namespace Winit.Modules.Auth.BL.Interfaces
{
    public interface IAuthBL
    {
        Task<ILoginResponse> ValidateUser(string userId, string password);
        Task<string> GetEmpUIDByUserId(string userId);
        Task<bool> UpdateUserPassword(string encryptedPassword, string empUID);
        Task<bool> VerifyUserIdAndSendRandomPassword(string randomSixDigitPassCode, string encryptedPassword, IEmpInfo empinfo);
    }
}
