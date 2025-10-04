using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.DL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.DL.Classes;

public class SQLiteStoreandUserReportsDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IStoreandUserReportsDL
{
    public SQLiteStoreandUserReportsDL(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public Task<int> CreateStoreRollingStats(List<IStoreRollingStatsModel> storeRollingStatsModelList)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResponse<IStoreUserVisitDetails>> GetStoreUserActivityDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResponse<IStoreUserInfo>> GetStoreUserSummary(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    public async Task<IStoreRollingStatsModel> GetStoreUserActivityDetailsByStoreUID(string StoreUID)
    {
        try
        {
            var sql = @"SELECT * from store_rolling_stats where store_uid = @StoreUID";
            var parameters = new Dictionary<string, object> { { "StoreUID", StoreUID } };

            return await ExecuteSingleAsync<IStoreRollingStatsModel>(sql, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching SurveyResponse UID", ex);
        }
    }
}
