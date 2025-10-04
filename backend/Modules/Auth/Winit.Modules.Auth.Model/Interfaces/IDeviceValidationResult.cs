namespace Winit.Modules.Auth.Model.Interfaces
{
    /// <summary>
    /// Interface for device validation result data
    /// </summary>
    public interface IDeviceValidationResult
    {
        /// <summary>
        /// Gets or sets whether the device validation passed
        /// </summary>
        bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the error message if validation failed
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error title for displaying alert dialogs
        /// </summary>
        string ErrorTitle { get; set; }

        /// <summary>
        /// Gets or sets whether a new app version user record needs to be inserted
        /// </summary>
        bool RequiresInsert { get; set; }

        /// <summary>
        /// Gets or sets whether the existing app version user record needs to be updated
        /// </summary>
        bool RequiresUpdate { get; set; }
    }
} 