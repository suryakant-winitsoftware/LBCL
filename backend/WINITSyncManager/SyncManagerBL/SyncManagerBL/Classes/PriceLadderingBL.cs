using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class PriceLadderingBL : IPriceLadderingBL
    {
        private readonly IPriceLadderingDL _priceLadderingDL;
        private readonly  IPriceLadderingStagingDL _priceLadderingStaging;
        public PriceLadderingBL(IPriceLadderingDL priceLadderingDL, IPriceLadderingStagingDL priceLadderingStaging)
        {
            _priceLadderingDL = priceLadderingDL;
            _priceLadderingStaging = priceLadderingStaging;
        }
        public async Task<List<IPriceLaddering>> GetPriceLadderingDetails(string sql)
        {
            return await _priceLadderingDL.GetPriceLadderingDetails(sql);
        }

        public async Task<int> InsertPriceLadderingDataIntoMonthTable(List<IPriceLaddering> priceLaddering, IEntityDetails entityDetails)
        {
            return await _priceLadderingStaging.InsertPriceLadderingDataIntoMonthTable(priceLaddering, entityDetails);
        }
    }
}
