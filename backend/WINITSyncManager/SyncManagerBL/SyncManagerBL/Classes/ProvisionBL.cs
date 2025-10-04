using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class ProvisionBL : IProvisionBL
    {
        private readonly IProvisionDL _mssqlprovisionDL;
        private readonly IProvisionDL _oracleprovisionDL;
        public ProvisionBL(Func<string,IProvisionDL > provisionDL)
        {
            _mssqlprovisionDL = provisionDL(Winit.Shared.Models.Constants.ConnectionStringName.SqlServer);
            _oracleprovisionDL = provisionDL(Winit.Shared.Models.Constants.ConnectionStringName.OracleServer);
        }

        public async Task<List<IProvision>> GetProvisionDetails(string sql)
        {
             return await _oracleprovisionDL.GetProvisionDetails(sql);
        }

        public async Task<int> InsertProvisionDataIntoMonthTable(List<IProvision> provisions, IEntityDetails entityDetails)
        {
             return await _mssqlprovisionDL.InsertProvisionDataIntoMonthTable(provisions, entityDetails);
        }
    }
}
