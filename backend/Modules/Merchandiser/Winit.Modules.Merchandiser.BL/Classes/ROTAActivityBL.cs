using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.BL.Interfaces;
using Winit.Modules.Merchandiser.DL.Interfaces;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.BL.Classes
{
    public class ROTAActivityBL : IROTAActivityBL
    {
        private readonly IROTAActivityDL _rotaActivityDL;

        public ROTAActivityBL(IROTAActivityDL rotaActivityDL)
        {
            _rotaActivityDL = rotaActivityDL;
        }

        public async Task<IROTAActivity> GetByUID(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));
            }

            return await _rotaActivityDL.GetByUID(uid);
        }

        public async Task<List<IROTAActivity>> GetAll()
        {
            return await _rotaActivityDL.GetAll();
        }

        public async Task<bool> Insert(IROTAActivity rotaActivity)
        {
            if (!await Validate(rotaActivity))
            {
                throw new ArgumentException("Invalid ROTA activity data");
            }

            return await _rotaActivityDL.Insert(rotaActivity);
        }

        public async Task<bool> Update(IROTAActivity rotaActivity)
        {
            if (!await Validate(rotaActivity))
            {
                throw new ArgumentException("Invalid ROTA activity data");
            }

            var existing = await GetByUID(rotaActivity.UID);
            if (existing == null)
            {
                throw new ArgumentException($"ROTA activity with UID {rotaActivity.UID} not found");
            }

            return await _rotaActivityDL.Update(rotaActivity);
        }

        public async Task<bool> Delete(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));
            }

            var existing = await GetByUID(uid);
            if (existing == null)
            {
                throw new ArgumentException($"ROTA activity with UID {uid} not found");
            }

            return await _rotaActivityDL.Delete(uid);
        }

        public async Task<List<IROTAActivity>> GetByJobPositionUID(string jobPositionUID)
        {
            if (string.IsNullOrEmpty(jobPositionUID))
            {
                throw new ArgumentException("Job Position UID cannot be null or empty", nameof(jobPositionUID));
            }

            return await _rotaActivityDL.GetByJobPositionUID(jobPositionUID);
        }

        public async Task<List<IROTAActivity>> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date cannot be later than end date");
            }

            return await _rotaActivityDL.GetByDateRange(startDate, endDate);
        }

        public async Task<List<IROTAActivity>> GetByJobPositionAndDateRange(string jobPositionUID, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(jobPositionUID))
            {
                throw new ArgumentException("Job Position UID cannot be null or empty", nameof(jobPositionUID));
            }

            if (startDate > endDate)
            {
                throw new ArgumentException("Start date cannot be later than end date");
            }

            return await _rotaActivityDL.GetByJobPositionAndDateRange(jobPositionUID, startDate, endDate);
        }

        public async Task<bool> Validate(IROTAActivity rotaActivity)
        {
            if (rotaActivity == null)
            {
                return false;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(rotaActivity.UID) ||
                string.IsNullOrEmpty(rotaActivity.CreatedBy) ||
                string.IsNullOrEmpty(rotaActivity.JobPositionUID) ||
                string.IsNullOrEmpty(rotaActivity.RotaActivityName))
            {
                return false;
            }

            // Validate string length
            if (rotaActivity.RotaActivityName.Length > 50)
            {
                return false;
            }

            // Validate dates
            if (rotaActivity.RotaDate == default)
            {
                return false;
            }

            // Additional business rules can be added here

            return true;
        }
    }
} 