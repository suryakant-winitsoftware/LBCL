using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IPricingMasterStagingDL
    {
        Task<int> InsertPricingDataIntoMonthTable(List<IPricingMaster> pricingMaster, IEntityDetails entityDetails);
    }
}
