using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IInsertDataInQueueBL
    {
        Task<int> InsertDataInQueue( IApiRequest Request);
    }
}
