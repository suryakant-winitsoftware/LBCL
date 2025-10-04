using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.BL.Interfaces
{
    public interface IPlanogramV1BL
    {
        Task<IPlanogramSetupV1> GetPlanogramSetupV1ByUIDAsync(string uid);
        Task<string> CreatePlanogramSetupV1Async(IPlanogramSetupV1 setup);
        Task<bool> UpdatePlanogramSetupV1Async(IPlanogramSetupV1 setup);
        Task<bool> DeletePlanogramSetupV1Async(string uid);
        Task<List<IPlanogramSetupV1>> GetAllPlanogramSetupV1Async();

        Task<IPlanogramExecutionV1> GetPlanogramExecutionV1ByUIDAsync(string uid);
        Task<string> CreatePlanogramExecutionV1Async(IPlanogramExecutionV1 execution);
        Task<bool> UpdatePlanogramExecutionV1Async(IPlanogramExecutionV1 execution);
        Task<bool> DeletePlanogramExecutionV1Async(string uid);
        Task<List<IPlanogramExecutionV1>> GetPlanogramExecutionV1ByStoreUIDAsync(string storeUid);
    }
} 