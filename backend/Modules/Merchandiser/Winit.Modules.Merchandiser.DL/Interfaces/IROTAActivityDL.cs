using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.DL.Interfaces
{
    public interface IROTAActivityDL
    {
        Task<IROTAActivity> GetByUID(string uid);
        Task<List<IROTAActivity>> GetAll();
        Task<bool> Insert(IROTAActivity rotaActivity);
        Task<bool> Update(IROTAActivity rotaActivity);
        Task<bool> Delete(string uid);
        Task<List<IROTAActivity>> GetByJobPositionUID(string jobPositionUID);
        Task<List<IROTAActivity>> GetByDateRange(DateTime startDate, DateTime endDate);
        Task<List<IROTAActivity>> GetByJobPositionAndDateRange(string jobPositionUID, DateTime startDate, DateTime endDate);
    }
} 