using Winit.Modules.Auth.Model.Interfaces;

namespace Winit.Modules.Auth.Model.Classes
{
    /// <summary>
    /// Represents the result of device validation operation
    /// </summary>
    public class DeviceValidationResult : IDeviceValidationResult
    {
        /// <summary>
        /// Gets or sets whether the device validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the error message if validation failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error title for displaying alert dialogs
        /// </summary>
        public string ErrorTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether a new app version user record needs to be inserted
        /// </summary>
        public bool RequiresInsert { get; set; }

        /// <summary>
        /// Gets or sets whether the existing app version user record needs to be updated
        /// </summary>
        public bool RequiresUpdate { get; set; }
    }
} 