using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface ITaxMasterStagingDL
    {
        Task<int> InsertTaxDataIntoMonthTable(List<SyncManagerModel.Interfaces.ITaxMaster> taxMasters, IEntityDetails entityDetails);
    }
}
