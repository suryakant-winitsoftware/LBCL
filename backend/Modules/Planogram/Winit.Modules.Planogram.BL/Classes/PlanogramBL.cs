using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Planogram.BL.Interfaces;
using Winit.Modules.Planogram.DL.Interfaces;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.BL.Classes
{
    public class PlanogramBL : IPlanogramBL
    {
        protected readonly IPlanogramDL _planogramDL = null;
        public PlanogramBL(IPlanogramDL planogramDL)
        {
            _planogramDL = planogramDL;
        }
        public async  Task<string> CreatePlanogramExecutionDetailAsync(IPlanogramExecutionDetail detail)
        {
            return await _planogramDL.CreatePlanogramExecutionDetailAsync(detail);
        }

        public async Task<string> CreatePlanogramExecutionHeaderAsync(IPlanogramExecutionHeader header)
        {
            return await _planogramDL.CreatePlanogramExecutionHeaderAsync(header);
        }

        public async Task<List<IPlanogramCategory>> GetPlanogramCategoriesAsync()
        {
            return await _planogramDL.GetPlanogramCategoriesAsync();
        }

        public async Task<List<IPlanogramExecutionDetail>> GetPlanogramExecutionDetailsByHeaderUIDAsync(string headerUID)
        {
            return await _planogramDL.GetPlanogramExecutionDetailsByHeaderUIDAsync(headerUID);
        }

        public async Task<List<IPlanogramRecommendation>> GetPlanogramRecommendationsByCategoryAsync(string categoryCode)
        {
            return await _planogramDL.GetPlanogramRecommendationsByCategoryAsync(categoryCode);
        }

        public async Task<IPlanogramSetup> GetPlanogramSetupByUIDAsync(string uid)
        {
            return await _planogramDL.GetPlanogramSetupByUIDAsync(uid);
        }

        public async Task<bool> UpdatePlanogramExecutionDetailStatusAsync(string uid, bool isCompleted)
        {
            return await _planogramDL.UpdatePlanogramExecutionDetailStatusAsync(uid, isCompleted);
        }

        // New CRUD methods for PlanogramSetup
        public async Task<List<IPlanogramSetup>> GetAllPlanogramSetupsAsync(int pageNumber = 1, int pageSize = 50)
        {
            return await _planogramDL.GetAllPlanogramSetupsAsync(pageNumber, pageSize);
        }

        public async Task<List<IPlanogramSetup>> GetPlanogramSetupsByCategoryAsync(string categoryCode)
        {
            return await _planogramDL.GetPlanogramSetupsByCategoryAsync(categoryCode);
        }

        public async Task<string> CreatePlanogramSetupAsync(IPlanogramSetup planogramSetup)
        {
            return await _planogramDL.CreatePlanogramSetupAsync(planogramSetup);
        }

        public async Task<bool> UpdatePlanogramSetupAsync(IPlanogramSetup planogramSetup)
        {
            return await _planogramDL.UpdatePlanogramSetupAsync(planogramSetup);
        }

        public async Task<bool> DeletePlanogramSetupAsync(string uid)
        {
            return await _planogramDL.DeletePlanogramSetupAsync(uid);
        }

        public async Task<object> SearchPlanogramSetupsAsync(
            string searchText, 
            List<string> categoryCodes, 
            decimal? minShelfCm, 
            decimal? maxShelfCm, 
            string selectionType,
            int pageNumber,
            int pageSize)
        {
            return await _planogramDL.SearchPlanogramSetupsAsync(
                searchText, categoryCodes, minShelfCm, maxShelfCm, 
                selectionType, pageNumber, pageSize);
        }
    }
}
