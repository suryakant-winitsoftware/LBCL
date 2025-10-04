using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IPayThroughAPMasterStaginigDL
    {
        Task<int> InsertPayThroughAPMasterDataIntoMonthTable(List<SyncManagerModel.Interfaces.IPayThroughAPMaster> payThroughAPMasters, IEntityDetails entityDetails);

    }
}
