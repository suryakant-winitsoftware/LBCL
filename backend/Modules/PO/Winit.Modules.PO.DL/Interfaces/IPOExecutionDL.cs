using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.PO.Model.Interfaces;

namespace Winit.Modules.PO.DL.Interfaces
{
    public interface IPOExecutionDL
    {
        Task<IPOExecution> GetByUIDAsync(string uid);
        Task<string> CreateAsync(IPOExecution poExecution);
        Task<bool> UpdateAsync(IPOExecution poExecution);
        Task<bool> DeleteAsync(string uid);
        Task<List<IPOExecution>> GetByStoreUIDAsync(string storeUid);
    }
} 