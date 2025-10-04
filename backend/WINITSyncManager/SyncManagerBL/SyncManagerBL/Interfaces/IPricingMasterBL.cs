using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IPricingMasterBL
    {
        Task<List<SyncManagerModel.Interfaces.IPricingMaster>> GetPricingMasterDetails(string sql);
        Task<int> InsertPricingDataIntoMonthTable(List<IPricingMaster> pricingMaster, IEntityDetails entitydetails);
    }
}
