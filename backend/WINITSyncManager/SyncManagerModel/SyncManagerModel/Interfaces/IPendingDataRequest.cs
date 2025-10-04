using SyncManagerModel.Base;

namespace SyncManagerModel.Interfaces
{
    public interface IPendingDataRequest : ISyncBaseModel
    {
        public string LinkedItemUid { get; set; }
        public string Status { get; set; } 
        public string LinkedItemType { get; set; }
    }
}
