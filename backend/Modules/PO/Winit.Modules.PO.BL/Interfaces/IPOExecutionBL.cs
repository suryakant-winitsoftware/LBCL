using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.PO.Model.Interfaces;

namespace Winit.Modules.PO.BL.Interfaces
{
    public interface IPOExecutionBL
    {
        Task<IPOExecution> GetByUIDAsync(string uid);
        Task<string> CreateAsync(IPOExecution poExecution);
        Task<bool> UpdateAsync(IPOExecution poExecution);
        Task<bool> DeleteAsync(string uid);
        Task<List<IPOExecution>> GetByStoreUIDAsync(string storeUid);
        Task<bool> ValidateAsync(IPOExecution poExecution);
    }
} 