using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Planogram.BL.Interfaces;
using Winit.Modules.Planogram.DL.Interfaces;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.BL.Classes
{
    public class PlanogramV1BL : IPlanogramV1BL
    {
        protected readonly IPlanogramV1DL _planogramV1DL;

        public PlanogramV1BL(IPlanogramV1DL planogramV1DL)
        {
            _planogramV1DL = planogramV1DL;
        }

        public async Task<IPlanogramSetupV1> GetPlanogramSetupV1ByUIDAsync(string uid)
        {
            return await _planogramV1DL.GetPlanogramSetupV1ByUIDAsync(uid);
        }

        public async Task<string> CreatePlanogramSetupV1Async(IPlanogramSetupV1 setup)
        {
            return await _planogramV1DL.CreatePlanogramSetupV1Async(setup);
        }

        public async Task<bool> UpdatePlanogramSetupV1Async(IPlanogramSetupV1 setup)
        {
            return await _planogramV1DL.UpdatePlanogramSetupV1Async(setup);
        }

        public async Task<bool> DeletePlanogramSetupV1Async(string uid)
        {
            return await _planogramV1DL.DeletePlanogramSetupV1Async(uid);
        }

        public async Task<List<IPlanogramSetupV1>> GetAllPlanogramSetupV1Async()
        {
            return await _planogramV1DL.GetAllPlanogramSetupV1Async();
        }

        public async Task<IPlanogramExecutionV1> GetPlanogramExecutionV1ByUIDAsync(string uid)
        {
            return await _planogramV1DL.GetPlanogramExecutionV1ByUIDAsync(uid);
        }

        public async Task<string> CreatePlanogramExecutionV1Async(IPlanogramExecutionV1 execution)
        {
            return await _planogramV1DL.CreatePlanogramExecutionV1Async(execution);
        }

        public async Task<bool> UpdatePlanogramExecutionV1Async(IPlanogramExecutionV1 execution)
        {
            return await _planogramV1DL.UpdatePlanogramExecutionV1Async(execution);
        }

        public async Task<bool> DeletePlanogramExecutionV1Async(string uid)
        {
            return await _planogramV1DL.DeletePlanogramExecutionV1Async(uid);
        }

        public async Task<List<IPlanogramExecutionV1>> GetPlanogramExecutionV1ByStoreUIDAsync(string storeUid)
        {
            return await _planogramV1DL.GetPlanogramExecutionV1ByStoreUIDAsync(storeUid);
        }
    }
} 