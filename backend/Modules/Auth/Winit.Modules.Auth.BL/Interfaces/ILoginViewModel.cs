using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Auth.BL.Interfaces
{
    /// <summary>
    /// Interface for login view model functionality
    /// </summary>
    public interface ILoginViewModel
    {
        /// <summary>
        /// Authenticates user and retrieves login token
        /// </summary>
        /// <param name="loginID">User login ID</param>
        /// <param name="password">User password</param>
        /// <param name="challengeCode">Challenge code for additional security</param>
        /// <param name="androidId">Android device ID</param>
        /// <returns>Login response containing token and auth master data</returns>
        Task<ILoginResponse?> GetTokenByLoginCredentials(string loginID, string password, string challengeCode, string androidId);
        
        /// <summary>
        /// Verifies user ID and sends random password via email
        /// </summary>
        /// <param name="UserId">User ID to verify</param>
        /// <returns>Success status and message</returns>
        Task<(bool IsSuccessResponse, string Msg)> VerifyUserIdAndSendRandomPassword(string UserId);
        
        /// <summary>
        /// Updates existing password with new password
        /// </summary>
        /// <param name="changePassword">Password change request data</param>
        /// <returns>Result message</returns>
        Task<string> UpdateExistingPasswordWithNewPassword(IChangePassword changePassword);
        
        /// <summary>
        /// Retrieves organization by organization UID
        /// </summary>
        /// <param name="orgUID">Organization unique identifier</param>
        /// <returns>Organization data</returns>
        Task<Winit.Modules.Org.Model.Interfaces.IOrg?> GetOrgByOrgUID(string orgUID);
    }
}
