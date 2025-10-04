
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.DL.Interfaces
{
    public interface IStoreandUserReportsDL
    {

        Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserInfo>> GetStoreUserSummary(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreUserVisitDetails>> GetStoreUserActivityDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> CreateStoreRollingStats(List<IStoreRollingStatsModel> storeRollingStatsModelList);

        Task<IStoreRollingStatsModel> GetStoreUserActivityDetailsByStoreUID(string StoreUID);

    }
}
