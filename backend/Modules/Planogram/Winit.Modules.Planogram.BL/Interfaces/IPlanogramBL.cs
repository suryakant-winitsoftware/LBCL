using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.BL.Interfaces
{
    public interface IPlanogramBL
    {
        // Existing methods
        Task<List<IPlanogramCategory>> GetPlanogramCategoriesAsync();
        Task<List<IPlanogramRecommendation>> GetPlanogramRecommendationsByCategoryAsync(string categoryCode);
        Task<IPlanogramSetup> GetPlanogramSetupByUIDAsync(string uid);
        Task<string> CreatePlanogramExecutionHeaderAsync(IPlanogramExecutionHeader header);
        Task<string> CreatePlanogramExecutionDetailAsync(IPlanogramExecutionDetail detail);
        Task<bool> UpdatePlanogramExecutionDetailStatusAsync(string uid, bool isCompleted);
        Task<List<IPlanogramExecutionDetail>> GetPlanogramExecutionDetailsByHeaderUIDAsync(string headerUID);
        
        // New CRUD methods for PlanogramSetup
        Task<List<IPlanogramSetup>> GetAllPlanogramSetupsAsync(int pageNumber = 1, int pageSize = 50);
        Task<List<IPlanogramSetup>> GetPlanogramSetupsByCategoryAsync(string categoryCode);
        Task<string> CreatePlanogramSetupAsync(IPlanogramSetup planogramSetup);
        Task<bool> UpdatePlanogramSetupAsync(IPlanogramSetup planogramSetup);
        Task<bool> DeletePlanogramSetupAsync(string uid);
        
        // Search and filter methods
        Task<object> SearchPlanogramSetupsAsync(
            string searchText, 
            List<string> categoryCodes, 
            decimal? minShelfCm, 
            decimal? maxShelfCm, 
            string selectionType,
            int pageNumber,
            int pageSize);
    }
}
