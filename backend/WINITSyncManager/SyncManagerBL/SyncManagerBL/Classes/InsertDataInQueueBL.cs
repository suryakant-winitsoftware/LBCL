using SyncManagerBL.Interfaces;
using SyncManagerDL.DL.Interfaces;
using SyncManagerModel.Interfaces;
 

namespace SyncManagerBL.Classes
{
    public class InsertDataInQueueBL : IInsertDataInQueueBL
    {
        protected readonly  IInsertDataInQueueDL _InsertDataInQueueRepository = null;
        public InsertDataInQueueBL( IInsertDataInQueueDL InsertDataInQueueRepository)
        {
            _InsertDataInQueueRepository = InsertDataInQueueRepository;
        }
        public async Task<int> InsertDataInQueue(IApiRequest Request)
        {
            return await _InsertDataInQueueRepository.InsertDataInQueue(Request);
        }
    }
}
