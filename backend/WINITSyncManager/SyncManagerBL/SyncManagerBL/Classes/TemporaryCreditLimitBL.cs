using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class TemporaryCreditLimitBL : ITemporaryCreditLimitBL
    {
        private readonly ITemporaryCreditLimitDL _mssqlTemporaryCreditLimitDL;
        private readonly ITemporaryCreditLimitDL _oracleTemporaryCreditLimitDL;
         private readonly ITemporaryCreditLimitPullDL _mssqlTemporaryCreditLimitPullDL;
        private readonly ITemporaryCreditLimitPullDL _oracleTemporaryCreditLimitPullDL;
        private readonly Iint_CommonMethodsBL _commonMethodsBL;
        public TemporaryCreditLimitBL(Func<string, ITemporaryCreditLimitDL> temporaryCreditLimit, Iint_CommonMethodsBL iint_CommonMethodsBL
            , Func<string, ITemporaryCreditLimitPullDL> temporaryCreditLimitPull)
        {
            this._mssqlTemporaryCreditLimitDL = temporaryCreditLimit(Winit.Shared.Models.Constants.ConnectionStringName.SqlServer);
            this._oracleTemporaryCreditLimitDL = temporaryCreditLimit(Winit.Shared.Models.Constants.ConnectionStringName.OracleServer);
            this._commonMethodsBL = iint_CommonMethodsBL;
            this._mssqlTemporaryCreditLimitPullDL = temporaryCreditLimitPull(Winit.Shared.Models.Constants.ConnectionStringName.SqlServer);
            this._oracleTemporaryCreditLimitPullDL = temporaryCreditLimitPull(Winit.Shared.Models.Constants.ConnectionStringName.OracleServer);
        }

        public async Task<List<ITemporaryCreditLimit>> GetTemporaryCreditLimitPushDetails(IEntityDetails entityDetails)
        {
            return await _mssqlTemporaryCreditLimitDL.GetTemporaryCreditLimitDetails(entityDetails);
        }

        public Task<int> InsertTemporaryCreditLimitDetailsIntoMonthTable(List<ITemporaryCreditLimit> temporaryCreditLimits, IEntityDetails entityDetails)
        {
            return _mssqlTemporaryCreditLimitPullDL.InsertTemporaryCreditLimitDetailsIntoMonthTable(temporaryCreditLimits, entityDetails);
        }

        public async Task<int> InsertTemporaryCreditLimitsIntoOracleStaging(List<ITemporaryCreditLimit> temporaryCreditLimits)
        {
            return await _oracleTemporaryCreditLimitDL.InsertCustomerdetailsIntoOracleStaging(temporaryCreditLimits);
        }

        public async Task<List<ITemporaryCreditLimit>> PullTemporaryCreditLimitDetailsFromOracle(string sql)
        {
            return await _oracleTemporaryCreditLimitPullDL.PullTemporaryCreditLimitDetailsFromOracle(sql);
        }

    }
}
