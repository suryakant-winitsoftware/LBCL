using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.ExpiryCheck.BL.Interfaces;
using Winit.Modules.ExpiryCheck.DL.Interfaces;
using Winit.Modules.ExpiryCheck.Model.Interfaces;

namespace Winit.Modules.ExpiryCheck.BL.Classes
{
    public class ExpiryCheckExecutionBL : IExpiryCheckExecutionBL
    {
        private readonly IExpiryCheckExecutionDL _expiryCheckExecutionDL;

        public ExpiryCheckExecutionBL(IExpiryCheckExecutionDL expiryCheckExecutionDL)
        {
            _expiryCheckExecutionDL = expiryCheckExecutionDL;
        }

        public async Task<IExpiryCheckExecution> GetByUIDAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));

            return await _expiryCheckExecutionDL.GetByUIDAsync(uid);
        }

        public async Task<string> CreateAsync(IExpiryCheckExecution expiryCheck)
        {
            if (!await ValidateExpiryCheckExecution(expiryCheck))
                throw new ArgumentException("Invalid expiry check execution data");

            return await _expiryCheckExecutionDL.CreateAsync(expiryCheck);
        }

        public async Task<bool> UpdateAsync(IExpiryCheckExecution expiryCheck)
        {
            if (!await ValidateExpiryCheckExecution(expiryCheck))
                throw new ArgumentException("Invalid expiry check execution data");

            return await _expiryCheckExecutionDL.UpdateAsync(expiryCheck);
        }

        public async Task<bool> DeleteAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));

            return await _expiryCheckExecutionDL.DeleteAsync(uid);
        }

        public async Task<List<IExpiryCheckExecution>> GetByStoreUIDAsync(string storeUid)
        {
            if (string.IsNullOrEmpty(storeUid))
                throw new ArgumentException("Store UID cannot be null or empty", nameof(storeUid));

            return await _expiryCheckExecutionDL.GetByStoreUIDAsync(storeUid);
        }

        public async Task<List<IExpiryCheckExecution>> GetByBeatHistoryUIDAsync(string beatHistoryUid)
        {
            if (string.IsNullOrEmpty(beatHistoryUid))
                throw new ArgumentException("Beat History UID cannot be null or empty", nameof(beatHistoryUid));

            return await _expiryCheckExecutionDL.GetByBeatHistoryUIDAsync(beatHistoryUid);
        }

        public async Task<bool> ValidateExpiryCheckExecution(IExpiryCheckExecution expiryCheck)
        {
            if (expiryCheck == null)
                return false;

            if (string.IsNullOrEmpty(expiryCheck.UID))
                return false;

            if (string.IsNullOrEmpty(expiryCheck.BeatHistoryUID))
                return false;

            if (string.IsNullOrEmpty(expiryCheck.StoreUID))
                return false;

            if (string.IsNullOrEmpty(expiryCheck.JobPositionUID))
                return false;

            if (string.IsNullOrEmpty(expiryCheck.EmpUID))
                return false;

            if (expiryCheck.ExecutionTime == DateTime.MinValue)
                return false;

            await Task.CompletedTask; // Since we're not doing any async validation yet
            return true;
        }
    }
} 