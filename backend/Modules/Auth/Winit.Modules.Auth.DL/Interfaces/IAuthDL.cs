using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Auth.DL.Interfaces
{
    public interface IAuthDL
    {
        Task<ILoginResponse> ValidateUser(string userId, string password);
        Task<string?> GetEmpUIDByUserId(string userId);
        Task<bool> UpdateUserPassword(string encryptedPassword, string empUID, object? connection = null, object? transaction = null);
        Task<bool> VerifyUserIdAndSendRandomPassword(string randomSixDigitPassCode, string encryptedPassword, IEmpInfo empinfo);
    }
}
