using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class PayThroughAPMasterBL : IPayThroughAPMasterBL
    {
        private readonly IPayThroughAPMasterDL _payThroughAPMasterDL;
        private readonly IPayThroughAPMasterStaginigDL _payThroughAPMasterStaginig;

        public PayThroughAPMasterBL(IPayThroughAPMasterDL payThroughAPMasterDL, IPayThroughAPMasterStaginigDL payThroughAPMasterStaginig)
        {
            _payThroughAPMasterDL = payThroughAPMasterDL;
            _payThroughAPMasterStaginig = payThroughAPMasterStaginig;
        }

        public async Task<List<IPayThroughAPMaster>> GetPayThroughAPMasterDetails(string sql)
        {
            return await _payThroughAPMasterDL.GetPayThroughAPMasterDetails(sql);
        }

        public async Task<int> InsertPayThroughAPMasterDataIntoMonthTable(List<IPayThroughAPMaster> payThroughAPMasters, IEntityDetails entityDetails)
        {
            return await _payThroughAPMasterStaginig.InsertPayThroughAPMasterDataIntoMonthTable(payThroughAPMasters, entityDetails);
        }
    }
}
