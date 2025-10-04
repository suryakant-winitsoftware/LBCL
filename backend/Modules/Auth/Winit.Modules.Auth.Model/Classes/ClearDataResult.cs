using Winit.Modules.Auth.Model.Interfaces;

namespace Winit.Modules.Auth.Model.Classes
{
    /// <summary>
    /// Result indicating whether database should be deleted
    /// </summary>
    public class ClearDataResult : IClearDataResult
    {
        /// <summary>
        /// Whether the database should be deleted (main flag for calling code)
        /// </summary>
        public bool ShouldDeleteDatabase { get; set; }

        /// <summary>
        /// Original action from server response
        /// </summary>
        public string ServerAction { get; set; } = string.Empty;

        /// <summary>
        /// Whether the determination was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if determination failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
} 