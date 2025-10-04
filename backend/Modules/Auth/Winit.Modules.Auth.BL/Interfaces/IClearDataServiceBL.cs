using Winit.Modules.Auth.Model.Interfaces;

namespace Winit.Modules.Auth.BL.Interfaces
{
    /// <summary>
    /// Interface for determining ClearData actions based on server configuration
    /// </summary>
    public interface IClearDataServiceBL
    {
        /// <summary>
        /// Determines whether database should be deleted based on server configuration
        /// </summary>
        /// <param name="empUID">Employee UID</param>
        /// <param name="onDataUpload">Callback to execute data upload if needed</param>
        /// <returns>ClearDataResult indicating whether database should be deleted</returns>
        Task<IClearDataResult> ShouldDeleteDatabaseAsync(string empUID, Func<Task> onDataUpload = null);
    }
} 