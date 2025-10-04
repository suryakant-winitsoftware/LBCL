using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.ExpiryCheck.Model.Interfaces;

namespace Winit.Modules.ExpiryCheck.DL.Interfaces
{
    public interface IExpiryCheckExecutionDL
    {
        Task<IExpiryCheckExecution> GetByUIDAsync(string uid);
        Task<string> CreateAsync(IExpiryCheckExecution expiryCheck);
        Task<bool> UpdateAsync(IExpiryCheckExecution expiryCheck);
        Task<bool> DeleteAsync(string uid);
        Task<List<IExpiryCheckExecution>> GetByStoreUIDAsync(string storeUid);
        Task<List<IExpiryCheckExecution>> GetByBeatHistoryUIDAsync(string beatHistoryUid);
        Task<List<IExpiryCheckExecutionLine>> GetLinesByHeaderUIDAsync(string headerUid);
    }
} 