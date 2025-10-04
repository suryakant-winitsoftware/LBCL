using Winit.Modules.Survey.DL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Classes
{
    public class StoreandUserReportsBL : Interfaces.IStoreandUserReportsBL
    {
        protected readonly DL.Interfaces.IStoreandUserReportsDL _storeandUserReportsDL;
        public StoreandUserReportsBL(IStoreandUserReportsDL storeandUserReportsDL)
        {
            _storeandUserReportsDL = storeandUserReportsDL;
        }
       public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserInfo>> GetStoreUserSummary(List<SortCriteria> sortCriterias,
           int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeandUserReportsDL.GetStoreUserSummary(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>> GetStoreUserActivityDetails(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeandUserReportsDL.GetStoreUserActivityDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<int> CreateStoreRollingStats(List<IStoreRollingStatsModel> storeRollingStatsModelList)
        {
            return await _storeandUserReportsDL.CreateStoreRollingStats(storeRollingStatsModelList);
        }

        public async  Task<IStoreRollingStatsModel> GetStoreUserActivityDetailsByStoreUID(string StoreUID)
        {
            return await _storeandUserReportsDL.GetStoreUserActivityDetailsByStoreUID(StoreUID);
        }
    }
}
