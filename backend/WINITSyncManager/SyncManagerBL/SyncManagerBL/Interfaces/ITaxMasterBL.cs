using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface ITaxMasterBL
    {
        Task<List<SyncManagerModel.Interfaces.ITaxMaster>> GetTaxMasterDetails(string sql);
        Task<int> InsertTaxDataIntoMonthTable(List<SyncManagerModel.Interfaces.ITaxMaster> taxMasters, IEntityDetails entityDetails);

    }
}
