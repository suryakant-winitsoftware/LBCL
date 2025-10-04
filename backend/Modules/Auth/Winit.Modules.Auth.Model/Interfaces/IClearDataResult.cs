namespace Winit.Modules.Auth.Model.Interfaces
{
    /// <summary>
    /// Interface for result indicating whether database should be deleted
    /// </summary>
    public interface IClearDataResult
    {
        /// <summary>
        /// Whether the database should be deleted (main flag for calling code)
        /// </summary>
        bool ShouldDeleteDatabase { get; set; }

        /// <summary>
        /// Original action from server response
        /// </summary>
        string ServerAction { get; set; }

        /// <summary>
        /// Whether the determination was successful
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        /// Error message if determination failed
        /// </summary>
        string ErrorMessage { get; set; }
    }
} 