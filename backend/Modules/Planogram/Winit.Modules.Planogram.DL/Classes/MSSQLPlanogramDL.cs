using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Planogram.DL.Interfaces;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.DL.Classes
{
    public class MSSQLPlanogramDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IPlanogramDL
    {
        public MSSQLPlanogramDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public Task<string> CreatePlanogramExecutionDetailAsync(IPlanogramExecutionDetail detail)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreatePlanogramExecutionHeaderAsync(IPlanogramExecutionHeader header)
        {
            throw new NotImplementedException();
        }

        public Task<List<IPlanogramCategory>> GetPlanogramCategoriesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<IPlanogramExecutionDetail>> GetPlanogramExecutionDetailsByHeaderUIDAsync(string headerUID)
        {
            throw new NotImplementedException();
        }

        public Task<List<IPlanogramRecommendation>> GetPlanogramRecommendationsByCategoryAsync(string categoryCode)
        {
            throw new NotImplementedException();
        }

        public Task<IPlanogramSetup> GetPlanogramSetupByUIDAsync(string uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePlanogramExecutionDetailStatusAsync(string uid, bool isCompleted)
        {
            throw new NotImplementedException();
        }

        // New CRUD methods for PlanogramSetup
        public Task<List<IPlanogramSetup>> GetAllPlanogramSetupsAsync(int pageNumber = 1, int pageSize = 50)
        {
            throw new NotImplementedException();
        }

        public Task<List<IPlanogramSetup>> GetPlanogramSetupsByCategoryAsync(string categoryCode)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreatePlanogramSetupAsync(IPlanogramSetup planogramSetup)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePlanogramSetupAsync(IPlanogramSetup planogramSetup)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePlanogramSetupAsync(string uid)
        {
            throw new NotImplementedException();
        }

        public Task<object> SearchPlanogramSetupsAsync(
            string searchText, 
            List<string> categoryCodes, 
            decimal? minShelfCm, 
            decimal? maxShelfCm, 
            string selectionType,
            int pageNumber,
            int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
