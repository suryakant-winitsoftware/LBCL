using SyncManagerModel.Interfaces;

namespace SyncManagerDL.DL.Interfaces
{
    public interface IInsertDataInQueueDL  
    {
        Task<int> InsertDataInQueue(IApiRequest Request);
    }
}
