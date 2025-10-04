namespace SyncManagerDL.Interfaces
{
    public interface IPricingMasterDL
    {
        Task<List<SyncManagerModel.Interfaces.IPricingMaster>> GetPricingMasterDetails(string sql);
    }
}
