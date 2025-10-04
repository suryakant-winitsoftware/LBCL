using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Mobile.Model.Interfaces;

namespace Winit.Modules.Auth.BL.Interfaces
{
    /// <summary>
    /// Interface for device validation business logic service
    /// </summary>
    public interface IDeviceValidationServiceBL
    {
        /// <summary>
        /// Validates if the current device is allowed to access the application for the given employee
        /// </summary>
        /// <param name="appVersionUser">App version user data (can be null for new users)</param>
        /// <param name="currentDeviceId">Current device identifier</param>
        /// <returns>Device validation result containing success status and error message if applicable</returns>
        Task<IDeviceValidationResult> ValidateDeviceAccessAsync(IAppVersionUser appVersionUser, string currentDeviceId);
    }
} 