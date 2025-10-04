using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Mobile.Model.Interfaces;

namespace Winit.Modules.Auth.BL.Classes
{
    /// <summary>
    /// Business logic service for validating device registration and access permissions
    /// 
    /// Device Validation Logic:
    /// - Allow access if currentDeviceId == AppVersionUser.DeviceId
    /// - Allow access if AppVersionUser.DeviceId is empty (not registered)
    /// - Allow access if AppVersionUser.IsTest == true (test users)
    /// - Deny access if currentDeviceId != AppVersionUser.DeviceId (show error message)
    /// 
    /// This service contains pure business logic for device validation.
    /// API calls should be handled by the Services layer.
    /// </summary>
    public class DeviceValidationServiceBL : IDeviceValidationServiceBL
    {
        #region Constants
        private const string DeviceRegistrationErrorTitle = "Device Registration Error";
        private const string DeviceAlreadyRegisteredMessage = "The user already registered with other device. Please contact your manager.";
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the DeviceValidationServiceBL class
        /// </summary>
        public DeviceValidationServiceBL()
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Validates if the current device is allowed to access the application for the given employee
        /// </summary>
        /// <param name="appVersionUser">App version user data (can be null for new users)</param>
        /// <param name="currentDeviceId">Current device identifier</param>
        /// <returns>Device validation result containing success status and error message if applicable</returns>
        public async Task<IDeviceValidationResult> ValidateDeviceAccessAsync(IAppVersionUser appVersionUser, string currentDeviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(currentDeviceId))
                {
                    return new DeviceValidationResult 
                    { 
                        IsValid = false, 
                        ErrorTitle = DeviceRegistrationErrorTitle,
                        ErrorMessage = "Device ID is required for validation." 
                    };
                }

                if (appVersionUser == null)
                {
                    // If no app version user record exists, allow access (first time registration)
                    // The caller (Services layer) should handle the insert operation
                    return new DeviceValidationResult 
                    { 
                        IsValid = true,
                        RequiresInsert = true 
                    };
                }

                // Apply device validation logic
                bool isDeviceValid = IsDeviceAccessAllowed(appVersionUser, currentDeviceId);

                if (!isDeviceValid)
                {
                    return new DeviceValidationResult 
                    { 
                        IsValid = false, 
                        ErrorTitle = DeviceRegistrationErrorTitle,
                        ErrorMessage = DeviceAlreadyRegisteredMessage 
                    };
                }

                // Device validation passed
                // Check if device ID needs to be updated
                bool requiresUpdate = string.IsNullOrEmpty(appVersionUser.DeviceId) || 
                                     appVersionUser.DeviceId != currentDeviceId;

                return new DeviceValidationResult 
                { 
                    IsValid = true,
                    RequiresUpdate = requiresUpdate
                };
            }
            catch (Exception ex)
            {
                // Log exception and return error result
                return new DeviceValidationResult 
                { 
                    IsValid = false, 
                    ErrorTitle = DeviceRegistrationErrorTitle,
                    ErrorMessage = $"An error occurred during device validation: {ex.Message}" 
                };
            }

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Determines if device access is allowed based on validation rules
        /// </summary>
        /// <param name="appVersionUser">App version user data</param>
        /// <param name="currentDeviceId">Current device identifier</param>
        /// <returns>True if device access is allowed, false otherwise</returns>
        private bool IsDeviceAccessAllowed(IAppVersionUser appVersionUser, string currentDeviceId)
        {
            // Allow access if:
            // 1. Current device ID matches registered device ID
            // 2. Registered device ID is empty (not set)
            // 3. User is marked as test user
            return currentDeviceId == appVersionUser.DeviceId || 
                   string.IsNullOrEmpty(appVersionUser.DeviceId) || 
                   appVersionUser.IsTest;
        }
        #endregion
    }
} 