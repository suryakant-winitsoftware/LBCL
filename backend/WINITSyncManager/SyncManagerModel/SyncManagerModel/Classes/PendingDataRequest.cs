using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;

namespace SyncManagerModel.Classes
{
    public class PendingDataRequest: SyncBaseModel,IPendingDataRequest
    {
        public string LinkedItemUid { get; set; }
        public string Status { get; set; }
        public string LinkedItemType { get; set; }

    }
}
