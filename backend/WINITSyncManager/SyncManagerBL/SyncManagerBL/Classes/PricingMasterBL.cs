using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{

    public class PricingMasterBL : IPricingMasterBL
    {
        private readonly IPricingMasterDL _pricingMasterDL; 
        private readonly IPricingMasterStagingDL _pricingMasterStagingDL; 
        public PricingMasterBL(IPricingMasterDL pricingMasterDL, IPricingMasterStagingDL pricingMasterStagingDL)
        {
            _pricingMasterDL = pricingMasterDL;
            _pricingMasterStagingDL = pricingMasterStagingDL;
        }
        public async Task<List<IPricingMaster>> GetPricingMasterDetails(string sql)
        {
            return  await _pricingMasterDL.GetPricingMasterDetails(sql);
        }

        public async Task<int> InsertPricingDataIntoMonthTable(List<IPricingMaster> pricingMaster, IEntityDetails entitydetails)
        {
           return await _pricingMasterStagingDL.InsertPricingDataIntoMonthTable(pricingMaster, entitydetails);
        }
    }
}
